#region header
// ========================================================================
// Copyright (c) 2020 - Julien Caillon (julien.caillon@gmail.com)
// This file (MyCreateStarterCommand.cs) is part of CommandLineUtilsPlus.Demo.
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


#if !WINDOWSONLYBUILD && !SELFCONTAINEDWITHEXE

using System.IO;
using CommandLineUtilsPlus.Command;
using McMaster.Extensions.CommandLineUtils;

namespace CommandLineUtilsPlus.Demo.Command {

    [Command(
        "starter", "st",
        Description = "Create a platform specific starter script for this tool.",
        ExtendedHelpText = "Allow a more natural way of calling this tool (i.e. `demoapp [command]` without having to specify dotnet)."
    )]
    public class MyACreateStarterCommand : ACreateStarterCommand {

        protected override string GetDllName() => Path.GetFileName(typeof(MyACreateStarterCommand).Assembly.Location);
    }
}

#endif
