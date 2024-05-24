// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Spectre.Console.Cli;

namespace MartinCostello.WaitForNuGetPackage;

internal class TimeoutInterceptor(CancellationTokenSource cancellationTokenSource) : ICommandInterceptor
{
    public void Intercept(CommandContext context, CommandSettings settings)
    {
        if (settings is WaitCommandSettings { Timeout: not null } waitSettings)
        {
            cancellationTokenSource.CancelAfter(waitSettings.Timeout.Value);
        }
    }
}
