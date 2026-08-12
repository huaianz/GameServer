using GameServer.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data
{
    /// <summary>
    /// 账号数据访问层，只管数据库, 不管业务
    /// </summary>
    public class UserRepository
    {
        private readonly Database _db;

        /// <summary>
        /// 接收一个 Database 实例
        /// </summary>
        /// <param name="db"></param>
        public UserRepository(Database db)
        {
            _db = db;
        }

        /// <summary>
        /// 用户名是否已存在
        /// </summary>
        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            //向Database要一个“连接”（开门）
            await using var conn = _db.CreateConnection();
            //异步打开连接
            await conn.OpenAsync();
            //创建SQL命令
            await using var cmd = new SqliteCommand(
                "SELECT COUNT(*) FROM users WHERE username = @u", conn);
            //把C#变量 username 的值安全地填入 @u 占位符
            cmd.Parameters.AddWithValue("@u", username);
            long count = (long)(await cmd.ExecuteScalarAsync() ?? 0L);
            return count > 0;
        }

        /// <summary>
        /// 插入新账号
        /// </summary>
        public async Task CreateUserAsync(UserAccount account)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqliteCommand(
                @"INSERT INTO users (username, salt, password_hash, create_time)
                  VALUES (@u, @s, @h, @t)", conn);
            cmd.Parameters.AddWithValue("@u", account.Username);
            cmd.Parameters.AddWithValue("@s", account.Salt);
            cmd.Parameters.AddWithValue("@h", account.PasswordHash);
            cmd.Parameters.AddWithValue("@t", account.CreateTime);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// 按用户名查账号(登录用)
        /// </summary>
        public async Task<UserAccount> GetUserByUsernameAsync(string username)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqliteCommand(
                "SELECT id, username, salt, password_hash, create_time FROM users WHERE username = @u", conn);
            cmd.Parameters.AddWithValue("@u", username);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UserAccount
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Salt = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    CreateTime = reader.GetString(4)
                };
            }
            return null;
        }
    }
}
