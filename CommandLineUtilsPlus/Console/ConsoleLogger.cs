#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ConsoleLogger.cs) is part of Oetools.Sakoe.
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
using System.Diagnostics;
using CommandLineUtilsPlus.Console.ProgressBar;

namespace CommandLineUtilsPlus.Console {

    /// <summary>
    /// A console logger, which allows to log the action of a program in a console.
    /// </summary>
    public class ConsoleLogger : ConsoleWriter, IConsoleLogger, IConsoleTraceLogger {

        /// <summary>
        /// The foreground color of debug message.
        /// </summary>
        public virtual ConsoleColor DebugForegroundColor { get; set; } = ConsoleColor.Gray;

        /// <summary>
        /// The foreground color of info message.
        /// </summary>
        public virtual ConsoleColor InfoForegroundColor { get; set; } = ConsoleColor.Cyan;

        /// <summary>
        /// The foreground color of done message.
        /// </summary>
        public virtual ConsoleColor DoneForegroundColor { get; set; } = ConsoleColor.Green;

        /// <summary>
        /// The foreground color of warn message.
        /// </summary>
        public virtual ConsoleColor WarnForegroundColor { get; set; } = ConsoleColor.Yellow;

        /// <summary>
        /// The foreground color of error message.
        /// </summary>
        public virtual ConsoleColor ErrorForegroundColor { get; set; } = ConsoleColor.Red;

        /// <summary>
        /// The foreground color of fatal message.
        /// </summary>
        public virtual ConsoleColor FatalForegroundColor { get; set; } = ConsoleColor.Magenta;

        /// <summary>
        /// The foreground color of the progress bar.
        /// </summary>
        public virtual ConsoleColor ProgressForegroundColor { get; set; } = ConsoleColor.White;

        /// <summary>
        /// The foreground color of the progress bar once it reaches 100%.
        /// </summary>
        public virtual ConsoleColor? ProgressForegroundColorDone { get; set; } = ConsoleColor.Green;

        /// <summary>
        /// The foreground color of the progress bar if the loading is done but it did not reach 100%.
        /// </summary>
        public virtual ConsoleColor? ProgressForegroundColorUncomplete { get; set; } = ConsoleColor.Red;

        /// <summary>
        /// The background color of the progress bar.
        /// </summary>
        public virtual ConsoleColor? ProgressBackgroundColor { get; set; } = ConsoleColor.DarkGray;

        /// <summary>
        /// The foreground color to use for the progression text.
        /// </summary>
        public virtual ConsoleColor ProgressTextColor { get; set; } = ConsoleColor.Gray;

        /// <summary>
        /// The character to use as the progress bar foreground.
        /// </summary>
        public virtual char ProgressForegroundCharacter { get; set; } = '■';

        /// <summary>
        /// The character to use as the progress bar background.
        /// </summary>
        public virtual char ProgressBackgroundCharacter { get; set; } = '■';

        /// <summary>
        /// The minimum time interval that should elapse between 2 refresh of the progress bar.
        /// The lower the smoother the animation. But low value degrade performances.
        /// </summary>
        public virtual int ProgressMinimumRefreshRateInMilliseconds { get; set; } = 100;

        /// <summary>
        /// The maximum time interval that should elapse between 2 refresh of the progress bar.
        /// Should be less than 1s to correctly display the clock.
        /// </summary>
        public virtual int ProgressMaximumRefreshRateInMilliseconds { get; set; } = 900;

        /// <summary>
        /// Stopwatch running from this instance creation.
        /// </summary>
        public Stopwatch Stopwatch { get; }

        /// <summary>
        /// Sets and gets the current threshold at which to start logging events.
        /// </summary>
        public ConsoleLogThreshold LogTheshold { get; set; } = ConsoleLogThreshold.Info;

        /// <inheritdoc />
        public virtual IConsoleTraceLogger Trace => LogTheshold <= ConsoleLogThreshold.Debug ? this : null;

        /// <summary>
        /// Progress bar display mode.
        /// </summary>
        public ConsoleProgressBarDisplayMode ProgressBarDisplayMode { get; set; }

        private readonly IConsoleInterface _console;
        private ConsoleProgressBar _progressBar;

        /// <summary>
        /// Initializes a new instance of <see cref="ConsoleLogger"/>.
        /// </summary>
        public ConsoleLogger(IConsoleInterface console) : base(console) {
            Stopwatch = Stopwatch.StartNew();
            _console = console;
        }

        /// <inheritdoc />
        public override void Dispose() {
            base.Dispose();
            _progressBar?.Dispose();
            _progressBar = null;
        }

        /// <inheritdoc />
        public void Fatal(string message, Exception e = null) {
            Log(ConsoleLogThreshold.Fatal, message, e);
        }

        /// <inheritdoc />
        public void Error(string message, Exception e = null) {
            Log(ConsoleLogThreshold.Error, message, e);
        }

        /// <inheritdoc />
        public void Warn(string message, Exception e = null) {
            Log(ConsoleLogThreshold.Warn, message, e);
        }

        /// <inheritdoc />
        public void Info(string message, Exception e = null) {
            Log(ConsoleLogThreshold.Info, message, e);
        }

        /// <inheritdoc />
        public void Done(string message, Exception e = null) {
            Log(ConsoleLogThreshold.Done, message, e);
        }

        /// <inheritdoc />
        public void Debug(string message, Exception e = null) {
            Log(ConsoleLogThreshold.Debug, message, e);
        }

        /// <summary>
        /// Writes in debug level
        /// </summary>
        /// <param name="message"></param>
        /// <param name="e"></param>
        public void Write(string message, Exception e = null) {
            Log(ConsoleLogThreshold.Debug, message, e);
        }

        /// <inheritdoc />
        public virtual IConsoleLogger If(bool condition) => condition ? this : null;

        /// <inheritdoc cref="IConsoleLogger.ReportProgress"/>
        public virtual void ReportProgress(int max, int current, string message) {
            if (ProgressBarDisplayMode == ConsoleProgressBarDisplayMode.Off) {
                return;
            }

            if (_console.IsOutputRedirected) {
                // cannot use the progress bar
                Debug(FormatLogMessageProgressAsText(max, current, message));
                return;
            }

            try {
                if (_progressBar == null) {
                    _progressBar = new ConsoleProgressBar(_console, max, message) {
                        ClearProgressBarOnStop = ProgressBarDisplayMode == ConsoleProgressBarDisplayMode.On,
                        TextColor = ProgressTextColor,
                        BackgroundCharacter = ProgressBackgroundCharacter,
                        ForegroundCharacter = ProgressForegroundCharacter,
                        BackgroundColor = ProgressBackgroundColor,
                        ForegroundColor = ProgressForegroundColor,
                        ForegroundColorDone = ProgressForegroundColorDone,
                        ForegroundColorUncomplete = ProgressForegroundColorUncomplete,
                        MaximumRefreshRateInMilliseconds = ProgressMaximumRefreshRateInMilliseconds,
                        MinimumRefreshRateInMilliseconds = ProgressMinimumRefreshRateInMilliseconds,
                    };
                }
                if (!_progressBar.IsRunning) {
                    base.WriteResultOnNewLine(null);
                }
                if (max > 0 && max != _progressBar.MaxTicks) {
                    _progressBar.MaxTicks = max;
                }
                _progressBar.Tick(current, message);
                if (max == current) {
                    StopProgressBar();
                }
            } catch (Exception) {
                // ignored
            }
        }

        private void StopProgressBar() {
            if (_progressBar?.Stop() ?? false) {
                HasWroteToOuput = false;
            }
        }

        /// <summary>
        /// Returns the message to output when the progress can not be displayed as a progress bar and must be output as raw text.
        /// This is the case when the progression is reported in a text file for instance.
        /// </summary>
        /// <param name="max"></param>
        /// <param name="current"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual string FormatLogMessageProgressAsText(int max, int current, string message) {
            return $"[{$"{(int) Math.Round((decimal) current / max * 100, 2)}%".PadLeft(4)}] {message}";
        }

        /// <inheritdoc />
        public void ReportGlobalProgress(int max, int current, string message) {
            Log(ConsoleLogThreshold.Info, FormatLogMessageGlobalProgress(max, current, message));
        }

        /// <summary>
        /// Returns the message to display when signaling a global progress.
        /// </summary>
        /// <param name="max"></param>
        /// <param name="current"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual string FormatLogMessageGlobalProgress(int max, int current, string message) {
            return $"{message} ({(int) Math.Round((decimal) current / max * 100, 2)}%)";
        }

        /// <inheritdoc />
        public override void WriteResult(string text, ConsoleColor? color = null) {
            StopProgressBar();
            base.WriteResult(text, color);
        }

        /// <inheritdoc />
        public override void WriteResultOnNewLine(string text, ConsoleColor? color = null) {
            StopProgressBar();
            base.WriteResultOnNewLine(text, color);
        }

        /// <inheritdoc />
        public override void Write(string text, ConsoleColor? color = null, int indentation = 0, string prefixForNewLines = null) {
            StopProgressBar();
            base.Write(text, color, indentation, prefixForNewLines);
        }

        /// <inheritdoc />
        public override void WriteOnNewLine(string text, ConsoleColor? color = null, int indentation = 0, string prefixForNewLines = null) {
            StopProgressBar();
            base.WriteOnNewLine(text, color, indentation, prefixForNewLines);
        }

        /// <inheritdoc />
        public override void WriteError(string text, ConsoleColor? color = null, int indentation = 0, string prefixForNewLines = null) {
            StopProgressBar();
            base.WriteError(text, color, indentation, prefixForNewLines);
        }

        /// <inheritdoc />
        public override void WriteErrorOnNewLine(string text, ConsoleColor? color = null, int indentation = 0, string prefixForNewLines = null) {
            StopProgressBar();
            base.WriteErrorOnNewLine(text, color, indentation, prefixForNewLines);
        }

        /// <summary>
        /// Returns the prefix for a message to log (e.g. LVL [00:00] ).
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        protected virtual string FormatLogPrefix(ConsoleLogThreshold level) {
            var elapsed = Stopwatch.Elapsed;
            return $"{level.ToString().ToUpper().PadRight(5, ' ')} [{elapsed.Minutes:D2}:{elapsed.Seconds:D2}.{elapsed.Milliseconds:D3}] ";
        }

        /// <summary>
        /// Log a message.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <param name="e"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        protected virtual void Log(ConsoleLogThreshold level, string message, Exception e = null) {
            if (level < LogTheshold) {
                return;
            }
            StopProgressBar();

            var logPrefix = FormatLogPrefix(level);

            ConsoleColor outputColor;
            switch (level) {
                case ConsoleLogThreshold.Debug:
                    outputColor = DebugForegroundColor;
                    break;
                case ConsoleLogThreshold.Info:
                    outputColor = InfoForegroundColor;
                    break;
                case ConsoleLogThreshold.Done:
                    outputColor = DoneForegroundColor;
                    break;
                case ConsoleLogThreshold.Warn:
                    outputColor = WarnForegroundColor;
                    break;
                case ConsoleLogThreshold.Error:
                    outputColor = ErrorForegroundColor;
                    break;
                case ConsoleLogThreshold.Fatal:
                    outputColor = FatalForegroundColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }

            if (e != null && LogTheshold <= ConsoleLogThreshold.Debug) {
                base.WriteErrorOnNewLine(logPrefix, DebugForegroundColor);
                base.WriteError(e.ToString(), DebugForegroundColor, logPrefix.Length);
            }
            if (level >= ConsoleLogThreshold.Error) {
                base.WriteErrorOnNewLine(logPrefix, outputColor);
                base.WriteError(message, outputColor, logPrefix.Length);
            } else {
                base.WriteOnNewLine(logPrefix, outputColor);
                base.Write(message, outputColor, logPrefix.Length);
            }
        }
    }
}
