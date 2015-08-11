using System.Threading;

namespace ReServer.Server
{
    public abstract class RSThread
    {
        /// <summary>
        /// The ReServer-designated name of the thread, for use in logging
        /// </summary>
        public string Name
        {
            get
            {
                return "Thread-" + Thread.CurrentThread.ManagedThreadId;
            }
        }
    }
}
