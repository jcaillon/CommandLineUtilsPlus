#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (CommandCommonOptionsConvention.cs) is part of Oetools.Sakoe.
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

using System.Linq;
using CommandLineUtilsPlus.Command;
using CommandLineUtilsPlus.Extension;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;

namespace CommandLineUtilsPlus.Conventions {

    /// <inheritdoc />
    public class CommandLoggerConvention : IConvention {

        private ICommandLineConsoleLogger _consoleLogger;

        //private CommandLineApplication _rootApplication;

        /// <summary>
        /// New instance of <see cref="CommandLoggerConvention"/>
        /// </summary>
        /// <param name="consoleLogger"></param>
        public CommandLoggerConvention(ICommandLineConsoleLogger consoleLogger) {
            _consoleLogger = consoleLogger;
        }

        /// <inheritdoc />
        public void Apply(ConventionContext context) {
            if (context.ModelAccessor == null) {
                return;
            }

            if (context.ModelAccessor.GetModel() is ACommand aCommand) {
                aCommand.SetConsoleLogger(_consoleLogger);
            }

            /*if (context.ModelAccessor.GetModel() is AExecutionCommand aExecutionCommand) {
                _rootApplication = _rootApplication ?? context.Application.GetRootCommandLineApplication();
                context.Application.OnParsingComplete(result => {
                    var verbosityOption = context.Application.GetOptions().First(o => AExecutionCommand.VerbosityShortName.Equals(o.ShortName));
                    SetOptionValueFromEnvironmentVariable.TryParseOptionValueFromEnvironmentVariable(_rootApplication, aExecutionCommand, verbosityOption);
                });
            }*/
        }
    }
}
