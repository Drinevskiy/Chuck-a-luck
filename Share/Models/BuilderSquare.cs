using System.Drawing;

namespace Share.Models
{
    public record struct BuilderSquare(Color SeaColor, OrientedShipPart? OrientedShipPart)
    {
        public override readonly string ToString()
        {
            return OrientedShipPart is null ? " " : "█";
        }
    }
}