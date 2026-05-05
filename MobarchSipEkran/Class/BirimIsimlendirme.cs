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
            if (gelecekBirim == "AD")
            {
                return "ADET";
            }
            if (gelecekBirim == "KL")
            {
                return "KOLİ";
            }
            if (gelecekBirim == "PL")
            {
                return "PALET";
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
            if (gelecekBirim== "ADET")
            {
                return "AD";
            }
            if (gelecekBirim == "PAKET")
            {
                return "PK";
            }
            if (gelecekBirim == "KOLİ")
            {
                return "KL";
            }
            if (gelecekBirim == "KİLOGRAM")
            {
                return "KG";
            }
            if (gelecekBirim == "LİTRE")
            {
                return "LT";
            }
            if (gelecekBirim == "METREKARE")
            {
                return "M2";
            }
            if (gelecekBirim == "METREKÜP")
            {
                return "M3";
            }
            if (gelecekBirim == "SANTİMETRE")
            {
                return "CM";
            }
            if (gelecekBirim == "MİLİMETRE")
            {
                return "MM";
            }
            if (gelecekBirim == "GRAM")
            {
                return "GR";
            }
            if (gelecekBirim == "LİTREGRAM")
            {
                return "LGR";
            }
            
            if (gelecekBirim == "TON")
            {
                return "TON";
            }
            if (gelecekBirim == "PALET")
            {
                return "PL";
            }

            return gelecekBirim;
        }
    }
}