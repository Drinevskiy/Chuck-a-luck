namespace Server.Models
{
    public class Player
    {
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public BetType BetType { get; set; } // Тип ставки
        public int BetNumber { get; set; }  // Число для ставки (только для BetType.Number)
        public int BetAmount { get; set; } // Размер ставки
        public int Balance { get; set; } = 100; // Баланс игрока (начальное значение — 100)
    }
}
