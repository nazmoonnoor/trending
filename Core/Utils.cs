using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class Utils
    {
        public static int TimeElapse(Action action)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            action();
            sw.Stop();
            return sw.Elapsed.Milliseconds;
        }
    }
}
