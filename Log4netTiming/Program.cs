using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using log4net;

namespace Log4netTiming
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            Log.Debug("Enter");
            try
            {
                const int defaultMaxExponent = 4;
                int maxExponent = defaultMaxExponent;
                if (args.Length > 0)
                {
                    if (! int.TryParse(args[0], out maxExponent))
                    {
                        PrintUsage();
                        return;
                    } 
                }
                new Program().Run(maxExponent);
            }
            catch (Exception e)
            {
                Log.Error("Exception", e);
            }
            Log.Debug("Exit");
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Parameters: [maxExponent] ");
            Console.WriteLine("   maxExponent: Controls the number of max iterations: 10^maxExponent");
        }

        public void Run(int maxExponent)
        {
            var iterationsList = Enumerable.Range(1, maxExponent).Select(exponent => (int)Math.Pow(10, exponent)).ToList();

            var prefixList = new List<string> {"OutputProduced", "NoOutput"};
            var postfixList = new List<string> {"Udp", "File"};
            var logList =
                from pre in prefixList
                from post in postfixList
                select LogManager.GetLogger(string.Format("{0}.{1}", pre, post));

            var dataTable = new DataTable();
            const string loggerColumnName = "Logger";
            dataTable.Columns.Add(loggerColumnName);
            foreach (var iterations in iterationsList)
            {
                dataTable.Columns.Add(iterations.ToString(CultureInfo.InvariantCulture));
            }

            foreach (var log in logList)
            {
                var localLog = log;
                var timedResults = LogLoop(localLog, iterationsList);

                var row = dataTable.NewRow();
                row[loggerColumnName] = log.Logger.Name;
                foreach (var timedResult in timedResults)
                {
                    row[timedResult.Key.ToString(CultureInfo.InvariantCulture)] = timedResult.Value.TotalMilliseconds;
                }
                dataTable.Rows.Add(row);
            }

            var csvString = dataTable.ToCsv();
            Console.WriteLine(csvString);
        }

        /// <summary>
        /// Measure the time it takes to perform logging a different number of times.
        /// For each number of iterations, the time spent is recorded and returned.
        /// </summary>
        /// <param name="log">the logger to which to log</param>
        /// <param name="iterationsList">The different number of iterations to use</param>
        /// <returns>A dictionary that maps from number of iterations to time elapsed</returns>
        private SortedDictionary<int,TimeSpan> LogLoop(ILog log, IEnumerable<int> iterationsList)
        {
            var result = new SortedDictionary<int, TimeSpan>();

            foreach (var iterations in iterationsList)
            {
                var localIterations = iterations;
                result.Add(iterations, Time(() => LogLoop(log, localIterations)));
            }

            //var f1 = iterationsList.Select(n => 1 + n);
            //result.Add(4, new TimeSpan()); 
            //var f2 = iterationsList.Select(n => result.Add(n, new TimeSpan()));
            //iterationsList.
            //    Select(nTimes =>
            //        result.Add(nTimes, Time(() => LogLoop(log, nTimes))));
            return result;
        }

        /// <summary>
        /// Measure the time it takes to perform logging
        /// </summary>
        /// <param name="log">the logger to which to log</param>
        /// <param name="iterations">The number of iterations to use</param>
        /// <returns>A dictionary that maps from number of iterations to time elapsed</returns>
        private void LogLoop(ILog log, int iterations)
        {
            for (var i = 0; i < iterations; i++)
            {
                log.Debug("What does the porcupine say?");
            }
        }

        /// <summary>
        /// Record the time it takes to perform the supplied action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns>The time elapsed</returns>
        private TimeSpan Time(Action action)
        {
            var watch = new Stopwatch();
            watch.Start();
            action();
            return watch.Elapsed;
        }
    }
}
