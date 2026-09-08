# GameServer

自研 C# TCP 游戏服务器，为《[原神风格 3D 射击 Demo](https://github.com/huaianz/shoot-yuanshen)》提供账号与云存档服务。纯手写 TCP 协议，不依赖 Web 框架。

## 功能特性

- **注册 / 登录**：用户名查重；密码用 PBKDF2 加盐哈希（10 万次迭代），验证使用固定时间比较，防时序攻击
- **Token 会话**：登录成功签发 token，服务端内存保存 `token → 用户名`（并发安全）
- **云存档**：金币、背包、角色等级经验、委托进度整体存取（JSON 字符串，服务器只存不解析）
- **心跳保活**：连接空闲超过 60 秒自动断开
- **安全**：全部 SQL 使用参数化查询防注入；数据库文件已加入 .gitignore，不会上传用户数据
- **MVC 分层**：Network / Controller / Repository / Models / Views，构造器注入组装

## 技术栈

- .NET 8 + C#（async/await 全异步）
- SQLite（Microsoft.Data.Sqlite）
- TCP 自定义二进制协议：`4 字节消息体长度 + 4 字节消息 ID + UTF-8 JSON`
- 每连接独立异步读循环 + 心跳检测协程

## 项目结构

```
GameServer/
├── Program.cs              入口：组装数据库/仓库/会话管理器，启动服务器
├── Core/                   配置、日志、密码哈希、会话管理
│   ├── Config.cs           端口 8888、心跳超时 60 秒
│   ├── PasswordHasher.cs   PBKDF2 加盐哈希
│   └── SessionManager.cs   token 会话表
├── Network/                TCP 收发、拆包、路由
│   ├── Server.cs           监听 + 注册消息处理器
│   ├── Connection.cs       读循环 / 心跳检测 / 发送
│   ├── Message.cs          协议打包与消息 ID 定义
│   └── Router.cs           按 msgId 分发到处理函数
├── Controllers/            业务编排
│   ├── AccountController.cs  注册 / 登录
│   └── PlayerController.cs   拉取 / 保存存档
├── Data/                   数据库访问层（参数化 SQL）
│   ├── Database.cs         建表 + 旧库自动补列
│   ├── UserRepository.cs
│   └── PlayerDataRepository.cs
├── Models/                 数据实体
└── Views/                  请求 / 响应 DTO
```

## 运行方法

需要 [.NET 8 SDK](https://dotnet.microsoft.com/)。在仓库根目录执行：

```bash
dotnet run --project GameServer
```

正常启动日志：

```
[INFO] GameServer 启动中...
[INFO] [数据库] users / player_data 表已就绪
[INFO] [服务器] 已启动, 监听端口 8888
```

- 默认监听 `8888`，可在 `Core/Config.cs` 修改
- SQLite 数据库文件 `game.db` 首次启动自动创建（已 gitignore）

## 协议说明

每条消息：`[4 字节消息体长度][4 字节消息 ID][消息体 JSON(UTF-8)]`

| 消息 ID | 名称 | 方向 | 说明 |
| --- | --- | --- | --- |
| 1 | 心跳 | 双向 | 服务器收到后回复 `{}` |
| 100 | 注册 | C→S | 用户名 + 密码 |
| 101 | 登录 | C→S | 成功返回 token |
| 200 | 拉取存档 | C→S | 携带 token |
| 201 | 保存存档 | C→S | 携带 token + 存档数据 |

响应统一包含 `code`（0=成功，1=失败）与 `msg`。

### 示例

**注册（100）**

```json
请求: {"username":"test","password":"123456"}
响应: {"code":0,"msg":"注册成功","username":"test","token":null}
```

**登录（101）**

```json
请求: {"username":"test","password":"123456"}
响应: {"code":0,"msg":"登录成功","username":"test","token":"60e9428fc7394b22a3a59fff0e63c9..."}
```

**拉取存档（200）**

```json
请求: {"token":"60e9428fc7394b22a3a59fff0e63c9..."}
响应: {"code":0,"msg":"ok","username":"test","coin":100,
      "inventoryJson":"[]","roleDataJson":"{}","questDataJson":"{}"}
```

**保存存档（201）**

```json
请求: {"token":"...","coin":500,"inventoryJson":"[]","roleDataJson":"{}","questDataJson":"{}"}
响应: {"code":0,"msg":"保存成功","username":"test","coin":500,
      "inventoryJson":"[]","roleDataJson":"{}","questDataJson":"{}"}
```

## 数据库

首次启动自动建表，旧库缺列时自动 `ALTER TABLE` 兼容：

- `users`：`id`、`username`(唯一)、`salt`、`password_hash`、`create_time`
- `player_data`：`username`(主键)、`coin`(默认100)、`inventory_json`、`role_data`、`quest_data`、`updated_at`

## 相关

- 客户端工程：[shoot-yuanshen](https://github.com/huaianz/shoot-yuanshen)
