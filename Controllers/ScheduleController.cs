using Microsoft.AspNetCore.Mvc;
using Ders_Sinav_Programi.Services;
using Ders_Sinav_Programi.Algorithm.Solution;

namespace Ders_Sinav_Programi.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly AlgorithmService _algorithmService = new AlgorithmService();

        // Sayfa ilk açıldığında (GET)
        public IActionResult Generate()
        {
            // Fakülteleri, Bölümleri ve Dönemleri dropdown için çekebiliriz (Şimdilik basit tutuyoruz)
            return View();
        }

        // Butona basıldığında (POST)
        [HttpPost]
        public IActionResult Generate(int bolumId, int donemId)
        {
            ViewBag.BolumId = bolumId;
            ViewBag.DonemId = donemId;
            ViewBag.Message = "Algoritma çalışıyor, lütfen bekleyin...";

            // Algoritmayı çalıştır ve sonucu al
            ScheduleSolution result = _algorithmService.GenerateSchedule(bolumId, donemId);

            ViewBag.Hard = result.HardViolations;
            ViewBag.Soft = result.SoftCost;
            ViewBag.Message = null; // İşlem bitti

            return View(result);
        }

        // Programı Görüntüle Sayfası
        public IActionResult ViewSchedule(int bolumId = 1, int donemId = 1, int sinifDuzeyiId = 1)
        {
            var scheduleList = _algorithmService.GetScheduleFromDb(bolumId, donemId, sinifDuzeyiId);

            ViewBag.BolumId = bolumId;
            ViewBag.DonemId = donemId;
            ViewBag.SinifDuzeyiId = sinifDuzeyiId;

            return View(scheduleList);
        }
    }
}