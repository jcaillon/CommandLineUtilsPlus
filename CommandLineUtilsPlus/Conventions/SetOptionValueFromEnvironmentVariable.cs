#region header
// ========================================================================
// Copyright (c) 2020 - Julien Caillon (julien.caillon@gmail.com)
// This file (Fuck.cs) is part of CommandLineUtilsPlus.
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

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using CommandLineUtilsPlus.Command;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace CommandLineUtilsPlus.Conventions {
    /// <summary>
    ///
    /// </summary>
    public class SetOptionValueFromEnvironmentVariable : IOptionValidator {

        private CommandLineApplication _rootApplication;
        private CommandLineApplication _application;
        private AExecutionCommand _command;
        private PropertyInfo _property;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rootApplication"></param>
        /// <param name="application"></param>
        /// <param name="model"></param>
        /// <param name="property"></param>
        public SetOptionValueFromEnvironmentVariable(CommandLineApplication rootApplication, CommandLineApplication application, object model, PropertyInfo property) {
            _rootApplication = rootApplication;
            _application = application;
            _command = model as AExecutionCommand;
            _property = property;
        }

        /// <inheritdoc />
        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context) {
            if (option.HasValue()) {
                return ValidationResult.Success;
            }
            var varName = $"{_rootApplication.Name}_{option.LongName}".ToUpper();
            var environmentVariables = Environment.GetEnvironmentVariables();
            if (environmentVariables.Contains(varName)) {
                _command?.DebugLogFromSetOptionValueFromEnvironmentVariable.Add($"Setting option --{option.LongName} with the value of the environment variable {varName}.");
                option.TryParse(environmentVariables[varName] as string);
                if (_property != null) {
                    var parser = _application.ValueParsers.GetParser(_property.PropertyType);
                    var value = parser.Parse(option.LongName, environmentVariables[varName] as string, CultureInfo.CurrentCulture);
                    StaticUtilities.SetPropertyValue(_property, _command, value);
                }
            }
            return ValidationResult.Success;
        }

        /// <summary>
        ///
        /// </summary>
        public static void TryParseOptionValueFromEnvironmentVariable(CommandLineApplication rootApplication, AExecutionCommand command, CommandOption option) {

        }
    }
}
