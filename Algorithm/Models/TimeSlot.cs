namespace Ders_Sinav_Programi.Algorithm.Models
{
    /// <summary>
    /// Bir zaman dilimi = (gün, saat) çifti.
    /// 5 gün × 8 saat = 40 slot.
    /// </summary>
    public class TimeSlot
    {
        public int Id { get; set; }                    // 1..40
        public int Day { get; set; }                   // 1..5 (Pazartesi=1)
        public int Hour { get; set; }                  // 1..8

        public string Label => $"Gün{Day}-Saat{Hour}";
    }
}
