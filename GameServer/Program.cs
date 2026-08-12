using System.Threading.Tasks;
using GameServer.Core;
using GameServer.Data;

namespace GameServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Logger.Info("GameServer 启动中...");

            Database db = new Database("game.db");
            await db.InitAsync();

            UserRepository users = new UserRepository(db);
            PlayerDataRepository playerData = new PlayerDataRepository(db);
            SessionManager sessions = new SessionManager();

            Server server = new Server(users, playerData, sessions);
            await server.StartAsync(Config.Port);
        }
    }
}