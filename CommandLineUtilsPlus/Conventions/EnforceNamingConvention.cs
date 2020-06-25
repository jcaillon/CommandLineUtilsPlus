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

using System;
using System.Collections.Generic;
using System.Linq;
using CommandLineUtilsPlus.Command;
using CommandLineUtilsPlus.Extension;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace CommandLineUtilsPlus.Conventions {

    /// <summary>
    /// This convention enforces the usage of Name for each command/argument and LongName for each option
    /// </summary>
    public class EnforceNamingConvention : IConvention {

        /// <inheritdoc />
        public void Apply(ConventionContext context) {
            if (context.ModelType == null) {
                return;
            }

            if (context.Application.Arguments.Any(a => string.IsNullOrEmpty(a.Name))) {
                throw new ModelException($"An argument name is not defined for class {context.ModelType}.");
            }

            if (context.Application.Options.Any(a => string.IsNullOrEmpty(a.LongName))) {
                throw new ModelException($"An option long name is not defined for class {context.ModelType}.");
            }

            if (string.IsNullOrEmpty(context.Application.Name)) {
                throw new ModelException($"A command name is not defined for class {context.ModelType}.");
            }

            foreach (var command in context.Application.Arguments.Where(a => string.IsNullOrEmpty(a.Name))) {
            }
        }
    }
}
