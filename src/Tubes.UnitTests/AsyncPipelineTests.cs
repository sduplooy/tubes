using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;

namespace Tubes.UnitTests;

public class AsyncPipelineTests
{
    [Fact]
    public void It_should_register_the_filter()
    {
        // Arrange
        var filters = new List<IAsyncFilter<string>>();
        var pipeline = new AsyncPipeline<string>(filters);
        var filter = new Mock<IAsyncFilter<string>>().Object;

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
        var pipeline = new AsyncPipeline<string>();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => pipeline.Register(null!)).ParamName.ShouldBe("filter");
    }

    [Fact]
    public async Task It_should_throw_an_exception_for_a_null_message()
    {
        // Arrange
        var pipeline = new AsyncPipeline<string>();
        var filter = new Mock<IAsyncFilter<string>>().Object;
        pipeline.Register(filter);

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentNullException>(() => pipeline.ExecuteAsync(null!));
        exception.ParamName.ShouldBe("message");
    }

    [Fact]
    public async Task It_should_execute_all_the_filters()
    {
        // Arrange
        var pipeline = new AsyncPipeline<string>();
        var callCount = 0;

        var filter1 = new Mock<IAsyncFilter<string>>();
        filter1.Setup(f => f.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask)
               .Callback(() => callCount++);

        var filter2 = new Mock<IAsyncFilter<string>>();
        filter2.Setup(f => f.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask)
               .Callback(() => callCount++);

        pipeline.Register(filter1.Object).Register(filter2.Object);

        // Act
        await pipeline.ExecuteAsync("test");

        // Assert
        callCount.ShouldBe(2);
        filter1.Verify(f => f.ExecuteAsync("test", It.IsAny<CancellationToken>()), Times.Once);
        filter2.Verify(f => f.ExecuteAsync("test", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task It_should_execute_the_filters_in_the_sequence_they_were_registered()
    {
        // Arrange
        var pipeline = new AsyncPipeline<string>();
        var executionOrder = new List<int>();

        var filter1 = new Mock<IAsyncFilter<string>>();
        filter1.Setup(f => f.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask)
               .Callback(() => executionOrder.Add(1));

        var filter2 = new Mock<IAsyncFilter<string>>();
        filter2.Setup(f => f.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask)
               .Callback(() => executionOrder.Add(2));

        pipeline.Register(filter1.Object).Register(filter2.Object);

        // Act
        await pipeline.ExecuteAsync("test");

        // Assert
        executionOrder.ShouldBe(new[] { 1, 2 });
    }

    [Fact]
    public async Task It_should_stop_processing_the_filters_when_stop_is_set_to_true()
    {
        // Arrange
        var message = new TestMessage { Stop = false };
        var callCount = 0;

        var pipeline = new AsyncPipeline<TestMessage>();

        var filter1 = new Mock<IAsyncFilter<TestMessage>>();
        filter1.Setup(f => f.ExecuteAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask)
               .Callback(() => callCount++);

        var filter2 = new Mock<IAsyncFilter<TestMessage>>();
        filter2.Setup(f => f.ExecuteAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask)
               .Callback(() => message.Stop = true);

        var filter3 = new Mock<IAsyncFilter<TestMessage>>();
        filter3.Setup(f => f.ExecuteAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask)
               .Callback(() => callCount++);

        pipeline.Register(filter1.Object).Register(filter2.Object).Register(filter3.Object);

        // Act
        await pipeline.ExecuteAsync(message);

        // Assert
        callCount.ShouldBe(1);
        message.Stop.ShouldBeTrue();
        filter1.Verify(f => f.ExecuteAsync(message, It.IsAny<CancellationToken>()), Times.Once);
        filter2.Verify(f => f.ExecuteAsync(message, It.IsAny<CancellationToken>()), Times.Once);
        filter3.Verify(f => f.ExecuteAsync(message, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task It_should_respect_cancellation_token()
    {
        // Arrange
        const string message = "test";
        var cts = new CancellationTokenSource();
        var callCount = 0;

        var pipeline = new AsyncPipeline<string>();

        var filter1 = new Mock<IAsyncFilter<string>>();
        filter1.Setup(f => f.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .Returns(async () =>
               {
                   callCount++;
                   await Task.Delay(100, cts.Token);
               });

        var filter2 = new Mock<IAsyncFilter<string>>();
        filter2.Setup(f => f.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask)
               .Callback(() => callCount++);

        pipeline.Register(filter1.Object).Register(filter2.Object);

        // Act
        var task = pipeline.ExecuteAsync(message, cts.Token);
        await cts.CancelAsync();

        // Assert
        await Should.ThrowAsync<TaskCanceledException>(() => task);
        callCount.ShouldBe(1);
        filter1.Verify(f => f.ExecuteAsync(message, It.IsAny<CancellationToken>()), Times.Once);
        filter2.Verify(f => f.ExecuteAsync(message, It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public void It_should_resolve_the_filter_using_the_service_provider()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<TestFilter>();
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        // Act & Assert
        var pipeline = new AsyncPipeline<TestMessage>(serviceProvider);
        pipeline.Register<TestFilter>(); //does not throw
    }
    
    [Fact]
    public void It_should_throw_an_exception_if_the_pipeline_was_constructed_without_a_service_provider()
    {
        // Arrange, Act & Assert
        var pipeline = new AsyncPipeline<TestMessage>();
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
        var pipeline = new AsyncPipeline<TestMessage>(serviceProvider);
        Assert.Throws<Exception>(() => pipeline.Register<TestFilter>())
            .Message.ShouldBe("Filter 'TestFilter' is not registered with the service provider.");
    }
    
    public class TestFilter : IAsyncFilter<TestMessage>
    {
        public Task ExecuteAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class TestMessage : IStopProcessing
    {
        public bool Stop { get; set; }
    }
}