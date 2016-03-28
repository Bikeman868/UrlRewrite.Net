using System;
using System.Runtime.InteropServices;

namespace UrlRewrite.Utilities
{
    public class PerformanceTimer
    {
        #region Static stuff for access to very high precision timing

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long performanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long frequency);

        private static readonly long _frequency;

        static PerformanceTimer()
        {
            QueryPerformanceFrequency(out _frequency);
        }

        public static long TimeNow
        {
            get
            {
                long startTime;
                QueryPerformanceCounter(out startTime);
                return startTime;
            }
        }

        #endregion

        public static double TicksToSeconds(long ticks)
        {
            return ((double)ticks) / _frequency;
        }

        public static long SecondsToTicks(double seconds)
        {
            return (long)(seconds * _frequency);
        }

        public static double TicksToMilliseconds(long ticks)
        {
            return 1000d * ticks / _frequency;
        }

        public static double TicksToMicroseconds(long ticks)
        {
            return 1000000d * ticks / _frequency;
        }

        public static double TicksToNanoseconds(long ticks)
        {
            return 1000000000d * ticks / _frequency;
        }

        private long _elapsedTime;
        private bool _running;

        public long ElapsedTicks { get { return _running ? _elapsedTime + (TimeNow - StartTicks) : _elapsedTime; } }
        public long StartTicks { get; private set; }

        public double ElapsedSeconds { get { return TicksToSeconds(ElapsedTicks); } }
        public double ElapsedMilliSeconds { get { return TicksToMilliseconds(ElapsedTicks); } }
        public double ElapsedMicroSeconds { get { return TicksToMicroseconds(ElapsedTicks); } }
        public double ElapsedNanoSeconds { get { return TicksToNanoseconds(ElapsedTicks); } }

        public PerformanceTimer Start()
        {
            _running = true;
            StartTicks = TimeNow;
            return this;
        }

        public PerformanceTimer Stop()
        {
            if (_running)
            {
                _elapsedTime += TimeNow - StartTicks;
                _running = false;
            }
            return this;
        }
    }
}
