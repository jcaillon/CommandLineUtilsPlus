#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ABaseCommand.cs) is part of Oetools.Sakoe.
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
using CommandLineUtilsPlus.Console;
using CommandLineUtilsPlus.Console.ProgressBar;
using CommandLineUtilsPlus.Extension;
using McMaster.Extensions.CommandLineUtils;

namespace CommandLineUtilsPlus.Command {

    /// <summary>
    /// Defines options that will be inherited by all the commands
    /// </summary>
    public abstract class AExecutionCommand : ACommand {

        /// <summary>
        /// The exit code to use on fatal error.
        /// </summary>
        public const int FatalExitCode = 9;

        /// <summary>
        /// Long name for the help option.
        /// </summary>
        public const string HelpLongName = "help";

        internal const string VerbosityShortName = "vb";

        /// <summary>
        /// Allows to specify the verbosity threshold above which to start logging messages.
        /// </summary>
        [Option("-" + VerbosityShortName + "|--verbosity <level>", "Sets the verbosity of this command line tool. To get the 'raw output' of a command (without displaying the log), you can set the verbosity to `none`. Specifying this option without a level value sets the verbosity to `debug`. Not specifying the option defaults to `info`.", CommandOptionType.SingleOrNoValue, Inherited = true)]
        public (bool HasValue, ConsoleLogThreshold? Value) VerbosityThreshold { get; set; }

        /// <summary>
        /// The path of the text log file in which to output the logs.
        /// </summary>
        [Option("-lo|--log-output <file>", "Output all the log message in a file, independently of the current verbosity. This allow to have a normal verbosity in the console while still logging everything to a file. Specifying this option without a value will output to a default log file.", CommandOptionType.SingleOrNoValue, Inherited = true)]
        public (bool HasValue, string Value) LogOutputFilePath { get; set; }

        /// <summary>
        /// Should the logo be display before the command execution.
        /// </summary>
        [Option("-wl|--with-logo", "Always show the logo on start.", CommandOptionType.NoValue, Inherited = true)]
        public bool IsLogoOn { get; set; }

        /// <summary>
        /// Specify how to display the progress bars.
        /// </summary>
        [Option("-pb|--progress-bar <mode>", "Sets the display mode of progress bars. Specify `off` to hide progress bars and `persistent` to make them persistent. Defaults to `on`, which show progress bars but hide them when done.", CommandOptionType.SingleValue, Inherited = true)]
        public ConsoleProgressBarDisplayMode? ProgressBarDisplayMode { get; set; }

        /// <summary>
        /// Get the verbosity to use for this command execution.
        /// </summary>
        protected ConsoleLogThreshold Verbosity => VerbosityThreshold.HasValue ? VerbosityThreshold.Value ?? ConsoleLogThreshold.Debug : ConsoleLogThreshold.Info;

        /// <summary>
        /// Access the console.
        /// </summary>
        protected IConsoleInterface Con { get; private set; }

        /// <summary>
        /// Help formatter that ease the display of an help text.
        /// </summary>
        protected IHelpWriter HelpWriter { get; private set; }

        /// <summary>
        /// The cancellation source for the current command execution.
        /// </summary>
        /// <remarks>
        /// It will be cancelled if the user presses CTRL+C.
        /// </remarks>
        protected CancellationTokenSource CancellationSource { get; set; }

        /// <summary>
        /// The cancel token for the current command execution.
        /// </summary>
        /// <inheritdoc cref="CancellationSource"/>
        protected CancellationToken? CancelToken => CancellationSource?.Token;

        /// <summary>
        /// Method called before the command execution.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="console"></param>
        protected virtual void ExecutePreCommand(CommandLineApplication app, IConsole console) { }

        /// <summary>
        /// Method called after the command execution.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="console"></param>
        protected virtual void ExecutePostCommand(CommandLineApplication app, IConsole console) { }

        internal List<string> DebugLogFromSetOptionValueFromEnvironmentVariable { get; set; } = new List<string>();

        private int _numberOfCancelKeyPress;

        private object _lock = new object();

        /// <summary>
        /// Method called on command execution.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="console"></param>
        /// <returns></returns>
        public int OnExecute(CommandLineApplication app, IConsole console) {
            Con = console as IConsoleInterface;
            HelpWriter = app.HelpTextGenerator as IHelpWriter;

            // set up the console logger
            ConsoleLogger.LogTheshold = Verbosity;
            ConsoleLogger.ProgressBarDisplayMode = ProgressBarDisplayMode ?? ConsoleProgressBarDisplayMode.On;
            if (LogOutputFilePath.HasValue) {
                var logFilePath = LogOutputFilePath.Value;
                if (string.IsNullOrEmpty(logFilePath)) {
                    logFilePath = GetLogFilePathDefaultValue();
                }
                ConsoleLogger.LogOutputFilePath = logFilePath;
            }

            // log stuff that might have happened during option parsing (set option from environment variable).
            foreach (var logLine in DebugLogFromSetOptionValueFromEnvironmentVariable) {
                Log?.Debug(logLine);
            }
            DebugLogFromSetOptionValueFromEnvironmentVariable.Clear();

            console.CancelKeyPress += ConsoleOnCancelKeyPress;

            if (IsLogoOn) {
                DrawLogo(app, Out);
            }

            int exitCode = FatalExitCode;
            Log.Debug($"Starting execution: {DateTime.Now:yyyy MMM dd} @ {DateTime.Now:HH:mm:ss}.");

            try {
                using (CancellationSource = new CancellationTokenSource()) {
                    ExecutePreCommand(app, console);
                    exitCode = ExecuteCommand(app, console);
                    ExecutePostCommand(app, console);
                }
                if (exitCode.Equals(0)) {
                    Log.Done("Exit code 0");
                } else {
                    Log.Warn($"Exit code {exitCode}");
                }
                if (Verbosity < ConsoleLogThreshold.None) {
                    Out.WriteOnNewLine(null);
                }

            } catch (Exception e) {
                Log.Error(e.Message, e);
                if (Verbosity > ConsoleLogThreshold.Debug) {
                    Log.Info($"Get more details on this error by switching to debug verbosity: -{VerbosityShortName}.");
                }
                if (e is CommandException ce) {
                    exitCode = ce.ExitCode;
                }
                Log.Fatal($"Exit code {exitCode}");
                if (Verbosity < ConsoleLogThreshold.None) {
                    Out.WriteOnNewLine(null);
                }
            }

            console.CancelKeyPress -= ConsoleOnCancelKeyPress;

            return exitCode;
        }

        /// <summary>
        /// Called when the options of the command line are not validated correctly.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="app"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public virtual int OnValidationError(ValidationResult r, CommandLineApplication app) {
            // log stuff that might have happened during option parsing (set option from environment variable).
            foreach (var logLine in DebugLogFromSetOptionValueFromEnvironmentVariable) {
                Log.Info(logLine);
            }
            var faultyMembers = string.Join(", ", r.MemberNames);
            Log.Error($"{(faultyMembers.Length > 0 ? $"Validation error for {faultyMembers}: ": "")}{r.ErrorMessage}");
            Log.Info($"Usage: {(app.HelpTextGenerator as CommandLineHelpGenerator)?.GenerateUsageString(app)}");
            Log.Info($"Specify --{HelpLongName} for a list of available options and commands.");
            Log.Fatal($"Exit code {FatalExitCode}");
            Out.WriteOnNewLine(null);
            return FatalExitCode;
        }

        /// <summary>
        /// Returns the path to the default log file.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetLogFilePathDefaultValue() {
            return Path.Combine(Directory.GetCurrentDirectory(), "app.log");
        }

        /// <summary>
        /// The method to override for each command
        /// </summary>
        /// <param name="app"></param>
        /// <param name="console"></param>
        /// <returns></returns>
        protected virtual int ExecuteCommand(CommandLineApplication app, IConsole console) {
            if (!IsLogoOn && app.Parent == null) {
                DrawLogo(app, Out);
            }
            app.ShowHelp();
            Log.Warn(GetProvideCommandHelpText(app));
            return 1;
        }

        /// <summary>
        /// Return the command option of this type from the property name.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected OptionAttribute GetCommandOptionFromPropertyName(string propertyName) {
            var propertyInfo = GetType().GetProperty(propertyName);
            if (propertyInfo != null && Attribute.GetCustomAttribute(propertyInfo, typeof(OptionAttribute), true) is OptionAttribute option) {
                return option;
            }
            return null;
        }

        /// <summary>
        /// On CTRL+C
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e) {
            if (Monitor.TryEnter(_lock, 500)) {
                try {
                    if (_numberOfCancelKeyPress < 3) {
                        Log.Warn($"CTRL+C pressed (press {3 - _numberOfCancelKeyPress} times more for instantaneous exit).");
                    } else {
                        Log.Warn($"CTRL+C pressed.");
                    }
                    if (_numberOfCancelKeyPress == 0) {
                        Log.Warn("Cancelling execution, please be patient...");
                    }
                    _numberOfCancelKeyPress++;
                    Con.ResetColor();
                    CancellationSource?.Cancel();
                    if (_numberOfCancelKeyPress < 4) {
                        e.Cancel = true;
                    }
                } finally {
                    Monitor.Exit(_lock);
                }
            }
        }

        /// <summary>
        /// Pause the program execution until a condition is fulfilled or CTRL+C is pressed or CTRL+D is pressed.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="messageActionOnCtrlC"></param>
        /// <returns>Returns true if CTRL+D has been pressed.</returns>
        protected bool SpinUntilConditionOrDetachedOrExited(Func<bool> condition, string messageActionOnCtrlC) {
            Out.WriteResultOnNewLine($"Press CTRL+C to {messageActionOnCtrlC}.");
            Out.WriteResultOnNewLine("Press CTRL+D to detach and go back to prompt.");
            do {
                SpinWait.SpinUntil(() => condition() || Con.KeyAvailable || CancellationSource.IsCancellationRequested);
                if (condition() || CancellationSource.IsCancellationRequested) {
                    break;
                }
                var pressedKey = Con.ReadKey();
                if ((pressedKey.Modifiers & ConsoleModifiers.Control) != 0 && pressedKey.Key == ConsoleKey.D) {
                    Log.Info("CTRL+D pressed, detaching.");
                    return true;
                }
            } while (!CancellationSource.IsCancellationRequested);

            return false;
        }

        /// <summary>
        /// Prompt the user with a yes/no question
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        protected bool? PromptYesNo(string question) {
            Con.CursorVisible = true;
            var numberOfCancelKeyPress = _numberOfCancelKeyPress;
            _numberOfCancelKeyPress = 5;
            try {
                do {
                    Out.WriteOnNewLine($"{question} [y/n]");
                    SpinWait.SpinUntil(() => Con.KeyAvailable || CancellationSource.IsCancellationRequested);
                    if (CancellationSource.IsCancellationRequested) {
                        break;
                    }
                    var pressedKey = Con.ReadKey();
                    if (pressedKey.Key == ConsoleKey.Y) {
                        return true;
                    }
                    if (pressedKey.Key == ConsoleKey.N) {
                        return false;
                    }
                    Out.WriteOnNewLine("Please answer 'y' or 'n' or use CTRL+C to exit.");
                } while (!CancellationSource.IsCancellationRequested);
                return null;
            } finally {
                Con.CursorVisible = false;
                _numberOfCancelKeyPress = numberOfCancelKeyPress;
            }
        }

        /// <summary>
        /// Prompt the user for a integer response.
        /// </summary>
        /// <remarks>TODO needs to be redone</remarks>
        /// <param name="question"></param>
        /// <returns></returns>
        protected int PromptInteger(string question) {
            Out.WriteOnNewLine(" ");
            return Prompt.GetInt(question);
        }

        /// <summary>
        /// Prompt the user for a string response.
        /// </summary>
        /// <remarks>TODO needs to be redone</remarks>
        /// <param name="question"></param>
        /// <returns></returns>
        protected string PromptString(string question) {
            Out.WriteOnNewLine(" ");
            return Prompt.GetString(question);
        }

        /// <summary>
        /// Prompt the user for a password response.
        /// </summary>
        /// <remarks>TODO needs to be redone</remarks>
        /// <param name="question"></param>
        /// <returns></returns>
        protected string PromptPassword(string question) {
            Out.WriteOnNewLine(" ");
            return Prompt.GetPassword(question);
        }

        /// <summary>
        /// Prompt the user for a password response.
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        protected SecureString PromptPasswordAsSecureString(string question) {
            Out.WriteOnNewLine(" ");
            return Prompt.GetPasswordAsSecureString(question);
        }
    }

}
