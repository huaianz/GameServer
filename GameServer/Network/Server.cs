using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using GameServer.Controllers;
using GameServer.Core;
using GameServer.Data;

namespace GameServer
{
    public class Server
    {
        private readonly Router _router = new();
        private readonly UserRepository _users;
        private readonly PlayerDataRepository _playerData;
        private readonly SessionManager _sessions;

        public Server(UserRepository users, PlayerDataRepository playerData, SessionManager sessions)
        {
            _users = users;
            _playerData = playerData;
            _sessions = sessions;
        }

        public async Task StartAsync(int port)
        {
            _router.Register(MsgId.Heartbeat, async (conn, json) =>
            {
                await conn.SendAsync(MsgId.Heartbeat, "{}");
            });

            AccountController account = new AccountController(_users, _sessions);
            _router.Register(MsgId.Register, account.OnRegister);
            _router.Register(MsgId.Login, account.OnLogin);

            PlayerController player = new PlayerController(_playerData, _sessions);
            _router.Register(MsgId.GetPlayerData, player.OnGetPlayerData);
            _router.Register(MsgId.SavePlayerData, player.OnSavePlayerData);

            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Logger.Info($"[服务器] 已启动, 监听端口 {port}");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Logger.Info($"[连接] {client.Client?.RemoteEndPoint?.ToString() ?? "?"}");
                _ = HandleConnectionAsync(client);
            }
        }

        private async Task HandleConnectionAsync(TcpClient client)
        {
            Connection conn = new Connection(client);
            await conn.RunAsync(_router.Route);
        }
    }
}