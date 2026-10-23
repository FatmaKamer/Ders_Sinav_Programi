using System;
using System.Collections.Generic;
using System.Linq;
using Ders_Sinav_Programi.Algorithm.Models;
using Ders_Sinav_Programi.Algorithm.Solution;

namespace Ders_Sinav_Programi.Algorithm
{
    public static class InitialSolutionGenerator
    {
        private static Random _random = new Random();

        public static ScheduleSolution Generate(List<Course> courses, List<Room> rooms, List<TimeSlot> timeSlots)
        {
            var solution = new ScheduleSolution();

            foreach (var course in courses)
            {
                // Dersin tipine uygun derslikleri bul (Lab mı Normal mi?)
                var suitableRooms = rooms.Where(r => r.IsLab == course.IsLab).ToList();
                if (suitableRooms.Count == 0) suitableRooms = rooms; // Fallback (eğer uygun oda yoksa hepsinden seç)

                // 1. BLOK (Tüm dersler için 1. blok var)
                // 2 veya 3 saatlikse tek blok, 4 saatlikse 1. yarısı (2 saat)
                int block1Duration = course.WeeklyHours >= 4 ? 2 : course.WeeklyHours;

                var (day1, startHour1) = GetRandomValidTimeSlot(timeSlots, block1Duration);
                var room1 = suitableRooms[_random.Next(suitableRooms.Count)];

                solution.Entries.Add(new ScheduleEntry
                {
                    CourseId = course.Id,
                    RoomId = room1.Id,
                    Day = day1,
                    StartHour = startHour1,
                    Duration = block1Duration
                });

                // 2. BLOK (Sadece 4 saatlik dersler için)
                if (course.WeeklyHours == 4)
                {
                    int day2, startHour2;
                    // 4 saatlik derslerin 2. bloğu FARKLI bir günde olmalı
                    do
                    {
                        (day2, startHour2) = GetRandomValidTimeSlot(timeSlots, 2);
                    } while (day1 == day2);

                    var room2 = suitableRooms[_random.Next(suitableRooms.Count)];

                    solution.Entries.Add(new ScheduleEntry
                    {
                        CourseId = course.Id,
                        RoomId = room2.Id,
                        Day = day2,
                        StartHour = startHour2,
                        Duration = 2
                    });
                }
            }

            return solution;
        }

        // Rastgele bir gün ve başlangıç saati seçer (gün sonuna taşmayacak şekilde)
        private static (int day, int startHour) GetRandomValidTimeSlot(List<TimeSlot> timeSlots, int duration)
        {
            // 8 saatlik bir günde, 3 saatlik ders en geç 6. saatte başlayabilir (6,7,8)
            // Yani max başlangıç saati = 8 - duration + 1
            int maxStartHour = 8 - duration + 1;

            int randomDay = _random.Next(1, 6); // 1 ile 5 arası
            int randomStartHour = _random.Next(1, maxStartHour + 1);

            return (randomDay, randomStartHour);
        }
    }
}