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

        public void AlertMsg(string msg,Page page)
        {
            page.ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('" + msg.Replace("'", "\\'") + "')", true);
        }
    }
}