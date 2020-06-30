#region header

// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ConsoleLogger2.cs) is part of Oetools.Sakoe.
//
// Oetools.Sakoe is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Oetools.Sakoe is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Oetools.Sakoe. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================

#endregion

using System;
using System.IO;
using System.Text;
using CommandLineUtilsPlus.Console;

namespace CommandLineUtilsPlus {
    /// <summary>
    /// A <see cref="ConsoleLogger"/> that can also log stuff to a text log file.
    /// </summary>
    public class CommandLineConsoleLogger : ConsoleLogger, ICommandLineConsoleLogger {

        /// <summary>
        /// The file size above which a warning is printed to the user asking to clean the log file.
        /// </summary>
        public virtual int MaxSizeInMegaBytes { get; set; } = 100;

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineConsoleLogger"/>.
        /// </summary>
        public CommandLineConsoleLogger(IConsoleInterface console) : base(console) { }

        private StringBuilder _logContent;
        private string _logOutputFilePath;

        /// <inheritdoc />
        public override IConsoleTraceLogger Trace => LogTheshold <= ConsoleLogThreshold.Debug ? this : null;

        /// <inheritdoc />
        public override IConsoleLogger If(bool condition) => condition ? this : null;

        /// <summary>
        /// Is this already disposed?
        /// </summary>
        public bool Disposed { get; private set; }

        /// <inheritdoc />
        public override void Dispose() {
            if (!Disposed) {
                base.Dispose();
                FlushLogToFile();
                Disposed = true;
            }
        }

        /// <inheritdoc cref="IConsoleLogger.ReportProgress"/>
        public override void ReportProgress(int max, int current, string message) {
            LogToFile(ConsoleLogThreshold.Debug, FormatLogMessageProgressAsText(max, current, message), null);
            base.ReportProgress(max, current, message);
        }

        /// <inheritdoc />
        protected override void Log(ConsoleLogThreshold level, string message, Exception e = null) {
            LogToFile(level, message, e);
            base.Log(level, message, e);
        }

        /// <summary>
        /// Sets or gets the path to a log file where every event will be written.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public string LogOutputFilePath {
            get => _logOutputFilePath;
            set {
                _logOutputFilePath = value;
                if (!StaticUtilities.IsPathRooted(_logOutputFilePath)) {
                    _logOutputFilePath = Path.Combine(Directory.GetCurrentDirectory(), _logOutputFilePath);
                }

                _logContent = new StringBuilder();
                OnAfterLogOutputFileInit();
            }
        }

        /// <summary>
        /// Method called after initializing (setting) <see cref="LogOutputFilePath"/>.
        /// </summary>
        /// <exception cref="Exception"></exception>
        protected virtual void OnAfterLogOutputFileInit() {
            if (File.Exists(_logOutputFilePath)) {
                if (new FileInfo(_logOutputFilePath).Length > MaxSizeInMegaBytes * 1024 * 1024) {
                    Warn($"The log file has a size superior to {MaxSizeInMegaBytes}MB, please consider clearing it: {_logOutputFilePath.PrettyQuote()}.");
                }
            } else {
                try {
                    var dirName = Path.GetDirectoryName(_logOutputFilePath);
                    if (!string.IsNullOrEmpty(dirName) && !Directory.Exists(dirName)) {
                        Directory.CreateDirectory(dirName);
                    }

                    File.WriteAllText(_logOutputFilePath, "");
                } catch (Exception e) {
                    throw new Exception($"Could not create the log file: {_logOutputFilePath.PrettyQuote()}. {e.Message}", e);
                }
            }

            Info($"Logging to file: {_logOutputFilePath.PrettyQuote()}.");
            _logContent.AppendLine("============================================");
            _logContent.AppendLine($"========= NEW LOG SESSION {DateTime.Now:yy-MM-dd} =========");
        }

        private void FlushLogToFile() {
            if (string.IsNullOrEmpty(LogOutputFilePath) || _logContent == null) {
                return;
            }

            File.AppendAllText(LogOutputFilePath, _logContent.ToString());
            _logContent.Clear();
        }

        private void LogToFile(ConsoleLogThreshold level, string message, Exception e) {
            if (_logContent != null) {
                _logContent.Append(FormatLogPrefixForFileOutput(level));
                _logContent.AppendLine(message);
                if (e != null) {
                    _logContent.AppendLine(e.ToString());
                }

                if (_logContent.Length > 100000) {
                    FlushLogToFile();
                }
            }
        }

        /// <summary>
        /// Returns the prefix for a message to log in a file (e.g. LVL [00:00] ).
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        protected virtual string FormatLogPrefixForFileOutput(ConsoleLogThreshold level) {
            var elapsed = Stopwatch.Elapsed;
            return $"{level.ToString().ToUpper().PadRight(5, ' ')} [{elapsed.Minutes:D2}:{elapsed.Seconds:D2}.{elapsed.Milliseconds:D3}] ";
        }
    }
}
