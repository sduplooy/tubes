using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Tubes.UnitTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void It_should_throw_an_exception_when_services_is_null()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ServiceCollectionExtensions.AddFiltersFromAssemblyContaining<object>(null!))
            .ParamName.ShouldBe("services");
    }

    [Fact]
    public void It_should_register_filters_from_the_specified_assembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFiltersFromAssemblyContaining<ServiceCollectionExtensionsTests>();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IFilter<TestMessage>));
        serviceDescriptor.ShouldNotBeNull();
        serviceDescriptor.ImplementationType.ShouldBe(typeof(TestFilter));
        serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact]
    public void It_should_register_async_filters_from_the_specified_assembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFiltersFromAssemblyContaining<ServiceCollectionExtensionsTests>();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IAsyncFilter<TestMessage>));
        serviceDescriptor.ShouldNotBeNull();
        serviceDescriptor.ImplementationType.ShouldBe(typeof(TestAsyncFilter));
        serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact]
    public void It_should_respect_the_specified_service_lifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFiltersFromAssemblyContaining<ServiceCollectionExtensionsTests>(ServiceLifetime.Singleton);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IFilter<TestMessage>));
        serviceDescriptor.ShouldNotBeNull();
        serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    // Test helper classes
    private class TestMessage;

    private class TestFilter : IFilter<TestMessage>
    {
        public void Process(TestMessage message) { }
    }

    private class TestAsyncFilter : IAsyncFilter<TestMessage>
    {
        public Task ProcessAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}