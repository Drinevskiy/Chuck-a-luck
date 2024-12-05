using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Share.Models
{

    public class BattleShipGame
    {
        public string Player1Name { get; init; }
        public string Player2Name { get; init; }
        private readonly Grid _player1Grid;
        private readonly Grid _player2Grid;
        public bool IsPlayer1Turn { get; private set; }
        private Dictionary<Action<string>, (Action onPlayer1Lost, Action onPlayer2Lost)>? _eventsDictionary;

        public event Action<string>? OnGameEnd
        {
            add
            {
                if (value is not null)
                {
                    Action onPlayer1Lost = () => value(Player2Name);
                    Action onPlayer2Lost = () => value(Player1Name);

                    _eventsDictionary ??= new();

                    _eventsDictionary[value] = (onPlayer1Lost, onPlayer2Lost);

                    _player1Grid.OnAllShipDestroyed += onPlayer1Lost;
                    _player2Grid.OnAllShipDestroyed += onPlayer2Lost;
                }
            }
            remove
            {
                if (value is not null && _eventsDictionary is not null)
                {
                    _player1Grid.OnAllShipDestroyed -= _eventsDictionary[value].onPlayer1Lost;
                    _player2Grid.OnAllShipDestroyed -= _eventsDictionary[value].onPlayer2Lost;

                    _eventsDictionary.Remove(value);
                }
            }
        }

        public BattleShipGame(string player1Name, string player2Name, Grid player1Grid, Grid player2Grid, bool isPlayer1Move = true)
        {
            Player1Name = player1Name;
            Player2Name = player2Name;

            _player1Grid = player1Grid;
            _player2Grid = player2Grid;

            IsPlayer1Turn = isPlayer1Move;
        }

        public ShotStatus? Player1Turn(int x, int y)
        {
            if (IsPlayer1Turn)
            {
                var res = _player2Grid.Shot(x, y);

                if (res == ShotStatus.Miss)
                    IsPlayer1Turn = false;

                return res;
            }

            return null;
        }

        public ShotStatus? Player2Turn(int x, int y)
        {
            if (!IsPlayer1Turn)
            {
                var res = _player1Grid.Shot(x, y);

                if (res == ShotStatus.Miss)
                    IsPlayer1Turn = true;

                return res;
            }

            return null;
        }


        public IEnumerable<IEnumerable<BattleSquare>> Player1Rows => _player1Grid.Rows;
        public IEnumerable<IEnumerable<BattleSquare>> Player2Rows => _player2Grid.Rows;

        public override string ToString()
        {
            return $"""
            {Player1Name} grid:
            {_player1Grid}

            {Player2Name} grid:
            {_player2Grid}

            Turn: {(IsPlayer1Turn ? Player1Name : Player2Name)}
            """;
        }
    }
}