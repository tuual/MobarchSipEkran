using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobarchSipEkran.Class
{
    public static class BirimIsimlendirme
    {
        public static string BirimDuzeltme(string gelecekBirim)
        {
            string[] birimler = { "AD", "KL", "PK", "KG", "LT", "M2", "M3", "CM", "MM", "GR", "LGR", "KGR", "TON" };
            if (gelecekBirim == "AD")
            {
                return "ADET";
            }
            if (gelecekBirim == "KL")
            {
                return "KOLİ";
            }
            if (gelecekBirim == "PK")
            {
                return "PAKET";
            }
            if (gelecekBirim == "KG")
            {
                return "KİLOGRAM";
            }
            if (gelecekBirim == "LT")
            {
                return "LİTRE";
            }
            if (gelecekBirim == "M2")
            {
                return "METREKARE";
            }
            if (gelecekBirim == "M3")
            {
                return "METREKÜP";
            }
            if (gelecekBirim == "CM")
            {
                return "SANTİMETRE";
            }
             if (gelecekBirim == "MM")
            {
                return "MİLİMETRE";
            }
             if (gelecekBirim == "GR")
            {
                return "GRAM";
            }
             if (gelecekBirim == "LGR")
            {
                return "LİTREGRAM";
            }
             if (gelecekBirim == "KGR")
            {
                return "KİLOGRAM";
            }
             if (gelecekBirim == "TON")
            {
                return "TON";
            }
                return gelecekBirim;
        }
    }
}