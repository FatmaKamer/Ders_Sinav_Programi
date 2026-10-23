using Ders_Sinav_Programi.Algorithm.Models;

namespace Ders_Sinav_Programi.Algorithm.Solution
{
    /// <summary>
    /// Bir tam program = Bir kuş = Bir çözüm.
    /// İçinde tüm derslerin atamaları var.
    /// </summary>
    public class ScheduleSolution
    {
        public List<ScheduleEntry> Entries { get; set; } = new();

        // Fitness değerleri (FitnessCalculator doldurur)
        public int HardViolations { get; set; }
        public int SoftCost { get; set; }

        /// <summary>
        /// Deep copy - komşu üretirken orijinali bozmamak için.
        /// </summary>
        public ScheduleSolution Clone()
        {
            return new ScheduleSolution
            {
                Entries = this.Entries.Select(e => new ScheduleEntry
                {
                    CourseId = e.CourseId,
                    RoomId = e.RoomId,
                    Day = e.Day,
                    StartHour = e.StartHour,
                    Duration = e.Duration
                }).ToList(),
                HardViolations = this.HardViolations,
                SoftCost = this.SoftCost
            };
        }

        /// <summary>
        /// Bu çözüm uygulanabilir mi? (hard ihlal yok mu?)
        /// </summary>
        public bool IsFeasible => HardViolations == 0;
    }
}
