namespace Ders_Sinav_Programi.Algorithm.Solution
{
    /// <summary>
    /// Bir dersin bir bloğunun atanması.
    /// Örnek: "BIL101 dersinin 1. bloğu, Pazartesi 1. saat, R1 dersliği"
    /// </summary>
    public class ScheduleEntry
    {
        public int CourseId { get; set; }
        public int RoomId { get; set; }
        public int Day { get; set; }                   // 1..5
        public int StartHour { get; set; }             // 1..8

        /// <summary>
        /// Bu dersin kaç saat sürdüğü (2, 3, 4).
        /// 4 saatlik dersler 2+2 olduğu için her entry 2 saat sürer.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Bu entry'nin işgal ettiği tüm saatleri döner.
        /// Örnek: StartHour=2, Duration=3 → [2, 3, 4]
        /// </summary>
        public List<int> GetOccupiedHours()
        {
            var hours = new List<int>();
            for (int i = 0; i < Duration; i++)
            {
                hours.Add(StartHour + i);
            }
            return hours;
        }
    }
}
