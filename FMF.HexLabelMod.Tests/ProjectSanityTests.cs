using System.Xml.Linq;

namespace FMF.HexLabelMod.Tests;

public class ProjectSanityTests
{
    private static readonly string ProjectFile = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "FMF.HexLabelMod.csproj"));

    [Fact]
    public void ProjectFile_Exists()
    {
        Assert.True(File.Exists(ProjectFile), "Expected project file at: {ProjectFile}");
    }

    [Fact]
    public void ProjectFile_Defines_TargetFramework()
    {
        var document = XDocument.Load(ProjectFile);
        var hasTargetFramework = document.Descendants().Any(node => node.Name.LocalName is "TargetFramework" or "TargetFrameworks");
        Assert.True(hasTargetFramework, "Expected TargetFramework or TargetFrameworks in project file.");
    }

    [Fact]
    public void ProjectFile_References_GregCore_FrikaMF()
    {
        var document = XDocument.Load(ProjectFile);
        var hasFrika = document.Descendants()
            .Any(n => n.Name.LocalName == "ProjectReference"
                && (n.Attribute("Include")?.Value.Contains("FrikaMF.csproj", StringComparison.OrdinalIgnoreCase) ?? false));
        Assert.True(hasFrika, "Expected ProjectReference to gregCore/framework/FrikaMF.csproj.");
    }
}

