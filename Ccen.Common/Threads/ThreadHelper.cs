using System;
using System.Threading;
using log4net;

namespace Amazon.Common.Threads
{
    public class ThreadHelper
    {
        public static void Sleep(TimeSpan time)
        {
            Thread.Sleep(time);
        }

        public static void Sleep(ManualResetEvent manualEvent, TimeSpan time)
        {
            manualEvent.WaitOne(time);
        }
    }
}
