#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ConsoleImplementation.cs) is part of Oetools.Sakoe.
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
    /// A wrapper around <see cref="Console"/> implementing <see cref="IConsoleInterface"/>.
    /// </summary>
    public class ConsoleInterface : IConsoleInterface {

        private bool? _isOutputRedirect;
        private bool _hasWindowWidth = true;

        /// <summary>
        /// Constructor.
        /// </summary>
        protected ConsoleInterface() {
            System.Console.OutputEncoding = Encoding.UTF8;
            System.Console.InputEncoding = Encoding.UTF8;
        }

        /// <inheritdoc />
        public event ConsoleCancelEventHandler CancelKeyPress {
            add => System.Console.CancelKeyPress += value;
            remove => System.Console.CancelKeyPress -= value;
        }

        /// <inheritdoc />
        public TextWriter Error => System.Console.Error;

        /// <inheritdoc />
        public TextReader In => System.Console.In;

        /// <inheritdoc />
        public TextWriter Out => System.Console.Out;

        /// <inheritdoc />
        public bool IsInputRedirected => System.Console.IsInputRedirected;

        /// <inheritdoc />
        public bool IsOutputRedirected => _isOutputRedirect ?? (_isOutputRedirect = System.Console.IsOutputRedirected).Value;

        /// <inheritdoc />
        public bool IsErrorRedirected => System.Console.IsErrorRedirected;

        /// <inheritdoc />
        public ConsoleColor ForegroundColor {
            get => System.Console.ForegroundColor;
            set => System.Console.ForegroundColor = value;
        }

        /// <inheritdoc />
        public ConsoleColor BackgroundColor {
            get => System.Console.BackgroundColor;
            set => System.Console.BackgroundColor = value;
        }

        /// <inheritdoc />
        public void Write(string text = null) {
            Out.Write(text);
        }

        /// <inheritdoc />
        public void WriteLine(string text = null) {
            Out.WriteLine(text);
        }

        /// <inheritdoc />
        public ConsoleKeyInfo ReadKey(bool intercept = true) => System.Console.ReadKey(intercept);

        /// <inheritdoc />
        public bool KeyAvailable => System.Console.KeyAvailable;

        /// <inheritdoc />
        public void ResetColor() {
            System.Console.ResetColor();
        }

        /// <inheritdoc />
        public int CursorTop {
            get => System.Console.CursorTop;
            set => System.Console.CursorTop = value;
        }

        /// <inheritdoc />
        public int WindowWidth {
            get {
                try {
                    return _hasWindowWidth ? System.Console.WindowWidth : 0;
                } catch (IOException) {
                    _hasWindowWidth = false;
                    return 0;
                }
            }
            set => System.Console.WindowWidth = value;
        }

        /// <inheritdoc />
        public bool CursorVisible {
            get {
                try {
                    return System.Console.CursorVisible;
                } catch (Exception) {
                    return false;
                }
            }
            set {
                try {
                    System.Console.CursorVisible = value;
                } catch (Exception) {
                    // ignored.
                }
            }
        }

        /// <inheritdoc />
        public void SetCursorPosition(int left, int top) {
            System.Console.SetCursorPosition(left, top);
        }

    }
}
