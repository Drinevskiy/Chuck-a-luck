using Microsoft.AspNetCore.SignalR;
using Server.Logic;
using Server.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Server.Hubs
{
    public class GameHub : Hub
    {
        // Хранение игр (группы игроков)
        private static ConcurrentDictionary<string, List<Player>> _gameGroups = new ConcurrentDictionary<string, List<Player>>();
        private GameLogic _gameLogic = new GameLogic();

        public async Task CreateGame(string groupName, string playerName)
        {
            if (_gameGroups.ContainsKey(groupName))
            {
                await Clients.Caller.SendAsync("ErrorMessage", $"Комната с именем '{groupName}' уже существует.");
                return;
            }

            var player = new Player
            {
                ConnectionId = Context.ConnectionId,
                Name = playerName
            };

            _gameGroups[groupName] = new List<Player> { player };

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task JoinGame(string groupName, string playerName)
        {
            if (!_gameGroups.ContainsKey(groupName))
            {
                await Clients.Caller.SendAsync("ErrorMessage", $"Комната с именем '{groupName}' не найдена.");
                return;
            }

            if (_gameGroups[groupName].Count >= 2)
            {
                await Clients.Caller.SendAsync("ErrorMessage", $"Комната '{groupName}' уже заполнена.");
                return;
            }

            var player = new Player
            {
                ConnectionId = Context.ConnectionId,
                Name = playerName
            };

            _gameGroups[groupName].Add(player);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);


            if (_gameGroups[groupName].Count == 2)
            {
                await Clients.Group(groupName).SendAsync("Start");
            }
        }

        public Task<List<Player>> GetPlayersInGroup(string groupName)
        {
            if (_gameGroups.TryGetValue(groupName, out var players))
            {
                var caller = players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);

                if (caller != null)
                {
                    var opponent = players.FirstOrDefault(p => p.ConnectionId != Context.ConnectionId);
                    var orderedPlayers = new List<Player> { caller };

                    if (opponent != null)
                    {
                        orderedPlayers.Add(opponent);
                    }

                    return Task.FromResult(orderedPlayers);
                }
            }

            return Task.FromResult(new List<Player>());
        }

        public async Task PlaceBet(string groupName, BetType betType, int betNumber, int betAmount)
        {
            if (_gameGroups.TryGetValue(groupName, out var players))
            {
                // Найти текущего игрока
                var player = players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (player != null)
                {
                    if (betAmount < 10)
                    {
                        await Clients.Caller.SendAsync("ErrorMessage", "Минимальная ставка — 10.");
                        return;
                    }

                    if (player.Balance < betAmount)
                    {
                        await Clients.Caller.SendAsync("ErrorMessage", "Недостаточно средств для ставки.");
                        return;
                    }

                    // Обновляем данные о ставке игрока
                    player.BetType = betType;
                    player.BetNumber = betNumber;
                    player.BetAmount = betAmount;
                    player.Balance -= betAmount;

                    // Найти соперника
                    var opponent = players.FirstOrDefault(p => p.ConnectionId != Context.ConnectionId);
                    if (opponent != null)
                    {
                        // Отправить информацию о ставке только сопернику
                        await Clients.Client(opponent.ConnectionId).SendAsync("PlaceBetOpponent", player);
                    }
                        // Отправить информацию о ставке только сопернику
                        await Clients.Caller.SendAsync("PlaceBetCurrent", player);
                }
            }
        }
        public async Task StartGame(string groupName)
        {
            if (_gameGroups.TryGetValue(groupName, out var players))
            {
                var result = _gameLogic.PlayGame(players);

                var diceRolls = result.DiceRolls; // Assuming DiceRolls is a suitable type for serialization
                await Clients.Group(groupName).SendAsync("DiceRollResults", diceRolls);


                var removedPlayers = new List<Player>();
                foreach (var player in players)
                {
                    if (result.PlayerResults.TryGetValue(player.ConnectionId, out var payout))
                    {
                        player.BetType = BetType.Number;
                        player.BetAmount = 10;
                        player.BetNumber = 1;

                        if (player.Balance >= 500)
                        {
                            await Clients.Client(player.ConnectionId).SendAsync("GameResult", player);
                            await Clients.Client(player.ConnectionId).SendAsync("ErrorMessage", "Вы выиграли игру, ваш баланс: " + player.Balance);
                            removedPlayers.Add(player);
                        }
                        else if (player.Balance <= 0)
                        {
                            await Clients.Client(player.ConnectionId).SendAsync("GameResult", player);
                            await Clients.Client(player.ConnectionId).SendAsync("ErrorMessage", "Вы проиграли игру, ваш баланс: 0.");
                            removedPlayers.Add(player);
                        }
                        else
                        {
                            await Clients.Client(player.ConnectionId).SendAsync("GameResult", player);
                        }
                    }
                }

                foreach (var player in removedPlayers)
                {
                    players.Remove(player);
                }

                if (!players.Any())
                {
                    _gameGroups.TryRemove(groupName, out _);
                }
            }
        }
    }
}