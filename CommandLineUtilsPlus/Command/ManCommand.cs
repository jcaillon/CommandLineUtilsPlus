#region header
// ========================================================================
// Copyright (c) 2020 - Julien Caillon (julien.caillon@gmail.com)
// This file (ManCommand.cs) is part of CommandLineUtilsPlus.
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
using McMaster.Extensions.CommandLineUtils;

namespace CommandLineUtilsPlus.Command {

    /// <summary>
    /// The manual command of this tool. Learn about the usage.
    /// </summary>
    public class ManCommand {

        /// <summary>
        /// Command name.
        /// </summary>
        public const string Name = "manual";

        /// <summary>
        /// Write the generic usage of this CLI.
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="app"></param>
        /// <param name="firstColumnWidth"></param>
        public static void WriteGenericToolUsage(IHelpWriter formatter, CommandLineApplication app, int firstColumnWidth) {
            formatter.WriteSectionTitle("COMMAND LINE USAGE");
            formatter.WriteOnNewLine(@"How to use this command line interface tool:
  - Each command is well documented on its own, don't be afraid to use the `--" + AExecutionCommand.HelpLongName + @"` option.
  - Some commands and options have aliases (shortcuts); for instance the following option `-");
            formatter.Write("-opt", ConsoleColor.Green);
            formatter.Write(@"ion` can also be written as `");
            formatter.Write("-opt", ConsoleColor.Green);
            formatter.Write($@"` (mind the highlighted letters).
  - Every single option can also be passed to this tool using an environment variable; the variable should be named like the option (in capital letters) and prefixed by `{app.GetRootCommandLineApplication().Name?.ToUpper()}_`.
    For instance, you can pass the value of the option named `--myoption` through an environment variable named `{app.GetRootCommandLineApplication().Name?.ToUpper()}_MYOPTION`.
  - You can escape white spaces in argument/option values by using double quotes (i.e. ""my value"").
  - If you need to use a double quote within a double quote, you can do so by double quoting the double quotes (i.e. ""my """"special"""" value"").
  - If an extra layer is needed, just double the doubling (i.e. -opt ""-mysubopt """"my special """"""""value"""""""""""""").
  - In the 'USAGE' help section, arguments between brackets (i.e. []) are optionals.");

            formatter.WriteOnNewLine(null);
            formatter.WriteSectionTitle("RESPONSE FILE PARSING");
            formatter.WriteOnNewLine($@"Instead of using a long command line (which is limited in size on every platform), you can use a response file that contains each argument/option that should be used.

Everything that is usually separated by a space in the command line should be separated by a new line in the file.
In response files, you do not have to double quote arguments containing spaces, they will be considered as a whole as long as they are on a separated line.

  `{app.GetRootCommandLineApplication().Name} @responsefile.txt`");

            formatter.WriteOnNewLine(null);
            formatter.WriteSectionTitle("EXIT CODE");
            formatter.WriteOnNewLine(@"The convention followed by this tool is the following.
  - 0 : used when a command completed successfully, without errors nor warnings.
  - 1-8 : used when a command completed but with warnings, the level can be used to pinpoint different kinds of warnings.
  - 9 : used when a command does not complete and ends up in error.");

            if (app.Commands.Count > 0) {
                formatter.WriteOnNewLine(null);
                formatter.WriteSectionTitle("LEARN MORE");
                formatter.WriteOnNewLine("Learn more about specific topics using the command:");
                formatter.WriteOnNewLine(null);
                formatter.WriteOnNewLine($"{app.GetFullCommandLine()} <TOPIC>".PrettyQuote());

                formatter.WriteOnNewLine(null);
                formatter.WriteSectionTitle("TOPICS");
                foreach (var command in app.Commands.ToList().OrderBy(c => c.Name)) {
                    formatter.WriteOnNewLine(command.Name?.PadRight(30));
                    formatter.Write(command.Description, padding: 30);
                }
            }

            formatter.WriteOnNewLine(null);
        }

        /// <summary>
        /// Method called on command execution.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="console"></param>
        /// <returns></returns>
        protected virtual int OnExecute(CommandLineApplication app, IConsole console) {
            WriteGenericToolUsage(app.HelpTextGenerator as IHelpWriter, app, 0);
            return 0;
        }

    }

    /// <summary>
    /// Print the help of each command of this tool.
    /// </summary>
    [Command(
        Name, "co",
        Description = "Print the help of each command of this tool."
    )]
    [CommandAdditionalHelpTextAttribute(nameof(GetAdditionalHelpText))]
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
