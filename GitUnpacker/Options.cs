using CommandLine;

// ReSharper disable All

namespace GitUnpacker
{
    /// <summary>
    /// Class for parsing command line arguments
    /// </summary>
    public class Options
    {
        #region Required

        /// <summary>
        /// Full path to directory with authors fodlers with *.git fodlers.
        /// </summary>
        [Option('i', "input", Required = true,
            HelpText = "Full path to directory with authors fodlers with *.git fodlers.")]
        public string InputDirectoryPath { get; set; }

        /// <summary>
        /// Full path to output directory.
        /// </summary>
        [Option('o', "output", Required = true,
            HelpText = "Full path to output directory.")]
        public string OutputDirectoryPath { get; set; }

        #endregion

        #region Optional

        /// <summary>
        /// Threads count. By default is 5.
        /// </summary>
        [Option("threads", Required = false, HelpText = "Threads count. By default is 5.")]
        public int ThreadsCount { get; set; } = 5;

        #endregion
    }
}
