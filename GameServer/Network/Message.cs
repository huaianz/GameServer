using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    /// <summary>
    /// 消息ID，以后每加一个功能就加一个ID
    /// </summary>
    public static class MsgId
    {
        public const int Heartbeat = 1;   // 心跳
        public const int Register = 100;  // 注册
        public const int Login = 101;   //登录
        public const int GetPlayerData = 200; // 获取玩家数据
        public const int SavePlayerData = 201; // 保存玩家数据
    }

    public class Message
    {
        public int MsgId
        {
            get; set;
        }
        public string Body
        {
            get; set;
        } = "{}";

        /// <summary>
        /// 打包
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static byte[] Pack(int msgId, string json)
        {
            //将JSON字符串按UTF-8编码转换为字节序列
            byte[] body = Encoding.UTF8.GetBytes(json);
            byte[] len = BitConverter.GetBytes(body.Length);
            //将消息类型转换成4字节
            byte[] id = BitConverter.GetBytes(msgId);
            //之所以4+4，前面的4是消息体body长度,后一个是4是消息类型
            byte[] result = new byte[4 + 4 + body.Length];
            //拷贝进result
            Buffer.BlockCopy(len, 0, result, 0, 4);
            Buffer.BlockCopy(id, 0, result, 4, 4);
            Buffer.BlockCopy(body, 0, result, 8, body.Length);
            return result;
        }
    }
}
