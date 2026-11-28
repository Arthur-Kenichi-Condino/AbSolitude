/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.IO;
using Unity.CodeEditor;
using IOPath = System.IO.Path;

namespace Microsoft.Unity.VisualStudio.Editor
{
	internal interface IVisualStudioInstallation
	{
		string Path { get; }
		bool SupportsAnalyzers { get; }
		string[] GetAnalyzers();
#if !UNITY_MSBUILD_ENABLED
		Version LatestLanguageVersionSupported { get; }
#endif
		CodeEditor.Installation ToCodeEditorInstallation();
		bool Open(string path, int line, int column, string solutionPath);
		void CreateExtraFiles(string projectDirectory);
#if !UNITY_MSBUILD_ENABLED
		IGenerator ProjectGenerator { get; }
#endif
	}

	internal abstract class VisualStudioInstallation : IVisualStudioInstallation
	{
		public string Name { get; set; }
		public string Path { get; set; }
		public Version Version { get; set; }
		public bool IsPrerelease { get; set; }

		public abstract bool SupportsAnalyzers { get; }
		public abstract string[] GetAnalyzers();
		public abstract void CreateExtraFiles(string projectDirectory);
#if !UNITY_MSBUILD_ENABLED
		public abstract Version LatestLanguageVersionSupported { get; }
		public abstract IGenerator ProjectGenerator { get; }
#endif
		public abstract bool Open(string path, int line, int column, string solutionPath);

#if !UNITY_MSBUILD_ENABLED
		protected Version GetLatestLanguageVersionSupported(VersionPair[] versions)
		{
			if (versions != null)
			{
				foreach (var entry in versions)
				{
					if (Version >= entry.IdeVersion)
						return entry.LanguageVersion;
				}
			}

			// default to 7.0
			return new Version(7, 0);
		}
#endif

		protected static string[] GetAnalyzers(string path)
		{
			var analyzersDirectory = FileUtility.GetAbsolutePath(IOPath.Combine(path, "Analyzers"));

			if (Directory.Exists(analyzersDirectory))
				return Directory.GetFiles(analyzersDirectory, "*Analyzers.dll", SearchOption.AllDirectories);

			return Array.Empty<string>();
		}

		public CodeEditor.Installation ToCodeEditorInstallation()
		{
			return new CodeEditor.Installation() { Name = Name, Path = Path };
		}
	}
}
