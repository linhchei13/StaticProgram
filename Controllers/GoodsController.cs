using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using StaticProgram;

namespace StaticProgram.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoodsController : ControllerBase
    {
        private const string CONN=
        "Server=(localdb)\\mssqllocaldb;Database=ShopDb;Trusted_Connection=True;TrustServerCertificate=True;";
        
        // GET ALL GOODS (No auth)
        [HttpGet("list")]
        public IActionResult ListGoods()
        {
            using var conn = new SqlConnection(CONN);
            conn.Open();

            // Không phân quyền
            string sql = "SELECT * FROM Goods";

            using var cmd = new SqlCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();

            var list = new System.Collections.Generic.List<object>();
            while (rdr.Read())
            {
                list.Add(new {
                    Id = rdr["Id"],
                    Name = rdr["Name"],
                    Price = rdr["Price"]
                });
            }

            return Ok(list);
        }

        [HttpPost("add")]
        public IActionResult AddGoods([FromBody] Goods item)
        {
            //  Basic validation
            if (item == null || string.IsNullOrEmpty(item.Name))
                return BadRequest("Invalid data");

            using var conn = new SqlConnection(CONN);
            conn.Open();

            //  SQL Injection
            string sql = $"INSERT INTO Goods(Name, Price) VALUES('{item.Name}', {item.Price})";

            new SqlCommand(sql, conn).ExecuteNonQuery();

            return Ok("Added");
        }

        
        // DELETE GOODS (Broken Access Control + SQLi)
        [HttpDelete("delete")]
        public IActionResult DeleteGoods(string id)
        {
            // Không check role (user thường xoá thoải mái)
            using var conn = new SqlConnection(CONN);
            conn.Open();

            // SQL Injection: id=1 OR 1=1 → xóa sạch
            string sql = $"DELETE FROM Goods WHERE Id = {id}";

            new SqlCommand(sql, conn).ExecuteNonQuery();

            return Ok($"Deleted {id}");
        }

        [HttpPost("update")]
        public IActionResult UpdateGoods(int id, string name, string price)
        {
            using var conn = new SqlConnection(CONN);
            conn.Open();

            //  SQL Injection (name có thể chèn payload)
            string sql = 
                $"UPDATE Goods SET Name='{name}', Price={price} WHERE Id={id}";

            new SqlCommand(sql, conn).ExecuteNonQuery();

            return Ok("Updated");
        }
    }
}
