namespace GameServer
{
    /// <summary>
    /// 配置类: 集中放服务器的可调参数
    /// </summary>
    public static class Config
    {
        public const int Port = 8888;               // 监听端口
        public const int HeartbeatTimeoutSec = 60;  // 60秒没消息就踢下线
    }
}