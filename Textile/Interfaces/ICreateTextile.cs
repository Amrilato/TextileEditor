namespace Textile.Interfaces;

internal interface ICreateTextile<TSelf>
{
    static abstract TSelf Create(int width, int height);
}
