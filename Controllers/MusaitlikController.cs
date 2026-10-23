using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Ders_Sinav_Programi.Helpers;
using Ders_Sinav_Programi.Models;
using System.Security.Claims;

namespace Ders_Sinav_Programi.Controllers
{
    [Authorize]
    public class MusaitlikController : Controller
    {
        private readonly DbHelper _db;

        public MusaitlikController(DbHelper db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            List<ZamanDilimiModel> saatler = new List<ZamanDilimiModel>();

            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var query = "SELECT Zaman_dilimi_id, Baslangic_saati, Bitis_saati FROM Zaman_Dilimi ORDER BY Baslangic_saati";
                using (var cmd = new SqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            saatler.Add(new ZamanDilimiModel
                            {
                                Id = (int)reader["Zaman_dilimi_id"],
                                
                                SaatAraligi = reader["Baslangic_saati"].ToString().Substring(0, 5) + "-" +
                                             reader["Bitis_saati"].ToString().Substring(0, 5)
                            });
                        }
                    }
                }
            }
            
            return View(saatler);
        }

        [HttpPost]
        public IActionResult Kaydet(List<MusaitlikVeriModel> veriler)
        {
            var kullaniciIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(kullaniciIdStr)) return Unauthorized();
            int kullaniciId = int.Parse(kullaniciIdStr);

            using (var conn = _db.GetConnection())
            {
                conn.Open();

                
                string deleteQuery = "DELETE FROM Musaitlik WHERE Kullanici_id = @userId";
                using (var deleteCmd = new SqlCommand(deleteQuery, conn))
                {
                    deleteCmd.Parameters.AddWithValue("@userId", kullaniciId);
                    deleteCmd.ExecuteNonQuery();
                }

                
                if (veriler != null && veriler.Count > 0)
                {
                    foreach (var item in veriler)
                    {
                        
                        string insertQuery = "INSERT INTO Musaitlik (Kullanici_id, Gun, Zaman_dilimi_id) VALUES (@userId, @gun, @dilimId)";

                        using (var insertCmd = new SqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@userId", kullaniciId);
                            insertCmd.Parameters.AddWithValue("@gun", item.Gun);
                            insertCmd.Parameters.AddWithValue("@dilimId", item.ZamanDilimiId);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            return Json(new { success = true, message = "Başarıyla kaydedildi!" });
        }
    }
}
