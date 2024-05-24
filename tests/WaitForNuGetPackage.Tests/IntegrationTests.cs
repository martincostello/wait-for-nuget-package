// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.WaitForNuGetPackage;

public static class IntegrationTests
{
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
        var actual = await Program.Main(["Polly", "--timeout", "00:00:01"]);

        // Assert
        actual.ShouldBe(2);
    }
}
