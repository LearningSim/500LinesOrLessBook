using System;
using System.IO;

namespace Blockcode
{
    public static class Logger
    {
        public static void Log(string message) =>
            File.AppendAllText("../../log.log", $"{DateTime.Now}|{message}\n");
    }
}