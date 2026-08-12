namespace GameServer.Models
{
    /// <summary>
    /// 玩家数据实体: 对应 player_data 表的一行
    /// InventoryJson 是整个背包的 JSON 字符串(客户端负责序列化)
    /// </summary>
    public class PlayerData
    {
        public string Username { get; set; }
        public int Coin { get; set; } = 100;              // 初始金币100
        public string InventoryJson { get; set; } = "[]"; // 初始空背包
        public string UpdatedAt { get; set; }
    }
}