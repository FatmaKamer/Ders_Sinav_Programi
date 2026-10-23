namespace Ders_Sinav_Programi.Models
{
    public class DersEkleViewModel
    {
        public List<FakulteModel> Fakulteler { get; set; }
        public List<AkademikDonemModel> Donemler { get; set; }
        public List<SinifDuzeyiModel> Siniflar { get; set; }
    }
}