namespace GameServer.Views
{
    /// <summary>
    /// 获取玩家数据请求: 只需要带 token
    /// </summary>
    public class GetPlayerDataRequest
    {
        public string token { get; set; }
    }

    /// <summary>
    /// 保存玩家数据请求: 带 token + 要保存的内容
    /// </summary>
    public class SavePlayerDataRequest
    {
        public string token { get; set; }
        public int coin { get; set; }
        public string inventoryJson { get; set; }
        public string roleDataJson { get; set; }   //角色等级/经验
    }

    /// <summary>
    /// 玩家数据响应
    /// </summary>
    public class PlayerDataResponse
    {
        public int code { get; set; }
        public string msg { get; set; }
        public string username { get; set; }
        public int coin { get; set; }
        public string inventoryJson { get; set; }
        public string roleDataJson { get; set; }   //角色等级/经验
    }
}