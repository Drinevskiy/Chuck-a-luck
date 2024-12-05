namespace Share.Models
{
    public class Ship
    {
        public int X { get; init; }
        public int Y { get; init; }
        public int Size { get; init; }
        public Orientation Orientation { get; init; }

        public Ship(int x, int y, int size, Orientation orientation)
        {
            if (x < 0)
                throw new ArgumentException("X must be not negative.", nameof(x));

            if (y < 0)
                throw new ArgumentException("Y must be not negative.", nameof(y));

            if (size <= 0)
                throw new ArgumentException("Size must be positive.", nameof(size));

            X = x;
            Y = y;

            Size = size;
            Orientation = orientation;
        }

        public void Deconstruct(out int x, out int y, out int size, out Orientation orientation)
        {
            x = X;
            y = Y;

            size = Size;
            orientation = Orientation;
        }
    }
}