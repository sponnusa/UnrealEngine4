﻿// Copyright 1998-2014 Epic Games, Inc. All Rights Reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using Tools.DotNETCommon.XmlHandler;
using UnrealBuildTool;
using System.Runtime.CompilerServices;
using System.Linq;

namespace AutomationTool
{
	#region ParamList

	/// <summary>
	/// Wrapper around List with support for multi parameter constructor, i.e:
	///   var Maps = new ParamList<string>("Map1", "Map2");
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ParamList<T> : List<T>
	{
		public ParamList(params T[] Args)
		{
			AddRange(Args);
		}

		public ParamList(ICollection<T> Collection)
			: base(Collection != null ? Collection : new T[] {})
		{
		}

		public override string ToString()
		{
			var Text = "";
			for (int Index = 0; Index < Count; ++Index)
			{
				if (Index > 0)
				{
					Text += ", ";
				}
				Text += this[Index].ToString();				
			}
			return Text;
		}
	}

	#endregion

	#region PathSeparator

	public enum PathSeparator
	{
		Default = 0,
		Slash,
		Backslash,
		Depot,
		Local
	}

	#endregion

	/// <summary>
	/// Base utility function for script commands.
	/// </summary>
	public partial class CommandUtils
	{
		#region Environment Setup

		static private CommandEnvironment CmdEnvironment;

		/// <summary>
		/// BuildEnvironment to use for this buildcommand. This is initialized by InitBuildEnvironment. As soon
		/// as the script execution in ExecuteBuild begins, the BuildEnv is set up and ready to use.
		/// </summary>
		static public CommandEnvironment CmdEnv
		{
			get
			{
				if (CmdEnvironment == null)
				{
					throw new AutomationException("Attempt to use CommandEnvironment before it was initialized.");
				}
				return CmdEnvironment;
			}
		}

		/// <summary>
		/// Initializes build environment. If the build command needs a specific env-var mapping or
		/// has an extended BuildEnvironment, it must implement this method accordingly.
		/// </summary>
		/// <returns>Initialized and ready to use BuildEnvironment</returns>
		static internal void InitCommandEnvironment()
		{
			CmdEnvironment = Automation.IsBuildMachine ? new CommandEnvironment() : new LocalCommandEnvironment(); ;
		}

		#endregion

		#region Logging

		/// <summary>
		/// Returns a formatted string for the specified exception (including inner exception if any).
		/// </summary>
		/// <param name="Ex"></param>
		/// <returns>Formatted exception string.</returns>
		public static string ExceptionToString(Exception Ex)
		{
			return LogUtils.FormatException(Ex);
		}

		/// <summary>
		/// Writes formatted text to log (with TraceEventType.Log).
		/// </summary>
		/// <param name="Format">Format string</param>
		/// <param name="Args">Parameters</param>
		[MethodImplAttribute(MethodImplOptions.NoInlining)]
		public static void Log(string Format, params object[] Args)
		{
			UnrealBuildTool.Log.WriteLine(1, TraceEventType.Information, Format, Args);
		}

		/// <summary>
		/// Writes formatted text to log (with TraceEventType.Log).
		/// </summary>
		/// <param name="Message">Text</param>
		[MethodImplAttribute(MethodImplOptions.NoInlining)]
		public static void Log(string Message)
		{
			UnrealBuildTool.Log.WriteLine(1, TraceEventType.Information, Message);
		}

		/// <summary>
		/// Writes formatted text to log (with TraceEventType.Error).
		/// </summary>
		/// <param name="Format">Format string</param>
		/// <param name="Args">Parameters</param>
		[MethodImplAttribute(MethodImplOptions.NoInlining)]
		public static void LogError(string Format, params object[] Args)
		{
			UnrealBuildTool.Log.WriteLine(1, TraceEventType.Error, Format, Args);
		}

		/// <summary>
		/// Writes formatted text to log (with TraceEventType.Error).
		/// </summary>
		/// <param name="Message">Text</param>
		[MethodImplAttribute(MethodImplOptions.NoInlining)]
		public static void LogError(string Message)
		{
			UnrealBuildTool.Log.WriteLine(1, TraceEventType.Error, Message);
		}

		/// <summary>
		/// Writes formatted text to log (with TraceEventType.Warning).
		/// </summary>
		/// <param name="Format">Format string</param>
		/// <param name="Args">Parameters</param>
		[MethodImplAttribute(MethodImplOptions.NoInlining)]
		public static void LogWarning(string Format, params object[] Args)
		{
			UnrealBuildTool.Log.WriteLine(1, TraceEventType.Warning, Format, Args);
		}

		/// <summary>
		/// Writes a message to log (with TraceEventType.Warning).
		/// </summary>
		/// <param name="Message">Text</param>
		[MethodImplAttribute(MethodImplOptions.NoInlining)]
		public static void LogWarning(string Message)
		{
			UnrealBuildTool.Log.WriteLine(1, TraceEventType.Warning, Message);
		}

		/// <summary>
		/// Writes formatted text to log (with TraceEventType.Verbose).
		/// </summary>
		/// <param name="Foramt">Format string</param>
		/// <param name="Args">Arguments</param>
		[MethodImplAttribute(MethodImplOptions.NoInlining)]
		public static void LogVerbose(string Format, params object[] Args)
		{
			UnrealBuildTool.Log.WriteLine(1, TraceEventType.Verbose, Format, Args);
		}

		/// <summary>
		/// Writes formatted text to log (with TraceEventType.Verbose).
		/// </summary>
		/// <param name="Message">Text</param>
		[MethodImplAttribute(MethodImplOptions.NoInlining)]
		public static void LogVerbose(string Message)
		{
			UnrealBuildTool.Log.WriteLine(1, TraceEventType.Verbose, Message);
		}

		/// <summary>
		/// Writes formatted text to log.
		/// </summary>
		/// <param name="Verbosity">Verbosity</param>
		/// <param name="Format">Format string</param>
		/// <param name="Args">Arguments</param>
		[MethodImplAttribute(MethodImplOptions.NoInlining)]
		public static void Log(TraceEventType Verbosity, string Format, params object[] Args)
		{
			UnrealBuildTool.Log.WriteLine(1, Verbosity, Format, Args);
		}

		/// <summary>
		/// Writes formatted text to log.
		/// </summary>
		/// <param name="Verbosity">Verbosity</param>
		/// <param name="Message">Text</param>
		[MethodImplAttribute(MethodImplOptions.NoInlining)]
		public static void Log(TraceEventType Verbosity, string Message)
		{
			UnrealBuildTool.Log.WriteLine(1, Verbosity, Message);
		}

		/// <summary>
		/// Dumps exception to log.
		/// </summary>
		/// <param name="Verbosity">Verbosity</param>
		/// <param name="Ex">Exception</param>
		[MethodImplAttribute(MethodImplOptions.NoInlining)]
		public static void Log(TraceEventType Verbosity, Exception Ex)
		{
			UnrealBuildTool.Log.WriteLine(1, Verbosity, LogUtils.FormatException(Ex));
		}

		#endregion

		#region IO

		/// <summary>
		/// Finds files in specified paths. 
		/// </summary>
		/// <param name="SearchPattern">Pattern</param>
		/// <param name="Recursive">Recursive search</param>
		/// <param name="Paths">Paths to search</param>
		/// <returns>An array of files found in the specified paths</returns>
		public static string[] FindFiles(string SearchPattern, bool Recursive, params string[] Paths)
		{
			List<string> FoundFiles = new List<string>();
			foreach (var PathToSearch in Paths)
			{
				var NormalizedPath = ConvertSeparators(PathSeparator.Default, PathToSearch);
				if (DirectoryExists(NormalizedPath))
				{
					var FoundInPath = InternalUtils.SafeFindFiles(NormalizedPath, SearchPattern, Recursive);
					if (FoundInPath == null)
					{
						throw new AutomationException(String.Format("Failed to find files in '{0}'", NormalizedPath));
					}
					FoundFiles.AddRange(FoundInPath);
				}
			}
			return FoundFiles.ToArray();
		}

		/// <summary>
		/// Finds files in specified paths. 
		/// </summary>
		/// <param name="SearchPattern">Pattern</param>
		/// <param name="Recursive">Recursive search</param>
		/// <param name="Paths">Paths to search</param>
		/// <returns>An array of files found in the specified paths</returns>
		public static string[] FindFiles_NoExceptions(string SearchPattern, bool Recursive, params string[] Paths)
		{
			List<string> FoundFiles = new List<string>();
			foreach (var PathToSearch in Paths)
			{
				var NormalizedPath = ConvertSeparators(PathSeparator.Default, PathToSearch);
				if (DirectoryExists(NormalizedPath))
				{
					var FoundInPath = InternalUtils.SafeFindFiles(NormalizedPath, SearchPattern, Recursive);
					if (FoundInPath != null)
					{
						FoundFiles.AddRange(FoundInPath);
					}
				}
			}
			return FoundFiles.ToArray();
		}
        /// <summary>
        /// Finds files in specified paths. 
        /// </summary>
        /// <param name="SearchPattern">Pattern</param>
        /// <param name="Recursive">Recursive search</param>
        /// <param name="Paths">Paths to search</param>
        /// <returns>An array of files found in the specified paths</returns>
        public static string[] FindFiles_NoExceptions(bool bQuiet, string SearchPattern, bool Recursive, params string[] Paths)
        {
            List<string> FoundFiles = new List<string>();
            foreach (var PathToSearch in Paths)
            {
                var NormalizedPath = ConvertSeparators(PathSeparator.Default, PathToSearch);
                if (DirectoryExists(NormalizedPath))
                {
                    var FoundInPath = InternalUtils.SafeFindFiles(NormalizedPath, SearchPattern, Recursive, bQuiet);
                    if (FoundInPath != null)
                    {
                        FoundFiles.AddRange(FoundInPath);
                    }
                }
            }
            return FoundFiles.ToArray();
        }
		/// <summary>
		/// Finds files in specified paths. 
		/// </summary>
		/// <param name="SearchPattern">Pattern</param>
		/// <param name="Recursive">Recursive search</param>
		/// <param name="Paths">Paths to search</param>
		/// <returns>An array of files found in the specified paths</returns>
		public static string[] FindDirectories(bool bQuiet, string SearchPattern, bool Recursive, params string[] Paths)
		{
			List<string> FoundDirs = new List<string>();
			foreach (var PathToSearch in Paths)
			{
				var NormalizedPath = ConvertSeparators(PathSeparator.Default, PathToSearch);
				if (DirectoryExists(NormalizedPath))
				{
					var FoundInPath = InternalUtils.SafeFindDirectories(NormalizedPath, SearchPattern, Recursive, bQuiet);
					if (FoundInPath == null)
					{
						throw new AutomationException(String.Format("Failed to find directories in '{0}'", NormalizedPath));
					}
					FoundDirs.AddRange(FoundInPath);
				}
			}
			return FoundDirs.ToArray();
		}
		/// <summary>
		/// Finds Directories in specified paths. 
		/// </summary>
		/// <param name="SearchPattern">Pattern</param>
		/// <param name="Recursive">Recursive search</param>
		/// <param name="Paths">Paths to search</param>
		/// <returns>An array of files found in the specified paths</returns>
		public static string[] FindDirectories_NoExceptions(bool bQuiet, string SearchPattern, bool Recursive, params string[] Paths)
		{
			List<string> FoundDirs = new List<string>();
			foreach (var PathToSearch in Paths)
			{
				var NormalizedPath = ConvertSeparators(PathSeparator.Default, PathToSearch);
				if (DirectoryExists(NormalizedPath))
				{
					var FoundInPath = InternalUtils.SafeFindDirectories(NormalizedPath, SearchPattern, Recursive, bQuiet);
					if (FoundInPath != null)
					{
						FoundDirs.AddRange(FoundInPath);
					}
				}
			}
			return FoundDirs.ToArray();
		}
		/// <summary>
		/// Deletes a file(s). 
		/// If the file does not exist, silently succeeds.
		/// If the deletion of the file fails, this function throws an Exception.
		/// </summary>
		/// <param name="Filename">Filename</param>
		public static void DeleteFile(params string[] Filenames)
		{
			foreach (var Filename in Filenames)
			{
				var NormalizedFilename = ConvertSeparators(PathSeparator.Default, Filename);
				if (!InternalUtils.SafeDeleteFile(NormalizedFilename))
				{
					throw new AutomationException(String.Format("Failed to delete file '{0}'", NormalizedFilename));
				}
			}
		}

		/// <summary>
		/// Deletes a file(s). 
		/// If the deletion of the file fails, prints a warning.
		/// </summary>
		/// <param name="Filename">Filename</param>
        public static bool DeleteFile_NoExceptions(params string[] Filenames)
		{
			bool Result = true;
			foreach (var Filename in Filenames)
			{
				var NormalizedFilename = ConvertSeparators(PathSeparator.Default, Filename);
				if (!InternalUtils.SafeDeleteFile(NormalizedFilename))
				{
					Log(TraceEventType.Warning, "Failed to delete file '{0}'", NormalizedFilename);
					Result = false;
				}
			}
			return Result;
		}
		/// <summary>
		/// Deletes a file(s). 
		/// If the deletion of the file fails, prints a warning.
		/// </summary>
		/// <param name="Filename">Filename</param>
        /// <param name="bQuiet">if true, then don't retry and don't print much.</param>
        public static bool DeleteFile_NoExceptions(string Filename, bool bQuiet = false)
		{
			bool Result = true;
			var NormalizedFilename = ConvertSeparators(PathSeparator.Default, Filename);
			if (!InternalUtils.SafeDeleteFile(NormalizedFilename, bQuiet))
			{
				Log(bQuiet ? TraceEventType.Information : TraceEventType.Warning, "Failed to delete file '{0}'", NormalizedFilename);
				Result = false;
			}
			return Result;
		}

		/// <summary>
		/// Deletes a directory(or directories) including its contents (recursively, will delete read-only files).
		/// If the deletion of the directory fails, this function throws an Exception.
		/// </summary>
		/// <param name="bQuiet">Suppresses log output if true</param>
		/// <param name="Directory">Directory</param>
		public static void DeleteDirectory(bool bQuiet, params string[] Directories)
		{
			foreach (var Directory in Directories)
			{
				var NormalizedDirectory = ConvertSeparators(PathSeparator.Default, Directory);
				if (!InternalUtils.SafeDeleteDirectory(NormalizedDirectory, bQuiet))
				{
					throw new AutomationException(String.Format("Failed to delete directory '{0}'", NormalizedDirectory));
				}
			}
		}

		/// <summary>
		/// Deletes a directory(or directories) including its contents (recursively, will delete read-only files).
		/// If the deletion of the directory fails, this function throws an Exception.
		/// </summary>
		/// <param name="Directory">Directory</param>
		public static void DeleteDirectory(params string[] Directories)
		{
			DeleteDirectory(false, Directories);
		}

		/// <summary>
		/// Deletes a directory(or directories) including its contents (recursively, will delete read-only files).
		/// If the deletion of the directory fails, prints a warning.
		/// </summary>
		/// <param name="bQuiet">Suppresses log output if true</param>
		/// <param name="Directory">Directory</param>
		public static bool DeleteDirectory_NoExceptions(bool bQuiet, params string[] Directories)
		{
			bool Result = true;
            foreach (var Directory in Directories)
            {
                var NormalizedDirectory = ConvertSeparators(PathSeparator.Default, Directory);
                try
                {
                    if (!InternalUtils.SafeDeleteDirectory(NormalizedDirectory, bQuiet))
                    {
                        Log(TraceEventType.Warning, "Failed to delete directory '{0}'", NormalizedDirectory);
                        Result = false;
                    }
                }
                catch (Exception Ex)
                {
					if (!bQuiet)
					{
						Log(TraceEventType.Warning, "Failed to delete directory, exception '{0}'", NormalizedDirectory);
						Log(TraceEventType.Warning, Ex);
					}
                    Result = false;
                }
            }


			return Result;
		}

		/// <summary>
		/// Deletes a directory(or directories) including its contents (recursively, will delete read-only files).
		/// If the deletion of the directory fails, prints a warning.
		/// </summary>
		/// <param name="Directory">Directory</param>
		public static bool DeleteDirectory_NoExceptions(params string[] Directories)
		{
			return DeleteDirectory_NoExceptions(false, Directories);
		}


		/// <summary>
		/// Attempts to delete a directory, if that fails deletes all files and folder from the specified directory.
		/// This works around the issue when the user has a file open in a notepad from that directory. Somehow deleting the file works but
		/// deleting the directory with the file that's open, doesn't.
		/// </summary>
		/// <param name="DirectoryName"></param>
		public static void DeleteDirectoryContents(string DirectoryName)
		{
			Log("DeleteDirectoryContents({0})", DirectoryName);
			const bool bQuiet = true;
			var Files = CommandUtils.FindFiles_NoExceptions(bQuiet, "*", false, DirectoryName);
			foreach (var Filename in Files)
			{
				CommandUtils.DeleteFile_NoExceptions(Filename);
			}
			var Directories = CommandUtils.FindDirectories_NoExceptions(bQuiet, "*", false, DirectoryName);
			foreach (var SubDirectoryName in Directories)
			{
				CommandUtils.DeleteDirectory_NoExceptions(bQuiet, SubDirectoryName);
			}
		}

		/// <summary>
		/// Checks if a directory(or directories) exists.
		/// </summary>
		/// <param name="Directory">Directory</param>
		/// <returns>True if the directory exists, false otherwise.</returns>
		public static bool DirectoryExists(params string[] Directories)
		{
			bool bExists = Directories.Length > 0;
			foreach (var DirectoryName in Directories)
			{
				var NormalizedDirectory = ConvertSeparators(PathSeparator.Default, DirectoryName);
				bExists = System.IO.Directory.Exists(NormalizedDirectory) && bExists;
			}
			return bExists;
		}

		/// <summary>
		/// Checks if a directory(or directories) exists.
		/// </summary>
		/// <param name="Directory">Directory</param>
		/// <returns>True if the directory exists, false otherwise.</returns>
		public static bool DirectoryExists_NoExceptions(params string[] Directories)
		{
			bool bExists = Directories.Length > 0;
			foreach (var DirectoryName in Directories)
			{
				var NormalizedDirectory = ConvertSeparators(PathSeparator.Default, DirectoryName);
				try
				{
					bExists = System.IO.Directory.Exists(NormalizedDirectory) && bExists;
				}
				catch (Exception Ex)
				{
					Log(TraceEventType.Warning, "Unable to check if directory exists: {0}", NormalizedDirectory);
					Log(TraceEventType.Warning, Ex);
					bExists = false;
					break;
				}
			}
			return bExists;
		}

		/// <summary>
		/// Creates a directory(or directories).
		/// If the creation of the directory fails, this function throws an Exception.
		/// </summary>
		/// <param name="Directory">Directory</param>
		public static void CreateDirectory(params string[] Directories)
		{
			foreach (var DirectoryName in Directories)
			{
				var NormalizedDirectory = ConvertSeparators(PathSeparator.Default, DirectoryName);
				if (!InternalUtils.SafeCreateDirectory(NormalizedDirectory))
				{
					throw new AutomationException(String.Format("Failed to create directory '{0}'", NormalizedDirectory));
				}
			}
		}

		/// <summary>
		/// Creates a directory (or directories).
		/// If the creation of the directory fails, this function prints a warning.
		/// </summary>
		/// <param name="Directory">Directory</param>
		public static bool CreateDirectory_NoExceptions(params string[] Directories)
		{
			bool Result = true;
			foreach (var DirectoryName in Directories)
			{
				var NormalizedDirectory = ConvertSeparators(PathSeparator.Default, DirectoryName);
				if (!InternalUtils.SafeCreateDirectory(NormalizedDirectory))
				{
					Log(TraceEventType.Warning, "Failed to create directory '{0}'", NormalizedDirectory);
					Result = false;
				}
			}
			return Result;
		}

		/// <summary>
		/// Renames/moves a file.
		/// If the rename of the file fails, this function throws an Exception.
		/// </summary>
		/// <param name="OldName">Old name</param>
		/// <param name="NewName">new name</param>
		public static void RenameFile(string OldName, string NewName)
		{
			var OldNormalized = ConvertSeparators(PathSeparator.Default, OldName);
			var NewNormalized = ConvertSeparators(PathSeparator.Default, NewName);
			if (!InternalUtils.SafeRenameFile(OldNormalized, NewNormalized))
			{
				throw new AutomationException(String.Format("Failed to rename/move file '{0}' to '{1}'", OldNormalized, NewNormalized));
			}
		}

		/// <summary>
		/// Renames/moves a file.
		/// If the rename of the file fails, this function prints a warning.
		/// </summary>
		/// <param name="OldName">Old name</param>
		/// <param name="NewName">new name</param>
		public static bool RenameFile_NoExceptions(string OldName, string NewName)
		{
			var OldNormalized = ConvertSeparators(PathSeparator.Default, OldName);
			var NewNormalized = ConvertSeparators(PathSeparator.Default, NewName);
			var Result = InternalUtils.SafeRenameFile(OldNormalized, NewNormalized);
			if (!Result)
			{
				Log(TraceEventType.Warning, "Failed to rename/move file '{0}' to '{1}'", OldName, NewName);
			}
			return Result;
		}

		/// <summary>
		/// Checks if a file(s) exists.
		/// </summary>
		/// <param name="Filename">Filename.</param>
		/// <returns>True if the file exists, false otherwise.</returns>
		public static bool FileExists(params string[] Filenames)
		{
			bool bExists = Filenames.Length > 0;
			foreach (var Filename in Filenames)
			{
				var NormalizedFilename = ConvertSeparators(PathSeparator.Default, Filename);
				bExists = InternalUtils.SafeFileExists(Filename) && bExists;
			}
			return bExists;
		}

		/// <summary>
		/// Checks if a file(s) exists.
		/// </summary>
		/// <param name="Filename">Filename.</param>
		/// <returns>True if the file exists, false otherwise.</returns>
		public static bool FileExists_NoExceptions(params string[] Filenames)
		{
			// Standard version doesn't throw, but keep this function for consistency.
			return FileExists(Filenames);
		}
        /// <summary>
        /// Checks if a file(s) exists.
        /// </summary>
        /// <param name="Filename">Filename.</param>
        /// <returns>True if the file exists, false otherwise.</returns>
        public static bool FileExists(bool bQuiet, params string[] Filenames)
        {
            bool bExists = Filenames.Length > 0;
            foreach (var Filename in Filenames)
            {
                var NormalizedFilename = ConvertSeparators(PathSeparator.Default, Filename);
                bExists = InternalUtils.SafeFileExists(Filename, bQuiet) && bExists;
            }
            return bExists;
        }

        /// <summary>
        /// Checks if a file(s) exists.
        /// </summary>
        /// <param name="Filename">Filename.</param>
        /// <returns>True if the file exists, false otherwise.</returns>
        public static bool FileExists_NoExceptions(bool bQuiet, params string[] Filenames)
        {
            // Standard version doesn't throw, but keep this function for consistency.
            return FileExists(bQuiet, Filenames);
        }

		static Stack<string> WorkingDirectoryStack = new Stack<string>();

		/// <summary>
		/// Pushes the current working directory onto a stack and sets CWD to a new value.
		/// </summary>
		/// <param name="WorkingDirectory">New working direcotry.</param>
		public static void PushDir(string WorkingDirectory)
		{
			string OrigCurrentDirectory = Environment.CurrentDirectory;
			WorkingDirectory = ConvertSeparators(PathSeparator.Default, WorkingDirectory);
			try
			{
				Environment.CurrentDirectory = WorkingDirectory;
			}
			catch (Exception Ex)
			{
				throw new AutomationException(String.Format("Unable to change current directory to {0}", WorkingDirectory), Ex);
			}

			WorkingDirectoryStack.Push(OrigCurrentDirectory);
		}

		/// <summary>
		/// Pushes the current working directory onto a stack and sets CWD to a new value.
		/// </summary>
		/// <param name="WorkingDirectory">New working direcotry.</param>
		public static bool PushDir_NoExceptions(string WorkingDirectory)
		{
			bool Result = true;
			WorkingDirectory = ConvertSeparators(PathSeparator.Default, WorkingDirectory);
			try
			{
				Environment.CurrentDirectory = WorkingDirectory;
				WorkingDirectoryStack.Push(Environment.CurrentDirectory);
			}
			catch
			{
				Log(TraceEventType.Warning, "Unable to change current directory to {0}", WorkingDirectory);
				Result = false;
			}
			return Result;
		}

		/// <summary>
		/// Pops the last working directory from a stack and sets it as the current working directory.
		/// </summary>
		public static void PopDir()
		{
			if (WorkingDirectoryStack.Count > 0)
			{
				Environment.CurrentDirectory = WorkingDirectoryStack.Pop();
			}
			else
			{
				throw new AutomationException("Unable to PopDir. WorkingDirectoryStack is empty.");
			}
		}

		/// <summary>
		/// Pops the last working directory from a stack and sets it as the current working directory.
		/// </summary>
		public static bool PopDir_NoExceptions()
		{
			bool Result = true;
			if (WorkingDirectoryStack.Count > 0)
			{
				Environment.CurrentDirectory = WorkingDirectoryStack.Pop();
			}
			else
			{
				Log(TraceEventType.Warning, "Unable to PopDir. WorkingDirectoryStack is empty.");
				Result = false;
			}
			return Result;
		}

		/// <summary>
		/// Clears the directory stack
		/// </summary>
		public static void ClearDirStack()
		{
			while (WorkingDirectoryStack.Count > 0)
			{
				PopDir();
			}
		}

		/// <summary>
		/// Changes the current working directory.
		/// </summary>
		/// <param name="WorkingDirectory">New working directory.</param>
		public static void ChDir(string WorkingDirectory)
		{
			WorkingDirectory = ConvertSeparators(PathSeparator.Default, WorkingDirectory);
			try
			{
				Environment.CurrentDirectory = WorkingDirectory;
			}
			catch (Exception Ex)
			{
				throw new ArgumentException(String.Format("Unable to change current directory to {0}", WorkingDirectory), Ex);
			}
		}

		/// <summary>
		/// Changes the current working directory.
		/// </summary>
		/// <param name="WorkingDirectory">New working directory.</param>
		public static bool ChDir_NoExceptions(string WorkingDirectory)
		{
			bool Result = true;
			WorkingDirectory = ConvertSeparators(PathSeparator.Default, WorkingDirectory);
			try
			{
				Environment.CurrentDirectory = WorkingDirectory;
			}
			catch
			{
				Log(TraceEventType.Warning, "Unable to change current directory to {0}", WorkingDirectory);
				Result = false;
			}
			return Result;
		}

		/// <summary>
		/// Sets file attributes. Will not change attributes that have not been specified.
		/// </summary>
		/// <param name="Filename">Filename</param>
		/// <param name="ReadOnly">Read-only attribute</param>
		/// <param name="Hidden">Hidden attribute.</param>
		/// <param name="Archive">Archive attribute.</param>
		public static void SetFileAttributes(string Filename, bool? ReadOnly = null, bool? Hidden = null, bool? Archive = null)
		{
			Filename = ConvertSeparators(PathSeparator.Default, Filename);
			if (!File.Exists(Filename))
			{
				throw new AutomationException("Unable to set attributes for a non-exisiting file.", new FileNotFoundException("File not found.", Filename));
			}

			FileAttributes Attributes = File.GetAttributes(Filename);
			Attributes = InternalSetAttributes(ReadOnly, Hidden, Archive, Attributes);
			File.SetAttributes(Filename, Attributes);
		}

		/// <summary>
		/// Sets file attributes. Will not change attributes that have not been specified.
		/// </summary>
		/// <param name="Filename">Filename</param>
		/// <param name="ReadOnly">Read-only attribute</param>
		/// <param name="Hidden">Hidden attribute.</param>
		/// <param name="Archive">Archive attribute.</param>
		public static bool SetFileAttributes_NoExceptions(string Filename, bool? ReadOnly = null, bool? Hidden = null, bool? Archive = null)
		{			
			Filename = ConvertSeparators(PathSeparator.Default, Filename);
			if (!File.Exists(Filename))
			{
				Log(TraceEventType.Warning, "Unable to set attributes for a non-exisiting file ({0})", Filename);
				return false;
			}

			bool Result = true;
			try
			{
				FileAttributes Attributes = File.GetAttributes(Filename);
				Attributes = InternalSetAttributes(ReadOnly, Hidden, Archive, Attributes);
				File.SetAttributes(Filename, Attributes);
			}
			catch (Exception Ex)
			{
				Log(TraceEventType.Warning, "Error trying to set file attributes for: {0}", Filename);
				Log(TraceEventType.Warning, Ex);
				Result = false;
			}
			return Result;
		}

		private static FileAttributes InternalSetAttributes(bool? ReadOnly, bool? Hidden, bool? Archive, FileAttributes Attributes)
		{
			if (ReadOnly != null)
			{
				if ((bool)ReadOnly)
				{
					Attributes |= FileAttributes.ReadOnly;
				}
				else
				{
					Attributes &= ~FileAttributes.ReadOnly;
				}
			}
			if (Hidden != null)
			{
				if ((bool)Hidden)
				{
					Attributes |= FileAttributes.Hidden;
				}
				else
				{
					Attributes &= ~FileAttributes.Hidden;
				}
			}
			if (Archive != null)
			{
				if ((bool)Archive)
				{
					Attributes |= FileAttributes.Archive;
				}
				else
				{
					Attributes &= ~FileAttributes.Archive;
				}
			}
			return Attributes;
		}

		/// <summary>
		/// Writes a line of formatted string to a file. Creates the file if it does not exists.
		/// If the file does exists, appends a new line.
		/// </summary>
		/// <param name="Filename">Filename</param>
		/// <param name="Format">Format string</param>
		/// <param name="Args">Arguments</param>
		public static void WriteToFile(string Filename, string Format, params object[] Args)
		{
			WriteToFile(Filename, String.Format(Format, Args));
		}

		/// <summary>
		/// Writes a line of formatted string to a file. Creates the file if it does not exists.
		/// If the file does exists, appends a new line.
		/// </summary>
		/// <param name="Filename">Filename</param>
		/// <param name="Text">Text to write</param>
		public static void WriteToFile(string Filename, string Text)
		{
			try
			{
				Filename = ConvertSeparators(PathSeparator.Default, Filename);
				FileStream Stream;
				if (File.Exists(Filename))
				{
					Stream = new FileStream(Filename, FileMode.Append, FileAccess.Write, FileShare.Read);
				}
				else
				{
					Stream = new FileStream(Filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
				}
				if (Stream != null)
				{
					using (StreamWriter Writer = new StreamWriter(Stream))
					{
						Writer.WriteLine(Text);
						Writer.Flush();
						Writer.Close();
					}
					Stream.Dispose();
				}
			}
			catch (Exception Ex)
			{
				throw new AutomationException(String.Format("Failed to Write to file {0}", Filename), Ex);
			}
		}

		/// <summary>
		/// Reads all text lines from a file.
		/// </summary>
		/// <param name="Filename">Filename</param>
		/// <returns>Array of lines of text read from the file. null if the file did not exist or could not be read.</returns>
		public static string[] ReadAllLines(string Filename)
		{
			Filename = ConvertSeparators(PathSeparator.Default, Filename);
			return InternalUtils.SafeReadAllLines(Filename);
		}

		/// <summary>
		/// Reads all text from a file.
		/// </summary>
		/// <param name="Filename">Filename</param>
		/// <returns>All text read from the file. null if the file did not exist or could not be read.</returns>
		public static string ReadAllText(string Filename)
		{
			Filename = ConvertSeparators(PathSeparator.Default, Filename);
			return InternalUtils.SafeReadAllText(Filename);
		}

		/// <summary>
		/// Writes lines to a file.
		/// </summary>
		/// <param name="Filename">Filename</param>
		/// <param name="Lines">Text</param>
		public static void WriteAllLines(string Filename, string[] Lines)
		{
			Filename = ConvertSeparators(PathSeparator.Default, Filename);
			if (!InternalUtils.SafeWriteAllLines(Filename, Lines))
			{
				throw new AutomationException("Unable to write to file: {0}", Filename);
			}
		}

		/// <summary>
		/// Writes lines to a file.
		/// </summary>
		/// <param name="Filename">Filename</param>
		/// <param name="Lines">Text</param>
		public static bool WriteAllLines_NoExceptions(string Filename, string[] Lines)
		{
			Filename = ConvertSeparators(PathSeparator.Default, Filename);
			return InternalUtils.SafeWriteAllLines(Filename, Lines);
		}

		/// <summary>
		/// Writes text to a file.
		/// </summary>
		/// <param name="Filename">Filename</param>
		/// <param name="Lines">Text</param>
		public static void WriteAllText(string Filename, string Text)
		{
			Filename = ConvertSeparators(PathSeparator.Default, Filename);
			if (!InternalUtils.SafeWriteAllText(Filename, Text))
			{
				throw new AutomationException("Unable to write to file: {0}", Filename);
			}
		}

		/// <summary>
		/// Writes text to a file.
		/// </summary>
		/// <param name="Filename">Filename</param>
		/// <param name="Lines">Text</param>
		public static bool WriteAllText_NoExceptions(string Filename, string Text)
		{
			Filename = ConvertSeparators(PathSeparator.Default, Filename);
			return InternalUtils.SafeWriteAllText(Filename, Text);
		}


		/// <summary>
		/// Writes byte array to a file.
		/// </summary>
		/// <param name="Filename">Filename</param>
		/// <param name="Bytes">Byte array</param>
		public static void WriteAllBytes(string Filename, byte[] Bytes)
		{
			Filename = ConvertSeparators(PathSeparator.Default, Filename);
			if (!InternalUtils.SafeWriteAllBytes(Filename, Bytes))
			{
				throw new AutomationException("Unable to write to file: {0}", Filename);
			}
		}

		/// <summary>
		/// Writes byte array to a file.
		/// </summary>
		/// <param name="Filename">Filename</param>
		/// <param name="Bytes">Byte array</param>
		public static bool WriteAllBytes_NoExceptions(string Filename, byte[] Bytes)
		{
			Filename = ConvertSeparators(PathSeparator.Default, Filename);
			return InternalUtils.SafeWriteAllBytes(Filename, Bytes);
		}

		/// <summary>
		/// Gets a character representing the specified separator type.
		/// </summary>
		/// <param name="SeparatorType">Separator type.</param>
		/// <returns>Separator character</returns>
		public static char GetPathSeparatorChar(PathSeparator SeparatorType)
		{
			char Separator;
			switch (SeparatorType)
			{
				case PathSeparator.Slash:
				case PathSeparator.Depot:
					Separator = '/';
					break;
				case PathSeparator.Backslash:
					Separator = '\\';
					break;
				default:
					Separator = Path.DirectorySeparatorChar;
					break;
			}
			return Separator;
		}

		/// <summary>
		/// Checks if the character is one of the two sperator types ('\' or '/')
		/// </summary>
		/// <param name="Character">Character to check.</param>
		/// <returns>True if the character is a separator, false otherwise.</returns>
		public static bool IsPathSeparator(char Character)
		{
			return (Character == '/' || Character == '\\');
		}

		/// <summary>
		/// Combines paths and replaces all path separators with the system default separator.
		/// </summary>
		/// <param name="Paths"></param>
		/// <returns>Combined Path</returns>
		public static string CombinePaths(params string[] Paths)
		{
			return CombinePaths(PathSeparator.Default, Paths);
		}

		/// <summary>
		/// Combines paths and replaces all path separators wth the system specified separator.
		/// </summary>
		/// <param name="SeparatorType">Type of separartor to use when combining paths.</param>
		/// <param name="Paths"></param>
		/// <returns>Combined Path</returns>
		public static string CombinePaths(PathSeparator SeparatorType, params string[] Paths)
		{
			// Pick a separator to use.
			var SeparatorToUse = GetPathSeparatorChar(SeparatorType);
			var SeparatorToReplace = SeparatorToUse == '/' ? '\\' : '/';

			// Allocate string builder
			int CombinePathMaxLength = 0;
			foreach (var PathPart in Paths)
			{
				CombinePathMaxLength += (PathPart != null) ? PathPart.Length : 0;
			}
			CombinePathMaxLength += Paths.Length;
			var CombinedPath = new StringBuilder(CombinePathMaxLength);

			// Combine all paths
			CombinedPath.Append(Paths[0]);
			for (int PathIndex = 1; PathIndex < Paths.Length; ++PathIndex)
			{
				var NextPath = Paths[PathIndex];
				if (String.IsNullOrEmpty(NextPath) == false)
				{
					int NextPathStartIndex = 0;
					if (CombinedPath.Length != 0)
					{
						var LastChar = CombinedPath[CombinedPath.Length - 1];
						var NextChar = NextPath[0];
						var IsLastCharPathSeparator = IsPathSeparator(LastChar);
						var IsNextCharPathSeparator = IsPathSeparator(NextChar);
						// Check if a separator between paths is required
						if (!IsLastCharPathSeparator && !IsNextCharPathSeparator)
						{
							CombinedPath.Append(SeparatorToUse);
						}
						// Check if one of the saprators needs to be skipped.
						else if (IsLastCharPathSeparator && IsNextCharPathSeparator)
						{
							NextPathStartIndex = 1;
						}
					}
					CombinedPath.Append(NextPath, NextPathStartIndex, NextPath.Length - NextPathStartIndex);
				}
			}
			// Make sure there's only one separator type used.
			CombinedPath.Replace(SeparatorToReplace, SeparatorToUse);
			return CombinedPath.ToString();
		}

		/// <summary>
		/// Converts all separators in path to the specified separator type.
		/// </summary>
		/// <param name="ToSperatorType">Desired separator type.</param>
		/// <param name="PathToConvert">Path</param>
		/// <returns>Path where all separators have been converted to the specified type.</returns>
		public static string ConvertSeparators(PathSeparator ToSperatorType, string PathToConvert)
		{
			return CombinePaths(ToSperatorType, PathToConvert);
		}

		/// <summary>
		/// Copies a file.
		/// </summary>
		/// <param name="Source"></param>
		/// <param name="Dest"></param>
		/// <returns>True if the operation was successful, false otherwise.</returns>
        public static void CopyFile(string Source, string Dest, bool bQuiet = false)
		{
			Source = ConvertSeparators(PathSeparator.Default, Source);
			Dest = ConvertSeparators(PathSeparator.Default, Dest);
			if (InternalUtils.SafeFileExists(Dest, true))
			{
				InternalUtils.SafeDeleteFile(Dest, bQuiet);
			}
			else if (!InternalUtils.SafeDirectoryExists(Path.GetDirectoryName(Dest), true))
			{
				if (!InternalUtils.SafeCreateDirectory(Path.GetDirectoryName(Dest), bQuiet))
				{
					throw new AutomationException("Failed to create directory {0} for copy", Path.GetDirectoryName(Dest));
				}
			}
			if (InternalUtils.SafeFileExists(Dest, true))
			{
				throw new AutomationException("Failed to delete {0} for copy", Dest);
			}
			if (!InternalUtils.SafeCopyFile(Source, Dest, bQuiet))
			{
				throw new AutomationException("Failed to copy {0} to {1}", Source, Dest);
			}
		}

		/// <summary>
		/// Copies a file. Does not throw exceptions.
		/// </summary>
		/// <param name="Source"></param>
		/// <param name="Dest"></param>
		/// <returns>True if the operation was successful, false otherwise.</returns>
        public static bool CopyFile_NoExceptions(string Source, string Dest, bool bQuiet = false)
		{
			Source = ConvertSeparators(PathSeparator.Default, Source);
			Dest = ConvertSeparators(PathSeparator.Default, Dest);
            if (InternalUtils.SafeFileExists(Dest, true))
			{
                InternalUtils.SafeDeleteFile(Dest, bQuiet);
			}
			else if (!InternalUtils.SafeDirectoryExists(Path.GetDirectoryName(Dest), true))
			{
				if (!InternalUtils.SafeCreateDirectory(Path.GetDirectoryName(Dest)))
				{
					return false;
				}
			}
			if (InternalUtils.SafeFileExists(Dest, true))
			{
				return false;
			}
			return InternalUtils.SafeCopyFile(Source, Dest, bQuiet);
		}

		/// <summary>
		/// Copies a file if the dest doesn't exist or the dest timestamp is different; after a copy, copies the timestamp
		/// </summary>
		/// <param name="Source"></param>
		/// <param name="Dest"></param>
		/// <returns>True if the operation was successful, false otherwise.</returns>
		public static void CopyFileIncremental(string Source, string Dest)
		{
			Source = ConvertSeparators(PathSeparator.Default, Source);
			Dest = ConvertSeparators(PathSeparator.Default, Dest);
			if (InternalUtils.SafeFileExists(Dest, true))
			{
				TimeSpan Diff = File.GetLastWriteTimeUtc(Dest) - File.GetLastWriteTimeUtc(Source);
				if (Diff.TotalSeconds > -1 && Diff.TotalSeconds < 1)
				{
					Log("CopyFileIncremental Skipping {0}, up to date.", Dest);
					return;
				}
				InternalUtils.SafeDeleteFile(Dest);
			}
			else if (!InternalUtils.SafeDirectoryExists(Path.GetDirectoryName(Dest), true))
			{
				if (!InternalUtils.SafeCreateDirectory(Path.GetDirectoryName(Dest)))
				{
					throw new AutomationException("Failed to create directory {0} for copy", Path.GetDirectoryName(Dest));
				}
			}
			if (InternalUtils.SafeFileExists(Dest, true))
			{
				throw new AutomationException("Failed to delete {0} for copy", Dest);
			}
			if (!InternalUtils.SafeCopyFile(Source, Dest))
			{
				throw new AutomationException("Failed to copy {0} to {1}", Source, Dest);
			}
			FileAttributes Attributes = File.GetAttributes(Dest);
			if ((Attributes & FileAttributes.ReadOnly) != 0)
			{
				File.SetAttributes(Dest, Attributes & ~FileAttributes.ReadOnly);
			}
			File.SetLastWriteTimeUtc(Dest, File.GetLastWriteTimeUtc(Source));
		}
		/// <summary>
		/// Returns directory name without filename. 
		/// The difference between this and Path.GetDirectoryName is that this
		/// function will not throw away the last name if it doesn't have an extension, for example:
		/// D:\Project\Data\Asset -> D:\Project\Data\Asset
		/// D:\Project\Data\Asset.ussset -> D:\Project\Data
		/// </summary>
		/// <param name="FilePath"></param>
		/// <returns></returns>
		public static string GetDirectoryName(string FilePath)
		{
			var LastSeparatorIndex = Math.Max(FilePath.LastIndexOf('/'), FilePath.LastIndexOf('\\'));
			var ExtensionIndex = FilePath.LastIndexOf('.');
			if (ExtensionIndex > LastSeparatorIndex || LastSeparatorIndex == (FilePath.Length - 1))
			{
				return FilePath.Substring(0, LastSeparatorIndex);
			}
			else
			{
				return FilePath;
			}
		}

		/// <summary>
		/// Returns the last directory name in the path string.
		/// For example: D:\Temp\Project\File.txt -> Project, Data\Samples -> Samples
		/// </summary>
		/// <param name="FilePath"></param>
		/// <returns></returns>
		public static string GetLastDirectoryName(string FilePath)
		{
			var LastDir = GetDirectoryName(FilePath);
			var LastSeparatorIndex = Math.Max(LastDir.LastIndexOf('/'), LastDir.LastIndexOf('\\'));
			if (LastSeparatorIndex >= 0)
			{
				LastDir = LastDir.Substring(LastSeparatorIndex + 1);
			}
			return LastDir;
		}

		/// <summary>
		/// Removes multi-dot extensions from a filename (i.e. *.automation.csproj)
		/// </summary>
		/// <param name="Filename">Filename to remove the extensions from</param>
		/// <returns>Clean filename.</returns>
		public static string GetFilenameWithoutAnyExtensions(string Filename)
		{
			do
			{
				Filename = Path.GetFileNameWithoutExtension(Filename);
			}
			while (Filename.IndexOf('.') >= 0);
			return Filename;
		}

		/// <summary>
		/// A container for a binary files (dll, exe) with its associated debug info.
		/// </summary>
		public class FileManifest
		{
			/// <summary>
			/// Items
			/// </summary>
			public readonly List<string> FileManifestItems = new List<string>();

			/// <summary>
			/// Constructor
			/// </summary>
			public FileManifest()
			{
			}
		}

		/// <summary>
		/// Reads a file manifest and returns it
		/// </summary>
		/// <param name="ManifestName">ManifestName</param>
		/// <returns></returns>
		public static FileManifest ReadManifest(string ManifestName)
		{
			return XmlHandler.ReadXml<FileManifest>(ManifestName);
		}

		private static void CloneDirectoryRecursiveWorker(string SourcePathBase, string TargetPathBase, List<string> ClonedFiles)
		{
            if (!InternalUtils.SafeCreateDirectory(TargetPathBase))
            {
                throw new AutomationException("Failed to create directory {0} for copy", TargetPathBase);
            }

			string[] SourceList = Directory.GetFiles(SourcePathBase);

			DirectoryInfo SourceDirectory = new DirectoryInfo(SourcePathBase);
			DirectoryInfo[] SourceSubdirectories = SourceDirectory.GetDirectories();

			// Copy the files
			FileInfo[] SourceFiles = SourceDirectory.GetFiles();
			foreach (FileInfo SourceFI in SourceFiles)
			{
				string TargetFilename = CommandUtils.CombinePaths(TargetPathBase, SourceFI.Name);
				SourceFI.CopyTo(TargetFilename);

				if (ClonedFiles != null)
				{
					ClonedFiles.Add(TargetFilename);
				}
			}

			// Recurse into subfolders
			foreach (DirectoryInfo SourceSubdir in SourceSubdirectories)
			{
				string NewSourcePath = CommandUtils.CombinePaths(SourcePathBase, SourceSubdir.Name);
				string NewTargetPath = CommandUtils.CombinePaths(TargetPathBase, SourceSubdir.Name);
				CloneDirectoryRecursiveWorker(NewSourcePath, NewTargetPath, ClonedFiles);
			}
		}

		/// <summary>
		/// Clones a directory.
		/// Warning: Will delete all of the existing files in TargetPath
		/// This is recursive, copying subfolders too.
		/// </summary>
		/// <param name="SourcePath">Source directory.</param>
		/// <param name="TargetPath">Target directory.</param>
		/// <param name="ClonedFiles">List of cloned files.</param>
		public static void CloneDirectory(string SourcePath, string TargetPath, List<string> ClonedFiles = null)
		{
			DeleteDirectory_NoExceptions(TargetPath);

			CloneDirectoryRecursiveWorker(SourcePath, TargetPath, ClonedFiles);
		}

		#endregion

		#region Environment variables

		/// <summary>
		/// Gets environment variable value.
		/// </summary>
		/// <param name="Name">Name of the environment variable</param>
		/// <returns>Environment variable value as string.</returns>
		public static string GetEnvVar(string Name)
		{
			return InternalUtils.GetEnvironmentVariable(Name, "");
		}

		/// <summary>
		/// Gets environment variable value.
		/// </summary>
		/// <param name="Name">Name of the environment variable</param>
		/// <param name="DefaultValue">Default value of the environment variable if the variable is not set.</param>
		/// <returns>Environment variable value as string.</returns>
		public static string GetEnvVar(string Name, string DefaultValue)
		{
			return InternalUtils.GetEnvironmentVariable(Name, DefaultValue);
		}

		/// <summary>
		/// Sets environment variable.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <param name="Value">Variable value.</param>
		/// <returns>True if the value has been set, false otherwise.</returns>
		public static void SetEnvVar(string Name, object Value)
		{
			try
			{
				Log("SetEnvVar {0}={1}", Name, Value);
				Environment.SetEnvironmentVariable(Name, Value.ToString());
			}
			catch (Exception Ex)
			{
				throw new AutomationException(String.Format("Failed to set environment variable {0} to {1}", Name, Value), Ex);
			}
		}

		/// <summary>
		/// Sets the environment variable if it hasn't been already.
		/// </summary>
		/// <param name="VarName">Environment variable name</param>
		/// <param name="Value">New value</param>
		public static void ConditionallySetEnvVar(string VarName, string Value)
		{
			if (String.IsNullOrEmpty(CommandUtils.GetEnvVar(VarName)))
			{
				Environment.SetEnvironmentVariable(VarName, Value);
			}
		}

		/// <summary>
		/// Gets environment variables set by a batch file.
		/// </summary>
		/// <param name="BatchFileName">Filename that sets the environment variables</param>
		/// <param name="AlsoSet">True if found variables should be automatically set withing this process.</param>
		/// <returns>Dictionary of environment variables set by the batch file.</returns>
		public static CaselessDictionary<string> GetEnvironmentVariablesFromBatchFile(string BatchFileName, bool AlsoSet = false)
		{
			if (File.Exists(BatchFileName))
			{
				// Create a wrapper batch file that echoes environment variables to a text file
				var EnvOutputFileName = CommandUtils.CombinePaths(Path.GetTempPath(), "HarvestEnvVars.txt");
				var EnvReaderBatchFileName = CommandUtils.CombinePaths(Path.GetTempPath(), "HarvestEnvVars.bat");

				{
					var EnvReaderBatchFileContent = new List<string>();

					// Run 'vcvars32.bat' (or similar x64 version) to set environment variables
					EnvReaderBatchFileContent.Add(String.Format("call \"{0}\"", BatchFileName));

					// Pipe all environment variables to a file where we can read them in
					EnvReaderBatchFileContent.Add(String.Format("set >\"{0}\"", EnvOutputFileName));

					File.WriteAllLines(EnvReaderBatchFileName, EnvReaderBatchFileContent);
				}

				InternalUtils.SafeDeleteFile(EnvOutputFileName);
				CommandUtils.Run(EnvReaderBatchFileName, "");

				var Result = new CaselessDictionary<string>();
				// Load environment variables
				var EnvStringsFromFile = File.ReadAllLines(EnvOutputFileName);
				foreach (var EnvString in EnvStringsFromFile)
				{
					// Parse the environment variable name and value from the string ("name=value")
					int EqualSignPos = EnvString.IndexOf('=');
					var EnvironmentVariableName = EnvString.Substring(0, EqualSignPos);
					var EnvironmentVariableValue = EnvString.Substring(EqualSignPos + 1);

					if (Environment.GetEnvironmentVariable(EnvironmentVariableName) != EnvironmentVariableValue)
					{
						Result.Add(EnvironmentVariableName, EnvironmentVariableValue);
					}
					if (AlsoSet)
					{
						// Set the environment variable
						Environment.SetEnvironmentVariable(EnvironmentVariableName, EnvironmentVariableValue);
					}
				}
				return Result;
			}
			else
			{
				throw new AutomationException("BatchFile {0} does not exist!", BatchFileName);
			}
		}

		#endregion

		#region CommandLine

		/// <summary>
		/// Converts a list of arguments to a string where each argument is separated with a space character.
		/// </summary>
		/// <param name="Args">Arguments</param>
		/// <returns>Single string containing all arguments separated with a space.</returns>
		public static string ArgsToCommandLine(params object[] Args)
		{
			string Arguments = "";
			if (Args != null)
			{
				for (int Index = 0; Index < Args.Length; ++Index)
				{
					Arguments += Args[Index].ToString();
					if (Index < (Args.Length - 1))
					{
						Arguments += " ";
					}
				}
			}
			return Arguments;
		}

		/// <summary>
		/// Parses the argument list for a parameter and returns whether it is defined or not.
		/// </summary>
		/// <param name="ArgList">Argument list.</param>
		/// <param name="Param">Param to check for.</param>
		/// <returns>True if param was found, false otherwise.</returns>
		public static bool ParseParam(object[] ArgList, string Param)
		{
			foreach (object Arg in ArgList)
			{
				if (Arg.ToString().Equals(Param, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Parses the command's Params list for a parameter and returns whether it is defined or not.
		/// </summary>
		/// <param name="Param">Param to check for.</param>
		/// <returns>True if param was found, false otherwise.</returns>
		public bool ParseParam(string Param)
		{
			return ParseParam(Params, Param);
		}

		/// <summary>
		/// Parses the argument list for a parameter and reads its value. 
		/// Ex. ParseParamValue(Args, "map=")
		/// </summary>
		/// <param name="ArgList">Argument list.</param>
		/// <param name="Param">Param to read its value.</param>
		/// <returns>Returns the value or Default if the parameter was not found.</returns>
		public static string ParseParamValue(object[] ArgList, string Param, string Default = null)
		{
			if (!Param.EndsWith("="))
			{
				Param += "=";
			}

			foreach (object Arg in ArgList)
			{
				string ArgStr = Arg.ToString();
				if (ArgStr.StartsWith(Param, StringComparison.InvariantCultureIgnoreCase))
				{
					return ArgStr.Substring(Param.Length);
				}
			}
			return Default;
		}

		/// <summary>
		/// Parses the command's Params list for a parameter and reads its value. 
		/// Ex. ParseParamValue(Args, "map=")
		/// </summary>
		/// <param name="Param">Param to read its value.</param>
		/// <returns>Returns the value or Default if the parameter was not found.</returns>
		public string ParseParamValue(string Param, string Default = null)
		{
			return ParseParamValue(Params, Param, Default);
		}

		/// <summary>
		/// Parses the command's Params list for a parameter and reads its value. 
		/// Ex. ParseParamValue(Args, "map=")
		/// </summary>
		/// <param name="Param">Param to read its value.</param>
		/// <returns>Returns the value or Default if the parameter was not found.</returns>
		public int ParseParamInt(string Param, int Default = 0)
		{
			string num = ParseParamValue(Params, Param, Default.ToString());
			return int.Parse(num);
		}

		/// <summary>
		/// Makes sure path can be used as a command line param (adds quotes if it contains spaces)
		/// </summary>
		/// <param name="InPath">Path to convert</param>
		/// <returns></returns>
		public static string MakePathSafeToUseWithCommandLine(string InPath)
		{
			if (InPath.Contains(' ') && InPath[0] != '\"')
			{
				InPath = "\"" + InPath + "\"";
			}
			return InPath;
		}

		#endregion

		#region Other

		public static string EscapePath(string InPath)
		{
			return InPath.Replace(":", "").Replace("/", "+").Replace("\\", "+").Replace(" ", "+");
		}

		/// <summary>
		/// Checks if collection is either null or empty.
		/// </summary>
		/// <param name="Collection">Collection to check.</param>
		/// <returns>True if the collection is either nur or empty.</returns>
		public static bool IsNullOrEmpty(ICollection Collection)
		{
			return Collection == null || Collection.Count == 0;
		}

		/// <summary>
		/// List of available target platforms.
		/// </summary>
		public static UnrealBuildTool.UnrealTargetPlatform[] KnownTargetPlatforms
		{
			get
			{
				if (UBTTargetPlatforms == null || UBTTargetPlatforms.Length == 0)
				{
					UBTTargetPlatforms = new UnrealBuildTool.UnrealTargetPlatform[UnrealBuildTool.UEBuildPlatform.BuildPlatformDictionary.Count];
					int Index = 0;
					foreach (var Platform in UnrealBuildTool.UEBuildPlatform.BuildPlatformDictionary)
					{
						UBTTargetPlatforms[Index++] = Platform.Key;
					}
				}
				return UBTTargetPlatforms;
			}
		}
		private static UnrealBuildTool.UnrealTargetPlatform[] UBTTargetPlatforms;

        /// <summary>
        /// Uploads the contents of a given directory to a series of MCP endpoints
        /// </summary>
        /// <param name="Location">The directory to upload</param>
        /// <param name="Username">Username for the MCP service permission</param>
        /// <param name="Password">Password for the MCP service permission</param>
        /// <param name="Services">List of MCP service URL enpodints</param>
        public static void UploadToEMS(string Location, string Username, string Password, List<string> Services)
        {
			//@todo add logging
			//@todo refactor for more general usability
			//@todo this information should be grabbed from Jenkins via an env var. See AWSContext

            if (Location == null)
            {
                throw new AutomationException("Location for EMS upload is not set.");
            }
            if (Username == null)
            {
                throw new AutomationException("Username for EMS upload is not set.");
            }
            if (Password == null)
            {
                throw new AutomationException("Password for EMS upload is not set.");
            }
            if (Services == null || Services.Count == 0)
            {
                throw new AutomationException("No services found EMS upload.");
            }

            string EnumerateUrl = "/api/cloudstorage/system";

            //iterate through files in the directory and store them
            //do this here rather than at upload time because they might be uploaded to multiple services
            List<EMSFileInfo> Files = new List<EMSFileInfo>();
            string[] FileNames = Directory.GetFiles(Location); 
            foreach (String FileName in FileNames)
            {
                string[] Parts = FileName.Split('\\');
                EMSFileInfo Info = new EMSFileInfo();
                Info.Bytes = System.IO.File.ReadAllBytes(FileName);
                Info.FileName = Parts[Parts.Length - 1];
                Files.Add(Info);
            }

            //auth header
            WebHeaderCollection Headers = new WebHeaderCollection();
            string BasicAuthString = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(Username + ":" + Password));
            Headers.Add("Authorization", "Basic " + BasicAuthString);

            DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(List<EnumerationResponse>));

            foreach (string BaseUrl in Services)
            {
                string Url = BaseUrl + EnumerateUrl;

                //list all the files 
                WebRequest Enumerate = WebRequest.Create(Url);
                Enumerate.Method = "GET";
                Enumerate.ContentType = "application/json";
                Enumerate.Headers = Headers;

                HttpWebResponse Response = (HttpWebResponse)Enumerate.GetResponse();

                //if there are any, iterate through and delete
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    List<EnumerationResponse> FoundFiles = (List<EnumerationResponse>) Serializer.ReadObject(Response.GetResponseStream());
                    foreach (EnumerationResponse File in FoundFiles)
                    {
                        WebRequest Delete = WebRequest.Create(Url + "/" + File.UniqueFilename);
                        Delete.Method = "DELETE";
                        Delete.Headers = Headers;
                        Delete.GetResponse();
                    }
                }

                //upload the files
                foreach (EMSFileInfo File in Files)
                {
                    WebRequest Upload = WebRequest.Create(Url + "?filename=" + File.FileName);
                    Upload.Method = "POST";
                    Upload.Headers = Headers;
                    Upload.ContentType = "text/plain";
                    Upload.ContentLength = File.Bytes.Length;
                    using (var payload = Upload.GetRequestStream())
                    {
                        payload.Write(File.Bytes, 0, File.Bytes.Length);

                    }
                    using (var response = (HttpWebResponse)Upload.GetResponse())
                    {
                    }
                }

            }

        }

		#endregion

		#region Properties

		/// <summary>
		/// Command line parameters for this command (empty by non-null by default)
		/// </summary>
		private object[] CommandLineParams = new object[0];
		public object[] Params
		{
			get { return CommandLineParams; }
			set { CommandLineParams = value; }
		}

		/// <summary>
		/// Path to the AutomationTool executable.
		/// </summary>
		public static string ExeFilename
		{
			get { return InternalUtils.ExecutingAssemblyLocation; }
		}

		/// <summary>
		/// Directory where the AutomationTool executable sits.
		/// </summary>
		public static string ExeDirectory
		{
			get { return InternalUtils.ExecutingAssemblyDirectory; }
		}

		/// <summary>
		/// Current directory.
		/// </summary>
		public static string CurrentDirectory
		{
			get { return Environment.CurrentDirectory; }
			set { ChDir(value); }
		}

		/// <summary>
		/// Checks if this command is running on a build machine.
		/// </summary>
		public static bool IsBuildMachine
		{
			get { return Automation.IsBuildMachine; }
		}

		#endregion
	}



	/// <summary>
	/// Use with "using" syntax to push and pop directories in a convenient, exception-safe way
	/// </summary>
	public class PushedDirectory : IDisposable
	{
		public PushedDirectory(string DirectoryName)
		{
			CommandUtils.PushDir(DirectoryName);
		}

		public void Dispose()
		{
			CommandUtils.PopDir();
			GC.SuppressFinalize(this);
		}
	}

    /// <summary>
    /// Helper class to associate a file and its contents
    /// </summary>
    public class EMSFileInfo
    {
        public string FileName { get; set; }
        public byte[] Bytes { get; set; }
    }

    /// <summary>
    /// Wrapper class for the enumerate files JSON response from MCP
    /// </summary>
    [DataContract]
    public sealed class EnumerationResponse
    {
        [DataMember(Name = "doNotCache", IsRequired = true)]
        public Boolean DoNotCache { get; set; }

        [DataMember(Name = "uniqueFilename", IsRequired = true)]
        public string UniqueFilename { get; set; }

        [DataMember(Name = "filename", IsRequired = true)]
        public string Filename { get; set; }

        [DataMember(Name = "hash", IsRequired = true)]
        public string Hash { get; set; }

        [DataMember(Name = "length", IsRequired = true)]
        public long Length { get; set; }

        [DataMember(Name = "uploaded", IsRequired = true)]
        public string Uploaded { get; set; }
    }

	/// <summary>
	/// Code signing
	/// </summary>
	[Help("NoSign", "Skips signing of code/content files.")]
	public class CodeSign
	{
		/// <summary>
		/// If so, what is the signing identity to search for?
		/// </summary>
		public static string SigningIdentity = "Epic Games";

		/// <summary>
		/// Should we use the machine store?
		/// </summary>
		public static bool bUseMachineStoreInsteadOfUserStore = false;

		/// <summary>
		/// How long to keep re-trying code signing for
		/// </summary>
		public static TimeSpan CodeSignTimeOut = new TimeSpan(0, 3, 0); // Keep trying to sign one file for up to 3 minutes

		/// <summary>
		/// Code signs the specified file
		/// </summary>
		public static void SignSingleExecutableIfEXEOrDLL(string Filename, bool bIgnoreExtension = false)
		{
            if (UnrealBuildTool.Utils.IsRunningOnMono)
            {
                CommandUtils.Log(TraceEventType.Information, String.Format("Can't sign '{0}', we are running under mono.", Filename));
                return;
            }
            if (!CommandUtils.FileExists(Filename))
			{
				throw new AutomationException("Can't sign '{0}', file does not exist.", Filename);
			}
			// Make sure the file isn't read-only
			FileInfo TargetFileInfo = new FileInfo(Filename);

			// Executable extensions
			List<string> Extensions = new List<string>();
			Extensions.Add(".dll");
			Extensions.Add(".exe");

			bool IsExecutable = bIgnoreExtension;

			foreach (var Ext in Extensions)
			{
				if (TargetFileInfo.FullName.EndsWith(Ext, StringComparison.InvariantCultureIgnoreCase))
				{
					IsExecutable = true;
					break;
				}
			}
			if (!IsExecutable)
			{
				CommandUtils.Log(TraceEventType.Verbose, String.Format("Won't sign '{0}', not an executable.", TargetFileInfo.FullName));
				return;
			}

			string SignToolName = null;
			if (WindowsPlatform.Compiler == WindowsCompiler.VisualStudio2013)
			{
				SignToolName = "C:/Program Files (x86)/Windows Kits/8.1/bin/x86/SignTool.exe";
			}
			else if (WindowsPlatform.Compiler == WindowsCompiler.VisualStudio2012)
			{
				SignToolName = "C:/Program Files (x86)/Windows Kits/8.0/bin/x86/SignTool.exe";
			}


			if (!File.Exists(SignToolName))
			{
				throw new AutomationException("SignTool not found at '{0}' (are you missing the Windows SDK?)", SignToolName);
			}

			TargetFileInfo.IsReadOnly = false;

			// Code sign the executable
			string TimestampServer = "http://timestamp.verisign.com/scripts/timestamp.dll";

			string SpecificStoreArg = bUseMachineStoreInsteadOfUserStore ? " /sm" : "";

			//@TODO: Verbosity choosing
			//  /v will spew lots of info
			//  /q does nothing on success and minimal output on failure
			string CodeSignArgs = String.Format("sign{0} /a /n \"{1}\" /t {2} /v {3}", SpecificStoreArg, SigningIdentity, TimestampServer, TargetFileInfo.FullName);

			DateTime StartTime = DateTime.Now;

			int NumTrials = 0;
			for (; ; )
			{
				ProcessResult Result = CommandUtils.Run(SignToolName, CodeSignArgs, null, CommandUtils.ERunOptions.AllowSpew);
				++NumTrials;

				if (Result.ExitCode != 1)
				{
					if (Result.ExitCode == 2)
					{
						CommandUtils.Log(TraceEventType.Error, String.Format("Signtool returned a warning."));
					}
					// Success!
					break;
				}
				else
				{
					// Keep retrying until we run out of time
					TimeSpan RunTime = DateTime.Now - StartTime;
					if (RunTime > CodeSignTimeOut)
					{
						throw new AutomationException("Failed to sign executable '{0}' {1} times over a period of {2}", TargetFileInfo.FullName, NumTrials, RunTime);
					}
				}
			}
		}

		/// <summary>
		/// Code signs the specified file or folder
		/// </summary>
		public static void SignMacFileOrFolder(string InPath, bool bIgnoreExtension = false)
		{
			bool bExists = CommandUtils.FileExists(InPath) || CommandUtils.DirectoryExists(InPath);
			if (!bExists)
			{
				throw new AutomationException("Can't sign '{0}', file or folder does not exist.", InPath);
			}

			// Executable extensions
			List<string> Extensions = new List<string>();
			Extensions.Add(".dylib");
			Extensions.Add(".app");

			bool IsExecutable = bIgnoreExtension || (Path.GetExtension(InPath) == "" && !InPath.EndsWith("PkgInfo"));

			foreach (var Ext in Extensions)
			{
				if (InPath.EndsWith(Ext, StringComparison.InvariantCultureIgnoreCase))
				{
					IsExecutable = true;
					break;
				}
			}
			if (!IsExecutable)
			{
				CommandUtils.Log(TraceEventType.Verbose, String.Format("Won't sign '{0}', not an executable.", InPath));
				return;
			}

			string SignToolName = "codesign";

			string CodeSignArgs = String.Format("-f --deep -s \"{0}\" -v \"{1}\"", "Developer ID Application", InPath);

			DateTime StartTime = DateTime.Now;

			int NumTrials = 0;
			for (; ; )
			{
				ProcessResult Result = CommandUtils.Run(SignToolName, CodeSignArgs, null, CommandUtils.ERunOptions.AllowSpew);
				int ExitCode = Result.ExitCode;
				++NumTrials;

				if (ExitCode == 0)
				{
					// Success!
					break;
				}
				else
				{
					// Keep retrying until we run out of time
					TimeSpan RunTime = DateTime.Now - StartTime;
					if (RunTime > CodeSignTimeOut)
					{
						throw new AutomationException("Failed to sign '{0}' {1} times over a period of {2}", InPath, NumTrials, RunTime);
					}
				}
			}
		}

		/// <summary>
		/// Codesigns multiple files, but skips anything that's not an EXE or DLL file
		/// Will automatically skip signing if -NoSign is specified in the command line.
		/// </summary>
		/// <param name="Files">List of files to sign</param>
		public static void SignMultipleIfEXEOrDLL(BuildCommand Command, List<string> Files)
		{
			if (!Command.ParseParam("NoSign"))
			{
				CommandUtils.Log("Signing up to {0} files...", Files.Count);
				UnrealBuildTool.UnrealTargetPlatform TargetPlatform = UnrealBuildTool.ExternalExecution.GetRuntimePlatform();
				if (TargetPlatform == UnrealBuildTool.UnrealTargetPlatform.Mac)
				{
					foreach (var File in Files)
					{
						SignMacFileOrFolder(File);
					}
				}
				else
				{
					foreach (var File in Files)
					{
						SignSingleExecutableIfEXEOrDLL(File);
					}
				}
			}
			else
			{
				CommandUtils.Log("Skipping signing {0} files due to -nosign.", Files.Count);
			}
		}
	}

}
