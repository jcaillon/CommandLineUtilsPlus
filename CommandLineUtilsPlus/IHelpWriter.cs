#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (IHelpWriter.cs) is part of Oetools.Sakoe.
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
    /// An interface that provides methods to write help text.
    /// </summary>
    public interface IHelpWriter {

        /// <summary>
        /// Writes a text on a new line.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="color"></param>
        /// <param name="padding"></param>
        /// <param name="prefixForNewLines"></param>
        void WriteOnNewLine(string result, ConsoleColor? color = null, int padding = 0, string prefixForNewLines = null);

        /// <summary>
        /// Write a text, continuing the current line.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="color"></param>
        /// <param name="padding"></param>
        /// <param name="prefixForNewLines"></param>
        void Write(string result, ConsoleColor? color = null, int padding = 0, string prefixForNewLines = null);

        /// <summary>
        /// Write the title of a section, changing the font color.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="padding"></param>
        void WriteSectionTitle(string result, int padding = 0);

        /// <summary>
        /// Write a tip text.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="padding"></param>
        /// <param name="prefixForNewLines"></param>
        void WriteTip(string result, int padding = 0, string prefixForNewLines = null);
    }
}
