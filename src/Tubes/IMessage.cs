namespace Tubes;

public interface IMessage<TResult, TError>
{
    Result<TResult, TError> Result { get; }
}
