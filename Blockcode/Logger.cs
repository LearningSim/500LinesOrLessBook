using System;
using System.Diagnostics;

namespace Blockcode
{
    public static class Logger
    {
        public static void Log(object message) =>
            Trace.WriteLine($"Trace|{DateTime.Now}|{message}");
    }
}