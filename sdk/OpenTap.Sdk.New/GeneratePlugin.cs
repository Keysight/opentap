//            Copyright Keysight Technologies 2012-2019
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, you can obtain one at http://mozilla.org/MPL/2.0/.
using OpenTap;
using OpenTap.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace OpenTap.Sdk.New
{
    [Display("dut", "C# template for a DUT plugin.", Groups: new[] { "sdk", "new" })]
    public class GenerateDut : GenerateType
    {
        [UnnamedCommandLineArgument("name", Required = true)]
        public string Name { get; set; }

        public override int Execute(CancellationToken cancellationToken)
        {
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("OpenTap.Sdk.New.Resources.DutTemplate.txt")))
            {
                var content = ReplaceInTemplate(reader.ReadToEnd(), TryGetNamespace(), Name);
                WriteFile(output ?? Path.Combine(WorkingDirectory, Name + ".cs"), content);
            }

            return (int)ExitCodes.Success;

        }
    }
    [Display("instrument", "C# template for a Instrument plugin.", Groups: new[] { "sdk", "new" })]
    public class GenerateInstrument : GenerateType
    {
        [UnnamedCommandLineArgument("name", Required = true)]
        public string Name { get; set; }

        public override int Execute(CancellationToken cancellationToken)
        {
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("OpenTap.Sdk.New.Resources.InstrumentTemplate.txt")))
            {
                var content = ReplaceInTemplate(reader.ReadToEnd(), TryGetNamespace(), Name);
                WriteFile(output ?? Path.Combine(WorkingDirectory, Name + ".cs"), content);
            }

            return (int)ExitCodes.Success;
        }
    }
    [Display("resultlistener", "C# template for a ResultListener plugin.", Groups: new[] { "sdk", "new" })]
    public class GenerateResultListener : GenerateType
    {
        [UnnamedCommandLineArgument("name", Required = true)]
        public string Name { get; set; }

        public override int Execute(CancellationToken cancellationToken)
        {
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("OpenTap.Sdk.New.Resources.ResultListenerTemplate.txt")))
            {
                var content = ReplaceInTemplate(reader.ReadToEnd(), TryGetNamespace(), Name);
                WriteFile(output ?? Path.Combine(WorkingDirectory, Name + ".cs"), content);
            }

            return (int)ExitCodes.Success;
        }
    }
    [Display("settings", "C# template for a ComponentSetting plugin.", Groups: new[] { "sdk", "new" })]
    public class GenerateSetting : GenerateType
    {
        [UnnamedCommandLineArgument("name", Required = true)]
        public string Name { get; set; }

        public override int Execute(CancellationToken cancellationToken)
        {
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("OpenTap.Sdk.New.Resources.SettingsTemplate.txt")))
            {
                var content = ReplaceInTemplate(reader.ReadToEnd(), TryGetNamespace(), Name);
                WriteFile(output ?? Path.Combine(WorkingDirectory, Name + ".cs"), content);
            }

            return (int)ExitCodes.Success;
        }
    }
    [Display("teststep", "C# template for a TestStep plugin.", Groups: new[] { "sdk", "new" })]
    public class GenerateTestStep : GenerateType
    {
        [UnnamedCommandLineArgument("name", Required = true)]
        public string Name { get; set; }

        public override int Execute(CancellationToken cancellationToken)
        {
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("OpenTap.Sdk.New.Resources.TestStepTemplate.txt")))
            {
                var content = ReplaceInTemplate(reader.ReadToEnd(), TryGetNamespace(), Name);
                WriteFile(output ?? Path.Combine(WorkingDirectory, Name + ".cs"), content);
            }

            return (int)ExitCodes.Success;
        }
    }


    [Display("testplan", "Deprecated! Creates a TestPlan (.TapPlan) containing all TestSteps types defined in this project.", Groups: new[] { "sdk", "new" })]
    [Obsolete("Use an editor to create TestPlans instead")]
    [Browsable(false)]
    public class GenerateTestPlan : GenerateType
    {
        [UnnamedCommandLineArgument("name", Required = true)]
        public string Name { get; set; }

        public override int Execute(CancellationToken cancellationToken)
        {
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("OpenTap.Sdk.New.Resources.TapPlanTemplate.txt")))
            {
                StringBuilder steps = new StringBuilder("\n");
                var ns = TryGetNamespace();
                var csFiles = Directory.GetFiles(WorkingDirectory, "*.cs", SearchOption.TopDirectoryOnly);
                foreach (var file in csFiles)
                {
                    var text = File.ReadAllText(file);
                    var match = Regex.Match(text, "public class (.*?) : I?TestStep");
                    if (match.Success)
                        steps.AppendLine($"    <TestStep type=\"{ns}.{match.Groups[1].Value}\"></TestStep>");
                }

                var content = ReplaceInTemplate(reader.ReadToEnd(), steps.ToString());
                WriteFile(output ?? Path.Combine(WorkingDirectory, Name + ".TapPlan"), content);
            }

            log.Warning("This feature is obsoleted. Use an editor to create a testplan.");
            log.Warning("For more information, see https://doc.opentap.io/User%20Guide/Editors/");

            return (int)ExitCodes.Success;
        }
    }
    [Display("cliaction", "C# template for a CliAction plugin.", Groups: new[] { "sdk", "new" })]
    public class GenerateCliAction : GenerateType
    {
        [UnnamedCommandLineArgument("name", Required = true)]
        public string Name { get; set; }

        public override int Execute(CancellationToken cancellationToken)
        {
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("OpenTap.Sdk.New.Resources.CliActionTemplate.txt")))
            {
                var content = ReplaceInTemplate(reader.ReadToEnd(), TryGetNamespace(), Name);
                WriteFile(output ?? Path.Combine(WorkingDirectory, Name + ".cs"), content);
            }

            return (int)ExitCodes.Success;
        }
    }
    [Display("packagexml", "Package Definition file (package.xml).", Groups: new[] { "sdk", "new" })]
    public class GeneratePackageXml : GenerateType
    {
        [UnnamedCommandLineArgument("package name", Required = true)]
        public string Name { get; set; }

        public override int Execute(CancellationToken cancellationToken)
        {
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("OpenTap.Sdk.New.Resources.PackageXmlTemplate.txt")))
            {
                var content = ReplaceInTemplate(reader.ReadToEnd(), TryGetNamespace(), Name);
                WriteFile(output ?? Path.Combine(WorkingDirectory, "package.xml"), content);
            }

            return (int)ExitCodes.Success;
        }
    }

    // SerializerPlugin
}
