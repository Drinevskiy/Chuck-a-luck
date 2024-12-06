namespace Domain.Models
{
    public class GameResult
    {
        public List<int> DiceRolls { get; set; } // Результаты броска кубиков
        public Dictionary<string, int> PlayerResults { get; set; } // Результаты игроков (выигрыш или проигрыш)
    }
}
