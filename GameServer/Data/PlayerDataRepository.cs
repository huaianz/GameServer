using System;
using System.Threading.Tasks;
using GameServer.Models;
using Microsoft.Data.Sqlite;

namespace GameServer.Data
{
    /// <summary>
    /// 玩家数据存取: 读/建/更新
    /// </summary>
    public class PlayerDataRepository
    {
        private readonly Database _db;

        public PlayerDataRepository(Database db)
        {
            _db = db;
        }

        /// <summary>
        /// 取玩家数据; 没有就自动建一行(初始100金币, 空背包, 空角色数据)
        /// </summary>
        public async Task<PlayerData> GetOrCreateAsync(string username)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            // 先查(注意列顺序和下面 reader.GetXxx(索引) 一一对应)
            await using var cmd = new SqliteCommand(
                "SELECT username, coin, inventory_json, role_data, updated_at FROM player_data WHERE username = @u", conn);
            cmd.Parameters.AddWithValue("@u", username);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new PlayerData
                {
                    Username = reader.GetString(0),      // username
                    Coin = reader.GetInt32(1),           // coin
                    InventoryJson = reader.GetString(2), // inventory_json
                    RoleDataJson = reader.GetString(3),  // role_data
                    UpdatedAt = reader.GetString(4)      // updated_at
                };
            }
            reader.Close();

            // 没有就插入默认行
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            await using var insert = new SqliteCommand(
                @"INSERT INTO player_data (username, coin, inventory_json, role_data, updated_at)
                  VALUES (@u, 100, '[]', '{}', @t)", conn);
            insert.Parameters.AddWithValue("@u", username);
            insert.Parameters.AddWithValue("@t", now);
            await insert.ExecuteNonQueryAsync();

            return new PlayerData
            {
                Username = username,
                Coin = 100,
                InventoryJson = "[]",
                RoleDataJson = "{}",
                UpdatedAt = now
            };
        }

        /// <summary>
        /// 保存/更新玩家数据(金币、背包、角色数据一起存)
        /// </summary>
        public async Task UpdateAsync(PlayerData data)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqliteCommand(
                @"UPDATE player_data SET coin = @c, inventory_json = @i, role_data = @r, updated_at = @t WHERE username = @u", conn);
            cmd.Parameters.AddWithValue("@c", data.Coin);
            cmd.Parameters.AddWithValue("@i", data.InventoryJson);
            cmd.Parameters.AddWithValue("@r", data.RoleDataJson);
            cmd.Parameters.AddWithValue("@t", data.UpdatedAt);
            cmd.Parameters.AddWithValue("@u", data.Username);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}