using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Ders_Sinav_Programi.Helpers;
using Ders_Sinav_Programi.Models;

namespace Ders_Sinav_Programi.Controllers
{
    public class YonetimController : Controller
    {
        private readonly DbHelper _db;
        public YonetimController(DbHelper db) { _db = db; }

        

        public IActionResult DersEkle()
        {
            var viewModel = new DersEkleViewModel
            {
                Fakulteler = new List<FakulteModel>(),
                Donemler = new List<AkademikDonemModel>(),
                Siniflar = new List<SinifDuzeyiModel>()
            };

            using (var conn = _db.GetConnection())
            {
                conn.Open();
                // Fakülteler
                using (var r = new SqlCommand("SELECT Fakulte_id, Fakulte_adi FROM Fakulte ORDER BY Fakulte_adi", conn).ExecuteReader())
                    while (r.Read()) viewModel.Fakulteler.Add(new FakulteModel { Id = (int)r["Fakulte_id"], Ad = r["Fakulte_adi"].ToString() });

                // Dönemler
                using (var r = new SqlCommand("SELECT Akademik_donem_id, Yil, Donem FROM Akademik_Donem ORDER BY Yil DESC", conn).ExecuteReader())
                    while (r.Read()) viewModel.Donemler.Add(new AkademikDonemModel { Id = (int)r["Akademik_donem_id"], Yil = (int)r["Yil"], Donem = r["Donem"].ToString() });

                // Sınıflar
                using (var r = new SqlCommand("SELECT Sinif_duzeyi_id, Sinif_duzeyi_adi FROM Sinif_Duzeyi", conn).ExecuteReader())
                    while (r.Read()) viewModel.Siniflar.Add(new SinifDuzeyiModel { Id = (int)r["Sinif_duzeyi_id"], Ad = r["Sinif_duzeyi_adi"].ToString() });
            }
            return View(viewModel);
        }

        [HttpGet]
        public JsonResult GetBolumler(int fakulteId)
        {
            var bolumler = new List<BolumModel>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT Bolum_id, Bolum_adi FROM Bolum WHERE Fakulte_id = @fId", conn);
                cmd.Parameters.AddWithValue("@fId", fakulteId);
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read()) bolumler.Add(new BolumModel { Id = (int)reader["Bolum_id"], Ad = reader["Bolum_adi"].ToString() });
            }
            return Json(bolumler);
        }

        [HttpGet]
        public JsonResult GetDersler(int bolumId, int donemId, int sinifId)
        {
            var dersler = new List<object>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                string query = @"
            SELECT d.Ders_id, d.Ders_kodu, d.Ders_adi, d.Kredi, d.Haftalik_saat, d.Zorunlu_mu, s.Sinif_duzeyi_adi
            FROM Ders d
            JOIN Sinif_Duzeyi s ON d.Sinif_duzeyi_id = s.Sinif_duzeyi_id
            WHERE d.Bolum_id = @bId AND d.Akademik_donem_id = @dId AND d.Sinif_duzeyi_id = @sId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@bId", bolumId);
                    cmd.Parameters.AddWithValue("@dId", donemId);
                    cmd.Parameters.AddWithValue("@sId", sinifId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dersler.Add(new
                            {
                                id = reader["Ders_id"],
                                dersKodu = reader["Ders_kodu"].ToString(),
                                dersAdi = reader["Ders_adi"].ToString(),
                                kredi = reader["Kredi"],
                                saat = reader["Haftalik_saat"],
                                zorunlu = reader["Zorunlu_mu"].ToString(),
                                sinifAd = reader["Sinif_duzeyi_adi"].ToString()
                            });
                        }
                    }
                }
            }
            return Json(dersler);
        }

        [HttpPost]
        public JsonResult DersKaydet(DersVeriModel model)
        {
            try
            {
                using (var conn = _db.GetConnection())
                {
                    conn.Open();
                    string query = @"INSERT INTO Ders (Ders_kodu, Ders_adi, Bolum_id, Sinif_duzeyi_id, Akademik_donem_id, Haftalik_saat, Kredi, Zorunlu_mu)
                             VALUES (@kod, @ad, @bId, @sId, @dId, @saat, @kredi, @zorunlu)";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@kod", model.DersKodu ?? "");
                        cmd.Parameters.AddWithValue("@ad", model.DersAdi ?? "");
                        cmd.Parameters.AddWithValue("@bId", model.BolumId);
                        cmd.Parameters.AddWithValue("@sId", model.SinifId);
                        cmd.Parameters.AddWithValue("@dId", model.DonemId);
                        cmd.Parameters.AddWithValue("@saat", model.Saat);
                        cmd.Parameters.AddWithValue("@kredi", model.Kredi);
                        cmd.Parameters.AddWithValue("@zorunlu", model.Zorunlu ?? "Zorunlu");
                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // --- SINAV EKLEME BÖLÜMÜ ---

        public IActionResult SinavEkle()
        {
            var viewModel = new SinavEkleViewModel
            {
                Fakulteler = new List<FakulteModel>(),
                Donemler = new List<AkademikDonemModel>(),
                Siniflar = new List<SinifDuzeyiModel>(),
                SinavTipleri = new List<SinavTipiModel>()
            };

            using (var conn = _db.GetConnection())
            {
                conn.Open();
                // Fakülteler
                using (var r = new SqlCommand("SELECT Fakulte_id, Fakulte_adi FROM Fakulte", conn).ExecuteReader())
                    while (r.Read()) viewModel.Fakulteler.Add(new FakulteModel { Id = (int)r["Fakulte_id"], Ad = r["Fakulte_adi"].ToString() });

                // Dönemler
                using (var r = new SqlCommand("SELECT Akademik_donem_id, Yil, Donem FROM Akademik_Donem ORDER BY Yil DESC", conn).ExecuteReader())
                    while (r.Read()) viewModel.Donemler.Add(new AkademikDonemModel { Id = (int)r["Akademik_donem_id"], Yil = (int)r["Yil"], Donem = r["Donem"].ToString() });

                // Sınıflar
                using (var r = new SqlCommand("SELECT Sinif_duzeyi_id, Sinif_duzeyi_adi FROM Sinif_Duzeyi", conn).ExecuteReader())
                    while (r.Read()) viewModel.Siniflar.Add(new SinifDuzeyiModel { Id = (int)r["Sinif_duzeyi_id"], Ad = r["Sinif_duzeyi_adi"].ToString() });

                // Sınav Tipleri (Senin tablona göre Sinif_tipi_id)
                using (var r = new SqlCommand("SELECT Sinav_tipi_id, Sinav_tipi_adi FROM Sinav_Tipi", conn).ExecuteReader())
                    while (r.Read()) viewModel.SinavTipleri.Add(new SinavTipiModel { Id = (int)r["Sinav_tipi_id"], Ad = r["Sinav_tipi_adi"].ToString() });
            }
            return View(viewModel);
        }

        [HttpGet]
        public JsonResult GetDerslerByFilter(int bolumId, int donemId, int sinifId)
        {
            var dersler = new List<object>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT Ders_id, Ders_adi FROM Ders WHERE Bolum_id = @bId AND Akademik_donem_id = @dId AND Sinif_duzeyi_id = @sId", conn);
                cmd.Parameters.AddWithValue("@bId", bolumId);
                cmd.Parameters.AddWithValue("@dId", donemId);
                cmd.Parameters.AddWithValue("@sId", sinifId);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) dersler.Add(new { id = r["Ders_id"], ad = r["Ders_adi"].ToString() });
            }
            return Json(dersler);
        }

        [HttpPost]
        public JsonResult SinavKaydet(SinavVeriModel model)
        {
            try
            {
                using (var conn = _db.GetConnection())
                {
                    conn.Open();
                    
                    string query = @"INSERT INTO Sinav (Ders_id, Sinav_tipi_id, Sinav_suresi, Akademik_donem_id) 
                                     VALUES (@dId, @tId, @sure, @donemId)";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@dId", model.DersId);
                        cmd.Parameters.AddWithValue("@tId", model.SinavTipiId);
                        cmd.Parameters.AddWithValue("@sure", model.Sure);
                        cmd.Parameters.AddWithValue("@donemId", model.DonemId);
                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DersSil(int id)
        {
            try
            {
                using (var conn = _db.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("DELETE FROM Ders WHERE Ders_id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpGet]
        public JsonResult GetSinavlar(int bolumId, int donemId, int sinifId)
        {
            var liste = new List<object>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                string query = @"SELECT s.Sinav_id, d.Ders_adi, t.Sinav_tipi_adi, s.Sinav_suresi 
                                 FROM Sinav s
                                 JOIN Ders d ON s.Ders_id = d.Ders_id
                                 JOIN Sinav_Tipi t ON s.Sinav_tipi_id = t.Sinav_tipi_id
                                 WHERE d.Bolum_id = @bId AND s.Akademik_donem_id = @donId AND d.Sinif_duzeyi_id = @sId";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@bId", bolumId);
                    cmd.Parameters.AddWithValue("@donId", donemId);
                    cmd.Parameters.AddWithValue("@sId", sinifId);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) liste.Add(new { id = r["Sinav_id"], dersAdi = r["Ders_adi"].ToString(), sinavTipi = r["Sinav_tipi_adi"].ToString(), sure = r["Sinav_suresi"] });
                }
            }
            return Json(liste);
        }

        [HttpPost]
        public JsonResult SinavSil(int id)
        {
            try
            {
                using (var conn = _db.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("DELETE FROM Sinav WHERE Sinav_id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
    }

    public class SinavVeriModel
    {
        public int DersId { get; set; }
        public int SinavTipiId { get; set; }
        public int Sure { get; set; }
        public int DonemId { get; set; }
    }
}