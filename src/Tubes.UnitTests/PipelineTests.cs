using Moq;
using Shouldly;

namespace Tubes.UnitTests;

public class PipelineTests
{
    [Fact]
    public void It_should_register_the_filter()
    {
        var filters = new List<Action<string>>();
        var pipeline = new Pipeline<string>(filters);
        
        Action<string> filter = _ => { };
        
        var result = pipeline.Register(filter);
        
        result.ShouldBeSameAs(pipeline);
        filters.ShouldContain(filter);
    }
    
    [Fact]
    public void It_should_throw_an_exception_for_a_null_filter()
    {
        var pipeline = new Pipeline<string>();
        Should.Throw<ArgumentNullException>(() => pipeline.Register(null!)).ParamName.ShouldBe("filter");
    }
    
    [Fact]
    public void It_should_execute_all_the_filters()
    {
        var pipeline = new Pipeline<string>();
        var callCount = 0;
        
        pipeline
            .Register(_ => callCount += 1)
            .Register(_ => callCount += 2)
            .Register(_ => callCount += 4);
        
        pipeline.Execute("test");
        
        callCount.ShouldBe(7);
    }
    
    [Fact]
    public void It_should_execute_the_filters_in_the_sequence_they_were_registered()
    {
        var pipeline = new Pipeline<string>();
        var executionOrder = new List<int>();
        
        pipeline
            .Register(_ => executionOrder.Add(1))
            .Register(_ => executionOrder.Add(2))
            .Register(_ => executionOrder.Add(3));
        
        pipeline.Execute("test");
        
        executionOrder.ShouldBe([1, 2, 3]);
    }
    
    [Fact]
    public void It_should_stop_processing_the_filters_when_stop_is_set_to_true()
    {
        var message = new TestMessage { Stop = false };
        var callCount = 0;
        
        var pipeline = new Pipeline<TestMessage>();
        pipeline
            .Register(_ => callCount++)
            .Register(m =>
            {
                m.Stop = true; 
                callCount++;
            })
            .Register(_ => callCount++);
        
        pipeline.Execute(message);
        
        callCount.ShouldBe(2);
        message.Stop.ShouldBeTrue();
    }
    
    private class TestMessage : IStopProcessing
    {
        public bool Stop { get; set; }
    }
}