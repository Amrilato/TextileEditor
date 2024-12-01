using Textile.Interfaces;

namespace Textile.Data;

public class Pedal(int width, int height) : HorizontalTextile(width, height), ICreateTextile<Pedal>
{
    public static Pedal Create(int width, int height) => new(width, height);
}
