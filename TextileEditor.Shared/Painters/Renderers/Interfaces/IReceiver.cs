namespace TextileEditor.Shared.Painters.Renderers;

public interface IReceiver<TData>
{
    void Receive(TData data);
}