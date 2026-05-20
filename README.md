# Autofac.Extras.EnterpriseLibraryConfigurator

Microsoft Patterns and Practices Enterprise Library 5 support for using [Autofac](https://autofac.org) as the container.

[![Build status](https://ci.appveyor.com/api/projects/status/3o5xlwu9t8on7oue?svg=true)](https://ci.appveyor.com/project/Autofac/autofac-extras-enterpriselibraryconfigurator)

> :warning: **PROJECT ARCHIVED**: This package was put into maintenance-only mode in 2015 and was subsequently archived with no intent to continue any level of support in 2026.

[The `Autofac.Extras.EnterpriseLibraryConfigurator` package](https://www.nuget.org/packages/Autofac.Extras.EnterpriseLibraryConfigurator/) provides a way to use Autofac as the backing store for dependency injection in [Microsoft Enterprise Library 5](http://entlib.codeplex.com/releases/view/43135) instead of using Unity. It does this in conjunction with the Autofac Common Service Locator implementation.

> :warning: **In Enterprise Library 6, Microsoft removed the tightly-coupled dependency resolution mechanisms from the application blocks so there's no more need for this configurator past Enterprise Library 5.**

## Using the Configurator

The simplest way to use the configurator is to set up your Enterprise Library configuration in your `app.config` or `web.config` and use the `RegisterEnterpriseLibrary()` extension. This extension parses the configuration and performs the necessary registrations. You then need to set the ``nterpriseLibraryContainer.Current` to use an `AutofacServiceLocator` from the Autofac Common Service Locator implementation.

```csharp
var builder = new ContainerBuilder();
builder.RegisterEnterpriseLibrary();
var container = builder.Build();
var csl = new AutofacServiceLocator(container);
EnterpriseLibraryContainer.Current = csl;
```

## Specifying a Registration Source

The `RegisterEnterpriseLibrary()` extension does allow you to specify your own `IConfigurationSource` so if your configuration is not in `app.config` or `web.config` you can still use Autofac.

```csharp
var config = GetYourConfigurationSource();
var builder = new ContainerBuilder();
builder.RegisterEnterpriseLibrary(config);
var container = builder.Build();
var csl = new AutofacServiceLocator(container);
EnterpriseLibraryContainer.Current = csl;
```

## Example

There is an example project showing Enterprise Library 5 configuration along with the Exception Handling Block [in the Autofac examples repository](https://github.com/autofac/Examples/tree/v3.5.2/src/EnterpriseLibraryExample.MvcApplication).
