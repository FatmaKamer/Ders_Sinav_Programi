using Ders_Sinav_Programi.Algorithm.Models;
using Ders_Sinav_Programi.Algorithm.Solution;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace Ders_Sinav_Programi.Services
{
    public static class AlgorithmDatabaseService
    {
        // Bağlantı Cümlesi (appsettings.json'dan aldık)
        private static readonly string ConnectionString =
            "Server=localhost\\SQLEXPRESS;Database=dersprogrami;Trusted_Connection=True;TrustServerCertificate=True;";

        // 1. Dersleri Veritabanından Çek
        public static List<Course> GetCoursesFromDb(int bolumId, int akademikDonemId)
        {
            var courses = new List<Course>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                // Dersleri ve Hoca bilgilerini (Ders_Ogretim_Uyesi üzerinden) çekiyoruz
                string query = @"
                    SELECT 
                        d.Ders_id, 
                        d.Ders_kodu, 
                        d.Ders_adi, 
                        d.Haftalik_saat, 
                        d.Ogrenci_sayisi, 
                        d.Sinif_duzeyi_id,
                        dou.Kullanici_id AS TeacherId,
                        st.Sinif_tipi_adi
                    FROM Ders d
                    LEFT JOIN Ders_Ogretim_Uyesi dou ON d.Ders_id = dou.Ders_id
                    LEFT JOIN Sinif_Tipi st ON d.Ders_tipi_id = st.Sinif_tipi_id
                    WHERE d.Bolum_id = @BolumId AND d.Akademik_donem_id = @DonemId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BolumId", bolumId);
                    cmd.Parameters.AddWithValue("@DonemId", akademikDonemId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // IsLab kontrolü: Ders tipi 'Bilgisayar Laboratuvarı' veya 'Elektronik Laboratuvarı' ise true
                            string sinifTipi = reader["Sinif_tipi_adi"]?.ToString() ?? "";
                            bool isLab = sinifTipi.Contains("Laboratuvar");

                            var course = new Course
                            {
                                Id = Convert.ToInt32(reader["Ders_id"]),
                                Code = reader["Ders_kodu"].ToString(),
                                Name = reader["Ders_adi"].ToString(),
                                WeeklyHours = Convert.ToInt32(reader["Haftalik_saat"]),
                                StudentCount = reader["Ogrenci_sayisi"] != DBNull.Value ? Convert.ToInt32(reader["Ogrenci_sayisi"]) : 30,
                                StudentGroupId = Convert.ToInt32(reader["Sinif_duzeyi_id"]),
                                TeacherId = reader["TeacherId"] != DBNull.Value ? Convert.ToInt32(reader["TeacherId"]) : 1,
                                IsLab = isLab,
                                IsCompulsory = true // Şimdilik varsayılan
                            };
                            courses.Add(course);
                        }
                    }
                }
            }
            return courses;
        }

        // 2. Derslikleri Veritabanından Çek
        public static List<Room> GetRoomsFromDb()
        {
            var rooms = new List<Room>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                string query = @"
                    SELECT 
                        s.Sinif_id, 
                        s.Kapasite, 
                        st.Sinif_tipi_adi,
                        s.Blok,
                        s.Kat
                    FROM Sinif s
                    LEFT JOIN Sinif_Tipi st ON s.Sinif_tipi_id = st.Sinif_tipi_id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string sinifTipi = reader["Sinif_tipi_adi"]?.ToString() ?? "";
                        bool isLab = sinifTipi.Contains("Laboratuvar");

                        var room = new Room
                        {
                            Id = Convert.ToInt32(reader["Sinif_id"]),
                            Name = $"{reader["Blok"]}-{reader["Kat"]}-{reader["Sinif_id"]}",
                            Capacity = Convert.ToInt32(reader["Kapasite"]),
                            IsLab = isLab
                        };
                        rooms.Add(room);
                    }
                }
            }
            return rooms;
        }

        // 3. Bulunan Programı Veritabanına Kaydet
        public static void SaveScheduleToDb(ScheduleSolution solution, List<Course> courses, int akademikDonemId)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                // Önce o dönem için eski kayıtları temizleyelim (üst üste eklenmesin)
                string deleteQuery = "DELETE FROM Ders_Programi WHERE Akademik_donem_id = @DonemId";
                using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn))
                {
                    deleteCmd.Parameters.AddWithValue("@DonemId", akademikDonemId);
                    deleteCmd.ExecuteNonQuery();
                }

                // Şimdi yeni çözümü kaydedelim
                string insertQuery = @"
                    INSERT INTO Ders_Programi (Ders_id, Kullanici_id, Sinif_id, Gun, Zaman_dilimi_id, Akademik_donem_id)
                    VALUES (@DersId, @KullaniciId, @SinifId, @Gun, @ZamanDilimiId, @DonemId)";

                foreach (var entry in solution.Entries)
                {
                    var course = courses.First(c => c.Id == entry.CourseId);

                    // Her ders bloğunun süresi kadar (örneğin 2 saatlikse) ayrı ayrı kayıt atıyoruz
                    for (int i = 0; i < entry.Duration; i++)
                    {
                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@DersId", entry.CourseId);
                            insertCmd.Parameters.AddWithValue("@KullaniciId", course.TeacherId);
                            insertCmd.Parameters.AddWithValue("@SinifId", entry.RoomId);
                            insertCmd.Parameters.AddWithValue("@Gun", entry.Day);

                            // Zaman_dilimi_id: Saat 1-8 arası (1. saat, 2. saat vs.)
                            insertCmd.Parameters.AddWithValue("@ZamanDilimiId", entry.StartHour + i);

                            insertCmd.Parameters.AddWithValue("@DonemId", akademikDonemId);

                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public static List<string> GetActiveConstraintsFromDb(int bolumId)
        {
            var activeConstraintCodes = new List<string>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                // Bolum_Kisitlari tablosundan, o bölüme ait ve Aktif_mi = 1 olan kısıtların kodlarını çekiyoruz.
                string query = @"
                    SELECT k.Kisit_Kodu 
                    FROM Bolum_Kisitlari bk
                    INNER JOIN Kisit_Katalogu k ON bk.Kisit_id = k.Kisit_id
                    WHERE bk.Bolum_id = @BolumId AND bk.Aktif_mi = 1";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BolumId", bolumId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            activeConstraintCodes.Add(reader["Kisit_Kodu"].ToString());
                        }
                    }
                }
            }
            return activeConstraintCodes;
        }
    }
}