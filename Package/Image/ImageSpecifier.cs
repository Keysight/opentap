﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;

namespace OpenTap.Package
{
    /// <summary>
    /// An <see cref="ImageSpecifier"/> defines an OpenTAP installation. The specifier can be resolved to an
    /// <see cref="ImageIdentifier"/> which can be deployed to an actual OpenTAP installation.
    /// </summary>
    public class ImageSpecifier
    {
        /// <summary>
        /// Optional name of the ImageSpecifier. Used for debugging purposes.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Desired packages in the installation
        /// </summary>
        public List<PackageSpecifier> Packages { get; set; } = new List<PackageSpecifier>();

        /// <summary> Installed specifiers. </summary>
        internal PackageSpecifier[] FixedPackages { get; set; } = Array.Empty<PackageSpecifier>();
        
        /// <summary> The installed packages. </summary>
        internal ImmutableArray<PackageDef> InstalledPackages { get; set; } = ImmutableArray<PackageDef>.Empty;

        /// <summary> Creates a new instance. </summary>
        public ImageSpecifier(List<PackageSpecifier> packages, string name = "")
        {
            Packages = packages;
            Name = name;
        }

        /// <summary> Creates a new instance. </summary>
        public ImageSpecifier()
        {
            
        }

        internal static ImageSpecifier FromAddedPackages(Installation installation, IEnumerable<PackageSpecifier> newPackages)
        {
            var toInstall = new List<PackageSpecifier>();
            var installed = installation.GetPackages().ToList();
            foreach (var package in newPackages)
            {
                var ext = installed.FirstOrDefault(x => x.Name == package.Name);
                if (ext != null)
                    installed.Remove(ext);

                if (File.Exists(package.Name))
                {
                    var package2 = PackageDef.FromPackage(package.Name);
                    toInstall.Add(new PackageSpecifier(package2.Name, package2.Version.AsExactSpecifier()));
                    installed.Add(package2);
                    continue;
                }
                toInstall.Add(package);
            }

            var fixedPackages = installed.Where(x => toInstall.Any(y => y.Name == x.Name) == false)
                .Select(x => new PackageSpecifier(x.Name, x.Version.AsCompatibleSpecifier(), x.Architecture, x.OS))
                .ToArray();

            toInstall.AddRange(fixedPackages);

            return new ImageSpecifier
            {
                Packages = toInstall,
                InstalledPackages = installed.ToImmutableArray(),
                FixedPackages = fixedPackages
            };
        }


        /// <summary>
        /// OpenTAP repositories to fetch the desired packages from
        /// These should be well formed URIs and will be interpreted relative to the BaseAddress set in AuthenticationSettings.
        /// </summary>
        public List<string> Repositories { get; set; } = new List<string>();

        internal List<PackageDef> AdditionalPackages { get; set; } = new List<PackageDef>();
        
        /// <summary> The OS this image specifier targets. </summary>
        public string OS { get; set; } = Installation.Current.OS;
        
        /// <summary> The CPU architecture that this image specifier targets. </summary>
        public CpuArchitecture Architecture { get; set; } = Installation.Current.Architecture;

        /// <summary>
        /// Resolve the desired packages from the specified repositories. This will check if the packages are available, compatible and can successfully be deployed as an OpenTAP installation
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ImageResolveException">The exception thrown if the image could not be resolved</exception>
        public ImageIdentifier Resolve(CancellationToken cancellationToken)
        {
            List<IPackageRepository> repositories = Repositories.Distinct().Select(PackageRepositoryHelpers.DetermineRepositoryType).GroupBy(p => p.Url).Select(g => g.First()).ToList();

            var cache = new PackageDependencyCache(OS, Architecture, Repositories);
            cache.LoadFromRepositories();
            cache.AddPackages(InstalledPackages);
            cache.AddPackages(AdditionalPackages);
            
            var resolver = new ImageResolver(cancellationToken);        
            var image = resolver.ResolveImage(this, cache.Graph);
            if (image.Success == false)
            {
                var unsatisfiedDependencies = InstalledPackages.Where(x => false == x.Dependencies.All(dep =>
                    InstalledPackages.Any(x2 =>
                        x2.Name == dep.Name && dep.Version.IsSatisfiedBy(x2.Version.AsExactSpecifier())))).ToArray();
                if (unsatisfiedDependencies.Any())
                {
                    throw new ImageResolveException(image,
                        string.Format("This is probably due to the current following package dependencies being broken: {0}",
                        string.Join(", ", unsatisfiedDependencies.Select(x => x.Name))));
                }
                throw new ImageResolveException(image);
            }

            var packages = image.Packages.Select(x => cache.GetPackageDef(x)).ToArray();
            if (packages.Any(x => x == null))
                throw new InvalidOperationException("Unable to lookup resolved package");
            ImageIdentifier image2 = new ImageIdentifier(packages, repositories.Select(s => s.Url));

            return image2;
        }

        /// <summary>
        /// Merges and resolves the packages for a number of images. May throw an exception if the packages cannot be resolved.
        /// </summary>
        /// <param name="images">The images to merge.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation before time. This will cause an OperationCancelledException to be thrown.</param>
        /// <returns></returns>
        /// <exception cref="ImageResolveException">The exception thrown if the image could not be resolved</exception>
        public static ImageIdentifier MergeAndResolve(IEnumerable<ImageSpecifier> images, CancellationToken cancellationToken)
        {
            
            var img = new ImageSpecifier(images.SelectMany(x =>x.Packages).Distinct().ToList());
            img.Repositories = images.SelectMany(x => x.Repositories).Distinct().ToList();
            return img.Resolve(cancellationToken);
        }

        /// <summary>
        /// Resolve specified packages in the ImageSpecifier with respect to the target installation.
        /// Specified packages will take precedence over already installed packages
        /// Already installed packages, which are not specified in the imagespecifier, will remain installed.
        /// </summary>
        /// <param name="deploymentInstallation">OpenTAP installation to merge with and deploy to.</param>
        /// <param name="cancellationToken">Standard CancellationToken</param>
        /// <returns>A new Installation</returns>
        /// <exception cref="ImageResolveException">In case of resolve errors, this method will throw ImageResolveExceptions.</exception>
        public Installation MergeAndDeploy(Installation deploymentInstallation, CancellationToken cancellationToken)
        {
            var imageSpecifier2 = FromAddedPackages(deploymentInstallation, Packages);
            imageSpecifier2.Name = Name;
            if(imageSpecifier2.Repositories?.Any() != true)
                imageSpecifier2.Repositories = Repositories;
            imageSpecifier2.OS = OS;
            imageSpecifier2.Architecture = Architecture;
            
            
            var image = imageSpecifier2.Resolve(cancellationToken);
            
            image.Deploy(deploymentInstallation.Directory, cancellationToken);
            return new Installation(deploymentInstallation.Directory);
        }

        /// <summary>
        /// Create an <see cref="ImageSpecifier"/> from JSON or XML value. Throws <see cref="InvalidOperationException"/> if value is not valid JSON or XML
        /// </summary>
        /// <param name="value">JSON or XML formatted <see cref="ImageSpecifier"/></param>
        /// <returns>An <see cref="ImageSpecifier"/></returns>
        public static ImageSpecifier FromString(string value)
        {
            return ImageHelper.GetImageFromString(value);
        }
    }

    /// <summary>
    /// Exception thrown when ImageSpecifier.Resolve fails. The exception contains a dependency graph specified Dot notation.
    /// </summary>
    public class ImageResolveException : AggregateException
    {
        internal ImageResolveException(string dotGraph, string message, List<Exception> dependencyIssues) : base(message, dependencyIssues)
        {
            DotGraph = dotGraph;
        }
        internal ImageResolveException(ImageResolution result) : base(result.ToString())
        {
            this.Result = result;
        }
        
        internal ImageResolveException(ImageResolution result, string message) : base(message)
        {
            this.Result = result;
        }

        internal ImageResolution Result;

        /// <summary>
        /// Dependency graph specified in Dot notation
        /// </summary>
        public string DotGraph { get; private set; }
    }
}