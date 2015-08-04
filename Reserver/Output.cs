using System;

namespace ReServer.Core
{
    /// <summary>
    /// A class for outputting logging and operational information from the server
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// Destination of the output (i.e. Console)
        /// </summary>
        public static OutputModeEnum OutputMode { get; set; }

        private static object _outputLock = new { outputLock = "thisIsAnArbitraryLockVariable" };

        /// <summary>
        /// Write the passed text to the current logging output
        /// </summary>
        /// <param name="text">The text to write</param>
        public static void Write(string text)
        {
            switch (OutputMode)
            {
                case OutputModeEnum.Console:
                    try
                    {
                        //Nothing
                    }
                    finally
                    {
                        Console.WriteLine(text);
                    }
                    break;
            }
        }
    }
}
