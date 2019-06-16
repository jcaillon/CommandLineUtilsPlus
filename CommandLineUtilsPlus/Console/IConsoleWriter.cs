#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (IConsoleOutput.cs) is part of Oetools.Sakoe.
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
using System.IO;

namespace CommandLineUtilsPlus.Console {

    /// <summary>
    /// Interface for writing to a console.
    /// </summary>
    public interface IConsoleWriter {

        /// <summary>
        /// Writes a result (no word wrap), appending to the existing line.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        void WriteResult(string text, ConsoleColor? color = null);

        /// <summary>
        /// Writes a result (no word wrap) on a new line.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        void WriteResultOnNewLine(string text, ConsoleColor? color = null);

        /// <summary>
        /// Writes text, appending to the current line. Has word wrap.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="color">The color to use.</param>
        /// <param name="indentation">Apply indentation when writing on a new line.</param>
        /// <param name="prefixForNewLines">The text to put at the beginning of each new line that need to be created because of word wrap.</param>
        void Write(string text, ConsoleColor? color = null, int indentation = 0, string prefixForNewLines = null);

        /// <summary>
        /// Writes text on a new line. Has word wrap.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="color">The color to use.</param>
        /// <param name="indentation">Apply indentation when writing on a new line.</param>
        /// <param name="prefixForNewLines">The text to put at the beginning of each new line that need to be created because of word wrap.</param>
        void WriteOnNewLine(string text, ConsoleColor? color = null, int indentation = 0, string prefixForNewLines = null);

        /// <summary>
        /// Writes a new error, appending to the current line. Has word wrap.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="color">The color to use.</param>
        /// <param name="indentation">Apply indentation when writing on a new line.</param>
        /// <param name="prefixForNewLines">The text to put at the beginning of each new line that need to be created because of word wrap.</param>
        void WriteError(string text, ConsoleColor? color = null, int indentation = 0, string prefixForNewLines = null);

        /// <summary>
        /// Writes an error on a new line. Has word wrap.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="color">The color to use.</param>
        /// <param name="indentation">Apply indentation when writing on a new line.</param>
        /// <param name="prefixForNewLines">The text to put at the beginning of each new line that need to be created because of word wrap.</param>
        void WriteErrorOnNewLine(string text, ConsoleColor? color = null, int indentation = 0, string prefixForNewLines = null);

        /// <summary>
        /// States that the next line of text written with <see cref="WriteOnNewLine"/> or <see cref="WriteErrorOnNewLine"/> will be a tree item.
        /// For this method to have an effect, the tree must have a current depth of at least 1, see <see cref="IncreaseTreeDepth"/>.
        /// To correctly draw the tree, it is needed to know if the next line will represent the last item for the current depth, see <paramref name="isLastItemOfCurrentDepth"/>.
        /// </summary>
        /// <param name="isLastItemOfCurrentDepth">Is the next item the last item for the current depth.</param>
        /// <returns></returns>
        IConsoleWriter NewTreeItem(bool isLastItemOfCurrentDepth = false);

        /// <summary>
        /// Increase the depth of the tree to draw. Default depth is 0. The tree starts displaying at depth 1 so this method is to be called first.
        /// </summary>
        /// <returns></returns>
        IConsoleWriter IncreaseTreeDepth();

        /// <summary>
        /// Decreases the depth of the current by 1.
        /// </summary>
        /// <returns></returns>
        IConsoleWriter DecreaseTreeDepth();

        /// <summary>
        /// Closes the current tree, resetting the depth to 0.
        /// </summary>
        /// <returns></returns>
        IConsoleWriter CloseTree();

        /// <summary>
        /// Set or get the text writer to write to.
        /// </summary>
        TextWriter OutputTextWriter { get; set; }
    }
}
