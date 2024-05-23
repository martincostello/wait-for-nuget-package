// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.WaitForNuGetPackage;

public static class IntegrationTests
{
    [Fact(Skip = "Needs implementing.")]
    public static async Task Application_Runs_With_No_Errors()
    {
        // Act
        var actual = await Program.Main(["Polly"]);

        // Assert
        actual.ShouldBe(0);
    }
}
