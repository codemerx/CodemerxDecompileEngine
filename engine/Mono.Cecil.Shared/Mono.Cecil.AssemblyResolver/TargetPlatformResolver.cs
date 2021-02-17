using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mono.Cecil.Extensions;

namespace Mono.Cecil.AssemblyResolver
{
	/*Telerik Authorship*/
	public class TargetPlatformResolver : ITargetPlatformResolver
	{
        private readonly Dictionary<AssemblyName, TargetPlatform> Mscorlibs = new Dictionary<AssemblyName, TargetPlatform>()
        {
            // .NET 4.0, 4.5, 4.5.1, 4.5.2
            { new AssemblyName("mscorlib", "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", new Version("4.0.0.0"), new byte[] { 183, 122, 92, 86, 25, 52, 224, 137 }) { TargetArchitecture = TargetArchitecture.I386 }, TargetPlatform.CLR_4 },
            { new AssemblyName("mscorlib", "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", new Version("4.0.0.0"), new byte[] { 183, 122, 92, 86, 25, 52, 224, 137 }) { TargetArchitecture = TargetArchitecture.AMD64 }, TargetPlatform.CLR_4 },
            // .NET 2.0, 3.0, 3.5
            { new AssemblyName("mscorlib", "mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", new Version("2.0.0.0"), new byte[] { 183, 122, 92, 86, 25, 52, 224, 137 }) { TargetArchitecture = TargetArchitecture.I386 }, TargetPlatform.CLR_2_3 },
            { new AssemblyName("mscorlib", "mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", new Version("2.0.0.0"), new byte[] { 183, 122, 92, 86, 25, 52, 224, 137 }) { TargetArchitecture = TargetArchitecture.AMD64 }, TargetPlatform.CLR_2_3 },
            // Silverlight
            { new AssemblyName("mscorlib", "mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", new Version("2.0.5.0"), new byte[] { 124, 236, 133, 215, 190, 167, 121, 142 }) { TargetArchitecture = TargetArchitecture.I386 }, TargetPlatform.Silverlight },
            { new AssemblyName("mscorlib", "mscorlib, Version=5.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", new Version("5.0.5.0"), new byte[] { 124, 236, 133, 215, 190, 167, 121, 142 }) { TargetArchitecture = TargetArchitecture.I386 }, TargetPlatform.Silverlight },
            // Windows Phone
            { new AssemblyName("mscorlib", "mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", new Version("2.0.5.0"), new byte[] { 124, 236, 133, 215, 190, 167, 121, 142 }) { TargetArchitecture = TargetArchitecture.AnyCPU }, TargetPlatform.WindowsPhone },
            { new AssemblyName("mscorlib", "mscorlib, Version=5.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", new Version("5.0.5.0"), new byte[] { 124, 236, 133, 215, 190, 167, 121, 142 }) { TargetArchitecture = TargetArchitecture.AnyCPU }, TargetPlatform.WindowsPhone },
            // .NET Compact Framework
            { new AssemblyName("mscorlib", "mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=969db8053d3322ac", new Version("2.0.0.0"), new byte[] { 150, 157, 184, 5, 61, 51, 34, 172 }) { TargetArchitecture = TargetArchitecture.AnyCPU }, TargetPlatform.WindowsCE },
            { new AssemblyName("mscorlib", "mscorlib, Version=3.5.0.0, Culture=neutral, PublicKeyToken=969db8053d3322ac", new Version("3.5.0.0"), new byte[] { 150, 157, 184, 5, 61, 51, 34, 172 }) { TargetArchitecture = TargetArchitecture.AnyCPU }, TargetPlatform.WindowsCE },
            // .NET 1, 1.1
            { new AssemblyName("mscorlib", "mscorlib, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", new Version("1.0.3300.0"), new byte[] { 183, 122, 92, 86, 25, 52, 224, 137 }) { TargetArchitecture = TargetArchitecture.AnyCPU }, TargetPlatform.CLR_1 },
            { new AssemblyName("mscorlib", "mscorlib, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", new Version("1.0.5000.0"), new byte[] { 183, 122, 92, 86, 25, 52, 224, 137 }) { TargetArchitecture = TargetArchitecture.AnyCPU }, TargetPlatform.CLR_1 }
        };

        private static ITargetPlatformResolver instance;

        private AssemblyFrameworkResolverCache assemblyFrameworkResolverCache;

        protected TargetPlatformResolver(AssemblyFrameworkResolverCache assemblyFrameworkResolverCache)
        {
            this.assemblyFrameworkResolverCache = assemblyFrameworkResolverCache;
        }

		public static ITargetPlatformResolver Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new TargetPlatformResolver(new AssemblyFrameworkResolverCache());
				}

				return instance;
			}
		}

        public AssemblyFrameworkResolverCache ResolverCache
        {
            get
            {
                return this.assemblyFrameworkResolverCache;
            }
        }

        protected virtual TargetPlatform DefaultTargetPlatform
        {
            get
            {
                return TargetPlatform.None;
            }
        }

        public void ClearCache()
        {
            this.assemblyFrameworkResolverCache.Clear();
        }

        public bool IsCLR4Assembly(ModuleDefinition module)
        {
            TargetPlatform targetPlatform = this.GetTargetPlatform(module.FilePath, module);

            return targetPlatform == TargetPlatform.CLR_4 ? true : false;
        }

        public TargetPlatform GetTargetPlatform(string assemblyFilePath, IAssemblyResolver assemblyResolver)
        {
            TargetPlatform targetPlatform = this.DefaultTargetPlatform;

            if (string.IsNullOrEmpty(assemblyFilePath))
            {
                return targetPlatform;
            }

            if (this.assemblyFrameworkResolverCache.AssemblyPathToTargetPlatform.ContainsKey(assemblyFilePath))
            {
                return this.assemblyFrameworkResolverCache.AssemblyPathToTargetPlatform[assemblyFilePath];
            }
            else
            {
                ModuleDefinition module = AssemblyDefinition.ReadAssembly(assemblyFilePath, new ReaderParameters(assemblyResolver)).MainModule;

                return this.GetTargetPlatform(assemblyFilePath, module);
            }
        }

        public TargetPlatform GetTargetPlatform(string assemblyFilePath, ModuleDefinition moduleDef)
        {
            TargetPlatform targetPlatform = this.DefaultTargetPlatform;

            if (string.IsNullOrEmpty(assemblyFilePath))
            {
                return targetPlatform;
            }

            if (this.assemblyFrameworkResolverCache.AssemblyPathToTargetPlatform.ContainsKey(assemblyFilePath))
            {
                return this.assemblyFrameworkResolverCache.AssemblyPathToTargetPlatform[assemblyFilePath];
            }
            else
            {
                Task<TargetPlatform> targetPlatformFromDepsJson = null;

                string moduleLocation = moduleDef.FullyQualifiedName ?? moduleDef.FilePath;
                string depsJsonLocation = Directory.GetFiles(Path.GetDirectoryName(moduleLocation), "*.deps.json").FirstOrDefault();

                if (File.Exists(depsJsonLocation))
                {
                    targetPlatformFromDepsJson = Task.Factory.StartNew(() =>
                    {
                        return this.GetPlatformTargetFromDepsJson(moduleLocation);
                    });
                }

                ///*Telerik Authorship*/
                if (moduleDef.Assembly != null)
                {
                    targetPlatform = this.GetTargetPlatform(moduleDef);

                    if (targetPlatform != this.DefaultTargetPlatform)
                    {
                        this.assemblyFrameworkResolverCache.AddAssemblyTargetPlatformToCache(assemblyFilePath, targetPlatform);
                        return targetPlatform;
                    }
                }

                AssemblyNameReference msCorlib = moduleDef.AssemblyReferences.FirstOrDefault(a => a.Name == "mscorlib");

                if (msCorlib == null)
                {
                    AssemblyNameReference systemRuntimeReference = moduleDef.AssemblyReferences.FirstOrDefault(x => x.Name == "System.Runtime");
                    targetPlatform = this.GetTargetPlatform(systemRuntimeReference);

                    if (targetPlatform != this.DefaultTargetPlatform)
                    {
                        this.assemblyFrameworkResolverCache.AddAssemblyTargetPlatformToCache(assemblyFilePath, targetPlatform);
                        this.assemblyFrameworkResolverCache.AddAssemblySystemRuntimeReference(assemblyFilePath, systemRuntimeReference);

                        return targetPlatform;
                    }

                    // the next line is only to keep the old functionality
                    msCorlib = moduleDef.Assembly.Name;
                }

                if (moduleDef.Assembly != null && moduleDef.Assembly.Name.IsWindowsRuntime || msCorlib.IsFakeMscorlibReference())
                {
                    this.assemblyFrameworkResolverCache.AddAssemblyTargetPlatformToCache(assemblyFilePath, TargetPlatform.WinRT);
                    return TargetPlatform.WinRT;
                }

                /*AssemblyName assemblyName = new AssemblyName(msCorlib.Name,
                                                    msCorlib.FullName,
                                                    msCorlib.Version,
                                                    msCorlib.PublicKeyToken,
                                                    Path.GetDirectoryName(assemliyFilePath)) { TargetArchitecture = moduleDef.GetModuleArchitecture() };
                IEnumerable<string> foundPaths = GetAssemblyPaths(assemblyName);

                return GetTargetPlatform(foundPaths.FirstOrDefault());*/

                /*Telerik Authorship*/
                if (targetPlatformFromDepsJson != null)
                {
                    targetPlatformFromDepsJson.Wait();
                    targetPlatform = targetPlatformFromDepsJson.Result;

                    if (targetPlatform != this.DefaultTargetPlatform)
                    {
                        this.assemblyFrameworkResolverCache.AddAssemblyTargetPlatformToCache(assemblyFilePath, targetPlatform);
                        return targetPlatform;
                    }
                }

                /*Telerik Authorship*/
                TargetArchitecture moduleArchitecture = moduleDef.GetModuleArchitecture();
                /*Telerik Authorship*/
                foreach (KeyValuePair<AssemblyName, TargetPlatform> pair in Mscorlibs)
                {
                    if (AssemblyNameComparer.AreVersionEquals(pair.Key.Version, msCorlib.Version) &&
                        AssemblyNameComparer.ArePublicKeyEquals(pair.Key.PublicKeyToken, msCorlib.PublicKeyToken) &&
                        moduleArchitecture.CanReference(pair.Key.TargetArchitecture))
                    {
                        this.assemblyFrameworkResolverCache.AddAssemblyTargetPlatformToCache(assemblyFilePath, pair.Value);
                        return pair.Value;
                    }
                }

                /*Telerik Authorship*/
                this.assemblyFrameworkResolverCache.AddAssemblyTargetPlatformToCache(assemblyFilePath, this.DefaultTargetPlatform);
                return this.DefaultTargetPlatform;
            }
        }

        public void AddPartCacheResult(IEnumerable<string> resultPaths, TargetPlatform runtime)
        {
            foreach (string resultPath in resultPaths)
            {
                if (!string.IsNullOrEmpty(resultPath) && !this.assemblyFrameworkResolverCache.AssemblyPathToTargetPlatform.ContainsKey(resultPath))
                {
                    this.assemblyFrameworkResolverCache.AssemblyPathToTargetPlatform[resultPath] = runtime;
                }
            }
        }

        public TargetPlatform GetTargetPlatform(ModuleDefinition module)
        {
            TargetPlatform targetPlatform = this.DefaultTargetPlatform;

            if (module.Assembly != null && !string.IsNullOrEmpty(module.Assembly.TargetFrameworkAttributeValue))
            {
                targetPlatform = this.GetTargetPlatform(module.Assembly.TargetFrameworkAttributeValue);

                if (targetPlatform != this.DefaultTargetPlatform)
                {
                    return targetPlatform;
                }
            }

            string moduleLocation = module.FullyQualifiedName ?? module.FilePath;
            targetPlatform = this.GetPlatformTargetFromModuleLocation(module, moduleLocation);

            if (targetPlatform != this.DefaultTargetPlatform)
            {
                return targetPlatform;
            }

            return this.DefaultTargetPlatform;
        }

        protected TargetPlatform GetTargetPlatform(string targetFrameworkAttributeValue)
        {
            FrameworkName targetFrameworkAttribute = new FrameworkName(targetFrameworkAttributeValue);

            return this.GetTargetPlatformFromFrameworkAttribute(targetFrameworkAttribute);
        }

        protected TargetPlatform GetTargetPlatform(AssemblyNameReference systemRuntimeReference)
        {
            if (systemRuntimeReference != null && systemRuntimeReference.Version != SystemInformation.DefaultAssemblyVersion)
            {
                if (systemRuntimeReference.Version.Major == 4 && (systemRuntimeReference.Version.Minor == 1 || systemRuntimeReference.Version.Minor == 2))
                {
                    return TargetPlatform.NetCore;
                }
            }

            return this.DefaultTargetPlatform;
        }

        protected TargetPlatform GetTargetPlatformFromFrameworkAttribute(FrameworkName targetFrameworkAttribute)
        {
            if (targetFrameworkAttribute.Identifier == ".NETCoreApp")
            {
                return TargetPlatform.NetCore;
            }
            /* AGPL */
            else if (targetFrameworkAttribute.Identifier == ".NETStandard")
            {
                return TargetPlatform.NetStandard;
            }
            /* End AGPL */
            else if (targetFrameworkAttribute.Identifier == ".NETFramework" && targetFrameworkAttribute.Version.Major == 4)
            {
                return TargetPlatform.CLR_4;
            }
            else if (targetFrameworkAttribute.Identifier == "Silverlight")
            {
                if (targetFrameworkAttribute.Profile == "WindowsPhone")
                {
                    return TargetPlatform.WindowsPhone;
                }

                return TargetPlatform.Silverlight;
            }
            else if (targetFrameworkAttribute.Identifier == "MonoAndroid" || targetFrameworkAttribute.Identifier == "Xamarin.iOS")
            {
                return TargetPlatform.Xamarin;
            }
            else if (targetFrameworkAttribute.Identifier == "WindowsPhoneApp" || targetFrameworkAttribute.Identifier == ".NETPortable")
            {
                return TargetPlatform.WinRT;
            }

            return this.DefaultTargetPlatform;
        }

        protected TargetPlatform GetPlatformTargetFromModuleLocation(ModuleDefinition module, string moduleLocation)
        {
            if (moduleLocation != null)
            {
                moduleLocation = moduleLocation.ToLowerInvariant();

                if (SystemInformation.IsInNetCoreSharedAssembliesDir(moduleLocation))
                {
                    return TargetPlatform.NetCore;
                }
            }

            return this.DefaultTargetPlatform;
        }

        protected TargetPlatform GetPlatformTargetFromDepsJson(string moduleLocation)
        {
            try
            {
                if (moduleLocation != null)
                {
                    string depsJsonLocation = Directory.GetFiles(Path.GetDirectoryName(moduleLocation), "*.deps.json").FirstOrDefault();

                    if (!string.IsNullOrEmpty(depsJsonLocation))
                    {
                        string depsJsonContent = File.ReadAllText(depsJsonLocation);
                        string platformString = Regex.Match(depsJsonContent, @"runtimeTarget(.*?)name"": ""(.*?)""", RegexOptions.Singleline).Groups[2].Value;

                        return this.TryFormatTargetFrameworkAttribute(platformString, moduleLocation);
                    }
                }
            }
            catch
            {
            }

            return this.DefaultTargetPlatform;
        }

        private TargetPlatform TryFormatTargetFrameworkAttribute(string platformString, string moduleLocation)
        {
            try
            {
                string formattedPlatformString = platformString.Split('/').FirstOrDefault();

                if (formattedPlatformString != null)
                {
                    FrameworkName frameworkName = new FrameworkName(formattedPlatformString);

                    lock (this.assemblyFrameworkResolverCache)
                    {
                        this.assemblyFrameworkResolverCache.AssemblyPathToFrameworkName.Add(moduleLocation, frameworkName);
                    }

                    return this.GetTargetPlatformFromFrameworkAttribute(frameworkName);
                }
            }
            catch
            {
            }

            return this.DefaultTargetPlatform;
        }
    }
}
