namespace Ders_Sinav_Programi.Algorithm.Models
{
    public class Teacher
    {
        public int Id { get; set; }                    // Kullanici_id
        public string Name { get; set; }               // Email (Kullanici'da isim yok)
        public Dictionary<string, bool> Availability { get; set; } = new();
    }
}
