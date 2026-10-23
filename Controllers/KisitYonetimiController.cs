using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Ders_Sinav_Programi.Models;
using Ders_Sinav_Programi.Helpers;
using System.Collections.Generic;
using System;

namespace Ders_Sinav_Programi.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class KisitYonetimiController : Controller
    {
        private readonly DbHelper _dbHelper;

        public KisitYonetimiController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public IActionResult Index()
        {
            var viewModel = new KisitYonetimiViewModel
            {
                Fakulteler = new List<FakulteModel>(),
                KisitKataloguListesi = new List<KisitKatalogu>()
            };

            using (SqlConnection conn = _dbHelper.GetConnection())
            {
                conn.Open();

                string queryFakulte = "SELECT Fakulte_id, Fakulte_adi FROM Fakulte";
                using (SqlCommand cmdFakulte = new SqlCommand(queryFakulte, conn))
                using (SqlDataReader reader = cmdFakulte.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        viewModel.Fakulteler.Add(new FakulteModel
                        {
                            Id = Convert.ToInt32(reader["Fakulte_id"]),
                            Ad = reader["Fakulte_adi"].ToString()
                        });
                    }
                }

                string queryKisit = "SELECT * FROM Kisit_Katalogu WHERE Aktif_mi = 1 AND Kisit_Kodu NOT LIKE 'G%' ORDER BY Kategori, Kisit_Turu";
                using (SqlCommand cmdKisit = new SqlCommand(queryKisit, conn))
                using (SqlDataReader reader = cmdKisit.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        viewModel.KisitKataloguListesi.Add(new KisitKatalogu
                        {
                            Kisit_id = Convert.ToInt32(reader["Kisit_id"]),
                            Kisit_Kodu = reader["Kisit_Kodu"].ToString(),
                            Kisit_Adi = reader["Kisit_Adi"].ToString(),
                            Aciklama = reader["Aciklama"]?.ToString(),
                            Kisit_Turu = reader["Kisit_Turu"].ToString(),
                            Varsayilan_Puan = Convert.ToInt32(reader["Varsayilan_Puan"]),
                            Kategori = reader["Kategori"].ToString(),
                            Parametre_Tipi = reader["Parametre_Tipi"].ToString(),
                            Parametre_Degeri = reader["Parametre_Degeri"]?.ToString()
                        });
                    }
                }
            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult GetBolumKisitlari(int bolumId)
        {
            var kisitlar = new List<object>();

            using (SqlConnection conn = _dbHelper.GetConnection())
            {
                conn.Open();
                string query = @"
            SELECT 
                0 as Id, 
                kk.Kategori, 
                kk.Kisit_Adi, 
                kk.Kisit_Turu, 
                kk.Parametre_Degeri as Parametre_Gosterim, 
                kk.Varsayilan_Puan as Ceza_Puani
            FROM Kisit_Katalogu kk
            WHERE kk.Kisit_Kodu LIKE 'G%' AND kk.Aktif_mi = 1
            
            UNION ALL
            
            SELECT 
                bk.Id, 
                kk.Kategori, 
                kk.Kisit_Adi, 
                kk.Kisit_Turu, 
                ISNULL(bk.Parametre_Degeri, kk.Parametre_Degeri) as Parametre_Gosterim, 
                ISNULL(bk.Ceza_Puani, kk.Varsayilan_Puan) as Ceza_Puani
            FROM Bolum_Kisitlari bk
            INNER JOIN Kisit_Katalogu kk ON bk.Kisit_id = kk.Kisit_id
            WHERE bk.Bolum_id = @bolumId AND bk.Aktif_mi = 1
            
            ORDER BY Id ASC, Kategori ASC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@bolumId", bolumId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            kisitlar.Add(new
                            {
                                id = Convert.ToInt32(reader["Id"]),
                                kategori = reader["Kategori"].ToString(),
                                kisit_Adi = reader["Kisit_Adi"].ToString(),
                                kisit_Turu = reader["Kisit_Turu"].ToString(),
                                parametre_Gosterim = reader["Parametre_Gosterim"]?.ToString(),
                                ceza_Puani = Convert.ToInt32(reader["Ceza_Puani"])
                            });
                        }
                    }
                }
            }
            return Json(kisitlar);
        }

        [HttpPost]
        public IActionResult KisitEkle(BolumKisitlari model)
        {
            try
            {
                using (SqlConnection conn = _dbHelper.GetConnection())
                {
                    conn.Open();

                    string checkQuery = "SELECT COUNT(1) FROM Bolum_Kisitlari WHERE Bolum_id = @bId AND Kisit_id = @kId";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@bId", model.Bolum_id);
                        checkCmd.Parameters.AddWithValue("@kId", model.Kisit_id);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (count > 0)
                        {
                            return Json(new { success = false, message = "Bu kısıt bölümünüzde zaten aktif." });
                        }
                    }

                    string insertQuery = @"
                        INSERT INTO Bolum_Kisitlari (Bolum_id, Kisit_id, Ceza_Puani, Parametre_Degeri, Guncellenme_Tarihi) 
                        VALUES (@bId, @kId, @puan, @param, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@bId", model.Bolum_id);
                        cmd.Parameters.AddWithValue("@kId", model.Kisit_id);
                        cmd.Parameters.AddWithValue("@puan", model.Ceza_Puani ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@param", string.IsNullOrEmpty(model.Parametre_Degeri) ? DBNull.Value : (object)model.Parametre_Degeri);

                        cmd.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult KisitSil(int id)
        {
            if (id <= 0) return Json(new { success = false });

            using (SqlConnection conn = _dbHelper.GetConnection())
            {
                conn.Open();
                string query = "DELETE FROM Bolum_Kisitlari WHERE Id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            return Json(new { success = true });
        }
    }
}