#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (CommandLineApplicationCustomHint.cs) is part of Oetools.Sakoe.
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
using System.Linq;
using CommandLineUtilsPlus.Command;
using CommandLineUtilsPlus.Console;
using CommandLineUtilsPlus.Conventions;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.HelpText;

namespace CommandLineUtilsPlus {

    /// <summary>
    /// A <see cref="CommandLineApplication"/> without the command hint.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class CommandLineApplicationPlus<TModel> : CommandLineApplication<TModel> where TModel : class {

        /// <summary>
        /// The console used for this CLI.
        /// </summary>
        protected readonly CommandLineConsoleInterface Console;

        /// <summary>
        /// The console logger used for this CLI.
        /// </summary>
        protected readonly ICommandLineConsoleLogger ConsoleLogger;

        /// <inheritdoc />
        protected CommandLineApplicationPlus(IHelpTextGenerator helpTextGenerator, CommandLineConsoleInterface console, ICommandLineConsoleLogger consoleLogger, string workingDirectory) : base(helpTextGenerator, console, workingDirectory) {
            Console = console;
            ConsoleLogger = consoleLogger;
        }

        /// <inheritdoc />
        public override void ShowHint() { }

        /// <summary>
        /// The entry point for the command line application.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="logger"></param>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static int ExecuteCommand(string[] args, Func<IConsoleInterface, ICommandLineConsoleLogger> logger = null, Func<IConsoleWriter, ICommandLineHelpGenerator> helper = null) {
            var console = new CommandLineConsoleInterface();
            var consoleLogger = logger?.Invoke(console) ?? new CommandLineConsoleLogger(console);
            var helpGenerator = helper?.Invoke(consoleLogger) ?? new CommandLineHelpGenerator(consoleLogger);
            try {
                using (var app = new CommandLineApplicationPlus<TModel>(helpGenerator, console, consoleLogger, Directory.GetCurrentDirectory())) {
                    return app.ExecuteCommandInternal(args);
                }
            } catch (Exception ex) {
                consoleLogger.LogTheshold = ConsoleLogThreshold.Debug;
                consoleLogger.Error(ex.Message, ex);
                consoleLogger.Fatal($"Exit code {AExecutionCommand.FatalExitCode}");
                consoleLogger.WriteOnNewLine(null);
                return AExecutionCommand.FatalExitCode;
            } finally {
                consoleLogger.Dispose();
            }
        }

        /// <summary>
        /// The entry point for the command line application.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual int ExecuteCommandInternal(string[] args) {
            try {
                Console.CursorVisible = false;
                ConfigureApp();
                return Execute(args);
            } catch (Exception ex) {
                ConsoleLogger.LogTheshold = ConsoleLogThreshold.Debug;

                if (ex is CommandParsingException) {
                    ConsoleLogger.Error(ex.Message);
                    if (ex is UnrecognizedCommandParsingException unrecognizedCommandParsingException && unrecognizedCommandParsingException.NearestMatches.Any()) {
                        ConsoleLogger.Info($"Did you mean {unrecognizedCommandParsingException.NearestMatches.First()}?");
                    }
                    ConsoleLogger.Info($"Specify --{AExecutionCommand.HelpLongName} for a list of available options and commands.");
                } else {
                    ConsoleLogger.Error(ex.Message, ex);
                }

                ConsoleLogger.Fatal($"Exit code {AExecutionCommand.FatalExitCode}");
                ConsoleLogger.WriteOnNewLine(null);
                return AExecutionCommand.FatalExitCode;
            } finally {
                Console.CursorVisible = true;
            }
        }

        /// <summary>
        /// Configure the application before executing it.
        /// </summary>
        protected virtual void ConfigureApp() {
            Conventions.UseDefaultConventions();
            Conventions.AddConvention(new CommandLoggerConvention(ConsoleLogger));
            Conventions.AddConvention(new DefaultOptionsConvention());
            Conventions.AddConvention(new OptionsValuesFromEnvironmentVariablesConvention());
            Conventions.AddConvention(new EnforceNamingConvention());
        }

        /// <inheritdoc />
        public override void Dispose() {
            ConsoleLogger?.Dispose();
            base.Dispose();
        }
    }
}
