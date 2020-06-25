#region header

// ========================================================================
// Copyright (c) 2020 - Julien Caillon (julien.caillon@gmail.com)
// This file (SetOptionsFromEnvironnementVariablesConvention.cs) is part of CommandLineUtilsPlus.
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommandLineUtilsPlus.Extension;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace CommandLineUtilsPlus.Conventions {
    /// <summary>
    ///
    /// </summary>
    public class OptionsValuesFromEnvironmentVariablesConvention : IConvention {

        private CommandLineApplication _rootApplication;

        /// <inheritdoc />
        public void Apply(ConventionContext context) {
            if (context.ModelType == null) {
                return;
            }

            _rootApplication = _rootApplication ?? context.Application.GetRootCommandLineApplication();

            var longNameToPropertyInfo = new Dictionary<string, PropertyInfo>();

            const BindingFlags binding = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var properties = context.ModelType.GetTypeInfo().GetProperties(binding);
            foreach (var propertyInfo in properties) {
                var optionAttribute = (OptionAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(OptionAttribute), true);
                if (optionAttribute == null) {
                    continue;
                }
                string longName = null;
                if (!string.IsNullOrEmpty(optionAttribute.Template)) {
                    longName = new CommandOption(optionAttribute.Template, CommandOptionType.NoValue).LongName;
                }
                if (string.IsNullOrEmpty(longName)) {
                    longName = optionAttribute.LongName;
                }
                if (string.IsNullOrEmpty(longName)) {
                    longName = propertyInfo.Name.ToKebabCase();
                }
                longNameToPropertyInfo.Add(longName, propertyInfo);
            }

            foreach (var option in context.Application.GetOptions()) {
                if (!string.IsNullOrEmpty(option.LongName) && longNameToPropertyInfo.ContainsKey(option.LongName)) {
                    var optionValueFromEnvironmentVariable = new SetOptionValueFromEnvironmentVariable(_rootApplication, context.Application, context.ModelAccessor?.GetModel(), longNameToPropertyInfo[option.LongName]);
                    if (option.Validators is List<IOptionValidator> validators && (validators.Count == 0 || !(validators[0] is SetOptionValueFromEnvironmentVariable))) {
                        validators.Insert(0, optionValueFromEnvironmentVariable);
                    }
                }
            }

        }
    }
}
