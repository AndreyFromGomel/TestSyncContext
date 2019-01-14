using System;
using System.Threading;
using System.Collections.Generic;

namespace TestSyncContext
{
    public class ThreadSyncContext : SynchronizationContext
    {
        protected sealed class WorkItem
        {
            private readonly SendOrPostCallback _callback;
            private readonly object _state;
            private readonly ManualResetEventSlim _reset;

            public WorkItem(SendOrPostCallback callback, object state, ManualResetEventSlim reset)
            {
                if (callback == null)
                    throw new ArgumentNullException(nameof(callback));

                _callback = callback;
                _state = state;
                _reset = reset;
            }

            public void Execute()
            {
                _callback(_state);
                if (_reset != null)
                {
                    _reset.Set();
                }
            }
        }

        private readonly Object _lockObject = new Object();
        private readonly Queue<WorkItem> _workItems = new Queue<WorkItem>();
        private int _executingThreadId;

        public ThreadSyncContext()
        {
            AssignThread();
        }

        public void AssignThread()
        {
            _executingThreadId = Environment.CurrentManagedThreadId;
        }

        private void VerifyThread()
        {
            if (_executingThreadId != Environment.CurrentManagedThreadId)
                throw new InvalidOperationException("Invalid thread");
        }

        private void ProcessQueue(WorkItem breakingItem)
        {
            WorkItem executedWorkItem;
            do
            {
                executedWorkItem = Dequeue();
                if (executedWorkItem != null)
                    executedWorkItem.Execute();

            } while (executedWorkItem != breakingItem);
        }

        private WorkItem Dequeue()
        {
            WorkItem currentItem = null;
            lock (_lockObject)
            {
                if (_workItems.Count > 0)
                    currentItem = _workItems.Dequeue();
            }
            return currentItem;
        }

        protected virtual void Enqueue(WorkItem workItem)
        {
            lock (_lockObject)
            {
                _workItems.Enqueue(workItem);
            }
        }

        public void Process()
        {
            VerifyThread();
            ProcessQueue(null);
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            Enqueue(new WorkItem(d, state, null));
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            if (_executingThreadId < 0)
                throw new InvalidOperationException("working thread not initialized");

            if (Environment.CurrentManagedThreadId == _executingThreadId)
            {
                WorkItem requestedWorkItem = new WorkItem(d, state, null);
                Enqueue(requestedWorkItem);
                ProcessQueue(requestedWorkItem);
            }
            else
            {
                using (var reset = new ManualResetEventSlim())
                {
                    Enqueue(new WorkItem(d, state, reset));
                    reset.Wait();
                }
            }
        }
    }
}
