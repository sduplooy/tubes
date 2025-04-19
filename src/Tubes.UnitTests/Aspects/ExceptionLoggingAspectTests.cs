using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Tubes.Aspects;

namespace Tubes.UnitTests.Aspects;

public class ExceptionLoggingAspectTests
{
    [Fact]
    public void It_should_throw_an_exception_when_next_is_null()
    {
        var logger = new Mock<ILogger<ExceptionLoggingAspect<string>>>();

        Should.Throw<ArgumentNullException>(() => 
            new ExceptionLoggingAspect<string>(null!, logger.Object)
                .Execute("msg", CancellationToken.None)).ParamName.ShouldBe("next");
    }

    [Fact]
    public void It_should_throw_an_exception_when_the_message_is_null()
    {
        var next = new Mock<Action<string, CancellationToken>>().Object;
        var logger = new Mock<ILogger<ExceptionLoggingAspect<string>>>().Object;
        var aspect = new ExceptionLoggingAspect<string>(next, logger);

        Should.Throw<ArgumentNullException>(() => 
            aspect.Execute(null!)).ParamName.ShouldBe("message");
    }

    [Fact]
    public void It_should_call_next_to_handle_the_message()
    {
        const string message = "test message";
        var nextCalled = false;

        Action<string, CancellationToken> next = (msg, ct) => {
            nextCalled = true;
            msg.ShouldBe(message);
        };

        var logger = new Mock<ILogger<ExceptionLoggingAspect<string>>>().Object;
        var aspect = new ExceptionLoggingAspect<string>(next, logger);

        aspect.Execute(message);

        nextCalled.ShouldBeTrue();
    }

    [Fact]
    public void It_should_log_the_exception_when_next_throws()
    {
        var message = "test message";
        var expectedException = new InvalidOperationException("Test exception");
        
        Action<string, CancellationToken> next = (_, _) => throw expectedException;
        
        var loggerMock = new Mock<ILogger<ExceptionLoggingAspect<string>>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
        
        var aspect = new ExceptionLoggingAspect<string>(next, loggerMock.Object);

        var exception = Should.Throw<InvalidOperationException>(() => aspect.Execute(message));
        
        exception.ShouldBeSameAs(expectedException);
        
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void It_should_not_log_when_error_logging_is_disabled()
    {
        var message = "test message";
        var expectedException = new InvalidOperationException("Test exception");
        
        Action<string, CancellationToken> next = (_,_) => throw expectedException;
        
        var loggerMock = new Mock<ILogger<ExceptionLoggingAspect<string>>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(false);
        
        var aspect = new ExceptionLoggingAspect<string>(next, loggerMock.Object);

        var exception = Should.Throw<InvalidOperationException>(() => aspect.Execute(message));
        
        exception.ShouldBeSameAs(expectedException);
        
        loggerMock.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}