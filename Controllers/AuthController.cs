using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using StaticProgram;

namespace StaticProgram.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const string CONN=
        "Server=(localdb)\\mssqllocaldb;Database=ShopDb;Trusted_Connection=True;TrustServerCertificate=True;";
        
        public AuthController()
        {
            InitializeDatabase();
        }
        
        private void InitializeDatabase()
        {
            try
            {
                // First, connect to master database to create the ShopDb if it doesn't exist
                string masterConn = "Server=(localdb)\\mssqllocaldb;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
                using (var conn = new SqlConnection(masterConn))
                {
                    conn.Open();
                    string createDbSql = @"
                        IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ShopDb')
                        CREATE DATABASE ShopDb";
                    using var cmd = new SqlCommand(createDbSql, conn);
                    cmd.ExecuteNonQuery();
                }

                // Now connect to ShopDb and create tables
                using var shopConn = new SqlConnection(CONN);
                shopConn.Open();
                
                // Create Users table if it doesn't exist
                string createUsersTable = @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
                    CREATE TABLE Users (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Username NVARCHAR(50) NOT NULL,
                        Password NVARCHAR(50) NOT NULL
                    )";
                
                // Create Goods table if it doesn't exist
                string createGoodsTable = @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Goods')
                    CREATE TABLE Goods (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(100) NOT NULL,
                        Price DECIMAL(10,2) NOT NULL
                    )";
                
                using var cmd1 = new SqlCommand(createUsersTable, shopConn);
                cmd1.ExecuteNonQuery();
                
                using var cmd2 = new SqlCommand(createGoodsTable, shopConn);
                cmd2.ExecuteNonQuery();
                
                // Insert some sample data if tables are empty
                string checkUsers = "SELECT COUNT(*) FROM Users";
                using var cmdCheck = new SqlCommand(checkUsers, shopConn);
                int userCount = (int)cmdCheck.ExecuteScalar();
                
                if (userCount == 0)
                {
                    string insertUsers = "INSERT INTO Users (Username, Password) VALUES ('admin', 'password'), ('user', '123456')";
                    using var cmdInsert = new SqlCommand(insertUsers, shopConn);
                    cmdInsert.ExecuteNonQuery();
                }
                
                string checkGoods = "SELECT COUNT(*) FROM Goods";
                using var cmdCheckGoods = new SqlCommand(checkGoods, shopConn);
                int goodsCount = (int)cmdCheckGoods.ExecuteScalar();
                
                if (goodsCount == 0)
                {
                    string insertGoods = "INSERT INTO Goods (Name, Price) VALUES ('Laptop', 1000.00), ('Mouse', 25.50), ('Keyboard', 75.00)";
                    using var cmdInsertGoods = new SqlCommand(insertGoods, shopConn);
                    cmdInsertGoods.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the app
                Console.WriteLine($"Database initialization error: {ex.Message}");
            }
        }
        [HttpPost("register")]
        public IActionResult Register(string username, string password)
        {
            using var conn = new SqlConnection(CONN);
            conn.Open();
            var sql = $"INSERT INTO Users (Username, Password) VALUES ('{username}', '{password}')";
            new SqlCommand(sql, conn).ExecuteNonQuery();
            return Ok("Registered");
        }
        
        [HttpPost("login")]
        public IActionResult Login(string username, string password)
        {
            // SQL Injection
            string sql = 
                $"SELECT * FROM Users WHERE Username='{username}' AND Password='{password}'";

            using var conn = new SqlConnection(CONN);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return Ok(new {
                    Message = "Login OK",
                    Username = reader["Username"],
                    Password = reader["Password"] // lộ mật khẩu
                });
            }

            return Unauthorized();
        }
    }
}
