#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (ABaseCommand.cs) is part of CommandLineUtilsPlus.
//
// CommandLineUtilsPlus is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// CommandLineUtilsPlus is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with CommandLineUtilsPlus. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using CommandLineUtilsPlus.Console;
using CommandLineUtilsPlus.Extension;
using McMaster.Extensions.CommandLineUtils;

namespace CommandLineUtilsPlus.Command {

    /// <summary>
    /// A base command.
    /// </summary>
    public abstract class ACommand {

        /// <summary>
        /// A logger.
        /// </summary>
        protected ILogger Log { get; private set; }

        /// <summary>
        /// Write to console.
        /// </summary>
        protected IConsoleWriter Out { get; private set; }

        /// <summary>
        /// The console logger.
        /// </summary>
        protected CommandLineConsoleLogger ConsoleLogger { get; private set; }

        /// <summary>
        /// Sets the console logger for this command.
        /// </summary>
        /// <param name="consoleLogger"></param>
        internal void SetConsoleLogger(CommandLineConsoleLogger consoleLogger) {
            ConsoleLogger = consoleLogger;
            Log = consoleLogger;
            Out = consoleLogger;
        }

        /// <summary>
        /// Return the help text that should be used to informed the user that a sub command is needed.
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        protected static string GetProvideCommandHelpText(CommandLineApplication application) {
            return $"You must provide a command: {$"{application.GetFullCommandLine()} [command]".PrettyQuote()}.";
        }

        /// <summary>
        /// Draw the application logo.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="console"></param>
        /// <returns></returns>
        protected virtual void DrawLogo(CommandLineApplication application, IConsoleWriter console) {
            console.WriteOnNewLine(application.GetRootCommandLineApplication().Name.ToUpper());
        }
    }
}
