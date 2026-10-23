using System;
using System.Collections.Generic;
using System.Linq;
using Ders_Sinav_Programi.Algorithm.Models;
using Ders_Sinav_Programi.Algorithm.Solution;

namespace Ders_Sinav_Programi.Algorithm.Fitness
{
    public static class FitnessCalculator
    {
        public static void Evaluate(ScheduleSolution solution, List<Course> courses, List<Room> rooms, List<string> activeConstraints)
        {
            int hardViolations = 0;
            int softCost = 0;

            // Kural 1: Hoca Çakışması (Eğer G1 aktifse)
            if (activeConstraints.Contains("G1"))
            {
                foreach (var entry1 in solution.Entries)
                {
                    var course1 = courses.First(c => c.Id == entry1.CourseId);
                    foreach (var entry2 in solution.Entries)
                    {
                        if (entry1 == entry2) continue;
                        var course2 = courses.First(c => c.Id == entry2.CourseId);
                        if (course1.TeacherId == course2.TeacherId && IsOverlapping(entry1, entry2))
                            hardViolations++;
                    }
                }
            }

            // Kural 2: Öğrenci Çakışması (Eğer G3 aktifse)
            if (activeConstraints.Contains("G3"))
            {
                foreach (var entry1 in solution.Entries)
                {
                    var course1 = courses.First(c => c.Id == entry1.CourseId);
                    foreach (var entry2 in solution.Entries)
                    {
                        if (entry1 == entry2) continue;
                        var course2 = courses.First(c => c.Id == entry2.CourseId);
                        if (course1.StudentGroupId == course2.StudentGroupId && IsOverlapping(entry1, entry2))
                            hardViolations++;
                    }
                }
            }

            // Kural 3: Derslik Çakışması (Eğer G2 aktifse)
            if (activeConstraints.Contains("G2"))
            {
                foreach (var entry1 in solution.Entries)
                {
                    foreach (var entry2 in solution.Entries)
                    {
                        if (entry1 == entry2) continue;
                        if (entry1.RoomId == entry2.RoomId && IsOverlapping(entry1, entry2))
                            hardViolations++;
                    }
                }
            }

            // Kural 4: Kapasite ve Lab Uyumu (Eğer G4 veya G5 aktifse)
            foreach (var entry in solution.Entries)
            {
                var course = courses.First(c => c.Id == entry.CourseId);
                var room = rooms.First(r => r.Id == entry.RoomId);

                if (activeConstraints.Contains("G4") && room.Capacity < course.StudentCount)
                    hardViolations++;

                if (activeConstraints.Contains("G5") && course.IsLab != room.IsLab)
                    hardViolations++;
            }

            // NOT: Döngülerde her çakışma 2 kere sayılır (A ile B, B ile A).
            // Gerçek ihlal sayısını bulmak için 2'ye bölünür.
            hardViolations = hardViolations / 2;

            // 2. SOFT KISITLAR (Yumuşak Maliyetler)

            // Kural 5: Öğrenci Boşlukları (S_1) - Döküman Bölüm 6.1
            // Bir sınıfın gün içinde dersleri arasında boşluk varsa ceza.
            var studentGroups = courses.Select(c => c.StudentGroupId).Distinct();
            foreach (var groupId in studentGroups)
            {
                for (int day = 1; day <= 5; day++)
                {
                    var groupEntries = solution.Entries
                        .Where(e => courses.First(c => c.Id == e.CourseId).StudentGroupId == groupId && e.Day == day)
                        .OrderBy(e => e.StartHour)
                        .ToList();

                    for (int i = 0; i < groupEntries.Count - 1; i++)
                    {
                        var currentEnd = groupEntries[i].StartHour + groupEntries[i].Duration - 1;
                        var nextStart = groupEntries[i + 1].StartHour;

                        int gap = nextStart - currentEnd - 1;
                        if (gap > 0) softCost += gap; // Her boşluk saati kadar ceza
                    }
                }
            }

            // Kural 6: Hoca Boşlukları (S_2) - Döküman Bölüm 6.2
            // Bir hocanın gün içindeki dersleri arasında boşluk varsa ceza.
            var teacherIds = courses.Select(c => c.TeacherId).Distinct();
            foreach (var teacherId in teacherIds)
            {
                for (int day = 1; day <= 5; day++)
                {
                    var teacherEntries = solution.Entries
                        .Where(e => courses.First(c => c.Id == e.CourseId).TeacherId == teacherId && e.Day == day)
                        .OrderBy(e => e.StartHour)
                        .ToList();

                    for (int i = 0; i < teacherEntries.Count - 1; i++)
                    {
                        var currentEnd = teacherEntries[i].StartHour + teacherEntries[i].Duration - 1;
                        var nextStart = teacherEntries[i + 1].StartHour;

                        int gap = nextStart - currentEnd - 1;
                        if (gap > 0) softCost += gap; // Her boşluk saati kadar ceza
                    }
                }
            }

            solution.HardViolations = hardViolations;
            solution.SoftCost = softCost;
        }

        // İki dersin zaman olarak çakışıp çakışmadığını kontrol eder
        private static bool IsOverlapping(ScheduleEntry e1, ScheduleEntry e2)
        {
            if (e1.Day != e2.Day) return false;

            var slots1 = e1.GetOccupiedHours();
            var slots2 = e2.GetOccupiedHours();

            return slots1.Intersect(slots2).Any();
        }
    }
}