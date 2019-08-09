#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ConsoleOutput.cs) is part of Oetools.Sakoe.
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
    /// Implementation of <see cref="IConsoleWriter"/>.
    /// </summary>
    public class ConsoleWriter : IConsoleWriter, IDisposable {

        /// <summary>
        /// The default foreground color for the text to write in the console.
        /// </summary>
        public virtual ConsoleColor DefaultTextColor { get; set; } = ConsoleColor.White;

        /// <inheritdoc />
        public TextWriter OutputTextWriter {
            get => _wordWrapWriter.UnderLyingWriter;
            set => _wordWrapWriter.UnderLyingWriter = value ?? _console.Out;
        }

        /// <inheritdoc cref="TextWriterOutputWordWrap.HasWroteToOuput"/>
        protected bool HasWroteToOuput {
            get => _wordWrapWriter.HasWroteToOuput;
            set => _wordWrapWriter.HasWroteToOuput = value;
        }

        private readonly TextWriterOutputWordWrap _wordWrapWriter;
        private readonly IConsoleInterface _console;
        private readonly TextTreeHelper _treeHelper = new TextTreeHelper();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="console"></param>
        protected ConsoleWriter(IConsoleInterface console) {
            _console = console;
            _console.ResetColor();
            _wordWrapWriter = new TextWriterOutputWordWrap(_console.Out);
        }

        /// <summary>
        /// Disposable implementation.
        /// </summary>
        public virtual void Dispose() {
            _console.ResetColor();
        }

        /// <inheritdoc />
        public virtual void WriteResult(string text, ConsoleColor? color = null) {
            _console.ForegroundColor = color ?? DefaultTextColor;
            _wordWrapWriter.Write(text, 0, false);
        }

        /// <inheritdoc />
        public virtual void WriteResultOnNewLine(string text, ConsoleColor? color = null) {
            _console.ForegroundColor = color ?? DefaultTextColor;
            _wordWrapWriter.Write(text, 0, true);
        }

        /// <inheritdoc />
        public virtual void Write(string text, ConsoleColor? color = null, int indentation = 0, string prefixForNewLines = null) {
            _console.ForegroundColor = color ?? DefaultTextColor;
            _wordWrapWriter.Write(text, _console.IsOutputRedirected ? 0 : _console.WindowWidth - 1, false, indentation, GetNewLinePrefix(prefixForNewLines));
        }

        /// <inheritdoc />
        public virtual void WriteOnNewLine(string text, ConsoleColor? color = null, int indentation = 0, string prefixForNewLines = null) {
            _console.ForegroundColor = color ?? DefaultTextColor;
            _wordWrapWriter.Write(GetText(text), _console.IsOutputRedirected ? 0 : _console.WindowWidth - 1, true, indentation, GetNewLinePrefix(prefixForNewLines));
        }

        /// <inheritdoc />
        public virtual void WriteError(string text, ConsoleColor? color = null, int indentation = 0, string prefixForNewLines = null) {
            _console.ForegroundColor = color ?? DefaultTextColor;
            _wordWrapWriter.Write(text, _console.IsOutputRedirected ? 0 : _console.WindowWidth - 1, false, indentation, GetNewLinePrefix(prefixForNewLines));
        }

        /// <inheritdoc />
        public virtual void WriteErrorOnNewLine(string text, ConsoleColor? color = null, int indentation = 0, string prefixForNewLines = null) {
            _console.ForegroundColor = color ?? DefaultTextColor;
            _wordWrapWriter.Write(GetText(text), _console.IsOutputRedirected ? 0 : _console.WindowWidth - 1, true, indentation, GetNewLinePrefix(prefixForNewLines));
        }


        private string GetText(string text) {
            if (!_treeHelper.Active) {
                return text;
            }
            return $"{_treeHelper.GetTextPrefix()}{text}";
        }

        private string GetNewLinePrefix(string prefixForNewLines) {
            if (!_treeHelper.Active) {
                return prefixForNewLines;
            }
            return string.IsNullOrEmpty(prefixForNewLines) ? _treeHelper.GetNewLinePrefix() : $"{prefixForNewLines}{_treeHelper.GetNewLinePrefix()}";
        }

        /// <inheritdoc />
        public IConsoleWriter NewTreeItem(bool isLastItemOfCurrentDepth = false) {
            _treeHelper.NewTreeItem(isLastItemOfCurrentDepth);
            return this;
        }

        /// <inheritdoc />
        public IConsoleWriter IncreaseTreeDepth() {
            _treeHelper.IncreaseTreeDepth();
            return this;
        }

        /// <inheritdoc />
        public IConsoleWriter DecreaseTreeDepth() {
            _treeHelper.DecreaseTreeDepth();
            return this;
        }

        /// <inheritdoc />
        public IConsoleWriter CloseTree() {
            _treeHelper.CloseTree();
            return this;
        }
    }
}
