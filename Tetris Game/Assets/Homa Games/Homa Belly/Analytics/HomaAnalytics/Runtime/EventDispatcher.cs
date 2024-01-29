using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class EventDispatcher : IDisposable
    {
        private const int HEARTBEAT_MILLISECONDS = 1000;
        private const int CONCURRENT_REQUESTS = 5;
        private const int REQUEST_TIMEOUT_SECONDS = 10;
        private const int MAX_RETRIES_PER_EVENT = 3;
        private const bool CHARLES_PROXY_ENABLED = false;

        private HomaAnalyticsOptions m_analyticsOptions = null;
        
        /// <summary>
        /// Pending events to be sent to the server
        /// </summary>
        private ConcurrentQueue<PendingEvent> m_pendingEvents = new ConcurrentQueue<PendingEvent>();

        /// <summary>
        /// We will keep events in this list until we receive a response. We will retry 
        /// </summary>
        private readonly List<PendingEvent> m_eventsWaitingForResponse = new List<PendingEvent>();

        private readonly object m_eventsWaitingForResponseLock = new object();

        private HttpClient m_httpClient = null;
        private CancellationTokenSource m_disposeCancellationTokenSource = new CancellationTokenSource();
        private bool m_toggled = true;

        public int PendingEventsCount => m_pendingEvents.Count;

        // ReSharper disable once InconsistentlySynchronizedField
        public int WaitingEventsResponseCount => m_eventsWaitingForResponse.Count;

        public void Initialize(HomaAnalyticsOptions options)
        {
            m_analyticsOptions = options;

            bool useCharlesProxy = Application.isEditor && CHARLES_PROXY_ENABLED;
            InitializeHttpClient(useCharlesProxy);
            
            m_httpClient.Timeout = new TimeSpan(0, 0, 0, REQUEST_TIMEOUT_SECONDS);

            RetrySentEventsHeartbeat();
        }

        private void InitializeHttpClient(bool useCharlesProxy)
        {
            if (Application.isEditor && useCharlesProxy)
            {
                var httpClientHandler = new HttpClientHandler()
                {
                    Proxy = new System.Net.WebProxy("http://localhost:8888", false),
                    UseProxy = true
                };

                m_httpClient = new HttpClient(httpClientHandler);
            }
            else
            {
                m_httpClient = new HttpClient();
            }
        }

        /// <summary>
        /// Send pending event to the server.
        /// </summary>
        public void DispatchPendingEvent(PendingEvent pendingEvent)
        {
            if(!m_toggled)
            {
                // Dispatching events is disabled, the event will be discarded
                pendingEvent.Dispatched();
                return;
            }

            if (pendingEvent.IsDispatched)
            {
                HomaAnalyticsLogger.LogWarning($"Can't dispatch an event that has been already dispatched: {pendingEvent.EventName} {pendingEvent.Id}");
                return;
            }
            
            m_pendingEvents.Enqueue(pendingEvent);
            
            TryToSendPendingEvents();
        }

        private void TryToSendPendingEvents()
        {
            int eventsToSend = m_pendingEvents.Count;
            
            int availableRequests;
            lock (m_eventsWaitingForResponseLock)
            {
                availableRequests = CONCURRENT_REQUESTS - m_eventsWaitingForResponse.Count;
            }
            
            if (eventsToSend > 0 && availableRequests > 0)
            {
                for (int i = 0; i < availableRequests; i++)
                {
                    if (m_pendingEvents.TryDequeue(out var eventToSend))
                    {
                        HomaAnalyticsTaskUtils.Consume(PostEvent(eventToSend, m_disposeCancellationTokenSource.Token));
                    }
                }
            }
        }

        private async Task PostEvent(PendingEvent pendingEvent, CancellationToken cancellationToken)
        {
            lock (m_eventsWaitingForResponseLock)
            {
                m_eventsWaitingForResponse.Add(pendingEvent);
            }
            
            try
            {
                var timeoutCancellationToken = pendingEvent.PrepareToSend(REQUEST_TIMEOUT_SECONDS);

                var data = new StringContent(pendingEvent.Json);
                // Removing the default header
                data.Headers.Remove("Content-Type");
                // Adding our custom header without any validation
                data.Headers.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

                pendingEvent.PostRetries++;
                
                // If we have reached the max retries, we remove the event without retrying again
                if (pendingEvent.PostRetries > MAX_RETRIES_PER_EVENT)
                {
                    if (m_analyticsOptions.VerboseLogs)
                    {
                        HomaAnalyticsLogger.LogWarning(
                            $"Event {pendingEvent.EventName}:{pendingEvent.Id} was not sent. Max retries reached.");
                    }

                    DisposeEventWaitingForResponse(pendingEvent, true);
                    pendingEvent.Dispatched();
                    return;
                }

                var response = await m_httpClient.PostAsync(m_analyticsOptions.EndPointUrl, data, timeoutCancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    if (m_analyticsOptions.VerboseLogs)
                    {
                        HomaAnalyticsLogger.Log($"Event {pendingEvent.EventName}-{pendingEvent.Id} sent successfully.");
                    }

                    pendingEvent.Dispatched();
                    DisposeEventWaitingForResponse(pendingEvent, false);
                    TryToSendPendingEvents();
                }
                else
                {
                    await LogFailedRequest(pendingEvent, response, cancellationToken);

                    if (pendingEvent.PostRetries >= MAX_RETRIES_PER_EVENT)
                    {
                        DisposeEventWaitingForResponse(pendingEvent, true);
                        pendingEvent.Dispatched();
                        if (m_analyticsOptions.VerboseLogs)
                        {
                            HomaAnalyticsLogger.LogWarning(
                                $"Event {pendingEvent.EventName}:{pendingEvent.Id} was not sent. Max retries reached.");
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Do nothing. This is expected because our retry policy will cancel ongoing events
                // and we will try to send them again.
                HomaAnalyticsLogger.LogWarning($"Event {pendingEvent.EventName}-{pendingEvent.Id} cancelled.");
            }
            catch (HttpRequestException e)
            {
                // Do nothing. This happens where there isn't connection.
                // wait for next try.
                HomaAnalyticsLogger.LogWarning($"HttpRequestException: Can't post message Event probably because there isn't internet connection: {pendingEvent} {e}");

                if (pendingEvent.PostRetries > 0)
                {
                    // We don't want to reach the max retries when there is no internet connection
                    // we want to preserve the event until we have connection.
                    pendingEvent.PostRetries--;
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException aggregateException)
                {
                    if (aggregateException.InnerExceptions.All(inner => inner is OperationCanceledException))
                        return;
                }
            
                HomaAnalyticsLogger.LogError($"Can't post message Event: {pendingEvent} {e}");
                DisposeEventWaitingForResponse(pendingEvent, true);
                pendingEvent.Dispatched();
            }
        }

        private async Task LogFailedRequest(PendingEvent pendingEvent,
            HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            var resultString = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(resultString))
            {
                HomaAnalyticsLogger.LogError(
                    $"Response:{response.StatusCode} {response.ReasonPhrase} Event: {pendingEvent}");
            }
            else
            {
                // Read error details
                if (Json.Deserialize(resultString) is Dictionary<string, object> dictionary)
                {
                    // Detect any error
                    if (dictionary.ContainsKey("status") && dictionary.TryGetValue("message", out var message))
                    {
                        HomaAnalyticsLogger.LogError(
                            $"Response: {response.StatusCode} {Convert.ToString(message)} Event: {pendingEvent}");
                    }
                }
                else
                {
                    HomaAnalyticsLogger.LogError(
                        $"Response:{response.StatusCode} {response.ReasonPhrase} {response.RequestMessage} Event: {pendingEvent}");
                }
            }
        }

        /// <summary>
        /// We will check at some intervals if some events have to be sent again.
        /// </summary>
        private async void RetrySentEventsHeartbeat()
        {
            while (true)
            {
                if (!Application.isPlaying)
                {
                    return;
                }
                    
                await Task.Delay(HEARTBEAT_MILLISECONDS);

                if (!Application.isPlaying)
                {
                    return;
                }

                var eventsToRetry = CancelAllSentEvents(sentEvent => sentEvent.GetElapsedTime() > REQUEST_TIMEOUT_SECONDS);
                    
                foreach (var pendingEvent in eventsToRetry)
                {
                    HomaAnalyticsLogger.LogWarning($"[HomaAnalytics] Event: {pendingEvent.EventName}:{pendingEvent.Id} timeout. It will be sent again.");
                    HomaAnalyticsTaskUtils.Consume(PostEvent(pendingEvent, m_disposeCancellationTokenSource.Token));
                }
            }
        }

        private void DisposeEventWaitingForResponse(PendingEvent pendingEvent, bool cancel)
        {
            lock (m_eventsWaitingForResponseLock)
            {
                m_eventsWaitingForResponse.Remove(pendingEvent);
            }

            if (cancel)
            {
                pendingEvent.CancelAndDispose();
            }
            else
            {
                pendingEvent.Dispose();
            }
        }


        /// <summary>
        /// Clear pending events and cancel active requests
        /// </summary>
        private void CancelAllEvents(bool removeEventsFromDisk)
        {
            // Pending events aren't sent yet, so we only need to remove and notify about it
            while (m_pendingEvents.TryDequeue(out var pendingEvent))
            {
                pendingEvent.Dispose();
                if (removeEventsFromDisk)
                {
                    pendingEvent.Dispatched();
                }
            }

            var sentEvents = CancelAllSentEvents(pendingEvent => true);

            if (removeEventsFromDisk)
            {
                foreach (var sentEvent in sentEvents)
                {
                    sentEvent.Dispatched();
                }
            }
        }

        /// <summary>
        /// Cancel all sent events that meets the given condition.
        /// </summary>
        private List<PendingEvent> CancelAllSentEvents(Func<PendingEvent, bool> conditionToCancel)
        {
            var cancelledEvents = new List<PendingEvent>();
            try
            {
                lock (m_eventsWaitingForResponseLock)
                {
                    for (var index = m_eventsWaitingForResponse.Count - 1; index >= 0; index--)
                    {
                        var sentEvent = m_eventsWaitingForResponse[index];
                        if (conditionToCancel(sentEvent))
                        {
                            DisposeEventWaitingForResponse(sentEvent, true);
                            cancelledEvents.Add(sentEvent);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                HomaAnalyticsLogger.LogError($"Error while canceling events {e}");
            }

            return cancelledEvents;
        }

        /// <summary>
        /// If false, we will stop dispatching events to the server
        /// </summary>
        public void Toggle(bool toggle)
        {
            m_toggled = toggle;

            if (!m_toggled)
            {
                CancelAllEvents(true);
            }
        }

        public void Dispose()
        {
            CancelAllEvents(false);

            m_disposeCancellationTokenSource?.Cancel();
        }
    }
}