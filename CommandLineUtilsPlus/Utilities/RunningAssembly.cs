#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (RunningAssembly.cs) is part of yadaman.
//
// yadaman is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// yadaman is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with yadaman. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using System;
using System.IO;

namespace CommandLineUtilsPlus.Utilities {

    /// <summary>
    /// Get info on the running assembly (this .exe).
    /// </summary>
    public static class RunningAssembly {

        private static AssemblyInfo _assemblyInfo;
        private static string _directory;

        /// <summary>
        /// Get information on the assembly.
        /// </summary>
        public static AssemblyInfo Info => _assemblyInfo ?? (_assemblyInfo = new AssemblyInfo(typeof(RunningAssembly).Assembly));

        /// <summary>
        /// Get this assembly location (path).
        /// </summary>
        public static string Location => Info.Location;

        /// <summary>
        /// Get this assembly directory.
        /// </summary>
        public static string Directory => (_directory ?? (_directory = Path.GetDirectoryName(Info.Location))) ?? throw new Exception("Running assembly location is null.");
    }
}
