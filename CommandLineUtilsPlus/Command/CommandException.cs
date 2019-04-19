#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (CommandException.cs) is part of Oetools.Sakoe.
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

namespace CommandLineUtilsPlus.Command {

    /// <summary>
    /// An exception that occurs during the execution of a command.
    /// </summary>
    public class CommandException : Exception {

        /// <summary>
        /// The exit code which should ultimately be returned by the application.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Instantiate a <see cref="CommandException"/>.
        /// </summary>
        /// <param name="message"></param>
        public CommandException(string message) : base(message) { }

        /// <summary>
        /// Instantiate a <see cref="CommandException"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public CommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
