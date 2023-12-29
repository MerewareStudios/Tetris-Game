using System;
using System.Collections.Generic;
using UnityEngine;

public class WorkerThread : MonoBehaviour
{
    public static WorkerThread Current;
    readonly Queue<Action> _jobs = new Queue<Action>();

    void Awake() {
        Current = this;
        // Idle();
    }

    void Update() {
        while (_jobs.Count > 0)
        {
            _jobs.Dequeue().Invoke();
            // Idle();
        }
    }

    private void Idle()
    {
        if (_jobs.Count == 0)
        {
            this.enabled = false;
        }
    }

    public void AddJob(Action newJob) {
        _jobs.Enqueue(newJob);
        // this.enabled = true;
    }
}
