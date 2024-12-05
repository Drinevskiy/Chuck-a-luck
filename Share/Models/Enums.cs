
namespace Share.Models
{
    public enum Orientation
    {
        LeftRight = 0,
        TopDown = 1,
    }

    public static class OrientationExtension
    {
        public static Orientation Opposite(this Orientation orientation)
        {
            return orientation == Orientation.LeftRight
                ? Orientation.TopDown
                : Orientation.LeftRight;
        }
    }

    public enum Direction
    {
        Top = 0,
        Right = 1,
        Bottom = 2,
        Left = 3,
    }

    [Flags]
    public enum ShipPart
    {
        Center = 0b_00,

        Start = 0b_01,
        End = 0b_10,
    }

    public static class ShipPartExtension
    {
        public static bool Contatins(this ShipPart first, ShipPart second)
        {
            return (first & second) == second;
        }
    }

    public record struct OrientedShipPart(Orientation Orientation, ShipPart ShipPart)
    {
        public ShipPart GetShipPart() { return this.ShipPart; }
    }

    public enum ShotStatus
    {
        Intact = 0,
        Shotted = 1,
        Destroyed = 2,

        Miss = Intact,
        Hit = Shotted,
        ShipSunk = Destroyed,
    }
}
