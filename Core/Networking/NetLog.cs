using System;
using System.Diagnostics;
using System.IO;

namespace VectorRumble.Networking
{
    /// <summary>
    /// A small log of what happened on the network (who joined or left, why a session ended),
    /// written to VectorRumble-net.log in the temp folder. A packaged game has no console, so
    /// without this there is nothing to look at when a match ends unexpectedly.
    /// </summary>
    public static class NetLog
    {
        static readonly object sync = new object();

        /// <summary>
        /// Where the log is written
        /// </summary>
        public static readonly string Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), "VectorRumble-net.log");

        public static void Write(string message)
        {
            string line = DateTime.Now.ToString("HH:mm:ss.fff") + "  " + message;
            Debug.WriteLine("[NET] " + message);

            try
            {
                lock (sync)
                    File.AppendAllText(Path, line + Environment.NewLine);
            }
            catch (Exception)
            {
                // a log that cannot be written must never stop the game
            }
        }
    }
}
