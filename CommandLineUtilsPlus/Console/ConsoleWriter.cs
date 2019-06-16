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
using System.Text;

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
            _wordWrapWriter.Write(text, _console.IsOutputRedirected ? 0 : _console.WindowWidth - 1, false, indentation, string.IsNullOrEmpty(prefixForNewLines) ? GetNewLinePrefix() : $"{prefixForNewLines}{GetNewLinePrefix()}");
        }

        /// <inheritdoc />
        public virtual void WriteOnNewLine(string text, ConsoleColor? color = null, int indentation = 0, string prefixForNewLines = null) {
            _console.ForegroundColor = color ?? DefaultTextColor;
            _wordWrapWriter.Write(GetText(text), _console.IsOutputRedirected ? 0 : _console.WindowWidth - 1, true, indentation, string.IsNullOrEmpty(prefixForNewLines) ? GetNewLinePrefix() : $"{prefixForNewLines}{GetNewLinePrefix()}");
        }

        /// <inheritdoc />
        public virtual void WriteError(string text, ConsoleColor? color = null, int indentation = 0, string prefixForNewLines = null) {
            _console.ForegroundColor = color ?? DefaultTextColor;
            _wordWrapWriter.Write(text, _console.IsOutputRedirected ? 0 : _console.WindowWidth - 1, false, indentation, string.IsNullOrEmpty(prefixForNewLines) ? GetNewLinePrefix() : $"{prefixForNewLines}{GetNewLinePrefix()}");
        }

        /// <inheritdoc />
        public virtual void WriteErrorOnNewLine(string text, ConsoleColor? color = null, int indentation = 0, string prefixForNewLines = null) {
            _console.ForegroundColor = color ?? DefaultTextColor;
            _wordWrapWriter.Write(GetText(text), _console.IsOutputRedirected ? 0 : _console.WindowWidth - 1, true, indentation, string.IsNullOrEmpty(prefixForNewLines) ? GetNewLinePrefix() : $"{prefixForNewLines}{GetNewLinePrefix()}");
        }

        private string GetText(string text) {
            if (_treeLevel == 0) {
                return text;
            }
            return $"{(IsNode() ? GetNodePrefix() : GetNewLinePrefix())}{text}";
        }

        private StringBuilder _prefix = new StringBuilder();
        private byte _treeLevel;
        private bool _addNode;
        private bool _isLastChild;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool IsNode() {
            if (_addNode) {
                _addNode = false;
                return true;
            }
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string GetNodePrefix() {
            if (_treeLevel == 0) {
                return null;
            }
            if (_isLastChild) {
                return $"{_prefix}└─ ";
            }
            return $"{_prefix}├─ ";
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string GetNewLinePrefix() {
            if (_treeLevel == 0) {
                return null;
            }
            return _isLastChild ? $"{_prefix}   " : $"{_prefix}│  ";
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="isLastChild"></param>
        /// <returns></returns>
        public virtual IConsoleWriter AddNode(bool isLastChild = false) {
            _addNode = true;
            _isLastChild = isLastChild;
            return this;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual IConsoleWriter PushLevel() {
            _treeLevel++;
            if (_treeLevel > 1) {
                _prefix.Append(_isLastChild ? "   " : "│  ");
            }
            return this;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual IConsoleWriter PopLevel() {
            if (_treeLevel > 0) {
                _treeLevel--;
            }
            if (_prefix.Length > 0) {
                _prefix.Length -= 3;
            }
            return this;
        }

    }
}
