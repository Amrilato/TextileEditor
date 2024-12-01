using Textile.Interfaces;

namespace Textile.Data;

public class Heddle(int width, int height) : VerticalTextile(width, height), ICreateTextile<Heddle>
{
    public static Heddle Create(int width, int height) => new(width, height);
}
