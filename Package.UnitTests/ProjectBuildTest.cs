﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace OpenTap.Package.UnitTests
{
    internal class CsProj
    {
        private string Filename => "test.csproj";
        public List<string> PropertyGroups { get; set; }
        public List<string> ItemGroups { get; set; }
        public List<string> Imports { get; set; }
        public ProjectBuildTest ProjectBuildTest { get; set; }

        public override string ToString()
        {

            return @"<Project Sdk=""Microsoft.NET.Sdk"" ToolsVersion=""Current"">" + '\n' +
                   string.Join("\n", PropertyGroups) + '\n' +
                   string.Join("\n", ItemGroups) + '\n' +
                   string.Join("\n", Imports) + '\n' +
                   "</Project>\n";
        }

        public CsProj(ProjectBuildTest caller)
        {
            ProjectBuildTest = caller;
            PropertyGroups = new List<string>()
            {
                @"                   
    <PropertyGroup>
        <TargetFrameworkIdentifier></TargetFrameworkIdentifier>
        <TargetFrameworkVersion></TargetFrameworkVersion>
        <TargetFramework>netstandard2.0</TargetFramework>
        <OutDir>bin</OutDir>
    </PropertyGroup>"
            };
            ItemGroups = new List<string>();
            Imports = new List<string>()
            {
                $@"
<Import Project=""{caller.TargetsFile}"" />"
            };
        }
        
        public (string Stdout, string Stderr, int ExitCode) Build()
        {
            using (var writer = new StreamWriter(Path.Combine(ProjectBuildTest.WorkingDirectory, Filename)))
                writer.Write(this.ToString());

            if (OperatingSystem.Current != OperatingSystem.Windows)
            {
                // // The MSBuild generator is sometimes too eager to skip steps on Linux -- make sure targets are run
                if (File.Exists(ProjectBuildTest.OutputFile))
                    File.Delete(ProjectBuildTest.OutputFile);
            }
            
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "dotnet",
                    Arguments = $"build {Filename} --verbosity normal",
                    WorkingDirectory = ProjectBuildTest.WorkingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var timeLimit = TimeSpan.FromSeconds(60);
            
            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            // For some reason, the process doesn't exit properly with WaitForExit
            while (process.HasExited == false)
            {
                if (DateTime.Now - process.StartTime > timeLimit)
                    break;
                
                TapThread.Sleep(TimeSpan.FromSeconds(1));
            }
            

            return (process.StandardOutput.ReadToEnd(), process.StandardError.ReadToEnd(), process.ExitCode);
        }
        
        public string GetGeneratedFileContent()
        {
            using (var reader = new StreamReader(ProjectBuildTest.OutputFile))
            {
                return reader.ReadToEnd();
            }
        }
    }

    
    public class ProjectBuildTest
    {
        // Current directory is assumed to be the OutputFolder containing tap.exe etc.
        public string WorkingDirectory { get; }
        public string OutputFile => Path.Combine(WorkingDirectory, "obj", "test.opentap.g.props");
        public string TargetsFile { get; }

        private static string FileRepository => Path.Combine(Directory.GetCurrentDirectory(), "TapPackages");
        

        public ProjectBuildTest()
        {
            WorkingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var packages = Directory.EnumerateFiles(FileRepository, "*.TapPackage").ToList();
            var str = string.Join("\n", packages);
            
            StringAssert.Contains("MyPlugin4", str);
            StringAssert.Contains("MyPlugin5", str);
            
            var targetsFile = new FileInfo("OpenTap.targets");
            if (targetsFile.Exists == false)
                throw new Exception(
                    $"OpenTap.targets not found in {Directory.GetCurrentDirectory()}. Tests cannot continue.");
            TargetsFile = targetsFile.FullName;

            var binDir = Path.Combine(WorkingDirectory, "bin");

            if (Directory.Exists(WorkingDirectory) == false)
                Directory.CreateDirectory(WorkingDirectory);
            if (Directory.Exists(binDir) == false)
                Directory.CreateDirectory(binDir);

            // We need to create a new OpenTAP installation because source builds behave a little differently from package installations regarding the package manager.
            // Since we are manually importing the targets file
            // '<Import Project=""..\OpenTap.targets"" />'
            // , which points at the file 'Keysight.OpenTap.Sdk.MSBuild.dll' in the output directory rather than a nuget package, we are still testing the correct files.
            if (Directory.EnumerateFiles(binDir, "OpenTap.dll").Any() == false)
                CreateOpenTapInstall(binDir);
        }

        private void CreateOpenTapInstall(string target)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "tap",
                    Arguments = $"package install OpenTAP -f -t {target}",
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }
        [Test]
        public void NoReferencesTest()
        {
            var result = new CsProj(this).Build();

            StringAssert.Contains("Got 0 OpenTapPackageReference targets.", result.Stdout);
            Assert.AreEqual(result.ExitCode, 0);
            Assert.False(File.Exists(OutputFile));
        }

        [Test]
        public void OneReferenceDefaultTest()
        {
            var csProj = new CsProj(this);
            csProj.ItemGroups.Add($@"
   <ItemGroup>
	  <OpenTapPackageReference Include=""MyPlugin4"" Repository=""{FileRepository}"" />	  
  </ItemGroup>
");
            csProj.ItemGroups.Add($@"
   <ItemGroup>
	  <OpenTapPackageReference Include=""MyPlugin4"" Repository=""{FileRepository}"" />	  
  </ItemGroup>
");
            var result = csProj.Build();

            StringAssert.Contains("Got 1 OpenTapPackageReference targets.", result.Stdout);
            StringAssert.Contains("MyPlugin4 Include=\"**\" Exclude=\"Dependencies/**\"", result.Stdout);
            StringAssert.Contains("Skipped duplicate entries", result.Stdout);
            StringAssert.Contains($@"<OpenTapPackageReference Include=""MyPlugin4"" Repository=""{FileRepository}"" />", result.Stdout);
            Assert.AreEqual(result.ExitCode, 0);

            Assert.True(File.Exists(OutputFile));
            var Generated = csProj.GetGeneratedFileContent();
            StringAssert.Contains("MyPlugin4.dll", Generated);
        }

        [Test]
        public void TwoEqualReferencesDefaultTest()
        {
            var csProj = new CsProj(this);
            csProj.ItemGroups.Add($@"
   <ItemGroup>
	  <OpenTapPackageReference Repository=""{FileRepository}"" Include=""MyPlugin4"" />	  
  </ItemGroup>
");

            csProj.ItemGroups.Add($@"
   <ItemGroup>
	  <OpenTapPackageReference Repository=""{FileRepository}"" Include=""MyPlugin5"" />	  
  </ItemGroup>
");

            var result = csProj.Build();

            StringAssert.Contains("Got 2 OpenTapPackageReference targets.", result.Stdout);
            StringAssert.Contains("MyPlugin4 Include=\"**\" Exclude=\"Dependencies/**\"", result.Stdout);
            StringAssert.Contains("MyPlugin5 Include=\"**\" Exclude=\"Dependencies/**\"", result.Stdout);
            Assert.AreEqual(result.ExitCode, 0);

            Assert.True(File.Exists(OutputFile));
            var Generated = csProj.GetGeneratedFileContent();
            StringAssert.Contains("MyPlugin4.dll", Generated);
            StringAssert.Contains("MyPlugin5.dll", Generated);
        }

        [Test]
        public void TwoEqualReferencesSpecifiedTest()
        {
            var csProj = new CsProj(this);
            csProj.ItemGroups.Add($@"
   <ItemGroup>
	  <OpenTapPackageReference Repository=""{FileRepository}"" Include=""MyPlugin4"" IncludeAssemblies=""test1;test2"" ExcludeAssemblies=""test3;test4"" />	  
  </ItemGroup>
");

            csProj.ItemGroups.Add($@"
   <ItemGroup>
	  <OpenTapPackageReference Repository=""{FileRepository}"" Include=""MyPlugin5"" IncludeAssemblies=""test1;test2"" ExcludeAssemblies=""test3;test4"" />	  
  </ItemGroup>
");

            var result = csProj.Build();

            StringAssert.Contains("Got 2 OpenTapPackageReference targets.", result.Stdout);
            StringAssert.Contains("MyPlugin4 Include=\"test1,test2\" Exclude=\"test3,test4\"", result.Stdout);
            StringAssert.Contains("MyPlugin5 Include=\"test1,test2\" Exclude=\"test3,test4\"", result.Stdout);
            Assert.AreEqual(result.ExitCode, 0);

            Assert.False(File.Exists(OutputFile));
        }

        [Test]
        public void TwoDifferentReferencesSpecifiedTest()
        {
            var csProj = new CsProj(this);
            csProj.ItemGroups.Add($@"
    <ItemGroup>
      <OpenTapPackageReference Include=""MyPlugin4"">    
        <Repository>{FileRepository}</Repository> 
        <IncludeAssemblies>test1;test2</IncludeAssemblies> 
        <ExcludeAssemblies>test3;test4</ExcludeAssemblies>
      </OpenTapPackageReference>
    </ItemGroup>
");
            csProj.ItemGroups.Add($@"
   <ItemGroup>
	  <OpenTapPackageReference Repository=""{FileRepository}"" Include=""MyPlugin5""> 
        <IncludeAssemblies>test5;test6</IncludeAssemblies> 
        <ExcludeAssemblies>test7;test8</ExcludeAssemblies>
      </OpenTapPackageReference>
  </ItemGroup>
");

            var result = csProj.Build();

            StringAssert.Contains("Got 2 OpenTapPackageReference targets.", result.Stdout);
            StringAssert.Contains("MyPlugin4 Include=\"test1,test2\" Exclude=\"test3,test4\"", result.Stdout);
            StringAssert.Contains("MyPlugin5 Include=\"test5,test6\" Exclude=\"test7,test8\"", result.Stdout);
            Assert.AreEqual(result.ExitCode, 0);

            Assert.False(File.Exists(OutputFile));
        }

        [Test]
        public void VSSDKTest()
        {
            // Package not available on Linux
            if (OperatingSystem.Current == OperatingSystem.Linux)
                return;
            var csProj = new CsProj(this);
            csProj.ItemGroups.Add($@"
    <ItemGroup>
<OpenTapPackageReference Include=""Visual Studio SDK"" Repository=""packages.opentap.io"" IncludeAssemblies=""**""  ExcludeAssemblies=""**/*Install**;**stdole**""/> 
    </ItemGroup>
            ");
            var result = csProj.Build();

            StringAssert.Contains("Got 1 OpenTapPackageReference targets.", result.Stdout);
            StringAssert.Contains(@"Visual Studio SDK Include=""**"" Exclude=""**/*Install**,**stdole**""",
                result.Stdout);

            Assert.AreEqual(result.ExitCode, 0);

            Assert.True(File.Exists(OutputFile));
            var Generated = csProj.GetGeneratedFileContent();

            StringAssert.DoesNotContain("OpenTap.VSSdk.Installer.dll", Generated);
            StringAssert.DoesNotContain("stdole.dll", Generated);

            StringAssert.Contains("OpenTap.VSSdk.Debugger.dll", Generated);
            StringAssert.Contains("EnvDTE.dll", Generated);
        }
    }
}