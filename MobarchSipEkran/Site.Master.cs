using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MobarchSipEkran
{
    public partial class SiteMaster : MasterPage
    {
        string syscarkod;
        protected void Page_Load(object sender, EventArgs e)
        {
            string path = Request.Url.AbsolutePath.ToLower();
            if (path.Contains("mainLogin.aspx"))
            {
                if (NavBar != null)
                    NavBar.Visible = false;


            }





            syscarkod = Session?["SISTEMCARIKOD"] as string;
            if (string.IsNullOrWhiteSpace(syscarkod))
            {
                Response.Redirect("mainLogin.aspx");
            }
            else
            {
                anaCariUnvanBulma();
            }


        }
        private void anaCariUnvanBulma()
        {
            var subeadi = Db.ExecuteDataTable("SELECT SUBE_ISMI FROM tSubeMaster");
            if (subeadi.Rows.Count > 0)
            {
                lblFirma.Text= subeadi.Rows[0][0].ToString();
            }
        }
    }
}
