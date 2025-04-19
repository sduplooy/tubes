using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Tubes.Aspects;

namespace Tubes.UnitTests.Aspects;

public class MessageLoggingAspectTests
{
    [Fact]
    public void It_should_throw_an_exception_when_next_is_null()
    {
        const string message = "test message";
        
        Action<string, CancellationToken> next = null!;
        var logger = new Mock<ILogger<MessageLoggingAspect<string>>>().Object;
        var aspect = new MessageLoggingAspect<string>(next, logger);

        Should.Throw<ArgumentNullException>(() => aspect.Execute(message, CancellationToken.None));
    }

    [Fact]
    public void It_should_throw_an_exception_when_logger_is_null()
    { 
        const string message = "test message";
        
        var next = new Mock<Action<string, CancellationToken>>().Object;
        ILogger<MessageLoggingAspect<string>> logger = null!;
        var aspect = new MessageLoggingAspect<string>(next, logger);

        Should.Throw<ArgumentNullException>(() => aspect.Execute(message, CancellationToken.None));
    }

    [Fact]
    public void It_should_throw_an_exception_when_message_is_null()
    {
        var next = new Mock<Action<string, CancellationToken>>().Object;
        var logger = new Mock<ILogger<MessageLoggingAspect<string>>>().Object;
        var aspect = new MessageLoggingAspect<string>(next, logger);

        Should.Throw<ArgumentNullException>(() => 
                aspect.Execute(null!, CancellationToken.None)).ParamName.ShouldBe("message");
    }

    [Fact]
    public void It_should_call_next_when_the_message_is_valid()
    {
        const string message = "test message";
        var cancellationToken = CancellationToken.None;
        var nextCalled = false;

        Action<string, CancellationToken> next = (msg, ct) => {
            nextCalled = true;
            msg.ShouldBe(message);
            ct.ShouldBe(cancellationToken);
        };

        var logger = new Mock<ILogger<MessageLoggingAspect<string>>>().Object;
        var aspect = new MessageLoggingAspect<string>(next, logger);

        aspect.Execute(message, cancellationToken);

        nextCalled.ShouldBeTrue();
    }

    [Fact]
    public void It_should_log_the_message_type_when_debug_logging_is_enabled()
    {
        var message = new TestMessage();
        var token = CancellationToken.None;

        var nextMock = new Mock<Action<TestMessage, CancellationToken>>();
        
        var loggerMock = new Mock<ILogger<MessageLoggingAspect<TestMessage>>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        
        var aspect = new MessageLoggingAspect<TestMessage>(nextMock.Object, loggerMock.Object);

        aspect.Execute(message, token);

        nextMock.Verify(n => n(message, token), Times.Once);
        
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void It_should_not_log_the_message_type_when_debug_logging_is_disabled()
    {
        var message = new TestMessage();
        var token = CancellationToken.None;

        var nextMock = new Mock<Action<TestMessage, CancellationToken>>();
        
        var loggerMock = new Mock<ILogger<MessageLoggingAspect<TestMessage>>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
        
        var aspect = new MessageLoggingAspect<TestMessage>(nextMock.Object, loggerMock.Object);

        aspect.Execute(message, token);

        nextMock.Verify(n => n(message, token), Times.Once);
        
        loggerMock.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
    
    public class TestMessage;
}