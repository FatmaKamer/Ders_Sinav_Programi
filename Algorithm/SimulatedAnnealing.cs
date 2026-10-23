using System;
using System.Collections.Generic;
using System.Linq;
using Ders_Sinav_Programi.Algorithm.Fitness;
using Ders_Sinav_Programi.Algorithm.Models;
using Ders_Sinav_Programi.Algorithm.Operators;
using Ders_Sinav_Programi.Algorithm.Solution;

namespace Ders_Sinav_Programi.Algorithm
{
    public class SimulatedAnnealing
    {
        // Ayarlar (Parametreler)
        public double InitialTemperature { get; set; } = 100.0;
        public double CoolingRate { get; set; } = 0.999;
        public int MaxIterations { get; set; } = 10000;

        public ScheduleSolution Solve(List<Course> courses, List<Room> rooms, List<TimeSlot> timeSlots, List<string> activeConstraints)
        {
            // 1. Rastgele bir başlangıç çözümü üret
            var currentSolution = InitialSolutionGenerator.Generate(courses, rooms, timeSlots);
            FitnessCalculator.Evaluate(currentSolution, courses, rooms, activeConstraints);

            // En iyi çözümü hafızada tut (çünkü SA bazen kötüye gidebilir)
            var bestSolution = currentSolution.Clone();
            double currentTemperature = InitialTemperature;

            Console.WriteLine($"Başlangıç Hard: {currentSolution.HardViolations}, Soft: {currentSolution.SoftCost}");

            // 2. Tavlama Döngüsü
            for (int i = 0; i < MaxIterations; i++)
            {
                // 2.1. Komşu çözüm üret ve değerlendir
                var neighbor = NeighborhoodOperators.GenerateNeighbor(currentSolution, courses, rooms, timeSlots);
                FitnessCalculator.Evaluate(neighbor, courses, rooms, activeConstraints);

                // 2.2. Fitness Farkını Hesapla (Lexicographic Kural)
                // Hard kısıtlar her zaman önceliklidir! (Hocanın dökümanı Bölüm 7.2)
                int deltaHard = neighbor.HardViolations - currentSolution.HardViolations;
                int deltaSoft = neighbor.SoftCost - currentSolution.SoftCost;

                bool accept = false;

                // EĞER HARD KISIT AZALDIYSA -> HER ZAMAN KABUL ET
                if (deltaHard < 0) accept = true;
                // EĞER HARD ARTTIYSA -> REDDET
                else if (deltaHard > 0) accept = false;
                // EĞER HARD AYNIYSA -> SOFT'A BAK (Standart SA Olasılığı)
                else if (deltaHard == 0)
                {
                    if (deltaSoft <= 0) accept = true; // Soft azaldıysa kabul et
                    else
                    {
                        // Soft arttıysa, sıcaklığa bağlı olasılıkla kabul et (Kötü çözümü kabul etme)
                        double probability = Math.Exp(-deltaSoft / currentTemperature);
                        if (new Random().NextDouble() < probability) accept = true;
                    }
                }

                // 2.3. Eğer kabul edildiyse mevcut çözümü güncelle
                if (accept)
                {
                    currentSolution = neighbor;

                    // Eğer bu yeni çözüm şimdiye kadarki en iyi çözümse, hafızaya kaydet
                    if (currentSolution.HardViolations < bestSolution.HardViolations ||
                       (currentSolution.HardViolations == bestSolution.HardViolations && currentSolution.SoftCost < bestSolution.SoftCost))
                    {
                        bestSolution = currentSolution.Clone();
                    }
                }

                // 2.4. Sıcaklığı düşür
                currentTemperature *= CoolingRate;

                // Sadece 1000'de bir ekrana yazdıralım (konsol kilitlenmesin)
                if (i % 1000 == 0)
                {
                    Console.WriteLine($"İterasyon {i,5}: Sıcaklık: {currentTemperature,5:F2} | Mevcut Hard: {currentSolution.HardViolations} | En İyi Hard: {bestSolution.HardViolations}");
                }
            }

            return bestSolution;
        }
    }
}