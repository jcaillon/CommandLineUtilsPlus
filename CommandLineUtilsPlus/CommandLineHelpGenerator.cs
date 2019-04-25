#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (HelpGenerator.cs) is part of Oetools.Sakoe.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLineUtilsPlus.Console;
using CommandLineUtilsPlus.Extension;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.HelpText;

namespace CommandLineUtilsPlus {

    /// <summary>
    /// A class that helps generate the usage of a command line application.
    /// </summary>
    public class CommandLineHelpGenerator : ICommandLineHelpGenerator {

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineHelpGenerator"/>.
        /// </summary>
        public CommandLineHelpGenerator(IConsoleWriter console) {
            _console = console;
        }

        private IConsoleWriter _console;

        /// <summary>
        /// The base indentation to apply to the whole help text.
        /// </summary>
        protected const int BaseIndentation = 1;

        /// <summary>
        /// Extra indentation to apply to the text of a particular section.
        /// </summary>
        protected const int SectionIndentation = 2;

        /// <inheritdoc />
        public virtual void Generate(CommandLineApplication application, TextWriter output) {
            try {
                _console.OutputTextWriter = output;
                GenerateCommandHelp(application);
            } finally {
                _console.OutputTextWriter = null;
            }
        }

        /// <summary>
        /// Generates the command usage text.
        /// </summary>
        /// <param name="application"></param>
        protected virtual void GenerateCommandHelp(CommandLineApplication application) {
            var arguments = application.Arguments.Where(a => a.ShowInHelpText).ToList();
            var options = application.GetOptions().Where(o => o.ShowInHelpText).ToList();
            var commands = application.Commands.Where(c => c.ShowInHelpText).ToList();

            var optionShortNameColumnWidth = options.Count == 0 ? 0 : options.Max(o => string.IsNullOrEmpty(o.ShortName) ? 0 : o.ShortName.Length);
            if (optionShortNameColumnWidth > 0) {
                optionShortNameColumnWidth += 3; // -name,_
            }
            var optionLongNameColumnWidth = options.Count == 0 ? 0 : options.Max(o => {
                var lgt = string.IsNullOrEmpty(o.LongName) ? 0 : o.LongName.Length;
                if (!string.IsNullOrEmpty(o.ValueName)) {
                    lgt += 3 + o.ValueName.Length; // space and <>
                }
                return lgt;
            });
            if (optionLongNameColumnWidth > 0) {
                optionLongNameColumnWidth += 2; // --name
            }
            var commandShortNameColumnWidth = commands.Count == 0 ? 0 : commands.Max(c => c.Names.Skip(1).FirstOrDefault()?.Length ?? 0);
            if (commandShortNameColumnWidth > 0) {
                commandShortNameColumnWidth += 2; // ,_
            }
            var commandLongNameColumnWidth = commands.Count == 0 ? 0 : commands.Max(c => c.Name?.Length ?? 0);

            var firstColumnWidth = optionShortNameColumnWidth + optionLongNameColumnWidth;
            firstColumnWidth = Math.Max(firstColumnWidth, commandShortNameColumnWidth + commandLongNameColumnWidth);
            firstColumnWidth = Math.Max(firstColumnWidth, arguments.Count == 0 ? 0 : arguments.Max(a => a.Name.IndexOf('[') < 0 ? a.Name.Length : a.Name.Length - 2));
            firstColumnWidth = Math.Max(firstColumnWidth, 20);
            firstColumnWidth = Math.Min(firstColumnWidth, 35);
            optionLongNameColumnWidth = firstColumnWidth - optionShortNameColumnWidth;
            commandLongNameColumnWidth = firstColumnWidth - commandShortNameColumnWidth;

            var fullCommandLine = application.GetFullCommandLine();

            if (!string.IsNullOrEmpty(application.Description)) {
                WriteOnNewLine(null);
                WriteSectionTitle("SYNOPSIS");
                WriteOnNewLine(application.Description);
            }

            var commandType = application.GetTypeFromCommandLineApp();

            GenerateUsage(fullCommandLine, arguments, options, commands, commandType?.GetProperty("RemainingArgs"));
            GenerateArguments(arguments, firstColumnWidth);
            GenerateOptions(options.Where(o => !o.Inherited).ToList(), optionShortNameColumnWidth, optionLongNameColumnWidth, false);
            GenerateOptions(options.Where(o => o.Inherited).ToList(), optionShortNameColumnWidth, optionLongNameColumnWidth, true);
            GenerateCommands(application, fullCommandLine, commands, commandShortNameColumnWidth, commandLongNameColumnWidth);

            if (commandType != null) {
                var additionalHelpTextAttribute = (CommandAdditionalHelpTextAttribute) Attribute.GetCustomAttribute(commandType, typeof(CommandAdditionalHelpTextAttribute), true);
                if (additionalHelpTextAttribute != null) {
                    var methodInfo = commandType.GetMethod(additionalHelpTextAttribute.MethodName, BindingFlags.Public | BindingFlags.Static);
                    if (methodInfo != null) {
                        methodInfo.Invoke(null, new object[] {
                            this, application, firstColumnWidth
                        });
                    }
                }
            }

            if (!string.IsNullOrEmpty(application.ExtendedHelpText)) {
                WriteOnNewLine(null);
                WriteSectionTitle("DESCRIPTION");
                WriteOnNewLine(application.ExtendedHelpText);
            }

            WriteOnNewLine(null);
        }

        /// <summary>
        /// Generate the line that shows usage
        /// </summary>
        protected virtual void GenerateUsage(string thisCommandLine, IReadOnlyList<CommandArgument> visibleArguments, IReadOnlyList<CommandOption> visibleOptions, IReadOnlyList<CommandLineApplication> visibleCommands, PropertyInfo remainingArgs) {
            WriteOnNewLine(null);
            WriteSectionTitle("USAGE");
            WriteOnNewLine(thisCommandLine);

            if (visibleOptions.Any()) {
                Write(" [options]");
            }
            foreach (var argument in visibleArguments) {
                Write($" {argument.Name}");
            }
            if (visibleCommands.Any()) {
                Write(" [command]");
            }
            if (remainingArgs != null) {
                if (Attribute.GetCustomAttribute(remainingArgs, typeof(DescriptionAttribute), true) is DescriptionAttribute description) {
                    Write($" {description.Description}");
                } else {
                    Write(" [-- <arg>...]");
                }
            }
        }

        /// <summary>
        /// Generate the lines that show information about arguments
        /// </summary>
        protected virtual void GenerateArguments(IReadOnlyList<CommandArgument> visibleArguments, int firstColumnWidth) {
            if (visibleArguments.Any()) {
                WriteOnNewLine(null);
                WriteSectionTitle("ARGUMENTS");

                foreach (var arg in visibleArguments) {
                    var name = arg.Name.Replace("[", "").Replace("]", "");
                    WriteOnNewLine(name.PadRight(firstColumnWidth + 2));
                    if (name.Length > firstColumnWidth) {
                        WriteOnNewLine(arg.Description, padding: firstColumnWidth + 2);
                    } else {
                        Write(arg.Description, padding: firstColumnWidth + 2);
                    }
                }
            }
        }

        /// <summary>
        /// Generate the lines that show information about options
        /// </summary>
        protected virtual void GenerateOptions(IReadOnlyList<CommandOption> visibleOptions, int optionShortNameColumnWidth, int optionLongNameColumnWidth, bool inheritedOptions) {
            var firstColumnWidth = optionShortNameColumnWidth + optionLongNameColumnWidth;
            if (visibleOptions.Any()) {
                WriteOnNewLine(null);
                WriteSectionTitle(inheritedOptions ? "INHERITED OPTIONS" : "OPTIONS");

                foreach (var opt in visibleOptions) {
                    var shortName = string.IsNullOrEmpty(opt.SymbolName) ? string.IsNullOrEmpty(opt.ShortName) ? "" : $"-{opt.ShortName}, " : $"-{opt.SymbolName}, ";
                    var longName = string.IsNullOrEmpty(opt.LongName) ? "" : $"--{opt.LongName}";
                    string valueName = "";
                    if (!string.IsNullOrEmpty(opt.ValueName)) {
                        valueName = opt.OptionType == CommandOptionType.SingleOrNoValue ? $"[:{opt.ValueName.Replace("_", " ")}]" : $" <{opt.ValueName.Replace("_", " ")}>";
                    }
                    var firstColumn = $"{shortName.PadRight(optionShortNameColumnWidth)}{$"{longName}{valueName}".PadRight(optionLongNameColumnWidth)}";
                    WriteOnNewLine(firstColumn.PadRight(firstColumnWidth + 2));
                    var text = opt.Description;
                    if (opt.OptionType == CommandOptionType.MultipleValue) {
                        text = $"(Can be used multiple times) {text}";
                    }
                    if (firstColumn.Length > firstColumnWidth) {
                        WriteOnNewLine(text, padding: firstColumnWidth + 2);
                    } else {
                        Write(text, padding: firstColumnWidth + 2);
                    }
                }
            }
        }

        /// <summary>
        /// Generate the lines that show information about subcommands.
        /// </summary>
        protected virtual void GenerateCommands(CommandLineApplication application, string thisCommandLine, IReadOnlyList<CommandLineApplication> visibleCommands, int commandShortNameColumnWidth, int commandLongNameColumnWidth) {
            var firstColumnWidth = commandShortNameColumnWidth + commandLongNameColumnWidth;
            if (visibleCommands.Any()) {
                WriteOnNewLine(null);
                WriteSectionTitle("COMMANDS");

                foreach (var cmd in visibleCommands.OrderBy(c => c.Name)) {
                    string firstColumn;
                    if (cmd.Names.Count() <= 1) {
                        firstColumn = cmd.Name;
                    } else {
                        firstColumn = $"{$"{cmd.Names.Skip(1).First()}, ".PadRight(commandShortNameColumnWidth)}{cmd.Name.PadRight(commandLongNameColumnWidth)}";
                    }
                    WriteOnNewLine(firstColumn.PadRight(firstColumnWidth + 2));
                    if (cmd.Name.Length > firstColumnWidth) {
                        WriteOnNewLine(cmd.Description, padding: firstColumnWidth + 2);
                    } else {
                        Write(cmd.Description, padding: firstColumnWidth + 2);
                    }
                }

                WriteOnNewLine(null);
                WriteTip($"Tip: run '{thisCommandLine} [command] --{application.OptionHelp.LongName}' for more information about a command.");
            }
        }

        private int _currentPadding;

        /// <inheritdoc cref="IConsoleWriter.WriteOnNewLine"/>
        public virtual void WriteOnNewLine(string result, ConsoleColor? color = null, int padding = 0, string prefixForNewLines = null) {
            _console.WriteOnNewLine(result, color, padding + BaseIndentation + _currentPadding, BaseIndentation + _currentPadding == 0 || string.IsNullOrEmpty(prefixForNewLines) ? prefixForNewLines : prefixForNewLines.PadLeft(prefixForNewLines.Length + BaseIndentation + _currentPadding, ' '));
        }

        /// <inheritdoc cref="IConsoleWriter.Write"/>
        public virtual void Write(string result, ConsoleColor? color = null, int padding = 0, string prefixForNewLines = null) {
            _console.Write(result, color, padding + BaseIndentation + _currentPadding, BaseIndentation + _currentPadding == 0 || string.IsNullOrEmpty(prefixForNewLines) ? prefixForNewLines : prefixForNewLines.PadLeft(prefixForNewLines.Length + BaseIndentation + _currentPadding, ' '));
        }

        /// <summary>
        /// Write a section title.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="padding"></param>
        public virtual void WriteSectionTitle(string result, int padding = 0) {
            _currentPadding = 0;
            _console.WriteOnNewLine(result, ConsoleColor.Cyan, padding + BaseIndentation + _currentPadding);
            _currentPadding = SectionIndentation;
        }

        /// <inheritdoc cref="IConsoleWriter.Write"/>
        /// <summary>Write a tip.</summary>
        public virtual void WriteTip(string result, int padding = 0, string prefixForNewLines = null) {
            _console.WriteOnNewLine(result, ConsoleColor.Gray, padding + BaseIndentation + _currentPadding, BaseIndentation + _currentPadding == 0 || string.IsNullOrEmpty(prefixForNewLines) ? prefixForNewLines : prefixForNewLines.PadLeft(prefixForNewLines.Length + BaseIndentation + _currentPadding, ' '));
        }

    }
}
