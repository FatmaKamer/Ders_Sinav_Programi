using System;
using System.Collections.Generic;
using System.Linq;
using Ders_Sinav_Programi.Algorithm.Models;
using Ders_Sinav_Programi.Algorithm.Solution;

namespace Ders_Sinav_Programi.Algorithm.Operators
{
    public static class NeighborhoodOperators
    {
        private static Random _random = new Random();

        // Mevcut çözümden rastgele bir komşu üretir
        public static ScheduleSolution GenerateNeighbor(ScheduleSolution current, List<Course> courses, List<Room> rooms, List<TimeSlot> timeSlots)
        {
            var neighbor = current.Clone();

            // Rastgele 1 veya 2 işlem seç
            int operation = _random.Next(1, 3); // 1 = Swap, 2 = Move

            if (operation == 1)
            {
                // SWAP: İki rastgele dersin zamanlarını değiştir
                int idx1 = _random.Next(neighbor.Entries.Count);
                int idx2 = _random.Next(neighbor.Entries.Count);

                var tempDay = neighbor.Entries[idx1].Day;
                var tempStartHour = neighbor.Entries[idx1].StartHour;

                neighbor.Entries[idx1].Day = neighbor.Entries[idx2].Day;
                neighbor.Entries[idx1].StartHour = neighbor.Entries[idx2].StartHour;

                neighbor.Entries[idx2].Day = tempDay;
                neighbor.Entries[idx2].StartHour = tempStartHour;
            }
            else
            {
                // MOVE: Rastgele bir dersin zamanını veya dersliğini değiştir
                int idx = _random.Next(neighbor.Entries.Count);
                var entry = neighbor.Entries[idx];
                var course = courses.First(c => c.Id == entry.CourseId);

                int moveType = _random.Next(1, 3); // 1 = Zaman, 2 = Derslik

                if (moveType == 1)
                {
                    // Zamanı değiştir (Blok kurallarına uygun)
                    int newDay = _random.Next(1, 6);
                    int maxStartHour = 8 - entry.Duration + 1;
                    int newStartHour = _random.Next(1, maxStartHour + 1);

                    entry.Day = newDay;
                    entry.StartHour = newStartHour;
                }
                else
                {
                    // Dersliği değiştir (Lab/Normal uyumlu olsun)
                    var suitableRooms = rooms.Where(r => r.IsLab == course.IsLab).ToList();
                    if (suitableRooms.Count > 0)
                    {
                        var newRoom = suitableRooms[_random.Next(suitableRooms.Count)];
                        entry.RoomId = newRoom.Id;
                    }
                }
            }

            return neighbor;
        }
    }
}