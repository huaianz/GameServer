using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    /// <summary>
    /// 路由，按msgId处理函数并调用
    /// </summary>
    public class Router
    {
        private readonly Dictionary<int, Func<Connection, string, Task>> _handlers = new();

        /// <summary>
        /// 注册一个消息处理器
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="handler"></param>
        public void Register(int msgId, Func<Connection, string, Task> handler)
        {
            _handlers[msgId] = handler;
        }

        /// <summary>
        /// 消息路由入口
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task Route(Connection conn, Message msg)
        {
            if (_handlers.TryGetValue(msg.MsgId, out var handler))
            {
                try
                {
                    //异步等待处理函数
                    await handler(conn, msg.Body);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[处理异常] msgId={msg.MsgId}: {ex}");
                }
            }
            else
            {
                Logger.Warn($"[路由] 未知 msgId={msg.MsgId}, 来源 {conn.RemoteEnd}");
            }
        }
    }
}
