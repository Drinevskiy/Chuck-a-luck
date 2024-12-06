using Domain.Models;

namespace Client.Services
{
    public interface IGameService
    {
        public Task ConnectToHub();
        public IDisposable CreateConnection(string method, Action handler);
        public IDisposable CreateConnection(string method, Action<string> handler);
        public IDisposable CreateConnection(string method, Action<Player> handler);
        public IDisposable CreateConnection(string method, Action<List<int>> handler);
        public IDisposable CreateConnection(string method, Action<int, int, bool> handler);
        public void RemoveConnections(string method);
        public Task<bool> CreateGame(string game, string username);
        public Task<bool> JoinGame(string game, string username);
        public Task<bool> StartGame(string game);
        public Task<bool> PlaceBet(string game, BetType currentBetType, int selectedNumber, int currentBetAmount);
        public Task<string> GetOpponentField(string game, string username);
        public Task Move(string game, string username, int x, int y, bool shot);
        public Task<List<Player>> GetUsersInfo(string gamename);
        public Task AddContent(string username, string content);
        public Task<string> GetMove(string game);
        public Task AddMove(string game, string username);
        public Task EndGame(string game, string username);
        public Task DeleteGame(string game);
    }
}
