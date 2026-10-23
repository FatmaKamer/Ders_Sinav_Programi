using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ders_Sinav_Programi.Models
{
    [Table("Kullanici")]
    public class KullaniciModel
    {
        [Key]
        public int Kullanici_id { get; set; } // PK[cite: 3]

        [Required]
        [StringLength(100)]
        public string Email { get; set; } // nvarchar(100)[cite: 3]

        [Required]
        [StringLength(255)]
        public string Sifre_hash { get; set; } // nvarchar(255)[cite: 3]

        public int? Rol_id { get; set; } // FK, boş geçilebilir[cite: 3]

        public bool? Aktif_mi { get; set; } // bit, boş geçilebilir[cite: 3]

        public DateTime? Olusturma_tarihi { get; set; } // datetime, boş geçilebilir[cite: 3]
    }
}