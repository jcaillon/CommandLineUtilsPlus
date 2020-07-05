#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (CreateStarterCommand.cs) is part of Oetools.Sakoe.
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

#if !WINDOWSONLYBUILD && !SELFCONTAINEDWITHEXE
using System.IO;
using CommandLineUtilsPlus.Extension;
using CommandLineUtilsPlus.Utilities;
using McMaster.Extensions.CommandLineUtils;

namespace CommandLineUtilsPlus.Command {

    /// <summary>
    /// A command which can be used to generate a startup script for the current CLI.
    /// </summary>
    public abstract class ACreateStarterCommand : AExecutionCommand {

        /// <summary>
        /// Get the starter script file path.
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        public static string GetStartScriptFilePath(string appName) {
            string starterFilePath = null;
            string executableDir = Path.GetDirectoryName(RunningAssembly.Info.Location);
            if (!string.IsNullOrEmpty(executableDir)) {
                starterFilePath = Path.Combine(executableDir, StaticUtilities.IsRuntimeWindowsPlatform ? $"{appName}.cmd" : appName);
            }
            return starterFilePath;
        }

        /// <summary>
        /// Returns the file name of the currently executing tool.
        /// </summary>
        /// <returns></returns>
        protected abstract string GetDllName();

        /// <inheritdoc />
        protected override int ExecuteCommand(CommandLineApplication app, IConsole console) {
            var appName = app.GetRootCommandLineApplication().Name;

            string executableDir = Path.GetDirectoryName(RunningAssembly.Info.Location);
            if (string.IsNullOrEmpty(executableDir)) {
                throw new CommandException($"Could not find the directory of the executing assembly: {RunningAssembly.Info.Location}.");
            }

            var starterFilePath = GetStartScriptFilePath(appName);
            Log.Debug($"Creating starter script: {starterFilePath.PrettyQuote()}.");

            if (StaticUtilities.IsRuntimeWindowsPlatform) {
                File.WriteAllText(starterFilePath, $"@echo off\r\ndotnet exec \"%~dp0{GetDllName()}\" %*");
            } else {
                File.WriteAllText(starterFilePath, (@"#!/bin/bash
SOURCE=""${BASH_SOURCE[0]}""
while [ -h ""$SOURCE"" ]; do
    DIR=""$( cd -P ""$( dirname ""$SOURCE"" )"" && pwd )""
    SOURCE=""$(readlink ""$SOURCE"")""
    [[ $SOURCE != /* ]] && SOURCE=""$DIR/$SOURCE""
done
DIR=""$( cd -P ""$( dirname ""$SOURCE"" )"" && pwd )""
dotnet exec ""$DIR/" + GetDllName() + @""" ""$@""").Replace("\r", ""));
            }

            Log?.Info($"Starter script created: {starterFilePath.PrettyQuote()}.");

            HelpWriter.WriteOnNewLine(null);
            HelpWriter.WriteSectionTitle("IMPORTANT README:");
            HelpWriter.WriteOnNewLine(@"
A starter script has been created in the same directory as this executable: " + starterFilePath.PrettyQuote() + $@".

It allows you to call this tool in a more natural way: `{appName} [command]`. This strips the need to run the .dll with dotnet (the script does that for you).

The directory containing the starter script created should be added to your system PATH in order to be able to call `{appName} [command]` from anywhere on your system.

The command to add this directory to your path is:");
            HelpWriter.WriteOnNewLine(null);

            if (StaticUtilities.IsRuntimeWindowsPlatform) {
                Out.WriteResultOnNewLine("for /f \"usebackq tokens=2,*\" %A in (`reg query HKCU\\Environment /v PATH`) do set my_user_path=%B && SetX Path \"%my_user_path%;" + Path.GetDirectoryName(starterFilePath) + "\"");
            } else {
                Out.WriteResultOnNewLine("echo $\"export PATH=\\$PATH:" + Path.GetDirectoryName(starterFilePath) + "\" >> ~/.bashrc && source ~/.bashrc && chmod +x \"" + starterFilePath + "\"");
            }

            HelpWriter.WriteOnNewLine(null);

            return 0;
        }
    }
}

#endif
