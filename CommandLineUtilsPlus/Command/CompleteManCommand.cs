#region header

// ========================================================================
// Copyright (c) 2020 - Julien Caillon (julien.caillon@gmail.com)
// This file (CompleteManCommand.cs) is part of CommandLineUtilsPlus.
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

#region header

// ========================================================================
// Copyright (c) 2020 - Julien Caillon (julien.caillon@gmail.com)
// This file (CompleteManCommand.cs) is part of CommandLineUtilsPlus.
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

using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace CommandLineUtilsPlus.Command {
    /// <summary>
    /// Print the help of each command of this tool.
    /// </summary>
    [Command(
        Name, "co",
        Description = "Print the help of each command of this tool."
    )]
    [CommandAdditionalHelpText(nameof(GetAdditionalHelpText))]
    public class CompleteManCommand {

        /// <summary>
        /// Command name
        /// </summary>
        public const string Name = "complete";

        /// <summary>
        ///
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="app"></param>
        /// <param name="firstColumnWidth"></param>
        public static void GetAdditionalHelpText(IHelpWriter formatter, CommandLineApplication app, int firstColumnWidth) {
            formatter.WriteOnNewLine(null);
            app.Parent.Commands.Remove(app);
            var rootCommand = app;
            while (rootCommand.Parent != null) {
                rootCommand = rootCommand.Parent;
            }
            ListCommands(formatter, rootCommand.Commands);
            formatter.WriteOnNewLine(null);
        }

        private static void ListCommands(IHelpWriter formatter, List<CommandLineApplication> subCommands) {
            foreach (var subCommand in subCommands.OrderBy(c => c.Name)) {
                subCommand.ShowHelp();
                if (subCommand.Commands != null && subCommand.Commands.Count > 0) {
                    ListCommands(formatter, subCommand.Commands);
                }
            }
        }

        /// <summary>
        /// Method called on command execution.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="console"></param>
        /// <returns></returns>
        protected virtual int OnExecute(CommandLineApplication app, IConsole console) {
            GetAdditionalHelpText(app.HelpTextGenerator as IHelpWriter, app, 0);
            return 0;
        }
    }
}
