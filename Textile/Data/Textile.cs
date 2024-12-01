using Textile.Interfaces;

namespace Textile.Data;

public class Textile(int width, int height) : VerticalTextile(width, height), ICreateTextile<Textile>
{
    public static Textile Create(int width, int height) => new(width, height);
}
