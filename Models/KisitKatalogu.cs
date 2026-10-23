using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ders_Sinav_Programi.Models
{
    [Table("Kisit_Katalogu")]
    public class KisitKatalogu
    {
        [Key]
        public int Kisit_id { get; set; }

        [Required]
        [StringLength(50)]
        public string Kisit_Kodu { get; set; }

        [Required]
        [StringLength(200)]
        public string Kisit_Adi { get; set; }

        public string Aciklama { get; set; }

        [Required]
        [StringLength(10)]
        public string Kisit_Turu { get; set; } // 'Hard' veya 'Soft'

        public int Varsayilan_Puan { get; set; } = 50;

        [Required]
        [StringLength(20)]
        public string Kategori { get; set; } // hoca, ders, ogrenci, salon, sinav

        [Required]
        [StringLength(20)]
        public string Parametre_Tipi { get; set; } // int, bool, time, list, none

        public string Parametre_Degeri { get; set; } // JSON formatında şablon değer

        public bool Aktif_mi { get; set; } = true;

        public int Ekleyen_id { get; set; }

        public DateTime Ekleme_Tarihi { get; set; } = DateTime.Now;

        // Navigation Property (Kullanıcı tablosuna bağlantı)
        [ForeignKey("Ekleyen_id")]
        public virtual KullaniciModel EkleyenKullanici { get; set; }
    }
}
