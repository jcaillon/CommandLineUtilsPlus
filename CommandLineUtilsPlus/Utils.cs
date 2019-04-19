#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (Utils.cs) is part of CommandLineUtilsPlus.
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
using System.IO;
using System.Runtime.InteropServices;

namespace CommandLineUtilsPlus {
    internal static class Utils {

#if !WINDOWSONLYBUILD
        private static bool? _isRuntimeWindowsPlatform;
#endif

        /// <summary>
        /// Returns true if the current execution is done on windows platform
        /// </summary>
        public static bool IsRuntimeWindowsPlatform {
            get {
#if WINDOWSONLYBUILD
                return true;
#else
                return (_isRuntimeWindowsPlatform ?? (_isRuntimeWindowsPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows))).Value;
#endif
            }
        }

        /// <summary>
        /// Equivalent to <see cref="Path.IsPathRooted"/> but throws no exceptions
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsPathRooted(string path) {
            try {
                return Path.IsPathRooted(path);
            } catch (Exception) {
                return false;
            }
        }

        /// <summary>
        /// A simple quote to use for result display
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string PrettyQuote(this string text) {
            return $"`{text}`";
        }
    }
}
