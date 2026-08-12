using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class Connection
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly CancellationTokenSource _cts = new();

        public string RemoteEnd => _client.Client?.RemoteEndPoint?.ToString() ?? "?";
        public DateTime LastReceiveTime { get; private set; } = DateTime.UtcNow;

        public Connection(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
        }

        /// <summary>
        ///  读消息循环 + 心跳检测循环 同时跑;
        ///  读循环结束(客户端断开)就停掉心跳检测并关闭连接
        /// </summary>
        /// <param name="onMessage"></param>
        /// <returns></returns>
        public async Task RunAsync(Func<Connection, Message, Task> onMessage)
        {
            Task readTask = ReadLoopAsync(onMessage);
            Task checkTask = CheckHeartbeatLoopAsync();

            await readTask;       // 阻塞在这里, 直到这个连接断开
            _cts.Cancel();        // 通知心跳循环停止
            await Task.WhenAll(checkTask);
            _client.Close();
            Logger.Info($"[断开] {RemoteEnd}");
        }

        /// <summary>
        /// 消息读取和协议解析
        /// </summary>
        /// <param name="onMessage"></param>
        /// <returns></returns>
        private async Task ReadLoopAsync(Func<Connection, Message, Task> onMessage)
        {
            try
            {
                while (true)
                {
                    //先读长度头
                    byte[] lenBuf = new byte[4];
                    if (!await ReadExactlyAsync(lenBuf, 4)) break;
                    //读消息类型
                    byte[] idBuf = new byte[4];
                    if (!await ReadExactlyAsync(idBuf, 4)) break;
                    //解析长度并校验
                    int len = BitConverter.ToInt32(lenBuf, 0);
                    if (len <= 0 || len > 65536) break;
                    //读正文
                    byte[] bodyBuf = new byte[len];
                    if (!await ReadExactlyAsync(bodyBuf, len)) break;

                    //刷新心跳时间
                    LastReceiveTime = DateTime.UtcNow;
                    //二进制转json文本
                    string json = Encoding.UTF8.GetString(bodyBuf);
                    await onMessage(this,
                        new Message { MsgId = BitConverter.ToInt32(idBuf, 0), Body = json });
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"[读循环异常] {RemoteEnd}: {ex.Message}");
            }
        }

        /// <summary>
        /// 心跳检测，每五秒看一次，超过60秒没消息踢下线
        /// </summary>
        /// <returns></returns>
        private async Task CheckHeartbeatLoopAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(5000, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                double idle = (DateTime.UtcNow - LastReceiveTime).TotalSeconds;
                if (idle > Config.HeartbeatTimeoutSec)
                {
                    Logger.Warn($"[心跳超时] {RemoteEnd} 空闲 {idle:F0} 秒, 踢下线");
                    break;
                }
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public async Task SendAsync(int msgId, string json)
        {
            //将消息ID和JSON字符串编码成字节数组
            byte[] data = Message.Pack(msgId, json);
            await _stream.WriteAsync(data, 0, data.Length);
            await _stream.FlushAsync();
        }

        /// <summary>
        /// 循环读取直到凑够指定字节数，防止出现收到半包
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private async Task<bool> ReadExactlyAsync(byte[] buffer, int count)
        {
            int total = 0;
            while (total < count)
            {
                int n = await _stream.ReadAsync(buffer, total, count - total, _cts.Token);
                if (n == 0) return false; // 对端关闭
                total += n;
            }
            return true;
        }
    }
}
