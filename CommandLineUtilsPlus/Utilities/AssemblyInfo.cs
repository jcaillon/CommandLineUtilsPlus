#region header
// ========================================================================
// Copyright (c) 2020 - Julien Caillon (julien.caillon@gmail.com)
// This file (AssemblyInfo.cs) is part of CommandLineUtilsPlus.
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
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace CommandLineUtilsPlus.Utilities {

    /// <summary>
    /// Get info on an assembly.
    /// </summary>
    public class AssemblyInfo {

        private readonly Assembly _assembly;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="assembly"></param>
        public AssemblyInfo(Assembly assembly) {
            _assembly = assembly;
        }

        /// <summary>
        /// Also called the InformationalVersion.
        /// Format: major.minor.patch[.build][-pre_release_name][+git_commit].
        /// </summary>
        /// <example>
        /// 1.0.0-beta
        /// </example>
        public string ProductVersion => GetAttributeValue<AssemblyInformationalVersionAttribute>(a => a.InformationalVersion, FileVersionInfo.GetVersionInfo(Location).ProductVersion);

        /// <summary>
        /// Same as product version but without the potential git commit.
        /// </summary>
        public string ProductShortVersion {
            get {
                var productVersion = ProductVersion;
                var idx = productVersion.IndexOf('+');
                return idx > 1 ? productVersion.Substring(0, idx) : productVersion;
            }
        }

        /// <summary>
        /// Format: major.minor.patch.build (patch and build are set to 0 if they were not specified).
        /// </summary>
        public Version AssemblyVersion => _assembly.GetName().Version ?? new Version(GetAttributeValue<AssemblyVersionAttribute>(a => a.Version, "0.0"));

        /// <summary>
        /// Format: major.minor.patch.build (patch and build are set to 0 if they were not specified).
        /// </summary>
        public string FileVersion => GetAttributeValue<AssemblyFileVersionAttribute>(a => a.Version, FileVersionInfo.GetVersionInfo(Location).FileVersion);

        /// <summary>
        /// Format: major.minor.patch.build (patch and build are set to 0 if they were not specified).
        /// </summary>
        public string AssemblyTitle => GetAttributeValue<AssemblyTitleAttribute>(a => a.Title);

        /// <summary>
        /// Copyright of the assembly.
        /// </summary>
        public string CopyRight => GetAttributeValue<AssemblyCopyrightAttribute>(a => a.Copyright);

        /// <summary>
        /// Company of the assembly.
        /// </summary>
        public string Company => GetAttributeValue<AssemblyCompanyAttribute>(a => a.Company);

        /// <summary>
        /// Product name of the assembly.
        /// </summary>
        public string Product => GetAttributeValue<AssemblyProductAttribute>(a => a.Product, CodeBaseFileName);

        /// <summary>
        /// Description of the assembly.
        /// </summary>
        public string Description => GetAttributeValue<AssemblyDescriptionAttribute>(a => a.Description, CodeBaseFileName);

        /// <summary>
        /// Full path of the assembly.
        /// </summary>
        public string Location => _assembly.Location;

        /// <summary>
        /// Is it a pre-release assembly version?
        /// </summary>
        public bool IsPreRelease => Product.Contains("-");

        /// <summary>
        /// Returns the name of the assembly.
        /// </summary>
        public string AssemblyFileName => Path.GetFileName(_assembly.Location);

        private string CodeBaseFileName => Path.GetFileNameWithoutExtension(_assembly.CodeBase);

        private string GetAttributeValue<TAttr>(Func<TAttr, string> resolveFunc, string defaultResult = null) where TAttr : Attribute {
            var attributes = _assembly.GetCustomAttributes(typeof(TAttr), false);
            return attributes.Length > 0 ? resolveFunc((TAttr) attributes[0]) : defaultResult;
        }
    }
}
