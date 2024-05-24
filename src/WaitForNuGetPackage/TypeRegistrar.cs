// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace MartinCostello.WaitForNuGetPackage;

//// https://github.com/spectreconsole/spectre.console/blob/0e2ed511a5cfa303ba99c97ebb3f36c50cfa526f/examples/Cli/Injection/Infrastructure/TypeRegistrar.cs

internal sealed class TypeRegistrar(IServiceCollection services) : ITypeRegistrar
{
    public ITypeResolver Build()
        => new TypeResolver(services.BuildServiceProvider());

    public void Register(Type service, Type implementation)
        => services.AddSingleton(service, implementation);

    public void RegisterInstance(Type service, object implementation)
        => services.AddSingleton(service, implementation);

    public void RegisterLazy(Type service, Func<object> func)
        => services.AddSingleton(service, (provider) => func());
}
