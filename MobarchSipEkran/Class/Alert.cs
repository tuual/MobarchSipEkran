using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MobarchSipEkran.Class
{
    public class Alert
    {

        public void AlertMsg(string msg,Page page,string baslik)
        {
            page.ClientScript.RegisterStartupScript(this.GetType(), baslik, "alert('" + msg.Replace("'", "\\'") + "')", true);
        }
    }
}