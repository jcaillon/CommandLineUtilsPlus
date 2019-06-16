#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (CustomHelpGenerator.cs) is part of CommandLineUtilsPlus.Demo.
//
// CommandLineUtilsPlus.Demo is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// CommandLineUtilsPlus.Demo is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with CommandLineUtilsPlus.Demo. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using System;
using CommandLineUtilsPlus.Console;

namespace CommandLineUtilsPlus.Demo {
    public class CustomHelpGenerator : CommandLineHelpGenerator {

        public override int BaseIndentation { get; set; } = 2;

        public override int SectionIndentation { get; set; } = 2;

        public override ConsoleColor TipTextColor { get; set; } = ConsoleColor.Magenta;

        public CustomHelpGenerator(IConsoleWriter console) : base(console) { }
    }
}
