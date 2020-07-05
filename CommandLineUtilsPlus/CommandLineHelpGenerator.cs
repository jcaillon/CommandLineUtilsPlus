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
using CommandLineUtilsPlus.Command;
using CommandLineUtilsPlus.Console;
using CommandLineUtilsPlus.Extension;
using McMaster.Extensions.CommandLineUtils;

namespace CommandLineUtilsPlus {

    /// <summary>
    /// A class that helps generate the usage of a command line application.
    /// </summary>
    public class CommandLineHelpGenerator : ICommandLineHelpGenerator {

        /// <summary>
        /// Whether or not the --help option should be described in the help of a command.
        /// </summary>
        public virtual bool ShowHelpOptionInUsage { get; set; } = false;

        /// <summary>
        /// The base indentation to apply to the whole help text.
        /// </summary>
        public virtual int BaseIndentation { get; set; } = 1;

        /// <summary>
        /// Extra indentation to apply to the text of a particular section.
        /// </summary>
        public virtual int SectionIndentation { get; set; } = 2;

        /// <summary>
        /// The minimum width for the first column.
        /// Arguments, options and commands are presented in 2 columns: the name (1st) and the description (2nd).
        /// </summary>
        public virtual int MinimumFirstColumnWidth { get; set; } = 20;

        /// <summary>
        /// The maximum width for the first column.
        /// Arguments, options and commands are presented in 2 columns: the name (1st) and the description (2nd).
        /// </summary>
        public virtual int MaximumFirstColumnWidth { get; set; } = 20;

        /// <summary>
        /// The text color for the letters matching an command alias or an option short name.
        /// </summary>
        public virtual ConsoleColor AliasTextColor { get; set; } = ConsoleColor.Green;

        /// <summary>
        /// The text color for a section title.
        /// </summary>
        public virtual ConsoleColor SectionTitleTextColor { get; set; } = ConsoleColor.Cyan;

        /// <summary>
        /// The text color for a tip.
        /// </summary>
        public virtual ConsoleColor TipTextColor { get; set; } = ConsoleColor.Gray;

        /// <summary>
        /// The default text color to display the help.
        /// </summary>
        public virtual ConsoleColor? DefaultTextColor { get; set; } = null;

        /// <summary>
        /// Writes the full command as a header of the help text.
        /// </summary>
        public virtual bool WriteFullCommandAsHeader { get; set; } = true;

        /// <summary>
        /// Add 2 new lines between the list of options and inherited options.
        /// </summary>
        public virtual bool SeparateOptionsAndInheritedOptions { get; set; } = true;

        /// <summary>
        /// The console writer associated with this instance.
        /// </summary>
        protected IConsoleWriter ConsoleWriter => _console;

        private IConsoleWriter _console;

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineHelpGenerator"/>.
        /// </summary>
        public CommandLineHelpGenerator(IConsoleWriter console) {
            _console = console;
        }

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
            var allOptions = application.GetOptions().Where(o => o.ShowInHelpText);
            if (!ShowHelpOptionInUsage) {
                allOptions = allOptions.Where(o => !AExecutionCommand.HelpLongName.Equals(o.LongName));
            }
            var options = allOptions.ToList();
            var commands = application.Commands.Where(c => c.ShowInHelpText).ToList();

            var firstColumnWidth = GetFirstColumnWidth(application, arguments, options, commands);

            var fullCommandLine = application.GetFullCommandLine();

            if (WriteFullCommandAsHeader) {
                WriteSectionTitle("================");
                WriteSectionTitle(fullCommandLine);
                WriteSectionTitle("================");
            }

            if (!string.IsNullOrEmpty(application.Description)) {
                if (!WriteFullCommandAsHeader) {
                    WriteOnNewLine(null);
                    WriteSectionTitle("SYNOPSIS");
                }
                WriteOnNewLine(application.Description);
            }

            var commandType = application.GetTypeFromCommandLineApp();

            GenerateUsage(fullCommandLine, arguments, options, commands, commandType?.GetProperty("RemainingArgs"));
            GenerateArguments(arguments, firstColumnWidth);
            var hasHighlightedLettersInOptions = GenerateOptions(options.Where(o => !o.Inherited).ToList(), firstColumnWidth, false);
            var hasHighlightedLettersInInheritedOptions = GenerateOptions(options.Where(o => o.Inherited).ToList(), firstColumnWidth, !options.All(o => o.Inherited));

            if (options.Any()) {
                WriteOnNewLine(null);
                WriteTip($"Tip: you can set any option --opt using an environment var named {application.GetRootCommandLineApplication()?.Name?.ToUpper()}_OPT.");
                if (hasHighlightedLettersInOptions || hasHighlightedLettersInInheritedOptions) {
                    WriteTip($"Tip: use the highlighted letters as a short alias for an option.");
                }
            }

            GenerateCommands(application, fullCommandLine, commands, firstColumnWidth);

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
        /// Returns the computed first column width that will be used to display a command help
        /// </summary>
        /// <param name="application"></param>
        /// <param name="arguments"></param>
        /// <param name="options"></param>
        /// <param name="commands"></param>
        /// <returns></returns>
        protected virtual int GetFirstColumnWidth(CommandLineApplication application, List<CommandArgument> arguments, List<CommandOption> options, List<CommandLineApplication> commands) {
            var optionColumnWidth = options.Count == 0
                ? 0
                : options.Max(o => {
                    var lgt = string.IsNullOrEmpty(o.LongName) ? 0 : o.LongName.Length;
                    if (!string.IsNullOrEmpty(o.ValueName)) {
                        lgt += 3 + o.ValueName.Length; // space and <>
                    }
                    return lgt;
                });

            if (optionColumnWidth > 0) {
                optionColumnWidth += 2; // --name
            }

            var commandColumnWidth = commands.Count == 0 ? 0 : commands.Max(c => c.Name?.Length ?? 0);

            var firstColumnWidth = Math.Max(optionColumnWidth, commandColumnWidth);
            firstColumnWidth = Math.Max(firstColumnWidth, arguments.Count == 0 ? 0 : arguments.Max(a => a.Name?.IndexOf('<') < 0 ? a.Name.Length : a.Name?.Length ?? 0 - 2));
            firstColumnWidth = Math.Max(firstColumnWidth, MinimumFirstColumnWidth);
            firstColumnWidth = Math.Min(firstColumnWidth, MaximumFirstColumnWidth);

            return firstColumnWidth;
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
                Write($" {(argument.Name?.IndexOf('<') < 0 ? $"<{argument.Name}>" : argument.Name)}");
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
                    var name = arg.Name?.Replace("<", "").Replace(">", "");
                    WriteOnNewLine((name ?? "").PadRight(firstColumnWidth + 2));
                    if (name?.Length > firstColumnWidth) {
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
        protected virtual bool GenerateOptions(IReadOnlyList<CommandOption> visibleOptions, int firstColumnWidth, bool inheritedOptions) {
            var hasHighlightedLetters = false;

            if (visibleOptions.Any()) {
                if (!inheritedOptions) {
                    WriteOnNewLine(null);
                    WriteSectionTitle("OPTIONS");
                } else if (SeparateOptionsAndInheritedOptions) {
                    WriteOnNewLine(null);
                    WriteOnNewLine(null);
                }


                foreach (var opt in visibleOptions) {
                    WriteOnNewLine("-");
                    if (string.IsNullOrEmpty(opt.ShortName) || !IsShortNameIncludedInLongName(opt.LongName, opt.ShortName)) {
                        Write("-");
                        Write(opt.LongName);
                    } else {
                        Write("-", AliasTextColor);
                        WriteLongNameIncludingShortName(opt.LongName, opt.ShortName, AliasTextColor);
                        hasHighlightedLetters = true;
                    }

                    var actualFirstColumnWidth = (opt.LongName ?? "").Length + 2;

                    if (!string.IsNullOrEmpty(opt.ValueName)) {
                        var valueName = opt.OptionType == CommandOptionType.SingleOrNoValue ? $"[:{opt.ValueName.Replace("_", " ")}]" : $" <{opt.ValueName.Replace("_", " ")}>";
                        Write(valueName);
                        actualFirstColumnWidth += valueName.Length;
                    }

                    var text = opt.Description;
                    if (opt.OptionType == CommandOptionType.MultipleValue) {
                        text = $"(Can be used multiple times) {text}";
                    }
                    if (actualFirstColumnWidth > firstColumnWidth) {
                        WriteOnNewLine(text, padding: firstColumnWidth + 2);
                    } else {
                        Write(new string(' ', firstColumnWidth - actualFirstColumnWidth + 2));
                        Write(text, padding: firstColumnWidth + 2);
                    }
                }
            }

            return hasHighlightedLetters;
        }

        /// <summary>
        /// Generate the lines that show information about subcommands.
        /// </summary>
        protected virtual void GenerateCommands(CommandLineApplication application, string thisCommandLine, IReadOnlyList<CommandLineApplication> visibleCommands, int firstColumnWidth) {
            if (visibleCommands.Any()) {
                WriteOnNewLine(null);
                WriteSectionTitle("COMMANDS");

                var hasHighlightedLetters = false;

                foreach (var cmd in visibleCommands.OrderBy(c => c.Name)) {
                    if (cmd.Names.Count() <= 1) {
                        WriteOnNewLine((cmd.Name ?? "").PadRight(firstColumnWidth + 2));
                    } else {
                        var commandAlias = cmd.Names.Skip(1).FirstOrDefault();
                        WriteOnNewLine(null);
                        if (string.IsNullOrEmpty(commandAlias) || !IsShortNameIncludedInLongName(cmd.Name, commandAlias)) {
                            Write(cmd.Name);
                        } else {
                            WriteLongNameIncludingShortName(cmd.Name, commandAlias, AliasTextColor);
                            hasHighlightedLetters = true;
                        }
                    }
                    if (cmd.Name?.Length > firstColumnWidth) {
                        WriteOnNewLine(cmd.Description, padding: firstColumnWidth + 2);
                    } else {
                        Write(new string(' ', (firstColumnWidth - (cmd.Name ?? "").Length) + 2));
                        Write(cmd.Description, padding: firstColumnWidth + 2);
                    }
                }

                WriteOnNewLine(null);
                WriteTip($"Tip: run '{thisCommandLine} [command] --{application.OptionHelp?.LongName}' for more information about a command.");
                if (hasHighlightedLetters) {
                    WriteTip("Tip: use the highlighted letters as a short alias for a command.");
                }
            }
        }

        /// <summary>
        /// Writes <paramref name="longName"/> but writes the letters matching <paramref name="shortName"/> in an alternative color.
        /// </summary>
        /// <param name="longName"></param>
        /// <param name="shortName"></param>
        /// <param name="alternativeFg"></param>
        /// <param name="baseFg"></param>
        protected void WriteLongNameIncludingShortName(string longName, string shortName, ConsoleColor? alternativeFg, ConsoleColor? baseFg = null) {
            if (string.IsNullOrEmpty(shortName)) {
                Write(longName, baseFg);
                return;
            }

            var shortNameIdx = 0;
            var shortNameLength = shortName.Length;
            for (int idxLongName = 0; idxLongName < longName.Length; idxLongName++) {
                var ch = longName[idxLongName];
                if (shortNameIdx < shortNameLength && ch == shortName[shortNameIdx]) {
                    shortNameIdx++;
                    Write(ch.ToString(), alternativeFg);
                } else {
                    Write(ch.ToString(), baseFg);
                }
            }
        }

        /// <summary>
        /// Test if <paramref name="shortName"/> is entirely included in <paramref name="longName"/>.
        /// For instance -ips is entirely included in --i-pass.
        /// </summary>
        /// <param name="longName"></param>
        /// <param name="shortName"></param>
        /// <returns></returns>
        protected bool IsShortNameIncludedInLongName(string longName, string shortName) {
            if (string.IsNullOrEmpty(longName) || string.IsNullOrEmpty(shortName)) {
                return false;
            }

            var shortNameIdx = 0;
            var shortNameLength = shortName.Length;
            for (int idxLongName = 0; idxLongName < longName.Length; idxLongName++) {
                if (shortNameIdx < shortNameLength && longName[idxLongName] == shortName[shortNameIdx]) {
                    shortNameIdx++;
                }
            }
            return shortNameIdx == shortNameLength;
        }

        private int _currentPadding;

        /// <inheritdoc cref="IConsoleWriter.WriteOnNewLine"/>
        public virtual void WriteOnNewLine(string result, ConsoleColor? color = null, int padding = 0, string prefixForNewLines = null) {
            _console.WriteOnNewLine(result, color ?? DefaultTextColor, padding + BaseIndentation + _currentPadding, BaseIndentation + _currentPadding == 0 || string.IsNullOrEmpty(prefixForNewLines) ? prefixForNewLines : prefixForNewLines.PadLeft(prefixForNewLines.Length + BaseIndentation + _currentPadding, ' '));
        }

        /// <inheritdoc cref="IConsoleWriter.Write"/>
        public virtual void Write(string result, ConsoleColor? color = null, int padding = 0, string prefixForNewLines = null) {
            _console.Write(result, color ?? DefaultTextColor, padding + BaseIndentation + _currentPadding, BaseIndentation + _currentPadding == 0 || string.IsNullOrEmpty(prefixForNewLines) ? prefixForNewLines : prefixForNewLines.PadLeft(prefixForNewLines.Length + BaseIndentation + _currentPadding, ' '));
        }

        /// <summary>
        /// Write a section title.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="padding"></param>
        public virtual void WriteSectionTitle(string result, int padding = 0) {
            _currentPadding = 0;
            _console.WriteOnNewLine(result, SectionTitleTextColor, padding + BaseIndentation + _currentPadding);
            _currentPadding = SectionIndentation;
        }

        /// <inheritdoc cref="IConsoleWriter.Write"/>
        /// <summary>Write a tip.</summary>
        public virtual void WriteTip(string result, int padding = 0, string prefixForNewLines = null) {
            _console.WriteOnNewLine(result, TipTextColor, padding + BaseIndentation + _currentPadding, BaseIndentation + _currentPadding == 0 || string.IsNullOrEmpty(prefixForNewLines) ? prefixForNewLines : prefixForNewLines.PadLeft(prefixForNewLines.Length + BaseIndentation + _currentPadding, ' '));
        }

    }
}
