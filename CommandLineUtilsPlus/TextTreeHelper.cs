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
        /// Returns true if a tree is currently being drawn
        /// </summary>
        public bool Active => _treeLevel > 0;

        /// <summary>
        /// Returns the prefix that should be prepended to the text to draw the tree.
        /// </summary>
        /// <returns></returns>
        public string GetTextPrefix() {
            if (_treeLevel == 0) {
                return null;
            }
            if (!_addNode) {
                return GetNewLinePrefix();
            }
            _addNode = false;

            if (_isLastChild) {
                return $"{_prefix}└─ ";
            }
            return $"{_prefix}├─ ";
        }

        /// <summary>
        /// Returns the prefix for new lines in case a word wrap needs to happen on the current line.
        /// </summary>
        /// <returns></returns>
        public string GetNewLinePrefix() {
            if (_treeLevel == 0) {
                return null;
            }
            return _isLastChild ? $"{_prefix}   " : $"{_prefix}│  ";
        }

        /// <summary>
        /// Prepare to add a new item for the current depth in the tree.
        /// </summary>
        /// <param name="isLastItemOfCurrentDepth"></param>
        /// <returns></returns>
        public void NewTreeItem(bool isLastItemOfCurrentDepth = false) {
            _addNode = true;
            _isLastChild = isLastItemOfCurrentDepth;
        }

        /// <summary>
        /// Increase tree depth.
        /// </summary>
        /// <returns></returns>
        public void IncreaseTreeDepth() {
            _treeLevel++;
            if (_treeLevel > 1) {
                _prefix.Append(_isLastChild ? "   " : "│  ");
            }
        }

        /// <summary>
        /// Decrease tree depth.
        /// </summary>
        /// <returns></returns>
        public void DecreaseTreeDepth() {
            if (_treeLevel > 0) {
                _treeLevel--;
            }
            if (_prefix.Length > 0) {
                _prefix.Length -= 3;
            }
        }

        /// <summary>
        /// Close the tree, resetting the current depth to 0.
        /// </summary>
        public void CloseTree() {
            _treeLevel = 0;
            _prefix.Clear();
            _addNode = false;
            _isLastChild = false;
        }

    }
}
