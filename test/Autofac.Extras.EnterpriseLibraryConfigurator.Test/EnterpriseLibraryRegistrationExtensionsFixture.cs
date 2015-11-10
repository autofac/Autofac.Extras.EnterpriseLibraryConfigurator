using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Extras.EnterpriseLibraryConfigurator;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel;
using Xunit;
using EntLibContainer = Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.Container;

namespace Autofac.Extras.EnterpriseLibraryConfiguratorEnterpriseLibraryConfigurator.Test
{
    public class EnterpriseLibraryRegistrationExtensionsFixture
    {
        [Fact]
        public void RegisterEnterpriseLibrary_NullConfigurationSource()
        {
            var builder = new ContainerBuilder();
            IConfigurationSource source = null;
            Assert.Throws<ArgumentNullException>(() => builder.RegisterEnterpriseLibrary(source));
        }

        [Fact]
        public void RegisterEnterpriseLibrary_NullContainerBuilder()
        {
            ContainerBuilder builder = null;
            var source = new DictionaryConfigurationSource();
            Assert.Throws<ArgumentNullException>(() => builder.RegisterEnterpriseLibrary());
            Assert.Throws<ArgumentNullException>(() => builder.RegisterEnterpriseLibrary(source));
        }

        [Fact]
        public void RegisterTypeRegistration_Default_NoParameters()
        {
            var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer());
            registration.IsDefault = true;
            registration.Lifetime = TypeRegistrationLifetime.Singleton;

            var builder = new ContainerBuilder();
            builder.RegisterTypeRegistration(registration);
            var container = builder.Build();

            var instance = container.Resolve<RegisteredServiceConsumer>();
            Assert.Equal("DEFAULTCTOR", instance.CtorParameter);
            var instance2 = container.Resolve<RegisteredServiceConsumer>();
            Assert.Same(instance, instance2);
        }

        [Fact]
        public void RegisterTypeRegistration_Default_WithEnumerationParameter()
        {
            var itemNames = new string[]
            {
                "first",
                "second",
                "third"
            };
            var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer(EntLibContainer.ResolvedEnumerable<ISampleService>(itemNames)));
            registration.IsDefault = true;
            registration.Lifetime = TypeRegistrationLifetime.Transient;

            var builder = new ContainerBuilder();
            builder.RegisterTypeRegistration(registration);
            var first = new SampleServiceImpl();
            builder.RegisterInstance(first).Named<ISampleService>("first");
            var second = new SampleServiceImpl();
            builder.RegisterInstance(second).Named<ISampleService>("second");
            var third = new SampleServiceImpl();
            builder.RegisterInstance(third).Named<ISampleService>("third");
            var container = builder.Build();

            var resolved = container.Resolve<RegisteredServiceConsumer>();
            Assert.IsAssignableFrom<IEnumerable<ISampleService>>(resolved.CtorParameter);
            var services = ((IEnumerable<ISampleService>)resolved.CtorParameter).ToArray();
            Assert.Same(first, services[0]);
            Assert.Same(second, services[1]);
            Assert.Same(third, services[2]);
        }

        [Fact]
        public void RegisterTypeRegistration_Default_WithSimpleParameter()
        {
            var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer("abc"));
            registration.IsDefault = true;
            registration.Lifetime = TypeRegistrationLifetime.Transient;

            var builder = new ContainerBuilder();
            builder.RegisterTypeRegistration(registration);
            var container = builder.Build();

            var instance = container.Resolve<RegisteredServiceConsumer>();
            Assert.Equal("abc", instance.CtorParameter);
            var instance2 = container.Resolve<RegisteredServiceConsumer>();
            Assert.NotSame(instance, instance2);
        }

        [Fact]
        public void RegisterTypeRegistration_Named_NoParameters()
        {
            var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer());
            registration.Name = "named-service";
            registration.Lifetime = TypeRegistrationLifetime.Singleton;

            var builder = new ContainerBuilder();
            builder.RegisterTypeRegistration(registration);
            var container = builder.Build();

            var instance = container.ResolveNamed<RegisteredServiceConsumer>("named-service");
            Assert.Equal("DEFAULTCTOR", instance.CtorParameter);
            var instance2 = container.ResolveNamed<RegisteredServiceConsumer>("named-service");
            Assert.Same(instance, instance2);
        }

        [Fact]
        public void RegisterTypeRegistration_Named_WithParameters()
        {
            var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer(EntLibContainer.Resolved<ISampleService>()));
            registration.Name = "named-service";
            registration.Lifetime = TypeRegistrationLifetime.Transient;

            var dependency = new SampleServiceImpl();
            var builder = new ContainerBuilder();
            builder.RegisterTypeRegistration(registration);
            builder.RegisterInstance(dependency).As<ISampleService>();
            var container = builder.Build();

            var instance = container.ResolveNamed<RegisteredServiceConsumer>("named-service");
            Assert.Same(dependency, instance.CtorParameter);
            var instance2 = container.ResolveNamed<RegisteredServiceConsumer>("named-service");
            Assert.NotSame(instance, instance2);
        }

        [Fact]
        public void RegisterTypeRegistration_NullContainerBuilder()
        {
            var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer("abc"));
            ContainerBuilder builder = null;
            Assert.Throws<ArgumentNullException>(() => builder.RegisterTypeRegistration(registration));
        }

        [Fact]
        public void RegisterTypeRegistration_NullTypeRegistration()
        {
            TypeRegistration registration = null;
            var builder = new ContainerBuilder();
            Assert.Throws<ArgumentNullException>(() => builder.RegisterTypeRegistration(registration));
        }

        [Fact]
        public void UsingConstructorFrom_MatchingConstructor()
        {
            var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer("abc"));
            var builder = new ContainerBuilder();
            var registrar = builder.RegisterType<RegisteredServiceConsumer>();
            registrar.UsingConstructorFrom(registration);
            builder.RegisterInstance("def").As<String>();
            var container = builder.Build();
            var instance = container.Resolve<RegisteredServiceConsumer>();
            Assert.Equal("def", instance.CtorParameter);
        }

        [Fact]
        public void UsingConstructorFrom_NoMatchingConstructor()
        {
            // There are no two-string constructors for RegisteredServiceConsumer.
            // The use case here is if someone selects UsingConstructorFrom for a
            // TypeRegistration that doesn't match the type being registered by Autofac.
            var registration = new TypeRegistration<ArgumentOutOfRangeException>(() => new ArgumentOutOfRangeException("paramName", "message"));
            var builder = new ContainerBuilder();
            var registrar = builder.RegisterType<RegisteredServiceConsumer>();
            Assert.Throws<ArgumentException>(() => registrar.UsingConstructorFrom(registration));
        }

        [Fact]
        public void UsingConstructorFrom_NullRegistrar()
        {
            var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer("abc"));
            IRegistrationBuilder<RegisteredServiceConsumer, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrar = null;
            Assert.Throws<ArgumentNullException>(() => registrar.UsingConstructorFrom(registration));
        }

        [Fact]
        public void UsingConstructorFrom_NullRegistration()
        {
            TypeRegistration<RegisteredServiceConsumer> registration = null;
            var builder = new ContainerBuilder();
            var registrar = builder.RegisterType<RegisteredServiceConsumer>();
            Assert.Throws<ArgumentNullException>(() => registrar.UsingConstructorFrom(registration));
        }

        [Theory]
        [InlineData(TypeRegistrationLifetime.Singleton, InstanceSharing.Shared, typeof(RootScopeLifetime))]
        [InlineData(TypeRegistrationLifetime.Transient, InstanceSharing.None, typeof(CurrentScopeLifetime))]
        public void WithInstanceScope_CheckLifetimeTranslation(TypeRegistrationLifetime entLibLifetime, InstanceSharing autofacInstanceSharing, Type autofacLifetimeType)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<RegisteredServiceConsumer>().WithInstanceScope(entLibLifetime);
            var container = builder.Build();

            IComponentRegistration autofacRegistration = null;
            Assert.True(container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(RegisteredServiceConsumer)), out autofacRegistration), "The service type was not registered into the container.");
            Assert.IsType(autofacLifetimeType, autofacRegistration.Lifetime);
            Assert.Equal(autofacInstanceSharing, autofacRegistration.Sharing);
        }

        [Fact]
        public void WithInstanceScope_NullRegistrar()
        {
            IRegistrationBuilder<RegisteredServiceConsumer, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrar = null;
            Assert.Throws<ArgumentNullException>(() => registrar.WithInstanceScope(TypeRegistrationLifetime.Singleton));
        }

        [Fact]
        public void WithParametersFrom_NoParameters()
        {
            var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer());
            var builder = new ContainerBuilder();
            builder
                .RegisterType<RegisteredServiceConsumer>()
                .Named<RegisteredServiceConsumer>("success")
                .UsingConstructor()
                .WithParametersFrom(registration);
            builder
                .RegisterType<RegisteredServiceConsumer>()
                .Named<RegisteredServiceConsumer>("fail")
                .UsingConstructor(typeof(string))
                .WithParametersFrom(registration);
            var container = builder.Build();

            container.ResolveNamed<RegisteredServiceConsumer>("success");
            Assert.Throws<DependencyResolutionException>(() => container.ResolveNamed<RegisteredServiceConsumer>("fail"));
        }

        [Fact]
        public void WithParametersFrom_NullRegistrar()
        {
            var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer("abc"));
            IRegistrationBuilder<RegisteredServiceConsumer, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrar = null;
            Assert.Throws<ArgumentNullException>(() => registrar.WithParametersFrom(registration));
        }

        [Fact]
        public void WithParametersFrom_NullRegistration()
        {
            TypeRegistration<RegisteredServiceConsumer> registration = null;
            var builder = new ContainerBuilder();
            var registrar = builder.RegisterType<RegisteredServiceConsumer>();
            Assert.Throws<ArgumentNullException>(() => registrar.WithParametersFrom(registration));
        }

        [Fact]
        public void WithParametersFrom_SimpleParameter()
        {
            var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer("abc"));
            var builder = new ContainerBuilder();
            builder
                .RegisterType<RegisteredServiceConsumer>()
                .Named<RegisteredServiceConsumer>("fail")
                .UsingConstructor(typeof(ISampleService))
                .WithParametersFrom(registration);
            builder
                .RegisterType<RegisteredServiceConsumer>()
                .Named<RegisteredServiceConsumer>("success")
                .UsingConstructor(typeof(string))
                .WithParametersFrom(registration);
            var container = builder.Build();

            var resolved = container.ResolveNamed<RegisteredServiceConsumer>("success");
            Assert.Equal("abc", resolved.CtorParameter);
            Assert.Throws<DependencyResolutionException>(() => container.ResolveNamed<RegisteredServiceConsumer>("fail"));
        }

        private interface ISampleService
        {
        }

        private class SampleServiceImpl : ISampleService
        {
        }

        private class RegisteredServiceConsumer
        {
            public object CtorParameter { get; private set; }

            public RegisteredServiceConsumer()
            {
                this.CtorParameter = "DEFAULTCTOR";
            }

            public RegisteredServiceConsumer(ISampleService service)
            {
                this.CtorParameter = service;
            }

            public RegisteredServiceConsumer(IEnumerable<ISampleService> services)
            {
                this.CtorParameter = services;
            }

            public RegisteredServiceConsumer(string input)
            {
                this.CtorParameter = input;
            }
        }
    }
}
