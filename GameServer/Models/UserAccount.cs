using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Models
{
    /// <summary>
    /// 账号实体
    /// </summary>
    public class UserAccount
    {
        //主键
        public int Id { get; set; }
        //用户名
        public string Username {  get; set; }
        //密码盐值
        public string Salt {  get; set; }
        //密码哈希值
        public string PasswordHash {  get; set; }
        //注册时间
        public string CreateTime {  get; set; }
    }
}
