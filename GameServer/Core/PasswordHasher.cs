using System;
using System.Security.Cryptography;

namespace GameServer.Core
{
    /// <summary>
    /// 密码哈希: 用 PBKDF2(Rfc2898DeriveBytes)。
    /// 比直接 SHA256 安全——自带"慢速+盐", 暴力破解成本高。
    /// </summary>
    public static class PasswordHasher
    {
        private const int Iterations = 100000; // 迭代次数(越大越慢越安全)
        private const int SaltSize = 16;       // 盐长度(字节)
        private const int HashSize = 32;       // 输出哈希长度(字节)

        /// <summary>
        /// 注册时生成随机盐
        /// </summary>
        public static string GenerateSalt()
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            return Convert.ToBase64String(salt);
        }

        /// <summary>
        /// 用"盐+密码"算哈希
        /// </summary>
        public static string Hash(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password, saltBytes, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(HashSize);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// 登录验证: 重算哈希并比对(固定时间比较, 防时序攻击)
        /// </summary>
        public static bool Verify(string password, string salt, string expectedHash)
        {
            string actual = Hash(password, salt);
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(actual),
                Convert.FromBase64String(expectedHash));
        }
    }
}