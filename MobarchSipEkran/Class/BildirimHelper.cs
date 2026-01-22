using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace MobarchSipEkran.Class
{
    public static class BildirimHelper
    {
        public static void MesajGoster(Control page, string mesaj,bool isError = false)
        {
            string type = isError ? "error" : "success";

            string script = $"setTimeout(function() {{ window.showToast('{mesaj}', '{type}'); }}, 200);";

            ScriptManager.RegisterStartupScript(page, page.GetType(), "toastNotif", script, true);
        }
    }
}