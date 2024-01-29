using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class TaskUtils
    {
        public static void ListenForErrors(this Task voidedTask)
        {
            voidedTask.ContinueWith(task =>
            {
                if (!task.IsCanceled) 
                    Debug.LogException(task.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public static async Task WaitUntil(Func<bool> predicate, CancellationToken cancellationToken = default)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            
            while (! predicate())
            {
                await Task.Delay(millisecondsDelay: 100, cancellationToken);
            }
        }
    }
}