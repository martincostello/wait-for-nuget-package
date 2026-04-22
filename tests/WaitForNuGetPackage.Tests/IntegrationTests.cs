// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.WaitForNuGetPackage;

public static class IntegrationTests
{
    [Theory]
    [InlineData("Polly@7.2.3")]
    [InlineData("Polly@8.4.0")]
    public static async Task Main_Returns_Zero_If_Package_Is_Published(string package)
    {
        // Act
        var actual = await Program.Main([package]);

        // Assert
        actual.ShouldBe(0);
    }

    [Fact]
    public static async Task Main_Returns_Zero_If_Package_Names_Are_Published()
    {
        // Act
        var actual = await Program.Main(["Polly@8.0.0", "Polly.Core@8.0.0"]);

        // Assert
        actual.ShouldBe(0);
    }

    [Fact]
    public static async Task Main_Returns_Zero_If_Package_Files_Are_Published()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var packageName = "Polly.Core";
        var packageVersion = "8.6.6";

        var tempDir = Directory.CreateTempSubdirectory();
        var packageFile = Path.Combine(tempDir.FullName, $"{packageName}.{packageVersion}.nupkg");
        var packageUrl = $"https://www.nuget.org/api/v2/package/{packageName}/{packageVersion}";

        using var client = new HttpClient();
        var packageBytes = await client.GetByteArrayAsync(packageUrl, cancellationToken);

        await File.WriteAllBytesAsync(packageFile, packageBytes, cancellationToken);

        // Act
        var actual = await Program.Main(["--file", packageFile]);

        // Assert
        actual.ShouldBe(0);
    }

    [Fact]
    public static async Task Main_Returns_Minus_One_If_No_Arguments()
    {
        // Act
        var actual = await Program.Main([]);

        // Assert
        actual.ShouldBe(-1);
    }

    [Fact]
    public static async Task Main_Returns_Minus_One_If_Invalid_Package()
    {
        // Act
        var actual = await Program.Main(["Polly@"]);

        // Assert
        actual.ShouldBe(-1);
    }

    [Fact]
    public static async Task Main_Returns_Minus_One_If_Invalid_Package_Version()
    {
        // Act
        var actual = await Program.Main(["Polly@abc"]);

        // Assert
        actual.ShouldBe(-1);
    }

    [Fact]
    public static async Task Main_Returns_Minus_One_If_Invalid_Package_Directory()
    {
        // Act
        var actual = await Program.Main(["--directory", "::::::"]);

        // Assert
        actual.ShouldBe(-1);
    }

    [Fact]
    public static async Task Main_Returns_Minus_One_If_Invalid_Package_File()
    {
        // Act
        var actual = await Program.Main(["--file", "::::::"]);

        // Assert
        actual.ShouldBe(-1);
    }

    [Fact]
    public static async Task Main_Returns_Minus_One_If_Package_Directory_Not_Found()
    {
        // Act
        var actual = await Program.Main(["--directory", Guid.NewGuid().ToString()]);

        // Assert
        actual.ShouldBe(-1);
    }

    [Fact]
    public static async Task Main_Returns_Minus_One_If_Package_File_Not_Found()
    {
        // Act
        var actual = await Program.Main(["--file", Guid.NewGuid().ToString()]);

        // Assert
        actual.ShouldBe(-1);
    }

    [Fact]
    public static async Task Main_Returns_Minus_One_If_No_Packages_Found_In_Directory()
    {
        // Arrange
        var tempDir = Directory.CreateTempSubdirectory();

        // Act
        var actual = await Program.Main(["--directory", tempDir.FullName]);

        // Assert
        actual.ShouldBe(-1);
    }

    [Theory]
    [InlineData("\\")]
    [InlineData("   ")]
    [InlineData("/index.html")]
    public static async Task Main_Returns_Minus_One_If_Invalid_Service_Index_Url(string serviceIndex)
    {
        // Act
        var actual = await Program.Main(["Polly", "--service-index", serviceIndex]);

        // Assert
        actual.ShouldBe(-1);
    }

    [Theory]
    [InlineData("foo")]
    [InlineData("-00:00:01")]
    public static async Task Main_Returns_Minus_One_If_Invalid_Since(string since)
    {
        // Act
        var actual = await Program.Main(["Polly", "--since", since]);

        // Assert
        actual.ShouldBe(-1);
    }

    [Theory]
    [InlineData("foo")]
    [InlineData("-00:00:01")]
    [InlineData("00:00:00")]
    public static async Task Main_Returns_Minus_One_If_Invalid_Timeout(string timeout)
    {
        // Act
        var actual = await Program.Main(["Polly", "--timeout", timeout]);

        // Assert
        actual.ShouldBe(-1);
    }

    [Fact]
    public static async Task Main_Returns_Two_If_Timeout()
    {
        // Act
        var actual = await Program.Main(["Polly", "--service-index", "https://api.nuget.org/v3/index.json", "--timeout", "00:00:01"]);

        // Assert
        actual.ShouldBe(2);
    }
}
