#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (TextTreeDrawer.cs) is part of CommandLineUtilsPlus.
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

using System.Text;

namespace CommandLineUtilsPlus {

    /// <summary>
    /// This class helps drawing trees as text.
    /// </summary>
    public class TextTreeHelper {

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
        public virtual TextTreeHelper AddNode(bool isLastChild = false) {
            _addNode = true;
            _isLastChild = isLastChild;
            return this;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual TextTreeHelper PushLevel() {
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
        public virtual TextTreeHelper PopLevel() {
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
