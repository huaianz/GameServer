using System;
using System.Text.Json;
using System.Threading.Tasks;
using GameServer.Core;
using GameServer.Data;
using GameServer.Models;
using GameServer.Views;

namespace GameServer.Controllers
{
    /// <summary>
    /// 账号业务: 真正的注册/登录, 接数据库 + 会话令牌
    /// </summary>
    public class AccountController
    {
        private readonly UserRepository _users;
        private readonly SessionManager _sessions;

        // 依赖注入: 需要的组件由外面传进来
        public AccountController(UserRepository users, SessionManager sessions)
        {
            _users = users;
            _sessions = sessions;
        }

        /// <summary>
        /// 注册: 查重 -> 加盐哈希 -> 入库
        /// </summary>
        public async Task OnRegister(Connection conn, string jsonBody)
        {
            RegisterRequest req;
            try
            {
                req = JsonSerializer.Deserialize<RegisterRequest>(jsonBody);
            }
            catch
            {
                await Reply(conn, MsgId.Register, 1, "请求格式错误", null, null);
                return;
            }

            if (string.IsNullOrEmpty(req?.username) || string.IsNullOrEmpty(req.password))
            {
                await Reply(conn, MsgId.Register, 1, "用户名或密码不能为空", null, null);
                return;
            }

            if (await _users.IsUsernameExistsAsync(req.username))
            {
                await Reply(conn, MsgId.Register, 1, "用户名已存在", null, null);
                return;
            }

            string salt = PasswordHasher.GenerateSalt();
            var account = new UserAccount
            {
                Username = req.username,
                Salt = salt,
                PasswordHash = PasswordHasher.Hash(req.password, salt),
                CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            await _users.CreateUserAsync(account);

            Logger.Info($"[注册] {req.username}");
            await Reply(conn, MsgId.Register, 0, "注册成功", req.username, null);
        }

        /// <summary>
        /// 登录: 查账号 -> 验证密码 -> 发会话令牌
        /// </summary>
        public async Task OnLogin(Connection conn, string jsonBody)
        {
            LoginRequest req;
            try
            {
                req = JsonSerializer.Deserialize<LoginRequest>(jsonBody);
            }
            catch
            {
                await Reply(conn, MsgId.Login, 1, "请求格式错误", null, null);
                return;
            }

            if (string.IsNullOrEmpty(req?.username) || string.IsNullOrEmpty(req.password))
            {
                await Reply(conn, MsgId.Login, 1, "用户名或密码不能为空", null, null);
                return;
            }

            var account = await _users.GetUserByUsernameAsync(req.username);
            if (account == null)
            {
                await Reply(conn, MsgId.Login, 1, "用户不存在", null, null);
                return;
            }

            if (!PasswordHasher.Verify(req.password, account.Salt, account.PasswordHash))
            {
                await Reply(conn, MsgId.Login, 1, "密码错误", null, null);
                return;
            }

            string token = _sessions.CreateSession(req.username);
            Logger.Info($"[登录] {req.username}");
            await Reply(conn, MsgId.Login, 0, "登录成功", req.username, token);
        }

        /// <summary>
        /// 统一的响应发送
        /// </summary>
        private async Task Reply(Connection conn, int msgId, int code, string msg, string username, string token)
        {
            var resp = new AccountResponse { code = code, msg = msg, username = username, token = token };
            await conn.SendAsync(msgId, JsonSerializer.Serialize(resp));
        }
    }
}