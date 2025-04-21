using System.Transactions;
using Moq;
using Shouldly;
using Tubes.Aspects;

namespace Tubes.UnitTests.Aspects;

public class TransactionAspectTests
{
    [Fact]
    public void It_should_throw_an_exception_when_next_is_null()
    {
        Action<string, CancellationToken> next = null!;

        Should.Throw<ArgumentNullException>(() => new TransactionAspect<string>(next));
    }

    [Fact]
    public void It_should_throw_an_exception_when_message_is_null()
    {
        var next = new Mock<Action<string, CancellationToken>>().Object;
        var aspect = new TransactionAspect<string>(next);

        Should.Throw<ArgumentNullException>(() =>
            aspect.Execute(null!, CancellationToken.None)).ParamName.ShouldBe("message");
    }

    [Fact]
    public void It_should_execute_next_within_transaction_scope()
    {
        const string message = "test";
        var token = CancellationToken.None;
        var transactionWasActive = false;

        Action<string, CancellationToken> next = (msg, tkn) => {
            transactionWasActive = Transaction.Current != null;
            msg.ShouldBe(message);
            tkn.ShouldBe(token);
        };

        var aspect = new TransactionAspect<string>(next);

        aspect.Execute(message, token);

        transactionWasActive.ShouldBeTrue();
    }

    [Fact]
    public void It_should_complete_transaction_when_execution_succeeds()
    {
        const string message = "test";
        var mockNext = new Mock<Action<string, CancellationToken>>();
        
        var aspect = new TransactionAspect<string>(mockNext.Object);

        aspect.Execute(message);

        mockNext.Verify(n => n(message, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void It_should_not_complete_transaction_when_execution_fails()
    {
        const string message = "test";
        var expectedException = new InvalidOperationException("Test exception");
        
        var mockNext = new Mock<Action<string, CancellationToken>>();
        mockNext.Setup(n => n(message, It.IsAny<CancellationToken>()))
                .Throws(expectedException);
        
        var aspect = new TransactionAspect<string>(mockNext.Object);

        var exception = Should.Throw<InvalidOperationException>(() => aspect.Execute(message));
        
        exception.ShouldBeSameAs(expectedException);
        mockNext.Verify(n => n(message, It.IsAny<CancellationToken>()), Times.Once);
    }
}