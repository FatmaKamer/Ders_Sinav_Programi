namespace Ders_Sinav_Programi.Models
{
    public class AkademikDonemModel
    {
        public int Id { get; set; }
        public int Yil { get; set; }
        public string Donem { get; set; }
        public string TamAd => $"{Yil}-{Yil + 1} {Donem}";
    }
}
