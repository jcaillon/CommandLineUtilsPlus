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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace CommandLineUtilsPlus.Utilities {
    internal static class StaticUtilities {
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

        public static void SetPropertyValue(PropertyInfo prop, object instance, object value)
        {
            var setter = prop.GetSetMethod(nonPublic: true);
            if (setter != null)
            {
                setter.Invoke(instance, new object[] { value });
            }
            else
            {
                var backingFieldName = string.Format("<{0}>k__BackingField", prop.Name);
                var backingField = prop.DeclaringType.GetTypeInfo().GetDeclaredField(backingFieldName);
                if (backingField == null)
                {
                    throw new InvalidOperationException(
                        $"Could not find a way to set {prop.DeclaringType.FullName}.{prop.Name}");
                }
                backingField.SetValue(instance, value);
            }
        }

        public static string ToKebabCase(this string str) {
            if (string.IsNullOrEmpty(str)) {
                return str;
            }

            var sb = new StringBuilder();
            var i = 0;
            var addDash = false;

            for (; i < str.Length; i++) {
                var ch = str[i];
                if (!char.IsLetterOrDigit(ch)) {
                    continue;
                }

                addDash = !char.IsUpper(ch);
                sb.Append(char.ToLowerInvariant(ch));
                i++;
                break;
            }

            for (; i < str.Length; i++) {
                var ch = str[i];
                if (char.IsUpper(ch)) {
                    if (addDash) {
                        addDash = false;
                        sb.Append('-');
                    }

                    sb.Append(char.ToLowerInvariant(ch));
                } else if (char.IsLetterOrDigit(ch)) {
                    addDash = true;
                    sb.Append(ch);
                } else {
                    addDash = false;
                    sb.Append('-');
                }
            }

            // trim trailing slashes
            while (sb.Length > 0 && sb[sb.Length - 1] == '-') {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }
    }
}
