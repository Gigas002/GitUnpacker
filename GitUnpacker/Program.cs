using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using GitUnpacker.Enums;
using GitUnpacker.Localization;

namespace GitUnpacker
{
    internal static class Program
    {
        #region Properties

        /// <summary>
        /// Input directory
        /// </summary>
        private static DirectoryInfo InputDirectoryInfo { get; set; }

        /// <summary>
        /// Output directory
        /// </summary>
        private static DirectoryInfo OutputDirectoryInfo { get; set; }

        /// <summary>
        /// Threads count
        /// </summary>
        private static int ThreadsCount { get; set; }

        /// <summary>
        /// Shows if errors while parsing occured
        /// </summary>
        private static bool IsParsingErrors { get; set; }

        #endregion

        private static async Task Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();

            //Parse options
            try
            {
                Parser.Default.ParseArguments<Options>(args).WithParsed(ParseCommandLineOptions)
                      .WithNotParsed(error => IsParsingErrors = true);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                return;
            }

            //If parsing error's occured
            if (IsParsingErrors)
            {
                #if DEBUG
                Console.WriteLine(Strings.PressAnyButton);
                Console.ReadKey();
                #endif

                return;
            }

            //Create progress-reporter
            ConsoleProgress<double> consoleProgress = new ConsoleProgress<double>(Console.WriteLine);

            //Unpack .git repos
            stopwatch.Start();
            await UnpackAsync(consoleProgress).ConfigureAwait(false);

            stopwatch.Stop();

            Console.WriteLine(Strings.Done);

            #if DEBUG
            Console.WriteLine(Strings.TimePassedMs, stopwatch.ElapsedMilliseconds);
            #endif

            Console.WriteLine(Strings.TimePassed, stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours,
                              stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds,
                              stopwatch.Elapsed.Milliseconds);
        }

        #region Methods

        /// <summary>
        /// Tries to parse command line options
        /// </summary>
        /// <param name="options"><see cref="Options"/> object</param>
        private static void ParseCommandLineOptions(Options options)
        {
            //Threads
            if (options.ThreadsCount <= 0)
                throw new Exception(string.Format(Strings.LesserOrEqual, nameof(ThreadsCount), 0));

            ThreadsCount = options.ThreadsCount;

            //Output
            if (string.IsNullOrWhiteSpace(options.OutputDirectoryPath))
                throw new Exception(string.Format(Strings.IsNullOrWhitespace, nameof(options.OutputDirectoryPath)));

            OutputDirectoryInfo = new DirectoryInfo(options.OutputDirectoryPath);

            try
            {
                OutputDirectoryInfo.Create();
                OutputDirectoryInfo.Refresh();
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format(Strings.UnableToCreateDirectory, OutputDirectoryInfo.FullName),
                                    exception);
            }

            //Input
            if (string.IsNullOrWhiteSpace(options.InputDirectoryPath))
                throw new Exception(string.Format(Strings.IsNullOrWhitespace, nameof(options.InputDirectoryPath)));

            InputDirectoryInfo = new DirectoryInfo(options.InputDirectoryPath);

            if (!InputDirectoryInfo.Exists)
                throw new Exception(string.Format(Strings.DirectoryDoesntExist, InputDirectoryInfo.FullName));
            if (!InputDirectoryInfo.EnumerateFileSystemInfos().Any())
                throw new Exception(string.Format(Strings.DirectoryIsEmpty, InputDirectoryInfo.FullName));
        }

        /// <summary>
        /// Run git process async
        /// </summary>
        /// <param name="inputRepoPath">Input repo's path</param>
        /// <param name="outputRepoPath">Output repo's path</param>
        /// <returns></returns>
        private static async ValueTask GitAsync(string inputRepoPath, string outputRepoPath) => await Task.Run(() =>
        {
            string args = $"{Git.Clone} \"{inputRepoPath}\" \"{outputRepoPath}\"";
            using Process process = new Process
            {
                StartInfo = new ProcessStartInfo(nameof(Git).ToLowerInvariant())
                {
                    Arguments = args, CreateNoWindow = true, RedirectStandardInput = true,
                    RedirectStandardOutput = true, UseShellExecute = false
                }
            };

            process.Start();
            process.WaitForExit();
        }).ConfigureAwait(false);

        /// <summary>
        /// Unpacks *.git repos in directory to usual repositories
        /// </summary>
        /// <param name="progress">Progress</param>
        /// <returns></returns>
        private static async ValueTask UnpackAsync(IProgress<double> progress)
        {
            //Inform that the work has started
            Console.WriteLine(Strings.WorkStarted);

            //Some stuff for progress
            int counter = 0;
            int gitDirectoriesCount = InputDirectoryInfo
                                     .EnumerateDirectories($"*{Git.InputRepoNameEnding}", SearchOption.AllDirectories)
                                     .Count();

            //For each author directory
            foreach (DirectoryInfo authorDirectoryInfo in InputDirectoryInfo.EnumerateDirectories())
            {
                using SemaphoreSlim semaphoreSlim = new SemaphoreSlim(ThreadsCount);
                List<Task> tasks = new List<Task>();

                //For each *.git directory
                foreach (DirectoryInfo gitDirectoryInfo in authorDirectoryInfo.EnumerateDirectories())
                {
                    //Wait if tasks count >= ThreadsCount
                    await semaphoreSlim.WaitAsync().ConfigureAwait(false);

                    tasks.Add(Task.Run(async () =>
                    {
                        //try-finally block to catch inner errors and progress reporting
                        try
                        {
                            //Run git for current repo
                            await GitAsync(gitDirectoryInfo.FullName,
                                           await CreateOutputRepoPathAsync(authorDirectoryInfo, gitDirectoryInfo).ConfigureAwait(false)).ConfigureAwait(false);
                        }
                        finally
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            semaphoreSlim.Release();
                            counter++;

                            #if DEBUG
                            Console.WriteLine(Strings.CounterCount, counter, gitDirectoriesCount);
                            #endif

                            progress.Report(Convert.ToDouble(counter) / Convert.ToDouble(gitDirectoriesCount) * 100.0);
                        }
                    }));
                }

                //Wait for all tasks
                await Task.WhenAll(tasks).ConfigureAwait(false);

                //Dispose tasks
                foreach (Task task in tasks) task.Dispose();
            }
        }

        /// <summary>
        /// Create path to current repo on output
        /// </summary>
        /// <param name="authorDirectoryInfo">Authors input directory</param>
        /// <param name="gitDirectoryInfo">Current repository</param>
        /// <returns>Full path to output repo</returns>
        private static ValueTask<string> CreateOutputRepoPathAsync(DirectoryInfo authorDirectoryInfo,
                                                                   DirectoryInfo gitDirectoryInfo) =>
            new ValueTask<string>(Path.Combine(OutputDirectoryInfo.FullName,
                                               authorDirectoryInfo.Name.Replace(Git.InputRepoNameEnding, string.Empty),
                                               gitDirectoryInfo.Name));

        #endregion
    }
}
