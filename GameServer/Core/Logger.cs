using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    /// <summary>
    /// 日志
    /// </summary>
    public static class Logger
    {
        //日志文件按启动时间命名, 每次运行一个文件
        private static readonly string LogFile =
            $"logs/server_{DateTime.Now:yyyyMMdd_HHmmss}.log";
        //lock保证多个连接同时写日志时文件不会乱
        private static readonly object _lock = new object();
        static Logger()
        {
            Directory.CreateDirectory("logs");
        }

        public static void Info(string msg)=> Write("INFO", msg);
        public static void Warn(string msg) => Write("WARN", msg);
        public static void Error(string msg) => Write("ERROR", msg);

        private static void Write(string level, string msg)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] [{level}] {msg}";
            lock (_lock)
            {
                Console.WriteLine(line);
                File.AppendAllText(LogFile, line + Environment.NewLine);
            }
        }

    }
}
