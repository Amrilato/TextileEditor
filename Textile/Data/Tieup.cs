using Textile.Interfaces;

namespace Textile.Data;

public class Tieup(int width, int height) : VerticalTextile(width, height), ICreateTextile<Tieup>
{
    public static Tieup Create(int width, int height) => new(width, height);
}
