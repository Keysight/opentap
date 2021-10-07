﻿using OpenTap.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTap.Package
{
    [Browsable(false)]
    [Display("install", Group: "image")]
    internal class ImageInstallAction : IsolatedPackageAction
    {
        /// <summary>
        /// Path to Image file containing XML or JSON formatted Image specification
        /// </summary>
        [UnnamedCommandLineArgument("image")]
        public string ImagePath { get; set; }

        /// <summary>
        /// Option to merge with target installation. Default is false, which means overwrite installation
        /// </summary>
        [CommandLineArgument("merge")]
        public bool Merge { get; set; }

        /// <summary>
        /// Never prompt for user input.
        /// </summary>
        [CommandLineArgument("non-interactive", Description = "Never prompt for user input.")]
        public bool NonInteractive { get; set; } = false;

        protected override int LockedExecute(CancellationToken cancellationToken)
        {
            if (NonInteractive)
                UserInput.SetInterface(new NonInteractiveUserInputInterface());
            
            var imageString = File.ReadAllText(ImagePath);
            var image = ImageSpecifier.FromString(imageString);
            if (Merge)
            {
                var installation = new Installation(Target);
                image.OnResolve += args =>
                {
                    return installation.GetPackages().FirstOrDefault(p => p.Name == args.PackageSpecifier.Name && args.PackageSpecifier.Version.IsCompatible(p.Version));
                };
                image.Packages.AddRange(installation.GetPackages().Select(p => new PackageSpecifier(p)));
            }

            ImageIdentifier imageIdentifier = null;
            try
            {
                imageIdentifier = image.Resolve(cancellationToken);
            }
            catch (AggregateException e)
            {
                foreach (var innerException in e.InnerExceptions)
                    log.Error(innerException.Message);
                throw new ExitCodeException((int)PackageExitCodes.PackageDependencyError, "Resulting installation has package dependencies issues. Please fix existing installation and try again.");
            }
            
            imageIdentifier.Deploy(Target, cancellationToken);
            return 0;
        }
    }
}
