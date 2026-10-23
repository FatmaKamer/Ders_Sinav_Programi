namespace Ders_Sinav_Programi.Algorithm.Models
{
    public class Room
    {
        public int Id { get; set; }                    // Sinif_id
        public string Name { get; set; }               // Sinif_tipi_adi + Kat + Blok
        public int Capacity { get; set; }              // Kapasite
        public bool IsLab { get; set; }                // Sinif_tipi_id == Lab mı?
        public bool HasProjector { get; set; }         // Projeksiyon_var_mi
        public bool HasComputer { get; set; }          // Bilgisayar_var_mi
    }
}
