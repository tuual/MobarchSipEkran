using System;

namespace MobarchSipEkran.Class
{
 public static class SiparisHesaplayici
    {
        public static SatirSonuc Hesapla(double hamFiyat, double miktar, double kdvOrani,bool isKdvDahil)
        {

            double kdvliBirimFiyat = 0;
            double oran = 1 + (kdvOrani / 100);

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
            public double BirimFiyat { get; set; }
            public double SatirToplam { get; set; }
        }
    }
}