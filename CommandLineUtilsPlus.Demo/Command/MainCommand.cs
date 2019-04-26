#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (MainCommand.cs) is part of CommandLineUtilsPlus.Demo.
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

using CommandLineUtilsPlus.Command;
using McMaster.Extensions.CommandLineUtils;

namespace CommandLineUtilsPlus.Demo.Command {

    /// <summary>
    /// The main command of the application, called when the user passes no arguments/commands
    /// </summary>
    [Command(
        Name = "demoapp",
        FullName = "THE DEMO APP",
        Description = "A demo app for the CommandLineUtilsPlus library."
    )]
    [HelpOption("-h|-?|" + AExecutionCommand.HelpLongName, Description = "Show this help text.", Inherited = true)]
    [Subcommand(typeof(ShowVersionCommand))]
    [Subcommand(typeof(SelfTestCommand))]
    [CommandAdditionalHelpText(nameof(GetAdditionalHelpText))]
    public class MainCommand : AParentCommand {
        public static void GetAdditionalHelpText(IHelpWriter writer, CommandLineApplication application, int firstColumnWidth) {
            writer.WriteOnNewLine(null);
            writer.WriteSectionTitle("HOW TO");
            writer.WriteOnNewLine($"Here, you could write some piece of documentation.");
        }
    }
}
