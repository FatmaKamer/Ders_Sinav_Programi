using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ders_Sinav_Programi.Models
{
    [Table("Bolum_Kisitlari")]
    public class BolumKisitlari
    {
        [Key]
        public int Id { get; set; }

        public int Bolum_id { get; set; }

        public int Kisit_id { get; set; }

        // Editör puanı değiştirmezse null kalır, veritabanı varsayılanı kullanır
        public int? Ceza_Puani { get; set; }

        // Editör JSON değerini değiştirirse (örn: max sınav sayısını 2'den 3'e çekerse) buraya kaydolur
        public string Parametre_Degeri { get; set; }

        public bool Aktif_mi { get; set; } = true;

        public int? Guncelleyen_id { get; set; }
        
        public DateTime? Guncellenme_Tarihi { get; set; }

        // Navigation Properties (İlişkili tablolar)
        [ForeignKey("Kisit_id")]
        public virtual KisitKatalogu KisitKatalogu { get; set; }

        [ForeignKey("Bolum_id")]
        public virtual BolumModel Bolum { get; set; }

        [ForeignKey("Guncelleyen_id")]
        public virtual KullaniciModel GuncelleyenKullanici { get; set; }
    }
}