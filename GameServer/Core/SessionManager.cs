using System;
using System.Collections.Concurrent;

namespace GameServer.Core
{
    /// <summary>
    /// 会话管理: 登录成功后发 token, 服务器记住 "token -> 用户名"。
    /// ConcurrentDictionary 线程安全, 多连接并发访问不怕。
    /// </summary>
    public class SessionManager
    {
        private readonly ConcurrentDictionary<string, string> _sessions = new();

        /// <summary>
        /// 登录成功时: 生成 token 并登记
        /// </summary>
        public string CreateSession(string username)
        {
            string token = Guid.NewGuid().ToString("N");
            _sessions[token] = username;
            return token;
        }

        /// <summary>
        /// 用 token 查用户名(以后客户端带 token 校验)
        /// </summary>
        public bool TryGetUsername(string token, out string username)
        {
            return _sessions.TryGetValue(token, out username);
        }

        /// <summary>
        /// 注销时移除
        /// </summary>
        public void RemoveSession(string token)
        {
            _sessions.TryRemove(token, out _);
        }
    }
}