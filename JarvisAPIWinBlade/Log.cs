using System.Diagnostics;

namespace Jarvis.API
{
    /// <summary>
    /// Used for debugging when using behaviors.
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// Logs an info message to the correct output log.
        /// </summary>
        /// <param name="msg">The message to log</param>
        public static void Info(string msg)
        {
            Debug.WriteLine("Info: " + msg);
            Debug.Flush();
        }

        /// <summary>
        /// Logs a warning message to the correct output log.
        /// </summary>
        /// <param name="msg">The warning to log</param>
        public static void Warn(string msg)
        {
            Debug.WriteLine("Warning: " + msg);
            Debug.Flush();
        }

        /// <summary>
        /// Logs an error message to the correct output log.
        /// </summary>
        /// <param name="msg">The error to log</param>
        public static void Error(string msg)
        {
            Debug.WriteLine("Error: " + msg);
            Debug.Flush();
        }
    }
}