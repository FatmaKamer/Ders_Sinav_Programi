using System;
using System.Collections.Generic;
using System.Linq;
using Ders_Sinav_Programi.Algorithm;
using Ders_Sinav_Programi.Algorithm.Models;
using Ders_Sinav_Programi.Algorithm.Solution;
using Ders_Sinav_Programi.Models;

namespace Ders_Sinav_Programi.Services
{
    public class AlgorithmService
    {
        public ScheduleSolution GenerateSchedule(int bolumId, int akademikDonemId)
        {
            var courses = AlgorithmDatabaseService.GetCoursesFromDb(bolumId, akademikDonemId);
            var rooms = AlgorithmDatabaseService.GetRoomsFromDb();
            var timeSlots = GetTimeSlots();

            var activeConstraints = AlgorithmDatabaseService.GetActiveConstraintsFromDb(bolumId);

            Console.WriteLine("Algoritmanın Gördüğü Kısıtlar: " + string.Join(", ", activeConstraints));

            var mbo = new MboSolver
            {
                PopulationSize = 10,
                NeighborsPerBird = 5,
                SharedNeighbors = 2,
                MaxTours = 2000,
                InitialTemperature = 50.0,
                CoolingRate = 0.99
            };

            var finalSolution = mbo.Solve(courses, rooms, timeSlots, activeConstraints);

            if (finalSolution.HardViolations == 0)
            {
                AlgorithmDatabaseService.SaveScheduleToDb(finalSolution, courses, akademikDonemId);
            }

            return finalSolution;
        }

        private List<TimeSlot> GetTimeSlots()
        {
            var slots = new List<TimeSlot>();
            int id = 1;
            for (int day = 1; day <= 5; day++)
            {
                for (int hour = 1; hour <= 8; hour++)
                {
                    slots.Add(new TimeSlot { Id = id, Day = day, Hour = hour });
                    id++;
                }
            }
            return slots;
        }

        public List<DersProgramiGorunumModel> GetScheduleFromDb(int bolumId, int donemId, int sinifDuzeyiId)
        {
            var result = new List<DersProgramiGorunumModel>();

            using (Microsoft.Data.SqlClient.SqlConnection conn = new Microsoft.Data.SqlClient.SqlConnection(
                "Server=localhost\\SQLEXPRESS;Database=dersprogrami;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                conn.Open();

                string query = @"
                    SELECT dp.Gun, dp.Zaman_dilimi_id, d.Ders_kodu, d.Ders_adi, 
                           s.Sinif_id, st.Sinif_tipi_adi, s.Blok, s.Kat,
                           k.Email AS HocaEmail
                    FROM Ders_Programi dp
                    INNER JOIN Ders d ON dp.Ders_id = d.Ders_id
                    INNER JOIN Sinif s ON dp.Sinif_id = s.Sinif_id
                    LEFT JOIN Sinif_Tipi st ON s.Sinif_tipi_id = st.Sinif_tipi_id
                    LEFT JOIN Kullanici k ON dp.Kullanici_id = k.Kullanici_id
                    WHERE d.Bolum_id = @BolumId AND dp.Akademik_donem_id = @DonemId AND d.Sinif_duzeyi_id = @SinifDuzeyiId";

                using (Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BolumId", bolumId);
                    cmd.Parameters.AddWithValue("@DonemId", donemId);
                    cmd.Parameters.AddWithValue("@SinifDuzeyiId", sinifDuzeyiId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new DersProgramiGorunumModel
                            {
                                Gun = Convert.ToInt32(reader["Gun"]),
                                Saat = Convert.ToInt32(reader["Zaman_dilimi_id"]),
                                DersKodu = reader["Ders_kodu"].ToString(),
                                DersAdi = reader["Ders_adi"].ToString(),
                                SinifAdi = $"{reader["Blok"]} - {reader["Kat"]} ({reader["Sinif_tipi_adi"]})",
                                HocaAdi = reader["HocaEmail"]?.ToString() ?? "Atanmadı"
                            });
                        }
                    }
                }
            }
            return result;
        }
    }
}