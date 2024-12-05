using Server.Models;

namespace Server.Logic
{
    public class GameLogic
    {
        private static Random _random = new Random();

        public GameResult PlayGame(List<Player> players)
        {
            // 1. Бросить три кубика
            var diceRolls = new List<int>
            {
                _random.Next(1, 7),
                _random.Next(1, 7),
                _random.Next(1, 7)
            };

            // 2. Посчитать сумму кубиков
            int sum = diceRolls.Sum();

            // 3. Посчитать результаты для каждого игрока
            var playerResults = new Dictionary<string, int>();
            foreach (var player in players)
            {
                int payout = 0;

                switch (player.BetType)
                {
                    case BetType.Number:
                        // Ставка на число
                        int matches = diceRolls.Count(d => d == player.BetNumber);
                        payout = matches > 0 ? player.BetAmount * matches : -player.BetAmount;
                        break;

                    case BetType.Small:
                        // Ставка на "Малые" числа (4–10), исключая тройки
                        if (sum >= 4 && sum <= 10 && !IsTriple(diceRolls))
                        {
                            payout = player.BetAmount;
                        }
                        else
                        {
                            payout = -player.BetAmount;
                        }
                        break;

                    case BetType.Big:
                        // Ставка на "Большие" числа (11–17), исключая тройки
                        if (sum >= 11 && sum <= 17 && !IsTriple(diceRolls))
                        {
                            payout = player.BetAmount;
                        }
                        else
                        {
                            payout = -player.BetAmount;
                        }
                        break;

                    case BetType.Triple:
                        // Ставка на тройку (все кубики одинаковые)
                        if (IsTriple(diceRolls))
                        {
                            payout = player.BetAmount * 30; // Выигрыш 30:1
                        }
                        else
                        {
                            payout = -player.BetAmount;
                        }
                        break;
                }

                // Обновить баланс игрока
                player.Balance += payout;

                // Сохранить результат для передачи игроку
                playerResults[player.ConnectionId] = payout;
            }

            // 4. Вернуть результат игры
            return new GameResult
            {
                DiceRolls = diceRolls,
                PlayerResults = playerResults
            };
        }

        private bool IsTriple(List<int> diceRolls)
        {
            return diceRolls.Distinct().Count() == 1;
        }
    }

}
