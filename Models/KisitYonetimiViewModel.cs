using System.Collections.Generic;

namespace Ders_Sinav_Programi.Models
{
    public class KisitYonetimiViewModel
    {
        public List<FakulteModel> Fakulteler { get; set; }

        public List<KisitKatalogu> KisitKataloguListesi { get; set; }
    }
}
