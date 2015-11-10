using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extras.EnterpriseLibraryConfigurator;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel;
using Xunit;

namespace Autofac.Extras.EnterpriseLibraryConfiguratorEnterpriseLibraryConfigurator.Test
{
    public class AutofacContainerConfiguratorFixture
    {
        [Fact]
        public void Ctor_NullBuilder()
        {
            Assert.Throws<ArgumentNullException>(() => new AutofacContainerConfigurator(null));
        }

        [Fact]
        public void RegisterAll_NullConfigurationSource()
        {
            var builder = new ContainerBuilder();
            var configurator = new AutofacContainerConfigurator(builder);
            var rootProvider = new StubRegistrationProvider();
            Assert.Throws<ArgumentNullException>(() => configurator.RegisterAll(null, rootProvider));
        }

        [Fact]
        public void RegisterAll_NullRegistrationProvider()
        {
            var builder = new ContainerBuilder();
            var configurator = new AutofacContainerConfigurator(builder);
            var configurationSource = new NullConfigurationSource();
            Assert.Throws<ArgumentNullException>(() => configurator.RegisterAll(configurationSource, null));
        }

        [Fact]
        public void RegisterAll_RegistersPlaceholderConfigurationChangeEventSource()
        {
            var container = this.ExecuteRegisterAllOnValidConfigurator();
            Assert.True(container.IsRegistered<ConfigurationChangeEventSource>(), "A ConfigurationChangeEventSource should have been found in the container.");
        }

        [Fact]
        public void RegisterAll_RegistersProvidedTypeRegistrations()
        {
            var container = this.ExecuteRegisterAllOnValidConfigurator();
            Assert.True(container.IsRegistered<ISampleService>(), "The provided registration was not added to the container.");
        }

        private IContainer ExecuteRegisterAllOnValidConfigurator()
        {
            var builder = new ContainerBuilder();
            var configurator = new AutofacContainerConfigurator(builder);
            var configurationSource = new NullConfigurationSource();
            var rootProvider = new StubRegistrationProvider();
            configurator.RegisterAll(configurationSource, rootProvider);
            return builder.Build();
        }

        private class StubRegistrationProvider : ITypeRegistrationsProvider
        {
            public IEnumerable<TypeRegistration> GetRegistrations(IConfigurationSource configurationSource)
            {
                yield return new TypeRegistration<ISampleService>(() => new SampleServiceImpl()) { IsDefault = true };
            }

            public IEnumerable<TypeRegistration> GetUpdatedRegistrations(IConfigurationSource configurationSource)
            {
                throw new NotImplementedException();
            }
        }

        private interface ISampleService
        {
        }

        private class SampleServiceImpl : ISampleService
        {
        }
    }
}
