using Moq;
using Shouldly;

namespace Tubes.UnitTests;

public class AsyncPipelineTests
{
    [Fact]
    public void It_should_register_the_filter()
    {
        var filters = new List<Action<string, CancellationToken>>();
        var pipeline = new AsyncPipeline<string>(filters);
        
        Action<string, CancellationToken> filter = (_, _) => { };
        
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
    public void It_should_execute_all_the_filters()
    {
        var pipeline = new AsyncPipeline<string>();
        var callCount = 0;
        
        pipeline
            .Register((_, _) => callCount += 1)
            .Register((_, _) => callCount += 2)
            .Register((_, _) => callCount += 4);
        
        pipeline.ExecuteAsync("test");
        
        callCount.ShouldBe(7);
    }
    
    [Fact]
    public void It_should_execute_the_filters_in_the_sequence_they_were_registered()
    {
        var pipeline = new AsyncPipeline<string>();
        var executionOrder = new List<int>();
        
        pipeline
            .Register((_, _) => executionOrder.Add(1))
            .Register((_, _) => executionOrder.Add(2))
            .Register((_, _) => executionOrder.Add(3));
        
        pipeline.ExecuteAsync("test");
        
        executionOrder.ShouldBe([1, 2, 3]);
    }
    
    [Fact]
    public void It_should_stop_processing_the_filters_when_stop_is_set_to_true()
    {
        var message = new TestMessage { Stop = false };
        var callCount = 0;
        
        var pipeline = new AsyncPipeline<TestMessage>();
        pipeline
            .Register((_, _) => callCount++)
            .Register((m, _) =>
            {
                m.Stop = true; 
                callCount++;
            })
            .Register((_, _) => callCount++);
        
        pipeline.ExecuteAsync(message);
        
        callCount.ShouldBe(2);
        message.Stop.ShouldBeTrue();
    }
    
    [Fact]
    public void It_should_stop_processing_when_cancellation_is_requested()
    {
        var message = new TestMessage();
        var callCount = 0;
        
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        var pipeline = new AsyncPipeline<TestMessage>();
        pipeline
            .Register((_, _) => callCount++)
            .Register((_, _) => callCount++)
            .Register((_, _) => callCount++);
        
        pipeline.ExecuteAsync(message, cts.Token);
        
        callCount.ShouldBe(0);
    }
    
    [Fact]
    public void It_should_stop_processing_after_cancellation_is_requested_mid_execution()
    {
        var message = new TestMessage();
        var callCount = 0;
        var cts = new CancellationTokenSource();
        
        var pipeline = new AsyncPipeline<TestMessage>();
        pipeline
            .Register((_, _) => callCount++)
            .Register((_, _) => 
            {
                callCount++;
                cts.Cancel();
            })
            .Register((_, _) => callCount++);
        
        pipeline.ExecuteAsync(message, cts.Token);
        
        callCount.ShouldBe(2);
    }
    
    private class TestMessage : IStopProcessing
    {
        public bool Stop { get; set; }
    }
}