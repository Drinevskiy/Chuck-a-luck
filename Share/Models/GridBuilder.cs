using System.Drawing;

namespace Share.Models
{
    public class GridBuilder
    {
        private Ship? _selectedShip;
        private Dictionary<Point, Ship> _ships;
        private event Action? _onAllShipDestroyed;
        private event Action<Ship>? _onShipDestroyed;
        private const int DEFAULT_SIZE = 10;
        public GridBuilder(int sizeX, int sizeY)
        {
            if (sizeX <= 0)
                throw new ArgumentException("Size must be positive", nameof(sizeX));

            if (sizeY <= 0)
                throw new ArgumentException("Size must be positive", nameof(sizeY));

            SizeX = sizeX;
            SizeY = sizeY;

            _ships = new Dictionary<Point, Ship>();
        }

        public GridBuilder()
     : this(DEFAULT_SIZE, DEFAULT_SIZE)
        { }

        public int SizeX { get; init; }
        public int SizeY { get; init; }

        public GridBuilder AddShip(Ship ship)
        {
            if (ship.X >= SizeX)
                throw new ArgumentOutOfRangeException(nameof(ship.X), "Invalid ship x.");

            if (ship.Y >= SizeY)
                throw new ArgumentOutOfRangeException(nameof(ship.Y), "Invalid ship y.");

            if (ship.Orientation == Orientation.LeftRight && ship.X + ship.Size > SizeX
                || ship.Orientation == Orientation.TopDown && ship.Y + ship.Size > SizeY)
            {
                throw new ArgumentOutOfRangeException(nameof(ship.Size), "Invalid ship size.");
            }

            SaveSelectedShip();
            _selectedShip = ship;

            return this;
        }

        public GridBuilder SaveSelectedShip()
        {
            if (_selectedShip is null)
                return this;

            if (IsSelectedShipValid())
            {
                _ships[new Point(_selectedShip.X, _selectedShip.Y)] = _selectedShip;
            }

            _selectedShip = null;

            return this;
        }

        public GridBuilder RemoveSelectedShip()
        {
            _selectedShip = null;
            return this;
        }

        public GridBuilder Clear()
        {
            _selectedShip = null;
            _ships.Clear();

            return this;
        }

        public GridBuilder SelectShip(int x, int y)
        {
            if (x < 0 || x >= SizeX)
                throw new ArgumentOutOfRangeException(nameof(x), "Invalid x.");

            if (y < 0 || y >= SizeY)
                throw new ArgumentOutOfRangeException(nameof(y), "Invalid y.");

            foreach (Ship ship in _ships.Values)
            {
                if (IsIntersectedWith(x, y, ship))
                {
                    _selectedShip = ship;
                    _ships.Remove(new Point(ship.X, ship.Y));

                    return this;
                }
            }

            return this;
        }

        public GridBuilder MoveSelectedShip(Direction direction)
        {
            if (_selectedShip is null)
                return this;

            var ship = _selectedShip;

            RemoveSelectedShip();

            try
            {
                (int x, int y) = direction switch
                {
                    Direction.Top => (ship.X, ship.Y - 1),
                    Direction.Right => (ship.X + 1, ship.Y),
                    Direction.Bottom => (ship.X, ship.Y + 1),
                    Direction.Left => (ship.X - 1, ship.Y),
                    _ => throw new NotImplementedException("Unknown direction type")
                };

                AddShip(new Ship(x, y, ship.Size, ship.Orientation));
            }
            catch (ArgumentOutOfRangeException)
            {
                AddShip(ship);
            }

            return this;
        }

        public GridBuilder RotateSelectedShip()
        {
            if (_selectedShip is null)
                return this;

            var ship = _selectedShip;

            RemoveSelectedShip();

            try
            {
                AddShip(new Ship(ship.X, ship.Y, ship.Size, ship.Orientation.Opposite()));
            }
            catch (ArgumentOutOfRangeException)
            {
                AddShip(ship);
            }

            return this;
        }

        public GridBuilder AddOnAllShipDestroyed(Action @event)
        {
            if (_onAllShipDestroyed is null)
            {
                _onAllShipDestroyed = @event;
            }
            else
            {
                _onAllShipDestroyed += @event;
            }

            return this;
        }

        public GridBuilder AddOnShipDestroyed(Action<Ship> @event)
        {
            if (_onAllShipDestroyed is null)
            {
                _onShipDestroyed = @event;
            }
            else
            {
                _onShipDestroyed += @event;
            }

            return this;
        }

        public IEnumerable<IEnumerable<BuilderSquare>> Rows
        {
            get
            {
                BuilderSquare[,] squares = Field;

                for (int i = 0; i < SizeY; i++)
                {
                    yield return Enumerable
                                    .Range(0, SizeX)
                                    .Select(j => squares[i, j]);
                }
            }
        }

        public BuilderSquare[,] Field
        {
            get
            {
                static BuilderSquare modifyShipPart(Ship ship, BuilderSquare s, int i)
                {
                    ShipPart newShipPart;

                    if (ship.Size == 1)
                    {
                        newShipPart = ShipPart.Start | ShipPart.End;
                    }
                    else if (i == 0)
                    {
                        newShipPart = ShipPart.End;
                    }
                    else if (i == ship.Size - 1)
                    {
                        newShipPart = ShipPart.Start;
                    }
                    else
                    {
                        newShipPart = ShipPart.Center;
                    }

                    return s with { OrientedShipPart = (s.OrientedShipPart ?? new()) with { ShipPart = newShipPart } };
                }

                static BuilderSquare modifyOrientation(Ship ship, BuilderSquare s)
                {
                    return s with { OrientedShipPart = (s.OrientedShipPart ?? new()) with { Orientation = ship.Orientation } };
                }

                var squares = new BuilderSquare[SizeY, SizeX];

                foreach (var ship in _ships.Values)
                {
                    MarkShip(ship, modifyShipPart, squares);
                    MarkShip(ship, modifyOrientation, squares);
                }

                if (_selectedShip is not null)
                {
                    Color borderColor = IsSelectedShipValid() ? Color.Green : Color.Red;

                    BuilderSquare modifySeaColor(BuilderSquare s)
                    {
                        return s with { SeaColor = borderColor };
                    }

                    MarkShip(_selectedShip, modifyShipPart, squares);
                    MarkShip(_selectedShip, modifyOrientation, squares);

                    MarkBorder(_selectedShip, modifySeaColor, squares);
                }

                return squares;
            }
        }

        public BuilderSquare this[int x, int y]
        {
            get => Field[x, y];
        }

        public Grid Build()
        {
            if (!IsSelectedShipValid())
                throw new InvalidOperationException("Can't build grid because selected ship is invalid");

            SaveSelectedShip();

            BuilderSquare[,] builderSquares = Field;

            BattleSquare[,] battleSquares = new BattleSquare[SizeY, SizeX];

            for (int i = 0; i < SizeY; i++)
            {
                for (int j = 0; j < SizeX; j++)
                {
                    battleSquares[i, j] = new BattleSquare(ShotStatus.Intact, builderSquares[i, j].OrientedShipPart);
                }
            }

            return new Grid(battleSquares, _ships.Count, _onAllShipDestroyed, _onShipDestroyed);
        }

        public override string ToString()
        {
            string res = new string('-', SizeX + 2) + '\n';

            foreach (var row in this.Rows)
            {
                res += '|';

                foreach (var square in row)
                {
                    res += square.ToString();
                }

                res += "|\n";
            }

            res += new string('-', SizeX + 2) + '\n';

            return res;
        }

        private static bool IsIntersectedWith(int x, int y, Ship ship)
        {
            if (ship.Orientation == Orientation.LeftRight)
            {
                return y == ship.Y && x >= ship.X && x < (ship.X + ship.Size);
            }
            else
            {
                return x == ship.X && y >= ship.Y && y < (ship.Y + ship.Size);
            }
        }

        private static void MoveNext(ref int x, ref int y, Ship ship) => _ = ship.Orientation == Orientation.LeftRight ? x++ : y++;

        private bool IsSelectedShipValid()
        {
            if (_selectedShip is null)
                return true;

            foreach (Ship ship in _ships.Values)
            {
                int x = ship.X;
                int y = ship.Y;

                for (int i = 0; i < ship.Size; i++, MoveNext(ref x, ref y, ship))
                {
                    if (IsIntersectedWith(x, y, _selectedShip))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static void MarkShip(Ship ship, Func<BuilderSquare, BuilderSquare> modify, BuilderSquare[,] grid)
        {
            MarkShip(ship, (s, i) => modify(s), grid);
        }

        private static void MarkShip(Ship ship, Func<Ship, BuilderSquare, BuilderSquare> modifyByShip, BuilderSquare[,] grid)
        {
            MarkShip(ship, (ship, s, i) => modifyByShip(ship, s), grid);
        }

        private static void MarkShip(Ship ship, Func<BuilderSquare, int, BuilderSquare> modifyByIndex, BuilderSquare[,] grid)
        {
            MarkShip(ship, (ship, s, i) => modifyByIndex(s, i), grid);
        }

        private static void MarkShip(Ship ship, Func<Ship, BuilderSquare, int, BuilderSquare> modifyByShipIndex, BuilderSquare[,] grid)
        {
            int col = ship.X;
            int row = ship.Y;

            for (int i = 0; i < ship.Size; i++, MoveNext(ref col, ref row, ship))
            {
                grid[row, col] = modifyByShipIndex(ship, grid[row, col], i);
            }
        }

        private void MarkBorder(Ship ship, Func<BuilderSquare, BuilderSquare> modify, BuilderSquare[,] grid)
        {
            var (x, y, size, Orientation) = ship;

            for (int i = -1; i < size + 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (j != 0 || i < 0 || i >= size)
                    {
                        if (Orientation == Orientation.LeftRight)
                        {
                            MarkSquare(x + i, y + j);
                        }
                        else
                        {
                            MarkSquare(x + j, y + i);
                        }
                    }
                }
            }

            void MarkSquare(int x, int y)
            {
                if (x < 0 || x >= SizeX || y < 0 || y >= SizeY)
                    return;

                int row = y;
                int col = x;

                grid[row, col] = modify(grid[row, col]);
            }
        }
    }

}