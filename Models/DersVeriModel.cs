namespace Ders_Sinav_Programi.Models
{
    public class DersVeriModel
    {
        public string DersKodu { get; set; }
        public string DersAdi { get; set; }
        public int BolumId { get; set; }
        public int SinifId { get; set; }
        public int DonemId { get; set; }
        public int Kredi { get; set; }
        public int Saat { get; set; }
        public string Zorunlu { get; set; }
    }
}
