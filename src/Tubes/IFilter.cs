namespace Tubes;

public interface IFilter<in TMessage>
{
    void Execute(TMessage message);
}