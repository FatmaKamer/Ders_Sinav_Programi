namespace Ders_Sinav_Programi.Models
{
    public class SinavEkleViewModel
    {
        public List<FakulteModel> Fakulteler { get; set; }
        public List<AkademikDonemModel> Donemler { get; set; }
        public List<SinifDuzeyiModel> Siniflar { get; set; }
        public List<SinavTipiModel> SinavTipleri { get; set; }
    }
}