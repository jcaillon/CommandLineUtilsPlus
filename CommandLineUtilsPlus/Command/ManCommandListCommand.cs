#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (DataDiggerCommand.cs) is part of Oetools.Sakoe.
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

using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace CommandLineUtilsPlus.Command {

    /// <summary>
    /// A command which lists all the commands as a tree.
    /// </summary>
    [Command(
        "list", "li",
        Description = "List all the commands of this tool."
    )]
    [CommandAdditionalHelpText(nameof(GetAdditionalHelpText))]
    public class ManCommandListCommand {

        /// <summary>
        /// Display all the commands.
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="app"></param>
        /// <param name="firstColumnWidth"></param>
        public static void GetAdditionalHelpText(IHelpWriter formatter, CommandLineApplication app, int firstColumnWidth) {
            formatter.WriteOnNewLine(null);
            formatter.WriteSectionTitle("LIST OF ALL THE COMMANDS");
            var rootCommand = app;
            while (rootCommand.Parent != null) {
                rootCommand = rootCommand.Parent;
            }
            formatter.WriteOnNewLine(rootCommand.Name);
            ListCommands(formatter, rootCommand.Commands, "");
            formatter.WriteOnNewLine(null);
        }

        private static void ListCommands(IHelpWriter formatter, List<CommandLineApplication> subCommands, string linePrefix) {
            var i = 0;
            foreach (var subCommand in subCommands.OrderBy(c => c.Name)) {
                formatter.WriteOnNewLine($"{linePrefix}{(i == subCommands.Count - 1 ? "└─ " : "├─ ")}{subCommand.Name}".PadRight(30));
                var linePrefixForNewLine = $"{linePrefix}{(i == subCommands.Count - 1 ? "   " : "│  ")}";
                formatter.Write(subCommand.Description, padding: 30, prefixForNewLines: linePrefixForNewLine);
                if (subCommand.Commands != null && subCommand.Commands.Count > 0) {
                    ListCommands(formatter, subCommand.Commands, linePrefixForNewLine);
                }
                i++;
            }
        }

        /// <summary>
        /// On command execution.
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
