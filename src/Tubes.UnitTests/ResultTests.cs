using Shouldly;

namespace Tubes.UnitTests;

public class ResultTests
{
    [Fact]
    public void It_should_create_a_success_result()
    {
        const string value = "Test value";
        
        var result = Result<string, Exception>.Success(value);
        
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(value);
    }
    
    [Fact]
    public void It_should_create_a_failure_result()
    {
        var error = new ArgumentException("Test error");
        
        var result = Result<string, ArgumentException>.Failure(error);
        
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeSameAs(error);
    }
    
    [Fact]
    public void It_should_throw_an_exception_when_trying_to_access_value_when_the_result_is_a_failure()
    {
        var error = new ArgumentException("Test error");
        var result = Result<string, ArgumentException>.Failure(error);
        
        var exception = Should.Throw<InvalidOperationException>(() => _ = result.Value);
        exception.Message.ShouldBe("Result is not successful");
    }
    
    [Fact]
    public void It_should_throw_an_exception_when_trying_to_access_error_when_the_result_is_successful()
    {
        const string value = "Test value";
        var result = Result<string, Exception>.Success(value);
        
        var exception = Should.Throw<InvalidOperationException>(() => _ = result.Error);
        exception.Message.ShouldBe("Result is successful");
    }
    
    [Fact]
    public void It_should_accept_value_types()
    {
        var successResult = Result<int, string>.Success(42);
        var failureResult = Result<int, string>.Failure("Error message");
        
        successResult.IsSuccess.ShouldBeTrue();
        successResult.Value.ShouldBe(42);
        
        failureResult.IsSuccess.ShouldBeFalse();
        failureResult.Error.ShouldBe("Error message");
    }
    
    [Fact]
    public void It_should_accept_reference_types()
    {
        var person = new Person();
        var error = new CustomError();
        
        var successResult = Result<Person, CustomError>.Success(person);
        var failureResult = Result<Person, CustomError>.Failure(error);
        
        successResult.IsSuccess.ShouldBeTrue();
        successResult.Value.ShouldBeSameAs(person);
        
        failureResult.IsSuccess.ShouldBeFalse();
        failureResult.Error.ShouldBeSameAs(error);
    }

    private record Person;
    private record CustomError;
}