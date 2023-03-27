﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace BadMedicine.Dicom.Tests;

/// <summary>
/// Tests to confirm that the dependencies in csproj files (NuGet packages) match those in the .nuspec files and that packages.md 
/// lists the correct versions (in documentation)
/// </summary>
internal class NuspecIsCorrectTests
{
    private static readonly string[] Analyzers = new string[] { "SecurityCodeScan","Microsoft.SourceLink.GitHub" };

    [TestCase("../../../../BadMedicine.Dicom/BadMedicine.Dicom.csproj", null, "../../../../Packages.md")]
    [TestCase("../../../../BadDicom/BadDicom.csproj", null, "../../../../Packages.md")]
    [TestCase("../../../../BadMedicine.Dicom.Tests/BadMedicine.Dicom.Tests.csproj", null, "../../../../Packages.md")]
    public void TestDependencyCorrect(string csproj, string nuspec, string packagesMarkdown)
    {
        if(csproj != null && !Path.IsPathRooted(csproj))
            csproj = Path.Combine(TestContext.CurrentContext.TestDirectory,csproj);
        if(nuspec != null && !Path.IsPathRooted(nuspec))
            nuspec = Path.Combine(TestContext.CurrentContext.TestDirectory,nuspec);
        if(packagesMarkdown != null && !Path.IsPathRooted(packagesMarkdown))
            packagesMarkdown = Path.Combine(TestContext.CurrentContext.TestDirectory,packagesMarkdown);

        if (!File.Exists(csproj))
            Assert.Fail("Could not find file {0}", csproj);
        if (nuspec != null && !File.Exists(nuspec))
            Assert.Fail("Could not find file {0}", nuspec);

        if (packagesMarkdown != null && !File.Exists(packagesMarkdown))
            Assert.Fail("Could not find file {0}", packagesMarkdown);

        //<PackageReference Include="NUnit3TestAdapter" Version="3.13.0" />
        Regex rPackageRef = new(@"<PackageReference\s+Include=""(.*)""\s+Version=""([^""]*)""", RegexOptions.IgnoreCase);

        //<dependency id="CsvHelper" version="12.1.2" />
        Regex rDependencyRef = new(@"<dependency\s+id=""(.*)""\s+version=""([^""]*)""", RegexOptions.IgnoreCase);

        //For each dependency listed in the csproj
        foreach (Match p in rPackageRef.Matches(File.ReadAllText(csproj)))
        {
            var package = p.Groups[1].Value;
            var version = p.Groups[2].Value;

            var found = false;

            //analyzers do not have to be listed as a dependency in nuspec (but we should document them in packages.md)
            if (!Analyzers.Contains(package) && nuspec != null)
            {
                //make sure it appears in the nuspec
                foreach (Match d in rDependencyRef.Matches(File.ReadAllText(nuspec)))
                {
                    var packageDependency = d.Groups[1].Value;
                    var versionDependency = d.Groups[2].Value;

                    if (packageDependency.Equals(package))
                    {
                        Assert.AreEqual(version, versionDependency, "Package {0} is version {1} in {2} but version {3} in {4}", package, version, csproj, versionDependency, nuspec);
                        found = true;
                    }
                }

                if (!found)
                    Assert.Fail("Package {0} in {1} is not listed as a dependency of {2}. Recommended line is:\r\n{3}", package, csproj, nuspec,
                        BuildRecommendedDependencyLine(package, version));
            }


            //And make sure it appears in the packages.md file
            if (packagesMarkdown != null)
            {
                found = false;
                foreach (var line in File.ReadAllLines(packagesMarkdown))
                {
                    if (Regex.IsMatch(line, $@"[\s[]{Regex.Escape(package)}[\s\]]", RegexOptions.IgnoreCase))
                    {
                        var count = new Regex(Regex.Escape(version)).Matches(line).Count;

                        Assert.AreEqual(2, count, "Markdown file {0} did not contain 2 instances of the version {1} for package {2} in {3}", packagesMarkdown, version, package, csproj);
                        found = true;
                    }
                }

                if (!found)
                    Assert.Fail("Package {0} in {1} is not documented in {2}. Recommended line is:\r\n{3}", package, csproj, packagesMarkdown,
                        BuildRecommendedMarkdownLine(package, version));
            }
        }
    }

    private static object BuildRecommendedDependencyLine(string package, string version)
    {
        return $"<dependency id=\"{package}\" version=\"{version}\" />";
    }

    private static object BuildRecommendedMarkdownLine(string package, string version)
    {
        return
            $"| {package} | [GitHub]() | [{version}](https://www.nuget.org/packages/{package}/{version}) | | | |";
    }
}