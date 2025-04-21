using Moq;
using Shouldly;
using Tubes.Aspects;

namespace Tubes.UnitTests.Aspects;

public class RetryAspectTests
{
    [Fact]
    public void It_should_construct_the_instance_when_the_parameters_are_valid()
    {
        var next = new Mock<Action<string, CancellationToken>>().Object;
        var options = new RetryOptions(3, TimeSpan.FromMilliseconds(10));

        Should.NotThrow(() => new RetryAspect<string>(next, options));
    }

    [Fact]
    public void It_should_throw_an_exception_when_next_is_null()
    {
        Action<string, CancellationToken> next = null!;
        var options = new RetryOptions(3, TimeSpan.FromMilliseconds(10));

        Should.Throw<ArgumentNullException>(() => new RetryAspect<string>(next, options));
    }

    [Fact]
    public void It_should_throw_an_exception_when_message_is_null()
    {
        var next = new Mock<Action<string, CancellationToken>>().Object;
        var options = new RetryOptions(3, TimeSpan.FromMilliseconds(10));
        var aspect = new RetryAspect<string>(next, options);

        Should.Throw<ArgumentNullException>(() =>
            aspect.Execute(null!, CancellationToken.None)).ParamName.ShouldBe("message");
    }

    [Fact]
    public void It_should_execute_next()
    {
        const string message = "test";
        var token = CancellationToken.None;
        var nextCalled = 0;

        Action<string, CancellationToken> next = (msg, tkn) => {
            nextCalled++;
            msg.ShouldBe(message);
            tkn.ShouldBe(token);
        };

        var options = new RetryOptions(3, TimeSpan.FromMilliseconds(10));
        var aspect = new RetryAspect<string>(next, options);

        aspect.Execute(message, token);

        nextCalled.ShouldBe(1);
    }

    [Fact]
    public void It_should_call_next_again_when_an_exception_is_thrown()
    {
        const string message = "test";
        const int maxRetries = 3;
        var nextCallCount = 0;
        
        var nextMock = new Mock<Action<string, CancellationToken>>();
        nextMock
            .Setup(n => n(message, CancellationToken.None))
            .Callback(() => nextCallCount++)
            .Throws(new InvalidOperationException());

        var options = new RetryOptions(maxRetries, TimeSpan.FromMilliseconds(1));
        var aspect = new RetryAspect<string>(nextMock.Object, options);

        Should.Throw<InvalidOperationException>(() => aspect.Execute(message));

        nextCallCount.ShouldBe(maxRetries + 1);
    }

    [Fact]
    public void It_should_increase_the_delay_between_retries()
    {
        const string message = "test";
        const int maxRetries = 3;
        var attemptsCount = 0;
        var slideTime = TimeSpan.FromMilliseconds(50);
        var stopwatch = new System.Diagnostics.Stopwatch();

        Action<string, CancellationToken> next = (_, _) => {
            attemptsCount++;
            if (attemptsCount == 1) // Only throw on first attempt
            {
                stopwatch.Start();
                throw new InvalidOperationException();
            }
            stopwatch.Stop();
        };

        var options = new RetryOptions(maxRetries, slideTime);
        var aspect = new RetryAspect<string>(next, options);

        aspect.Execute(message);

        attemptsCount.ShouldBe(2);
        stopwatch.ElapsedMilliseconds.ShouldBeGreaterThanOrEqualTo((long)slideTime.TotalMilliseconds);
    }

    [Fact]
    public void It_should_throw_the_original_exception_when_max_retries_exceeded()
    {
        const string message = "test";
        const int maxRetries = 2;
        var expectedException = new InvalidOperationException("Test exception");
        var nextCallCount = 0;

        Action<string, CancellationToken> next = (_, _) => {
            nextCallCount++;
            throw expectedException;
        };

        var options = new RetryOptions(maxRetries, TimeSpan.FromMilliseconds(1));
        var aspect = new RetryAspect<string>(next, options);

        var exception = Should.Throw<InvalidOperationException>(() => aspect.Execute(message));
        exception.ShouldBeSameAs(expectedException);
        nextCallCount.ShouldBe(maxRetries + 1);
    }

    [Fact]
    public void It_should_throw_an_exception_when_max_retries_is_less_than_zero()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => 
            new RetryOptions(-1, TimeSpan.FromMilliseconds(10))).ParamName.ShouldBe("maxRetries");
    }
}