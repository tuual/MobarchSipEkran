using System;

namespace MobarchSipEkran.Class
{
 public static class SiparisHesaplayici
    {
        public static SatirSonuc Hesapla(decimal hamFiyat, decimal miktar,decimal kdvOrani,bool isKdvDahil)
        {

            decimal kdvliBirimFiyat = 0;
            decimal oran = 1 + (kdvOrani / 100);

            if (!isKdvDahil)
            {
                kdvliBirimFiyat = hamFiyat* oran;

            }
            else
            {
                kdvliBirimFiyat = hamFiyat;
            }
            return new SatirSonuc
            {
                BirimFiyat = kdvliBirimFiyat,
                SatirToplam = Math.Round(kdvliBirimFiyat * miktar,2)
            };
        }
        public class SatirSonuc
        {
            public decimal BirimFiyat { get; set; }
            public decimal SatirToplam { get; set; }
        }
    }
}