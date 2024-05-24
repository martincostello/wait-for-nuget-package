// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Spectre.Console.Cli;

namespace MartinCostello.WaitForNuGetPackage;

//// See https://github.com/spectreconsole/spectre.console/blob/0e2ed511a5cfa303ba99c97ebb3f36c50cfa526f/examples/Cli/Injection/Infrastructure/TypeResolver.cs

internal sealed class TypeResolver(IServiceProvider serviceProvider) : ITypeResolver, IDisposable
{
    public object? Resolve(Type? type)
    {
        if (type is null)
        {
            return null;
        }

        return serviceProvider.GetService(type);
    }

    public void Dispose()
    {
        if (serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
