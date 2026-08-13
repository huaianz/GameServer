using System;
using System.Text.Json;
using System.Threading.Tasks;
using GameServer.Core;
using GameServer.Data;
using GameServer.Views;

namespace GameServer.Controllers
{
    /// <summary>
    /// 玩家数据业务: 拉取/保存, 全部要求带 token(登录凭证)
    /// </summary>
    public class PlayerController
    {
        private readonly PlayerDataRepository _playerData;
        private readonly SessionManager _sessions;

        public PlayerController(PlayerDataRepository playerData, SessionManager sessions)
        {
            _playerData = playerData;
            _sessions = sessions;
        }

        /// <summary>
        /// 获取玩家数据(msgId=200)
        /// </summary>
        public async Task OnGetPlayerData(Connection conn, string jsonBody)
        {
            GetPlayerDataRequest req;
            try
            {
                req = JsonSerializer.Deserialize<GetPlayerDataRequest>(jsonBody);
            }
            catch
            {
                await Reply(conn, MsgId.GetPlayerData, 1, "请求格式错误", null, 0, null);
                return;
            }

            if (!_sessions.TryGetUsername(req?.token ?? "", out string username))
            {
                await Reply(conn, MsgId.GetPlayerData, 1, "未登录或登录已失效", null, 0, null);
                return;
            }

            var data = await _playerData.GetOrCreateAsync(username);
            Logger.Info($"[玩家数据] {username} 金币={data.Coin}");

            await Reply(conn, MsgId.GetPlayerData, 0, "ok", username, data.Coin, data.InventoryJson);
        }

        /// <summary>
        /// 保存玩家数据(msgId=201)
        /// </summary>
        public async Task OnSavePlayerData(Connection conn, string jsonBody)
        {
            SavePlayerDataRequest req;
            try
            {
                req = JsonSerializer.Deserialize<SavePlayerDataRequest>(jsonBody);
            }
            catch
            {
                await Reply(conn, MsgId.SavePlayerData, 1, "请求格式错误", null, 0, null);
                return;
            }

            if (!_sessions.TryGetUsername(req?.token ?? "", out string username))
            {
                await Reply(conn, MsgId.SavePlayerData, 1, "未登录或登录已失效", null, 0, null);
                return;
            }

            int coin = Math.Max(0, req.coin);

            var data = new Models.PlayerData
            {
                Username = username,
                Coin = coin,
                InventoryJson = req.inventoryJson ?? "[]",
                UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            await _playerData.UpdateAsync(data);

            Logger.Info($"[玩家数据] {username} 保存: 金币={coin}");
            await Reply(conn, MsgId.SavePlayerData, 0, "保存成功", username, coin, data.InventoryJson);
        }

        private async Task Reply(Connection conn, int msgId, int code, string msg, string username, int coin, string inventoryJson)
        {
            var resp = new PlayerDataResponse
            {
                code = code,
                msg = msg,
                username = username,
                coin = coin,
                inventoryJson = inventoryJson
            };
            await conn.SendAsync(msgId, JsonSerializer.Serialize(resp));
        }
    }
}