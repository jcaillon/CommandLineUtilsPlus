#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (CommandAdditionalHelpTextAttribute.cs) is part of Oetools.Sakoe.
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

namespace CommandLineUtilsPlus {

    /// <summary>
    /// Special attribute that allows to know the name of the method that should be used in order to get additional help text of the command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAdditionalHelpTextAttribute : Attribute {

        /// <summary>
        /// The name of the method to use in order to get the default value for this property.
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// A new instance of <see cref="CommandAdditionalHelpTextAttribute"/>.
        /// </summary>
        /// <param name="methodName"></param>
        public CommandAdditionalHelpTextAttribute(string methodName) {
            MethodName = methodName;
        }
    }
}
