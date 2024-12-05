using System.Runtime.CompilerServices;

namespace Share.Models
{
    public class Grid
    {
        private readonly BattleSquare[,] _field;
        public int ShipCount { get; init; }
        private int _shipsAliveCount;
        public event Action? OnAllShipDestroyed;
        public event Action<Ship>? OnShipDestroyed;

        internal Grid(BattleSquare[,] field, int shipCount, Action? onAllShipDestroyed = null, Action<Ship>? onShipDestroyed = null)
        {
            if (shipCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(shipCount), "Ship count must be positive.");

            _field = field;

            ShipCount = shipCount;
            _shipsAliveCount = shipCount;

            OnAllShipDestroyed = onAllShipDestroyed;
            OnShipDestroyed = onShipDestroyed;
        }
        public int SizeX => _field.GetLength(1);
        public int SizeY => _field.GetLength(0);

        public int ShipsAliveCount
        {
            get => _shipsAliveCount;
            set
            {
                _shipsAliveCount = value;

                if (value == 0)
                    OnAllShipDestroyed?.Invoke();
            }
        }

        public ShotStatus Shot(int x, int y)
        {
            if (x < 0 || x >= SizeX)
                throw new ArgumentOutOfRangeException(nameof(x), "Invalid x.");

            if (y < 0 || y >= SizeY)
                throw new ArgumentOutOfRangeException(nameof(y), "Invalid y.");

            int row = y;
            int col = x;

            _field[row, col].ShotStatus = ShotStatus.Shotted;

            if (_field[row, col].OrientedShipPart is null)
            {
                return ShotStatus.Miss;
            }
            else
            {
                var orientation = _field[row, col].OrientedShipPart?.Orientation ?? throw new NullReferenceException("Oriented ship part is null.");

                int startX = -1;
                int startY = -1;

                int size = -1;

                bool isNotStart = true;

                for (
                    int i = row, j = col;
                    i >= 0 && j >= 0 && (isNotStart | (isNotStart = !(_field[i, j].OrientedShipPart?.ShipPart.Contatins(ShipPart.Start)) ?? false));
                    MoveToStart(ref j, ref i, orientation))
                {
                    if (_field[i, j].ShotStatus == ShotStatus.Intact)
                    {
                        return ShotStatus.Hit;
                    }
                    else
                    {
                        size++;
                    }
                }

                bool isNotEnd = true;

                for (
                    int i = row, j = col;
                    i >= 0 && j >= 0 && (isNotEnd | (isNotEnd = !(_field[i, j].OrientedShipPart?.ShipPart.Contatins(ShipPart.End)) ?? false));
                    MoveToEnd(ref j, ref i, orientation))
                {
                    if (_field[i, j].ShotStatus == ShotStatus.Intact)
                    {
                        return ShotStatus.Shotted;
                    }
                    else
                    {
                        (startX, startY) = (j, i);
                        size++;
                    }
                }

                var ship = new Ship(startX, startY, size, orientation);

                BattleSquare modifyShipDestroyed(BattleSquare battleSquare)
                {
                    battleSquare.ShotStatus = ShotStatus.Destroyed;
                    return battleSquare;
                }

                BattleSquare modifyBorderShotted(BattleSquare battleSquare)
                {
                    battleSquare.ShotStatus = ShotStatus.Shotted;
                    return battleSquare;
                }

                MarkShip(ship, modifyShipDestroyed);
                MarkBorder(ship, modifyBorderShotted);

                OnShipDestroyed?.Invoke(ship);

                ShipsAliveCount--;

                return ShotStatus.ShipSunk;
            }
        }

        public IEnumerable<IEnumerable<BattleSquare>> Rows
        {
            get
            {
                for (int i = 0; i < SizeY; i++)
                {
                    yield return Enumerable
                                    .Range(0, SizeX)
                                    .Select(j => _field[i, j]);
                }
            }
        }

        public BattleSquare[,] Field => (BattleSquare[,])_field.Clone();

        public BattleSquare this[int x, int y]
        {
            get => _field[x, y];
        }

        public override string ToString()
        {
            string res = string.Empty;

            foreach (var row in this.Rows)
            {
                foreach (var square in row)
                {
                    res += square.ToString();
                }

                res += '\n';
            }

            return res;
        }

        private static void MoveToStart(ref int x, ref int y, Orientation orientation) => _ = orientation == Orientation.LeftRight ? x++ : y++;
        private static void MoveToEnd(ref int x, ref int y, Orientation orientation) => _ = orientation == Orientation.LeftRight ? x-- : y--;

        private void MarkShip(Ship ship, Func<BattleSquare, BattleSquare> modifyByShipIndex)
        {
            int col = ship.X;
            int row = ship.Y;

            for (int i = 0; i < ship.Size; i++, MoveToStart(ref col, ref row, ship.Orientation))
            {
                _field[row, col] = modifyByShipIndex(_field[row, col]);
            }
        }

        private void MarkBorder(Ship ship, Func<BattleSquare, BattleSquare> modify)
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

                _field[row, col] = modify(_field[row, col]);
            }
        }
    }
}