namespace Tubes;

public interface IFilter<in TMessage>
{
    void Process(TMessage message);
}