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
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.HelpText;

namespace CommandLineUtilsPlus {

    /// <summary>
    /// A <see cref="CommandLineApplication"/> without the command hint.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class CommandLineApplicationPlus<TModel> : CommandLineApplication<TModel> where TModel : class {

        /// <inheritdoc />
        public CommandLineApplicationPlus(IHelpTextGenerator helpTextGenerator, IConsole console, string workingDirectory, bool throwOnUnexpectedArg) : base(helpTextGenerator, console, workingDirectory, throwOnUnexpectedArg) {
        }

        /// <inheritdoc />
        public override void ShowHint() { }

        /// <summary>
        /// The entry point for the command line application.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int ExecuteCommand(string[] args) {
            var console = new CommandLineConsoleInterface();
            var consoleLogger = new CommandLineConsoleLogger(console);
            var helpGenerator = new CommandLineHelpGenerator(consoleLogger);
            try {
                console.CursorVisible = false;
                using (var app = new CommandLineApplicationPlus<TModel>(helpGenerator, console, Directory.GetCurrentDirectory(), true)) {
                    app.Conventions.UseDefaultConventions();
                    app.Conventions.AddConvention(new CommandLineApplicationConvention(consoleLogger));
                    return app.Execute(args);
                }
            } catch (Exception ex) {
                var log = consoleLogger;
                log.LogTheshold = ConsoleLogThreshold.Debug;

                if (ex is CommandParsingException) {
                    log.Error(ex.Message);
                    if (ex is UnrecognizedCommandParsingException unrecognizedCommandParsingException && unrecognizedCommandParsingException.NearestMatches.Any()) {
                        log.Info($"Did you mean {unrecognizedCommandParsingException.NearestMatches.First()}?");
                    }
                    log.Info($"Specify {AExecutionCommand.HelpLongName} for a list of available options and commands.");
                } else {
                    log.Error(ex.Message, ex);
                }

                log.Fatal($"Exit code {AExecutionCommand.FatalExitCode}");
                log.WriteOnNewLine(null);
                return AExecutionCommand.FatalExitCode;
            } finally {
                consoleLogger.Dispose();
                console.CursorVisible = true;
            }
        }
    }
}
