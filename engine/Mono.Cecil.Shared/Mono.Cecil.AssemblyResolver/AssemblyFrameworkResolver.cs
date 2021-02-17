using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Versioning;

namespace Mono.Cecil.AssemblyResolver
{
    public class AssemblyFrameworkResolver : IAssemblyFrameworkResolver
    {
        private static IAssemblyFrameworkResolver instance = null;

        private ITargetPlatformResolver targetPlatformResolver;

        private AssemblyFrameworkResolver(ITargetPlatformResolver targetPlatformResolver)
        {
            this.targetPlatformResolver = targetPlatformResolver;
        }

        public static IAssemblyFrameworkResolver Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AssemblyFrameworkResolver(TargetPlatformResolver.Instance);
                }

                return instance;
            }
        }

        public virtual FrameworkVersion DefaultFrameworkVersion
        {
            get
            {
                return FrameworkVersion.Unknown;
            }
        }

        public bool IsCLR4Assembly(ModuleDefinition module)
        {
            return this.targetPlatformResolver.IsCLR4Assembly(module);
        }

        public FrameworkVersion GetFrameworkVersionForModule(ModuleDefinition module)
        {
            // TODO: handle Silverlight/ WinPhone projects
            TargetPlatform platform = this.targetPlatformResolver.GetTargetPlatform(module.FilePath, module);
            FrameworkVersion assemblyFramework = this.DefaultFrameworkVersion;

            switch (platform)
            {
                case TargetPlatform.CLR_1:
                    return FrameworkVersion.v1_1;
                case TargetPlatform.CLR_2:
                    return FrameworkVersion.v2_0;
                case TargetPlatform.CLR_2_3:
                case TargetPlatform.CLR_3_5:
                    return FrameworkVersion.v3_5;
                case TargetPlatform.NetCore:
                    return GetFrameworkVersionInternal(module, TargetPlatform.NetCore);
                /* AGPL */
                case TargetPlatform.NetStandard:
                    return GetFrameworkVersionInternal(module, TargetPlatform.NetStandard);
                /* End AGPL */
                case TargetPlatform.WinRT:
                    return GetFrameworkVersionInternal(module, TargetPlatform.WinRT);
                case TargetPlatform.Xamarin:
                    return GetFrameworkVersionInternal(module, TargetPlatform.Xamarin);
                case TargetPlatform.Silverlight:
                    return FrameworkVersion.Silverlight;
                case TargetPlatform.WindowsCE:
                    return FrameworkVersion.WindowsCE;
                case TargetPlatform.WindowsPhone:
                    return FrameworkVersion.WindowsPhone;
                default:
                    return this.DefaultFrameworkVersion;
            }
        }

        private FrameworkVersion GetFrameworkVersionInternal(ModuleDefinition module, TargetPlatform targetPlatform)
        {
            FrameworkVersion frameworkVersion = this.DefaultFrameworkVersion;
            frameworkVersion = this.GetFrameworkVersionFromTargetFrameworkAttribute(module.Assembly.TargetFrameworkAttributeValue, targetPlatform);

            if (frameworkVersion != this.DefaultFrameworkVersion)
            {
                return frameworkVersion;
            }

            string moduleLocation = module.FullyQualifiedName ?? module.FilePath;

            frameworkVersion = this.GetFrameworkVersionThroughModuleLocation(module, targetPlatform);

            if (frameworkVersion != this.DefaultFrameworkVersion)
            {
                return frameworkVersion;
            }

            frameworkVersion = this.GetFrameworkVersionFromSystemRuntimeVersion(module, targetPlatform);

            if (frameworkVersion != this.DefaultFrameworkVersion)
            {
                return frameworkVersion;
            }

            if (this.targetPlatformResolver.ResolverCache.AssemblyPathToFrameworkName.ContainsKey(moduleLocation))
            {
                return this.GetFrameworkVersionFromFrameworkName(this.targetPlatformResolver.ResolverCache.AssemblyPathToFrameworkName[moduleLocation], targetPlatform);
            }
            else
            {
                Task<FrameworkVersion> frameworkVersionTask = Task.Factory.StartNew(() => { return this.GetFrameworkVersionFromDepsJson(moduleLocation, targetPlatform); });
                frameworkVersionTask.Wait();

                return frameworkVersionTask.Result;
            }
        }

        private FrameworkVersion GetFrameworkVersionFromTargetFrameworkAttribute(string targetFrameworkAttribute, TargetPlatform targetPlatform)
        {
            try
            {
                FrameworkName frameworkName = new FrameworkName(targetFrameworkAttribute);

                return this.GetFrameworkVersionFromFrameworkName(frameworkName, targetPlatform);
            }
            catch
            {
            }

            return this.DefaultFrameworkVersion;
        }

        private FrameworkVersion GetFrameworkVersionFromFrameworkName(FrameworkName frameworkName, TargetPlatform targetPlatform)
        {
            if (targetPlatform == TargetPlatform.WinRT)
            {
                if (frameworkName.Identifier == ".NETPortable")
                {
                    if (frameworkName.Version == new Version(4, 0))
                    {
                        return FrameworkVersion.NetPortableV4_0;
                    }
                    else if (frameworkName.Version == new Version(4, 6))
                    {
                        return FrameworkVersion.NetPortableV4_6;
                    }
                    else if (frameworkName.Version == new Version(4, 5))
                    {
                        return FrameworkVersion.NetPortableV4_5;
                    }
                    else if (frameworkName.Version == new Version(5, 0))
                    {
                        return FrameworkVersion.NetPortableV5_0;
                    }
                }
                else if (frameworkName.Identifier == ".NETCore")
                {
                    if (frameworkName.Version == new Version(4, 5))
                    {
                        return FrameworkVersion.WinRT_4_5;
                    }
                    else if (frameworkName.Version == new Version(4, 5, 1))
                    {
                        return FrameworkVersion.WinRT_4_5_1;
                    }
                    else if (frameworkName.Version == new Version(5, 0))
                    {
                        return FrameworkVersion.UWP;
                    }
                }
                else if (frameworkName.Identifier == "WindowsPhoneApp")
                {
                    return FrameworkVersion.WindowsPhone;
                }
            }
            else if (targetPlatform == TargetPlatform.NetCore)
            {
                if (frameworkName.Identifier == ".NETCoreApp")
                {
                    /* AGPL */
                    if (frameworkName.Version == new Version(5, 0))
                    {
                        return FrameworkVersion.NetCoreV5_0;
                    }
                    else if (frameworkName.Version == new Version(3, 1))
                    {
                        return FrameworkVersion.NetCoreV3_1;
                    }
                    else if (frameworkName.Version == new Version(3, 0))
                    {
                        return FrameworkVersion.NetCoreV3_0;
                    }
                    else if (frameworkName.Version == new Version(2, 2))
                    {
                        return FrameworkVersion.NetCoreV2_2;
                    }
                    /* End AGPL */
                    else if (frameworkName.Version == new Version(2, 1))
                    {
                        return FrameworkVersion.NetCoreV2_1;
                    }
                    else if (frameworkName.Version == new Version(2, 0))
                    {
                        return FrameworkVersion.NetCoreV2_0;
                    }
                    else if (frameworkName.Version == new Version(1, 1))
                    {
                        return FrameworkVersion.NetCoreV1_1;
                    }
                    else if (frameworkName.Version == new Version(1, 0))
                    {
                        return FrameworkVersion.NetCoreV1_0;
                    }
                }
            }
            /* AGPL */
            else if (targetPlatform == TargetPlatform.NetStandard)
            {
                if (frameworkName.Identifier == ".NETStandard")
                {
                    if (frameworkName.Version == new Version(2, 1))
                    {
                        return FrameworkVersion.NetStandardV2_1;
                    }
                    else if (frameworkName.Version == new Version(2, 0))
                    {
                        return FrameworkVersion.NetStandardV2_0;
                    }
                    else if (frameworkName.Version == new Version(1, 6))
                    {
                        return FrameworkVersion.NetStandardV1_6;
                    }
                    else if (frameworkName.Version == new Version(1, 5))
                    {
                        return FrameworkVersion.NetStandardV1_5;
                    }
                    else if (frameworkName.Version == new Version(1, 4))
                    {
                        return FrameworkVersion.NetStandardV1_4;
                    }
                    else if (frameworkName.Version == new Version(1, 3))
                    {
                        return FrameworkVersion.NetStandardV1_3;
                    }
                    else if (frameworkName.Version == new Version(1, 2))
                    {
                        return FrameworkVersion.NetStandardV1_2;
                    }
                    else if (frameworkName.Version == new Version(1, 1))
                    {
                        return FrameworkVersion.NetStandardV1_1;
                    }
                    else if (frameworkName.Version == new Version(1, 0))
                    {
                        return FrameworkVersion.NetStandardV1_0;
                    }
                }
            }
            /* End AGPL */
            else if (targetPlatform == TargetPlatform.Xamarin)
            {
                if (frameworkName.Identifier == "MonoAndroid")
                {
                    return FrameworkVersion.XamarinAndroid;
                }
                else if (frameworkName.Identifier == "Xamarin.iOS")
                {
                    return FrameworkVersion.XamarinIOS;
                }
            }

            return this.DefaultFrameworkVersion;
        }

        private FrameworkVersion GetFrameworkVersionFromSystemRuntimeVersion(ModuleDefinition module, TargetPlatform targetPlatform)
        {
            AssemblyNameReference systemRuntime = null;
            string moduleLocation = module.FullyQualifiedName ?? module.FilePath;

            if (this.targetPlatformResolver.ResolverCache.AssemblyPathToSystemRuntimeReference.ContainsKey(moduleLocation))
            {
                systemRuntime = this.targetPlatformResolver.ResolverCache.AssemblyPathToSystemRuntimeReference[moduleLocation];
            }
            else
            {
                systemRuntime = module.AssemblyReferences.FirstOrDefault(x => x.Name == "System.Runtime");
            }

            if (systemRuntime != null && systemRuntime.Version != SystemInformation.DefaultAssemblyVersion)
            {
                if (targetPlatform == TargetPlatform.NetCore && systemRuntime.Version.Major == 4 && systemRuntime.Version.Minor == 2)
                {
                    return FrameworkVersion.NetCoreV2_0;
                }
                else if (targetPlatform == TargetPlatform.WinRT && systemRuntime.Version == new Version(4, 0, 20, 0))
                {
                    return FrameworkVersion.UWP;
                }
            }

            return this.DefaultFrameworkVersion;
        }

        private FrameworkVersion GetFrameworkVersionFromDepsJson(string moduleLocation, TargetPlatform targetPlatform)
        {
            if (moduleLocation != null)
            {
                string depsJsonLocation = Directory.GetFiles(Path.GetDirectoryName(moduleLocation), "*.deps.json").FirstOrDefault();

                if (!string.IsNullOrEmpty(depsJsonLocation))
                {
                    string depsJsonContent = File.ReadAllText(depsJsonLocation);
                    string platformString = Regex.Match(depsJsonContent, @"runtimeTarget(.*?)name"": ""(.*?)""", RegexOptions.Singleline).Groups[2].Value;

                    return this.TryFormatTargetFrameworkAttribute(platformString, targetPlatform);
                }
            }

            return this.DefaultFrameworkVersion;
        }

        private FrameworkVersion TryFormatTargetFrameworkAttribute(string platformString, TargetPlatform targetPlatform)
        {
            try
            {
                string formattedPlatformString = platformString.Split('/').FirstOrDefault();

                if (formattedPlatformString != null)
                {
                    return this.GetFrameworkVersionFromTargetFrameworkAttribute(formattedPlatformString, targetPlatform);
                }
            }
            catch
            {
            }

            return this.DefaultFrameworkVersion;
        }

        private FrameworkVersion GetFrameworkVersionThroughModuleLocation(ModuleDefinition module, TargetPlatform targetPlatform)
        {
            FrameworkVersion frameworkVersion = this.DefaultFrameworkVersion;

            try
            {
                string moduleLocation = module.FullyQualifiedName ?? module.FilePath;

                if (moduleLocation != null)
                {
                    moduleLocation = moduleLocation.ToLowerInvariant();

                    /* AGPL */
                    string[] modulePath = moduleLocation.Split(Path.DirectorySeparatorChar);
                    /* End AGPL */

                    if (SystemInformation.IsInNetCoreSharedAssembliesDir(moduleLocation))
                    {
                        if (modulePath[modulePath.Length - 2].StartsWith("1.0"))
                        {
                            frameworkVersion = FrameworkVersion.NetCoreV1_0;
                        }
                        else if (modulePath[modulePath.Length - 2].StartsWith("1.1"))
                        {
                            frameworkVersion = FrameworkVersion.NetCoreV1_1;
                        }
                        else if (modulePath[modulePath.Length - 2].StartsWith("2.0"))
                        {
                            frameworkVersion = FrameworkVersion.NetCoreV2_0;
                        }
                        else if (modulePath[modulePath.Length - 2].StartsWith("2.1"))
                        {
                            frameworkVersion = FrameworkVersion.NetCoreV2_1;
                        }
                        /* AGPL */
                        else if (modulePath[modulePath.Length - 2].StartsWith("2.2"))
                        {
                            frameworkVersion = FrameworkVersion.NetCoreV2_2;
                        }
                        else if (modulePath[modulePath.Length - 2].StartsWith("3.0"))
                        {
                            frameworkVersion = FrameworkVersion.NetCoreV3_0;
                        }
                        else if (modulePath[modulePath.Length - 2].StartsWith("3.1"))
                        {
                            frameworkVersion = FrameworkVersion.NetCoreV3_1;
                        }
                        else if (modulePath[modulePath.Length - 2].StartsWith("5.0"))
                        {
                            frameworkVersion = FrameworkVersion.NetCoreV5_0;
                        }
                        /* End AGPL */
                    }
                    else if (moduleLocation.StartsWith(SystemInformation.WINRT_METADATA.ToLowerInvariant()) || moduleLocation.StartsWith(SystemInformation.WINDOWS_WINMD_LOCATION.ToLowerInvariant()))
                    {
                        frameworkVersion = FrameworkVersion.WinRT_System;
                    }
                }
            }
            catch
            {
            }

            return frameworkVersion;
        }
    }
}
