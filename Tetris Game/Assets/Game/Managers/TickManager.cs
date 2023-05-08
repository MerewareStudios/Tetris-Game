using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TickManager : Singleton<TickManager>
{
	[SerializeField] private float tickInternal = 0.1f;
	[System.NonSerialized] private List<ITickable> tickables = new();
	[System.NonSerialized] private Coroutine commonTickRoutine = null;
	[System.NonSerialized] private List<CustomTickable> customTickables = new();

    public bool Running
    {
        get { return commonTickRoutine != null; }
    }

    private void StopTicking()
    {
        if (commonTickRoutine != null)
        {
            StopCoroutine(commonTickRoutine);
            commonTickRoutine = null;
        }
    }
    private void StartTicking()
    {
        commonTickRoutine = StartCoroutine(TickRoutine());
    }

    private IEnumerator TickRoutine()
    {
        while (true)
        {
            for (int i = 0; i < tickables.Count; i++)
            {
                tickables[i].OnTick();
            }
            yield return new WaitForSeconds(tickInternal);
        }
    }

    public void AddTickable(ITickable tickable)
	{
		tickables.Add(tickable);

        if (!Running)
        {
            StartTicking();
        }
	}
    public void RemoveTickable(ITickable tickable)
    {
        if (tickables.Contains(tickable))
        {
            tickables.Remove(tickable);

            if (tickables.Count == 0)
            {
                StopTicking();
            }
        }
    }

    public void AddCustomTickable(CustomTickable customTickable)
    {
        customTickables.Add(customTickable);
        customTickable.Start();
    }
    public void RemoveCustomTickable(CustomTickable customTickable)
    {
        if (customTickables.Contains(customTickable))
        {
            customTickable?.Stop();
            tickables.Remove(customTickable);
        }
    }

    public static TickManager operator +(TickManager tickManager, ITickable tickable)
    {
        tickManager.AddTickable(tickable);
        return tickManager;
    }
    public static TickManager operator -(TickManager tickManager, ITickable tickable)
    {
        tickManager.RemoveTickable(tickable);
        return tickManager;
    }
    public static TickManager operator +(TickManager tickManager, CustomTickable customTickable)
    {
        tickManager.AddCustomTickable(customTickable);
        return tickManager;
    }
    public static TickManager operator -(TickManager tickManager, CustomTickable customTickable)
    {
        tickManager.RemoveCustomTickable(customTickable);
        return tickManager;
    }

    public interface ITickable
    {
        public void OnTick();
    }

    public class CustomTickable : ITickable
    {
        public float tickInternal;
        private Coroutine coroutine;
        public System.Action OnTickAction;

        public CustomTickable(float tickInterval, System.Action OnTickAction)
        {
            this.tickInternal = tickInterval;
            this.OnTickAction = OnTickAction;
        }

        public void OnTick()
        {
            OnTickAction?.Invoke();
        }
        private IEnumerator TickRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(this.tickInternal);
                OnTick();
            }
        }
        public void Start()
        {
            coroutine = TickManager.THIS.StartCoroutine(TickRoutine());
        }
        public void Stop()
        {
            if (coroutine != null)
            {
                TickManager.THIS.StopCoroutine(coroutine);
                coroutine = null;
            }
        }
    }
}
