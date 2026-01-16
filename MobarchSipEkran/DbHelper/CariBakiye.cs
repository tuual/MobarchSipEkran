using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobarchSipEkran.DbHelper
{
    public  sealed class CariBakiye
    {
       
            public string CariKod { get; set; }
            public decimal Bakiye { get; set; }        // CM_BORCT - CM_ALACT
            public decimal RiskLimiti { get; set; }    // CARISK


            
            public decimal Kullanilabilir => RiskLimiti - Math.Max(0m, Bakiye);
            
        
    }
}