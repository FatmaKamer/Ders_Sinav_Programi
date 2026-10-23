namespace Ders_Sinav_Programi.Algorithm.Models
{
    public class Course
    {
        public int Id { get; set; }                    // Ders_id
        public string Code { get; set; }               // Ders_kodu
        public string Name { get; set; }               // Ders_adi
        public int TeacherId { get; set; }             // Ders_Ogretim_Uyesi.Kullanici_id
        public int StudentGroupId { get; set; }        // Sinif_duzeyi_id (şimdilik)
        public int WeeklyHours { get; set; }           // Haftalik_saat (2, 3, 4)
        public int StudentCount { get; set; }          // Ogrenci_sayisi
        public bool IsLab { get; set; }                // Sinif_tipi_id == Lab mı?
        public bool IsCompulsory { get; set; }         // Zorunlu_mu
    }
}
