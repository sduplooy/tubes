using Moq;
using Shouldly;

namespace Tubes.UnitTests;

public class AsyncPipelineTests
{
    [Fact]
    public void It_should_register_the_filter()
    {
        var filters = new List<Func<string, CancellationToken, Task>>();
        var pipeline = new AsyncPipeline<string>(filters);
        
        var filter = new Func<string, CancellationToken, Task>((_, _) => Task.CompletedTask);
        
        var result = pipeline.Register(filter);
        
        result.ShouldBeSameAs(pipeline);
        filters.ShouldContain(filter);
    }
    
    [Fact]
    public void It_should_throw_an_exception_for_a_null_filter()
    {
        var pipeline = new AsyncPipeline<string>();
        Should.Throw<ArgumentNullException>(() => pipeline.Register(null!)).ParamName.ShouldBe("filter");
    }
    
    [Fact]
    public async Task It_should_execute_all_the_filters()
    {
        var pipeline = new AsyncPipeline<string>();
        var callCount = 0;
        
        pipeline
            .Register((_, _) => Task.FromResult(callCount += 1))
            .Register((_, _) => Task.FromResult(callCount += 2))
            .Register((_, _) => Task.FromResult(callCount += 4));
        
        await pipeline.ExecuteAsync("test");
        
        callCount.ShouldBe(7);
    }
    
    [Fact]
    public async Task It_should_execute_the_filters_in_the_sequence_they_were_registered()
    {
        var pipeline = new AsyncPipeline<string>();
        var executionOrder = new List<int>();
        
        pipeline
            .Register((_, ct) => Task.Run(() => executionOrder.Add(1), ct))
            .Register((_, ct) => Task.Run(() => executionOrder.Add(2), ct))
            .Register((_, ct) => Task.Run(() => executionOrder.Add(3), ct));
        
        await pipeline.ExecuteAsync("test");
        
        executionOrder.ShouldBe([1, 2, 3]);
    }
    
    [Fact]
    public async Task It_should_stop_processing_the_filters_when_stop_is_set_to_true()
    {
        var message = new TestMessage { Stop = false };
        var callCount = 0;
        
        var pipeline = new AsyncPipeline<TestMessage>();
        pipeline
            .Register((_, _) => Task.FromResult(callCount++))
            .Register((m, _) =>
            {
                m.Stop = true; 
                return Task.FromResult(callCount++);
            })
            .Register((_, _) => Task.FromResult(callCount++));
        
        await pipeline.ExecuteAsync(message, CancellationToken.None);
        
        callCount.ShouldBe(2);
        message.Stop.ShouldBeTrue();
    }
    
    [Fact]
    public async Task It_should_stop_processing_when_cancellation_is_requested()
    {
        var message = new TestMessage();
        var callCount = 0;
        
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        
        var pipeline = new AsyncPipeline<TestMessage>();
        pipeline
            .Register((_, _) => Task.FromResult(callCount++))
            .Register((_, _) => Task.FromResult(callCount++))
            .Register((_, _) => Task.FromResult(callCount++));
        
        await pipeline.ExecuteAsync(message, cts.Token);
        
        callCount.ShouldBe(0);
    }
    
    [Fact]
    public async Task It_should_stop_processing_after_cancellation_is_requested_mid_execution()
    {
        var message = new TestMessage();
        var callCount = 0;
        var cts = new CancellationTokenSource();
        
        var pipeline = new AsyncPipeline<TestMessage>();
        pipeline
            .Register((_, _) => Task.FromResult(callCount++))
            .Register((_, _) => 
            {
                callCount++;
                cts.CancelAsync();
                return Task.CompletedTask;
            })
            .Register((_, _) => Task.FromResult(callCount++));
        
        await pipeline.ExecuteAsync(message, cts.Token);
        
        callCount.ShouldBe(2);
    }
    
    private class TestMessage : IStopProcessing
    {
        public bool Stop { get; set; }
    }
}