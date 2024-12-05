using Microsoft.AspNetCore.SignalR;
using Server.Interfaces;
using Server.Logic;
using Server.Models;
using System;
using System.Collections.Concurrent;
using System.Data.Common;

namespace Server.Hubs
{
    public class GameHub : Hub<IClient>
    {
        // Хранение игр (группы игроков)
        private static ConcurrentDictionary<string, List<Player>> _gameGroups = new ConcurrentDictionary<string, List<Player>>();
        private GameLogic _gameLogic = new GameLogic();

        // Подключение к игре
        public async Task JoinGame(string groupName, string playerName)
        {
            // Добавить игрока в группу
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // Добавить игрока в список игроков группы
            var player = new Player
            {
                ConnectionId = Context.ConnectionId,
                Name = playerName
            };

            _gameGroups.AddOrUpdate(groupName, new List<Player> { player }, (key, players) =>
            {
                players.Add(player);
                return players;
            });

            // Уведомить группу
            await Clients.Group(groupName).PlayerJoined(playerName);
        }

        // Сделать ставку
        public async Task PlaceBet(string groupName, BetType betType, int betNumber, int betAmount)
        {
            // Найти группу игроков
            if (_gameGroups.TryGetValue(groupName, out var players))
            {
                // Найти игрока
                var player = players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (player != null)
                {
                    // Проверить минимальную ставку
                    if (betAmount < 10)
                    {
                        await Clients.Caller.ErrorMessage("Минимальная ставка — 10.");
                        return;
                    }

                    // Проверить, хватает ли баланса для ставки
                    if (player.Balance < betAmount)
                    {
                        await Clients.Caller.ErrorMessage("Недостаточно средств для ставки.");
                        return;
                    }

                    // Обновить данные о ставке
                    player.BetType = betType;
                    player.BetNumber = betNumber;
                    player.BetAmount = betAmount;
                    player.Balance -= betAmount; // Списать ставку с баланса

                    await Clients.Caller.BetPlaced(betType, betNumber, betAmount, player.Balance);
                }
            }
        }

        // Запустить игру
        public async Task StartGame(string groupName)
        {
            if (_gameGroups.TryGetValue(groupName, out var players))
            {
                // Запустить игру
                var result = _gameLogic.PlayGame(players);

                // Проверить победителей и проигравших
                var removedPlayers = new List<Player>();
                foreach (var player in players)
                {
                    if (result.PlayerResults.TryGetValue(player.ConnectionId, out var playerResult))
                    {
                        // Проверить, выиграл ли игрок
                        if (player.Balance >= 500)
                        {
                            await Clients.Client(player.ConnectionId).ErrorMessage("Вы выиграли игру, ваш баланс: " + player.Balance);
                            removedPlayers.Add(player);
                        }
                        // Проверить, проиграл ли игрок
                        else if (player.Balance <= 0)
                        {
                            await Clients.Client(player.ConnectionId).ErrorMessage("Вы проиграли игру, ваш баланс: 0.");
                            removedPlayers.Add(player);
                        }
                        else
                        {
                            await Clients.Client(player.ConnectionId).GameResult(result.DiceRolls, playerResult, player.Balance);
                        }
                    }
                }

                // Удалить проигравших и победителей из группы
                foreach (var player in removedPlayers)
                {
                    players.Remove(player);
                }

                // Если все игроки покинули группу, удалить её
                if (!players.Any())
                {
                    _gameGroups.TryRemove(groupName, out _);
                }
            }
        }
    }
}