using System;
using System.Threading;

namespace TestSyncContext
{
    public class SyncContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object state)
        {
            Console.WriteLine("Post");
            base.Post(d, state);
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            Console.WriteLine("Send");
            base.Send(d, state);
        }
    }
}