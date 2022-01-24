﻿using OpenTap.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTap.Image.Tests
{
    public class MockRepository : IPackageRepository
    {
        public string Url { get; private set; }
        readonly List<PackageDef> AllPackages;
        internal int ResolveCount = 0;


        public MockRepository(string url)
        {
            Url = url;
            AllPackages = new List<PackageDef>
                {
                    DefinePackage("OpenTAP","8.8.0"),
                    DefinePackage("OpenTAP","9.10.0"),
                    DefinePackage("OpenTAP","9.10.1"),
                    DefinePackage("OpenTAP","9.11.0"),
                    DefinePackage("OpenTAP","9.11.1"),
                    DefinePackage("OpenTAP","9.12.0"),
                    DefinePackage("OpenTAP","9.12.1"),
                    DefinePackage("OpenTAP","9.13.0"),
                    DefinePackage("OpenTAP","9.13.1"),
                    DefinePackage("OpenTAP","9.13.2-beta.1"),
                    DefinePackage("OpenTAP","9.13.2"),
                    DefinePackage("OpenTAP","9.14.0"),
                    DefinePackage("Demonstration",  "9.0.0", CpuArchitecture.AnyCPU, "windows", ("OpenTAP", "^9.9.0")),
                    DefinePackage("Demonstration",  "9.0.1", CpuArchitecture.AnyCPU, "windows", ("OpenTAP", "^9.10.0")),
                    DefinePackage("Demonstration",  "9.0.2", CpuArchitecture.AnyCPU, "windows", ("OpenTAP", "^9.11.0")),
                    DefinePackage("Demonstration",  "9.1.0", CpuArchitecture.AnyCPU, "windows", ("OpenTAP", "^9.12.0")),
                    DefinePackage("Demonstration",  "9.2.0", CpuArchitecture.AnyCPU, "windows", ("OpenTAP", "^9.12.0")),
                    DefinePackage("MyDemoTestPlan", "1.0.0", CpuArchitecture.AnyCPU, "windows", ("OpenTAP", "^9.12.1"), ("Demonstration", "^9.0.2")),
                    DefinePackage("MyDemoTestPlan", "1.1.0", CpuArchitecture.AnyCPU, "windows", ("OpenTAP", "^9.13.1"), ("Demonstration", "^9.0.2")),
                    DefinePackage("ExactDependency","1.0.0", CpuArchitecture.AnyCPU, "windows", ("OpenTAP", "9.13.1")),
                    DefinePackage("Cyclic",         "1.0.0", CpuArchitecture.AnyCPU, "windows", ("Cyclic2", "1.0.0")),
                    DefinePackage("Cyclic2",        "1.0.0", CpuArchitecture.AnyCPU, "windows", ("Cyclic", "1.0.0")),
                    DefinePackage("Native",         "1.0.0", CpuArchitecture.x86,    "windows"),
                    DefinePackage("Native",         "1.0.0", CpuArchitecture.x64,    "windows"),
                    DefinePackage("Native",         "1.0.0", CpuArchitecture.x86,    "linux"),
                    DefinePackage("Native",         "1.0.0", CpuArchitecture.x64,    "linux"),
                    DefinePackage("Native2",        "1.0.0", CpuArchitecture.x86,    "windows"),
                    DefinePackage("Native2",        "1.0.0", CpuArchitecture.x64,    "windows"),
                    DefinePackage("Native2",        "1.0.0", CpuArchitecture.x86,    "linux"),
                    DefinePackage("Native2",        "1.0.0", CpuArchitecture.x64,    "linux"),
                };
        }

        private PackageDef DefinePackage(string name, string version, CpuArchitecture arch = CpuArchitecture.AnyCPU, string os = "windows", params (string name, string version)[] dependencies)
        {
            return new PackageDef
            {
                Name = name,
                Version = SemanticVersion.Parse(version),
                Architecture = arch,
                OS = os,
                Dependencies = dependencies.Select(d => new PackageDependency(d.name, VersionSpecifier.Parse(d.version))).ToList()
            };
        }

        public PackageDef[] CheckForUpdates(IPackageIdentifier[] packages, CancellationToken cancellationToken)
        {
            return Array.Empty<PackageDef>();
        }

        public void DownloadPackage(IPackageIdentifier package, string destination, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public string[] GetPackageNames(CancellationToken cancellationToken, params IPackageIdentifier[] compatibleWith)
        {
            ResolveCount++;
            return AllPackages.Select(p => p.Name).Distinct().ToArray();
        }
        public PackageDef[] GetPackages(PackageSpecifier package, CancellationToken cancellationToken, params IPackageIdentifier[] compatibleWith)
        {
            ResolveCount++;
            var list = AllPackages.Where(p => p.Name == package.Name)
                              .GroupBy(p => p.Version)
                              .OrderByDescending(g => g.Key).ToList();
            return list.FirstOrDefault(g => package.Version.IsCompatible(g.Key)).ToArray();
        }

        public PackageVersion[] GetPackageVersions(string packageName, CancellationToken cancellationToken, params IPackageIdentifier[] compatibleWith)
        {
            ResolveCount++;
            return AllPackages.Where(p => p.Name == packageName)
                              .Select(p => new PackageVersion(p.Name, p.Version, p.OS, p.Architecture, p.Date, new List<string>()))
                              .OrderByDescending(p => p.Version)
                              .ToArray();
        }
    }

}
