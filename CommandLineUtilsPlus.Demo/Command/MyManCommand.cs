#region header
// ========================================================================
// Copyright (c) 2020 - Julien Caillon (julien.caillon@gmail.com)
// This file (MyManCommand.cs) is part of CommandLineUtilsPlus.Demo.
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

using System.IO;
using CommandLineUtilsPlus.Command;
using McMaster.Extensions.CommandLineUtils;

namespace CommandLineUtilsPlus.Demo.Command {

    [Command(
        Name, "ma", "man",
        Description = "The manual of this tool. Learn about the usage of this tool."
    )]
    [Subcommand(typeof(CompleteManCommand))]
    [Subcommand(typeof(MyMarkdownManCommand))]
    [Subcommand(typeof(ManCommandCommand))]
    [CommandAdditionalHelpText(nameof(WriteToolUsage))]
    public class MyManCommand : ManCommand {

        public static void WriteToolUsage(IHelpWriter formatter, CommandLineApplication app, int firstColumnWidth) {
            formatter.WriteOnNewLine(null);
            formatter.WriteSectionTitle("WHAT IS THIS TOOL");
            formatter.WriteOnNewLine(app.Parent?.Description);

            formatter.WriteOnNewLine(null);
            WriteGenericToolUsage(formatter, app, firstColumnWidth);
        }

        protected override int OnExecute(CommandLineApplication app, IConsole console) {
            WriteToolUsage(app.HelpTextGenerator as IHelpWriter, app, 0);
            return 0;
        }
    }

    [Command("export-md", "md", Description = "Export the documentation of this tool to a markdown file.")]
    internal class MyMarkdownManCommand : MarkdownManCommand {

        /// <summary>
        /// Writes the about section.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="writer"></param>
        /// <param name="mdGenerator"></param>
        protected override void WriteAbout(CommandLineApplication app, StreamWriter writer, MarkdownHelpGenerator mdGenerator) {
            writer.WriteLine("## ABOUT");
            writer.WriteLine();
            MyManCommand.WriteToolUsage(mdGenerator, app.Parent, 0);
            writer.WriteLine();
        }
    }

    [Command(
        "command", "cm", "cmd",
        Description = "Manual for the commands of this tool."
    )]
    [Subcommand(typeof(ManCommandListCommand))]
    internal class ManCommandCommand : ADemoParentCommand {
    }
}
