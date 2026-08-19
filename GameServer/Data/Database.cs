using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace GameServer.Data
{
    public class Database
    {
        private readonly string _connectionString;

        public Database(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
        }

        public async Task InitAsync()
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
                CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT NOT NULL UNIQUE,
                    salt TEXT NOT NULL,
                    password_hash TEXT NOT NULL,
                    create_time TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS player_data (
                    username TEXT PRIMARY KEY,
                    coin INTEGER NOT NULL DEFAULT 100,
                    inventory_json TEXT NOT NULL DEFAULT '[]',
                    updated_at TEXT NOT NULL
                );";
            await using var cmd = new SqliteCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
            Logger.Info("[数据库] users / player_data 表已就绪");

            string checkSql = "SELECT COUNT(*) FROM pragma_table_info('player_data') WHERE name = 'role_data';";
            await using var checkCmd = new SqliteCommand(checkSql, conn);
            long colCount = (long)await checkCmd.ExecuteScalarAsync();
            if (colCount == 0)
            {
                await using var alterCmd = new SqliteCommand(
                    "ALTER TABLE player_data ADD COLUMN role_data TEXT NOT NULL DEFAULT '{}';", conn);
                await alterCmd.ExecuteNonQueryAsync();
                Logger.Info("[数据库] 已为 player_data 补充 role_data 列");
            }
        }

        public SqliteConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }
    }
}