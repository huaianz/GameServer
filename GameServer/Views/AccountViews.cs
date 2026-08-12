using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Views
{
    #region 客户端发给服务器
    public class RegisterRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class LoginRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }
    #endregion

    #region 服务器返回给客户端
    public class AccountResponse
    {
        //状态码
        public int code { get; set; }        // 0=成功 1=失败
        public string msg { get; set; }
        public string username { get; set; }
        public string token { get; set; }    // 会话令牌
    }
    #endregion
}
