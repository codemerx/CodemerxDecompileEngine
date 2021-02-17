using System;
using Microsoft.Win32;
using System.IO;
/* AGPL */
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
/* End AGPL */

namespace Mono.Cecil.AssemblyResolver
{
    public static class SystemInformation
    {
        private static readonly string[] supportedExtensions = { ".dll", ".exe", ".winmd", ".xap", ".zip", ".appx" };

        private static readonly string[] resolvableExtensions = { ".dll", ".exe", ".winmd" };

        /* AGPL */
        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        /* End AGPL */

        public static readonly Version DefaultAssemblyVersion = new Version(0, 0, 0, 0);

        /* AGPL */
        public static string SILVERLIGHT_RUNTIME = IsWindows ? GetSilverlightRunTime(ProgramFilesX86) : null;

        public static string SILVERLIGHT_RUNTIME_64 = IsWindows ? GetSilverlightRunTime(ProgramW6432) : null;
        /* End AGPL */

        public static string SILVERLIGHT_DEFAULT = @"{0}\Reference Assemblies\Microsoft\Framework\Silverlight\{1}\{2}.dll";

        public static string SILVERLIGHT_SDK = @"{0}\Microsoft SDKs\Silverlight\{1}\Libraries\Client\{2}.dll";

        public static string COMPACT_FRAMEWORK = @"{0}\Microsoft.NET\SDK\CompactFramework\{1}\WindowsCE\{2}.dll";

        /* AGPL */
        public static string NETCORE_SHAREDASSEMBLIES_RELATIVE_PATH = Path.Combine("dotnet", "shared", "Microsoft.NETCore.App");
        /* End AGPL */

		public static string WINDOWS_PHONE_STRING_PATHERN = @"{0}\Reference Assemblies\Microsoft\Framework\Silverlight\{1}\Profile\WindowsPhone{2}\{3}.dll";

        public static string CLR = @"{0}\assembly\{1}\{2}\{3}\{4}{5}";

        public static string CLR_4 = @"{0}\Microsoft.NET\assembly\{1}\{2}\{3}\{4}{5}";

        public static string CLR_Default_32 = Path.Combine(WindowsPath, @"Microsoft.NET\Framework");

        public static string CLR_Default_64 = Path.Combine(WindowsPath, @"Microsoft.NET\Framework64");

        public static string WINRT_METADATA = Path.Combine(WindowsPath, @"System32\WinMetadata");

        private static string WIN_10_KITS = Path.Combine(ProgramFilesX86, @"Windows Kits\10");

        public static string UWP_REFERENCES = Path.Combine(WIN_10_KITS, "References");

        public static string UAP_PLATFORM = Path.Combine(WIN_10_KITS, @"Platforms\UAP");

        public static string WINDOWS_WINMD_LOCATION = Path.Combine(ProgramFilesX86, @"Windows Kits\8.0");

        public static string[] CLR_GAC_VERSIONS = new string[] { "GAC_64", "GAC_32", "GAC_MSIL", "GAC" };

        public static string[] WINDOWS_PHONE_VERSIONS = new string[] { "v3.0", "v4.0" };

        public static string[] WINDOWS_CE_VERSIONS = new string[] { "v2.0", "v3.5" };

        public static string[] SILVERLIGHT_VERSIONS = new string[] { "v4.0", "v5.0", "v3.0", "v2.0" };

        /* AGPL */
		public static string[] NETCORE_VERSIONS = new string[] { "1.0", "1.1", "2.0", "2.1", "2.2", "3.0", "3.1", "5.0" };
        /* End AGPL */

        public static readonly string[] CoreAssemblies = { 
                                                              "mscorlib.dll",
                                                              "system.dll",
                                                              "system.Xml.dll",
                                                              "system.Data.dll",
                                                              "system.Web.dll",
                                                              "system.Drawing.dll",
                                                              "system.Windows.Forms.dll"
                                                          };

        public static readonly string[] SystemAssemblies30 = { 
                                                                 @"Reference Assemblies\Microsoft\Framework\v3.0\System.ServiceModel.dll",
                                                                 @"Reference Assemblies\Microsoft\Framework\v3.0\System.Workflow.ComponentModel.dll",
                                                                 @"Reference Assemblies\Microsoft\Framework\v3.0\System.Workflow.Runtime.dll",
                                                                 @"Reference Assemblies\Microsoft\Framework\v3.0\System.Workflow.Activities.dll",

                                                                 @"Reference Assemblies\Microsoft\Framework\v3.0\System.IdentityModel.dll",
                                                                 @"Reference Assemblies\Microsoft\Framework\v3.0\System.IdentityModel.Selectors.dll",
                                                                 @"Reference Assemblies\Microsoft\Framework\v3.0\System.IO.Log.dll",
                                                                 @"Reference Assemblies\Microsoft\Framework\v3.0\System.Runtime.Serialization.dll",
                                                             };

        public static readonly string[] SystemAssemblies35 = { 
                                                                   @"Reference Assemblies\Microsoft\Framework\v3.5\System.Core.dll",
                                                                   @"Reference Assemblies\Microsoft\Framework\v3.5\System.Xml.Linq.dll",
                                                                   @"Reference Assemblies\Microsoft\Framework\v3.5\System.Data.DataSetExtensions.dll",
                                                                   @"Reference Assemblies\Microsoft\Framework\v3.0\WindowsBase.dll",
                                                                   @"Reference Assemblies\Microsoft\Framework\v3.0\PresentationCore.dll",
                                                                   @"Reference Assemblies\Microsoft\Framework\v3.0\PresentationFramework.dll",
                                                             };

        public static string WindowsPath
        {
            get
            {
                /* AGPL */
                if (IsWindows)
                {
                    return Environment.GetEnvironmentVariable("windir") ?? Environment.GetEnvironmentVariable("SystemRoot");
                }

                return string.Empty;
                /* End AGPL */
            }
        }

        public static string ProgramFilesX86
        {
            get
            {
                /* AGPL */
                if (IsWindows)
                {
                    return Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? Environment.GetEnvironmentVariable("ProgramFiles");
                }

                return string.Empty;
                /* End AGPL */
            }
        }

        public static string ProgramW6432
        {
            get
            {
                /* AGPL */
                if (IsWindows)
                {
                    return Environment.GetEnvironmentVariable("ProgramW6432") ?? string.Empty;
                }

                return string.Empty;
                /* End AGPL */
            }
        }

        public static bool IsInNetCoreSharedAssembliesDir(string assemblyPath)
		{
			assemblyPath = assemblyPath.ToLowerInvariant();

            /* AGPL */
            return GetNetCoreSharedAssemblyRootDirectories().Any(d => assemblyPath.StartsWith(d.ToLowerInvariant()));
            /* End AGPL */
        }

        /* AGPL */
        public static List<string> GetNetCoreSharedAssemblyDirectories(string targetVersion)
        {
            List<string> rootDirectories = GetNetCoreSharedAssemblyRootDirectories();
            List<string> versionSpecificDirectories = new List<string>();

            foreach (string rootDirectory in rootDirectories)
            {
                if (Directory.Exists(rootDirectory))
                {
                    versionSpecificDirectories.AddRange(Directory.GetDirectories(rootDirectory, targetVersion + "*"));
                }
            }

            return versionSpecificDirectories;
        }

        private static List<string> GetNetCoreSharedAssemblyRootDirectories()
        {
            List<string> directories = new List<string>();

            if (IsWindows)
            {
                directories.Add(Path.Combine(ProgramW6432, NETCORE_SHAREDASSEMBLIES_RELATIVE_PATH));
                directories.Add(Path.Combine(ProgramFilesX86, NETCORE_SHAREDASSEMBLIES_RELATIVE_PATH));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                directories.Add(Path.Combine("/", "usr", "share", NETCORE_SHAREDASSEMBLIES_RELATIVE_PATH));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                directories.Add(Path.Combine("/", "usr", "local", "share", NETCORE_SHAREDASSEMBLIES_RELATIVE_PATH));
            }

            return directories;
        }
        /* End AGPL */

        private static string GetSilverlightRunTime(string targetProgramFilesDir)
        {
            string runtimeLocation = targetProgramFilesDir + @"\Microsoft Silverlight\";
            string version = SilverlightVersion;
            if (version != null)
            {
                return runtimeLocation + version + "\\{0}.dll";
            }
            return runtimeLocation;
        }

        public static string SilverlightVersion
        {
            get
            {
                return (string)Registry.GetValue(Registry.LocalMachine.Name + @"\Software\Microsoft\Silverlight", "Version", null) ??
                    (string)Registry.GetValue(Registry.LocalMachine.Name + @"\Software\Wow6432Node\Microsoft\Silverlight", "Version", null);
            }
        }

        public static string WindowsPhonePath
        {
            get
            {
                return string.Format(@"{0}\Reference Assemblies\Microsoft\Framework\Silverlight\v4.0\Profile\WindowsPhone", ProgramFilesX86);
            }
        }

        public static string[] SupportedExtensions
        {
            get { return supportedExtensions; }
        }

        public static string[] ResolvableExtensions
        {
            get { return resolvableExtensions; }
        }
    }
}
