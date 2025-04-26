using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Shouldly;

namespace Tubes.UnitTests;

public class PipelineTests
{
    [Fact]
    public void It_should_register_the_filter()
    {
        // Arrange
        var filters = new List<IFilter<string>>();
        var pipeline = new Pipeline<string>(filters);
        var filter = new Mock<IFilter<string>>().Object;

        // Act
        var result = pipeline.Register(filter);

        // Assert
        result.ShouldBeSameAs(pipeline);
        filters.ShouldContain(filter);
    }

    [Fact]
    public void It_should_throw_an_exception_for_a_null_filter()
    {
        // Arrange
        var pipeline = new Pipeline<string>();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => pipeline.Register(null!)).ParamName.ShouldBe("filter");
    }

    [Fact]
    public void It_should_throw_an_exception_for_a_null_message()
    {
        // Arrange
        var pipeline = new Pipeline<string>();
        var filter = new Mock<IFilter<string>>().Object;
        pipeline.Register(filter);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => pipeline.Execute(null!)).ParamName.ShouldBe("message");
    }

    [Fact]
    public void It_should_execute_all_the_filters()
    {
        // Arrange
        var pipeline = new Pipeline<string>();
        var callCount = 0;

        var filter1 = new Mock<IFilter<string>>();
        filter1.Setup(f => f.Execute(It.IsAny<string>())).Callback(() => callCount++);

        var filter2 = new Mock<IFilter<string>>();
        filter2.Setup(f => f.Execute(It.IsAny<string>())).Callback(() => callCount++);

        pipeline.Register(filter1.Object).Register(filter2.Object);

        // Act
        pipeline.Execute("test");

        // Assert
        callCount.ShouldBe(2);
        filter1.Verify(f => f.Execute("test"), Times.Once);
        filter2.Verify(f => f.Execute("test"), Times.Once);
    }

    [Fact]
    public void It_should_execute_the_filters_in_the_sequence_they_were_registered()
    {
        // Arrange
        var pipeline = new Pipeline<string>();
        var executionOrder = new List<int>();

        var filter1 = new Mock<IFilter<string>>();
        filter1.Setup(f => f.Execute(It.IsAny<string>())).Callback(() => executionOrder.Add(1));

        var filter2 = new Mock<IFilter<string>>();
        filter2.Setup(f => f.Execute(It.IsAny<string>())).Callback(() => executionOrder.Add(2));

        pipeline.Register(filter1.Object).Register(filter2.Object);

        // Act
        pipeline.Execute("test");

        // Assert
        executionOrder.ShouldBe(new[] { 1, 2 });
    }

    [Fact]
    public void It_should_stop_processing_the_filters_when_stop_is_set_to_true()
    {
        // Arrange
        var message = new TestMessage { Stop = false };
        var callCount = 0;

        var pipeline = new Pipeline<TestMessage>();

        var filter1 = new Mock<IFilter<TestMessage>>();
        filter1.Setup(f => f.Execute(It.IsAny<TestMessage>())).Callback(() => callCount++);

        var filter2 = new Mock<IFilter<TestMessage>>();
        filter2.Setup(f => f.Execute(It.IsAny<TestMessage>())).Callback(() => message.Stop = true);

        var filter3 = new Mock<IFilter<TestMessage>>();
        filter3.Setup(f => f.Execute(It.IsAny<TestMessage>())).Callback(() => callCount++);

        pipeline.Register(filter1.Object).Register(filter2.Object).Register(filter3.Object);

        // Act
        pipeline.Execute(message);

        // Assert
        callCount.ShouldBe(1);
        message.Stop.ShouldBeTrue();
        filter1.Verify(f => f.Execute(message), Times.Once);
        filter2.Verify(f => f.Execute(message), Times.Once);
        filter3.Verify(f => f.Execute(message), Times.Never);
    }

    [Fact]
    public void It_should_resolve_the_filter_using_the_service_provider()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<TestFilter>();
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        // Act & Assert
        var pipeline = new Pipeline<TestMessage>(serviceProvider);
        pipeline.Register<TestFilter>(); //does not throw
    }
    
    [Fact]
    public void It_should_throw_an_exception_if_the_pipeline_was_constructed_without_a_service_provider()
    {
        // Arrange, Act & Assert
        var pipeline = new Pipeline<TestMessage>();
        Assert.Throws<Exception>(() => pipeline.Register<TestFilter>())
            .Message.ShouldBe("Service provider is not available. Did you construct the pipeline with a service provider?");
    }
    
    [Fact]
    public void It_should_throw_an_exception_when_the_filter_is_not_registered_with_the_service_provider()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        // Act & Assert
        var pipeline = new Pipeline<TestMessage>(serviceProvider);
        Assert.Throws<Exception>(() => pipeline.Register<TestFilter>())
            .Message.ShouldBe("Filter 'TestFilter' is not registered with the service provider.");
    }
    
    public class TestFilter : IFilter<TestMessage>
    {
        public void Execute(TestMessage message)
        {
        }
    }

    public class TestMessage : IStopProcessing
    {
        public bool Stop { get; set; }
    }
}