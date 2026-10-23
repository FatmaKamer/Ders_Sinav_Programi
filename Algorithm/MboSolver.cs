using System;
using System.Collections.Generic;
using System.Linq;
using Ders_Sinav_Programi.Algorithm.Fitness;
using Ders_Sinav_Programi.Algorithm.Models;
using Ders_Sinav_Programi.Algorithm.Operators;
using Ders_Sinav_Programi.Algorithm.Solution;

namespace Ders_Sinav_Programi.Algorithm
{
    public class MboSolver
    {
        // MBO Parametreleri
        public int PopulationSize { get; set; } = 10;       // Sürüdeki kuş sayısı (n)
        public int NeighborsPerBird { get; set; } = 5;      // Her kuşun ürettiği komşu (k)
        public int SharedNeighbors { get; set; } = 2;       // Liderin takipçilere vereceği komşu (x)
        public int MaxTours { get; set; } = 2000;           // Tur sayısı (m)

        // SA Parametreleri (Hibrit için)
        public double InitialTemperature { get; set; } = 50.0;
        public double CoolingRate { get; set; } = 0.999;

        private Random _random = new Random();

        public ScheduleSolution Solve(List<Course> courses, List<Room> rooms, List<TimeSlot> timeSlots, List<string> activeConstraints)
        {
            Console.WriteLine("MBO + SA Hibrit Algoritması Başlıyor...");

            // 1. Başlangıç Sürüsünü (Popülasyonu) Oluştur
            List<ScheduleSolution> population = new List<ScheduleSolution>();
            for (int i = 0; i < PopulationSize; i++)
            {
                var bird = InitialSolutionGenerator.Generate(courses, rooms, timeSlots);
                FitnessCalculator.Evaluate(bird, courses, rooms, activeConstraints);
                population.Add(bird);
            }

            // Sürüyü en iyiden en kötüye sırala
            SortPopulation(population);

            var bestSolution = population[0].Clone();
            double temperature = InitialTemperature;

            Console.WriteLine($"Başlangıç En İyi Hard: {bestSolution.HardViolations}, Soft: {bestSolution.SoftCost}");

            // 2. Ana Döngü (Turlar)
            for (int tour = 0; tour < MaxTours; tour++)
            {
                // Lider kuş (population[0]) komşularını üretsin
                List<ScheduleSolution> leaderSharedNeighbors = new List<ScheduleSolution>();

                for (int k = 0; k < NeighborsPerBird; k++)
                {
                    var neighbor = NeighborhoodOperators.GenerateNeighbor(population[0], courses, rooms, timeSlots);
                    FitnessCalculator.Evaluate(neighbor, courses, rooms, activeConstraints);
                    leaderSharedNeighbors.Add(neighbor);
                }

                // Liderin komşularını en iyiden en kötüye sırala
                leaderSharedNeighbors = leaderSharedNeighbors
                    .OrderBy(s => s.HardViolations)
                    .ThenBy(s => s.SoftCost)
                    .ToList();

                // Liderin en iyi komşusu, mevcut liderden daha iyiyse lideri güncelle (SA kuralı ile)
                TryAcceptNewSolution(population, 0, leaderSharedNeighbors[0], temperature);

                // Takipçi kuşlar (1'den PopulationSize-1'e kadar)
                int shareIndex = 0;
                for (int i = 1; i < PopulationSize; i++)
                {
                    List<ScheduleSolution> candidates = new List<ScheduleSolution>();

                    // Liderden pay al (x kadar)
                    for (int x = 0; x < SharedNeighbors && shareIndex < leaderSharedNeighbors.Count; x++)
                    {
                        candidates.Add(leaderSharedNeighbors[shareIndex]);
                        shareIndex++;
                    }

                    // Kuş kendi komşularını da üretsin (k - x kadar)
                    for (int k = SharedNeighbors; k < NeighborsPerBird; k++)
                    {
                        var neighbor = NeighborhoodOperators.GenerateNeighbor(population[i], courses, rooms, timeSlots);
                        FitnessCalculator.Evaluate(neighbor, courses, rooms, activeConstraints);
                        candidates.Add(neighbor);
                    }

                    // Adaylar arasından en iyisini seç
                    var bestCandidate = candidates
                        .OrderBy(s => s.HardViolations)
                        .ThenBy(s => s.SoftCost)
                        .First();

                    // SA kuralı ile kabul et
                    TryAcceptNewSolution(population, i, bestCandidate, temperature);
                }

                // Sürüyü tekrar sırala
                SortPopulation(population);

                // En iyi çözümü güncelle
                if (IsBetter(population[0], bestSolution))
                {
                    bestSolution = population[0].Clone();
                }

                // Sıcaklığı düşür
                temperature *= CoolingRate;

                // Her 100 turda bir ekrana yazdır
                if (tour % 100 == 0)
                {
                    Console.WriteLine($"Tur {tour,4}: Sıcaklık: {temperature,5:F2} | En İyi Hard: {bestSolution.HardViolations} | En İyi Soft: {bestSolution.SoftCost}");
                }

                // Erken çıkış (Mükemmel çözüm bulunduysa)
                if (bestSolution.HardViolations == 0 && bestSolution.SoftCost == 0)
                {
                    Console.WriteLine($"\n[MÜKEMMEL ÇÖZÜM] Tur {tour}'da Hard=0 ve Soft=0 bulundu!");
                    break;
                }
            }

            return bestSolution;
        }

        // SA Kabul Kriteri (Kötü çözümü sıcaklığa göre kabul etme)
        private void TryAcceptNewSolution(List<ScheduleSolution> population, int birdIndex, ScheduleSolution candidate, double temperature)
        {
            var current = population[birdIndex];

            int deltaHard = candidate.HardViolations - current.HardViolations;
            int deltaSoft = candidate.SoftCost - current.SoftCost;

            bool accept = false;

            if (deltaHard < 0) accept = true; // Hard azaldıysa kesin kabul
            else if (deltaHard > 0) accept = false; // Hard arttıysa kesin red
            else
            {
                if (deltaSoft <= 0) accept = true; // Soft azaldıysa kabul
                else
                {
                    // Soft arttıysa SA olasılığı
                    double probability = Math.Exp(-deltaSoft / temperature);
                    if (_random.NextDouble() < probability) accept = true;
                }
            }

            if (accept)
            {
                population[birdIndex] = candidate.Clone();
            }
        }

        private void SortPopulation(List<ScheduleSolution> population)
        {
            population.Sort((a, b) =>
            {
                if (a.HardViolations != b.HardViolations)
                    return a.HardViolations.CompareTo(b.HardViolations);
                return a.SoftCost.CompareTo(b.SoftCost);
            });
        }

        private bool IsBetter(ScheduleSolution a, ScheduleSolution b)
        {
            if (a.HardViolations != b.HardViolations)
                return a.HardViolations < b.HardViolations;
            return a.SoftCost < b.SoftCost;
        }
    }
}