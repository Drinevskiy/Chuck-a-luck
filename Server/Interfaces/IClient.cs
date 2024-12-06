using Domain.Models;

namespace Server.Interfaces
{
    public interface IClient
    {
        Task PlayerJoined(string playerName);
        Task BetPlaced(BetType betType, int betNumber, int betAmount, int balance);
        Task GameResult(List<int> diceRolls, int playerResult, int balance);
        Task ErrorMessage(string message);
        Task Start();
    }
}
