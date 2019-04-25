#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (ICommandLineConsoleLogger.cs) is part of CommandLineUtilsPlus.
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
using CommandLineUtilsPlus.Console;

namespace CommandLineUtilsPlus {

    /// <summary>
    /// A <see cref="ConsoleLogger"/> that can also log stuff to a text log file.
    /// </summary>
    public interface ICommandLineConsoleLogger : IConsoleWriter, IConsoleLogger, IDisposable {

        /// <summary>
        /// Sets or gets the path to a log file where every event will be written.
        /// </summary>
        /// <exception cref="Exception"></exception>
        string LogOutputFilePath { get; set; }
    }
}
