#region header

// ========================================================================
// Copyright (c) 2020 - Julien Caillon (julien.caillon@gmail.com)
// This file (MarkdownManCommand.cs) is part of CommandLineUtilsPlus.
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
// This file (MarkdownManCommand.cs) is part of CommandLineUtilsPlus.
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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using CommandLineUtilsPlus.Extension;
using CommandLineUtilsPlus.Utilities;
using McMaster.Extensions.CommandLineUtils;

namespace CommandLineUtilsPlus.Command {
    /// <summary>
    /// Export the documentation of this tool to a markdown file.
    /// </summary>
    [Command(
        "export-md", "md",
        Description = "Export the documentation of this tool to a markdown file."
    )]
    public class MarkdownManCommand {

        /// <summary>
        /// The file path in which to output the markdown documentation.
        /// </summary>
        [Required]
        [LegalFilePath]
        [Argument(0, "<file>", "The file in which to print this markdown manual.")]
        public string OutputFile { get; set; }

        /// <summary>
        /// Method called on command execution.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="console"></param>
        /// <returns></returns>
        protected virtual int OnExecute(CommandLineApplication app, IConsole console) {
            app.Parent.Commands.Clear();
            var rootCommand = app.GetRootCommandLineApplication();

            using (var stream = new FileStream(OutputFile, FileMode.Create)) {
                using (var writer = new StreamWriter(stream, Encoding.UTF8)) {
                    var mdGenerator = new MarkdownHelpGenerator(writer);
                    WriteMarkdownContent(app, writer, rootCommand, mdGenerator);
                }
            }
            return 0;
        }

        /// <summary>
        /// Writes the markdown whole content.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="writer"></param>
        /// <param name="rootCommand"></param>
        /// <param name="mdGenerator"></param>
        protected virtual void WriteMarkdownContent(CommandLineApplication app, StreamWriter writer, CommandLineApplication rootCommand, MarkdownHelpGenerator mdGenerator) {
            writer.WriteLine($"# {rootCommand.FullName}");
            writer.WriteLine();
            writer.WriteLine($"> This markdown can be generated using the command: {app.GetFullCommandLine().PrettyQuote()}.");
            writer.WriteLine("> ");
            writer.WriteLine($"> This version has been generated on {DateTime.Now:yy-MM-dd} at {DateTime.Now:HH:mm:ss}.");
            writer.WriteLine();

            WriteAbout(app, writer, mdGenerator);

            WriteTableOfContent(writer, rootCommand);

            WriteCommandsOverview(writer, rootCommand);

            ListCommands(mdGenerator, rootCommand.Commands);
        }

        /// <summary>
        /// Writes the command overview.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="rootCommand"></param>
        protected virtual void WriteCommandsOverview(StreamWriter writer, CommandLineApplication rootCommand) {
            writer.WriteLine("## COMMANDS OVERVIEW");
            writer.WriteLine();
            writer.WriteLine("| Full command line | Short description |");
            writer.WriteLine("| --- | --- |");
            WriteCommandsOverviewRow(writer, rootCommand.Commands);
            writer.WriteLine();
        }

        /// <summary>
        /// Writes the TOC.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="rootCommand"></param>
        protected virtual void WriteTableOfContent(StreamWriter writer, CommandLineApplication rootCommand) {
            writer.WriteLine("## TABLE OF CONTENT");
            writer.WriteLine();
            writer.WriteLine("- [Commands overview](#commands-overview)");
            writer.WriteLine("- [About the build command](#about-the-build-command)");
            WriteTableOfContentItem(writer, rootCommand.Commands, "");
            writer.WriteLine();
        }

        /// <summary>
        /// Writes the about section.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="writer"></param>
        /// <param name="mdGenerator"></param>
        protected virtual void WriteAbout(CommandLineApplication app, StreamWriter writer, MarkdownHelpGenerator mdGenerator) {
            writer.WriteLine("## ABOUT");
            writer.WriteLine();
            ManCommand.WriteGenericToolUsage(mdGenerator, app.Parent, 0);
            writer.WriteLine();
        }

        /// <summary>
        /// Writes all TOC items.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="subCommands"></param>
        /// <param name="linePrefix"></param>
        private static void WriteTableOfContentItem(StreamWriter writer, List<CommandLineApplication> subCommands, string linePrefix) {
            var i = 0;
            foreach (var subCommand in subCommands.OrderBy(c => c.Name)) {
                writer.WriteLine($"{linePrefix}- [{subCommand.Name}](#{subCommand.GetFullCommandLine().Replace(" ", "-")})");
                if (subCommand.Commands != null && subCommand.Commands.Count > 0) {
                    WriteTableOfContentItem(writer, subCommand.Commands, $"{linePrefix}  ");
                }
                i++;
            }
        }

        /// <summary>
        /// Writes the rows for the command overview.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="subCommands"></param>
        protected virtual void WriteCommandsOverviewRow(StreamWriter writer, List<CommandLineApplication> subCommands) {
            var i = 0;
            foreach (var subCommand in subCommands.OrderBy(c => c.Name)) {
                writer.WriteLine($"| [{subCommand.GetFullCommandLine()}](#{subCommand.GetFullCommandLine().Replace(" ", "-")}) | {subCommand.Description?.Replace("\n", " ").Replace("\r", "")} |");
                if (subCommand.Commands != null && subCommand.Commands.Count > 0) {
                    WriteCommandsOverviewRow(writer, subCommand.Commands);
                }
                i++;
            }
        }

        /// <summary>
        /// Write the help text of each command.
        /// </summary>
        /// <param name="mdGenerator"></param>
        /// <param name="subCommands"></param>
        protected virtual void ListCommands(MarkdownHelpGenerator mdGenerator, List<CommandLineApplication> subCommands) {
            var i = 0;
            foreach (var subCommand in subCommands.OrderBy(c => c.Name)) {
                mdGenerator.WriteOnNewLine($"## {subCommand.GetFullCommandLine().ToUpper()}");
                mdGenerator.GenerateCommandHelp(subCommand);
                mdGenerator.WriteOnNewLine("**[\\[Go back to the table of content\\].](#table-of-content)**");
                if (subCommand.Commands != null && subCommand.Commands.Count > 0) {
                    ListCommands(mdGenerator, subCommand.Commands);
                }
                i++;
            }
        }
    }
}
