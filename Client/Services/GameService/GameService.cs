using Domain.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
//using Newtonsoft.Json.Linq;


namespace Client.Services
{
    public class GameService : IGameService
    {
        private readonly HubConnection _connection;
        private readonly IJSRuntime _jsRuntime;

        public GameService(IJSRuntime jsRuntime)
        {
            _connection = new HubConnectionBuilder()
                        .WithUrl("https://localhost:7190/gamehub")
                        .Build();

            _jsRuntime = jsRuntime;
        }

        public async Task ConnectToHub()
        {
            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public IDisposable CreateConnection(string method, Action handler)
        {
            return _connection.On(method, handler);
        }

        public IDisposable CreateConnection(string method, Action<Player> handler)
        {
            return _connection.On<Player>(method, handler);
        }

        public IDisposable CreateConnection(string method, Action<List<int>> handler)
        {
            return _connection.On<List<int>>(method, handler);
        }

        public IDisposable CreateConnection(string method, Action<string> handler)
        {
            return _connection.On(method, handler);
        }

        public IDisposable CreateConnection(string method, Action<int, int, bool> handler)
        {
            return _connection.On(method, handler);
        }

        

        public void RemoveConnections(string method)
        {
            _connection.Remove(method);
        }

        public async Task<bool> CreateGame(string game, string username)
        {
            try
            {
                await _connection.InvokeAsync("CreateGame", game, username);
                return true;
            }
            catch (HubException ex)
            {
                await _jsRuntime.InvokeVoidAsync("alert", ex.Message);

                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> JoinGame(string game, string username)
        {
            try
            {
                await _connection.InvokeAsync("JoinGame", game, username);
                return true;
            }
            catch (HubException ex)
            {
                await _jsRuntime.InvokeVoidAsync("alert", ex.Message);

                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<List<Player>> GetUsersInfo(string gamename)
        {
            try
            {
                var result = await _connection.InvokeAsync<List<Player>>("GetPlayersInGroup", gamename);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public async Task<bool> PlaceBet(string game, BetType currentBetType, int selectedNumber, int currentBetAmount)
        {
            try
            {
                var result = await _connection.InvokeAsync<string>("PlaceBet", game, currentBetType, selectedNumber, currentBetAmount);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }


        public async Task<bool> StartGame(string gamename)
        {
            try
            {
                await _connection.InvokeAsync("StartGame", gamename);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<string> GetOpponentField(string game, string username)
        {
            try
            {
                var result = await _connection.InvokeAsync<string>("GetOpponentField", game, username);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        public async Task Move(string game, string username, int x, int y, bool shot)
        {
            try
            {
                await _connection.InvokeAsync("Move", game, username, x, y, shot);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }

        

        public async Task AddContent(string username, string content)
        {
            try
            {
                await _connection.InvokeAsync("AddContent", username, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }

        public async Task<string> GetMove(string game)
        {
            try
            {
                var result = await _connection.InvokeAsync<string>("GetMove", game);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        public async Task AddMove(string game, string username)
        {
            try
            {
                await _connection.InvokeAsync("AddMove", game, username);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }

        public async Task EndGame(string game, string username)
        {
            try
            {
                await _connection.InvokeAsync("EndGame", game, username);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }

        public async Task DeleteGame(string game)
        {
            try
            {
                await _connection.InvokeAsync("DeleteGame", game);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }
    }
}
