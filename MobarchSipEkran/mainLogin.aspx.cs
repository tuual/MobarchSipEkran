using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MobarchSipEkran
{
	public partial class mainLogin : System.Web.UI.Page
	{

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
                Session.Remove("ConnStr");
                Session.Remove("SISTEMCARIKOD");
                Session.Remove("ALTCARIKOD");
                Session.Remove("VKN");
                Session.Remove("Kadi");
            }
		}


        protected void btnGiris_Click(object sender, EventArgs e)
        {
            var vkn = (txtVKN.Text ?? "").Trim();
            var kadi = (txtKadi.Text ?? "").Trim();
            var sifre = (txtParola.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(vkn) || string.IsNullOrWhiteSpace(kadi) || string.IsNullOrWhiteSpace(sifre))
            {
                Alert("Lütfen tüm alanları doldurunuz.");
                return;
            }

            try
            {
                var userRow = Db.ExecuteRow(@"SELECT TOP 1 SISTEMCARIKOD , ALTCARIKOD FROM tWebKullaniciGiris 
                                            WHERE txtVKN = @VKN AND txtKadi = @KADI and txtSifre = @SIFRE",
                                            new System.Data.SqlClient.SqlParameter("@VKN", vkn),
                                            new System.Data.SqlClient.SqlParameter("@KADI", kadi),
                                            new System.Data.SqlClient.SqlParameter("@SIFRE", sifre)
                                            );

                if (userRow == null)
                {
                    Alert("Bilgiler hatalı. Lütfen tekrar kontrol edin.");
                    return;
                }


                string sistemCariKod = Convert.ToString(userRow["SISTEMCARIKOD"]); 
                string altcarikod = Convert.ToString(userRow["ALTCARIKOD"]);

                if (string.IsNullOrWhiteSpace(sistemCariKod) || string.IsNullOrWhiteSpace(altcarikod))
                {
                    Alert("Kullanıcı bilgileri eksik. Lütfen yöneticinizle iletişime geçin.");
                    return;
                }


                var connRow = Db.ExecuteRow(@"SELECT TOP 1 SISTEMBAGLANTI, SISTEMKADI, SISTEMSIFRE,SISTEMDB FROM tWebBilgiler WHERE SISTEMCARIKOD = @SC",
                    new System.Data.SqlClient.SqlParameter("@SC", sistemCariKod));


                if (connRow == null)
                {
                    Alert("Sistem bağlantı bilgisi bulunamadı. (tWebBilgiler)");
                    return;
                }

                String ds = Convert.ToString(connRow["SISTEMBAGLANTI"]);
                String uid = Convert.ToString(connRow["SISTEMKADI"]);
                String pwd = Convert.ToString(connRow["SISTEMSIFRE"]);
                String db = Convert.ToString(connRow["SISTEMDB"]);

                if (string.IsNullOrWhiteSpace(ds) || string.IsNullOrWhiteSpace(db))
                {
                    Alert("Sistem bağlantı bilgileri eksik. Lütfen yöneticinizle iletişime geçin.");
                    return;
                }

                string customerConnStr = BuildSqlConnString(ds, db, uid, pwd);

                using(var con = new SqlConnection(customerConnStr))
                {
                    con.Open();
                }

                Session["ConnStr"] = customerConnStr;
                Session["SISTEMCARIKOD"] = sistemCariKod;
                Session["ALTCARIKOD"] = altcarikod;
                Session["VKN"] = vkn;
                Session["Kadi"] = kadi;        
                    
                ClassDoldurma(customerConnStr, sistemCariKod, altcarikod, vkn, kadi);
                Response.Redirect("~/mainSiparis.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                Alert("Giriş Sırasında Bir hata ile karşılaşıldı " + '\n' + ex.ToString());
            }


            // ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Giriş Başarılı!')",true);
        }

        private string BuildSqlConnString(string dataSource, string database, string user, string pass)
        {
            var sb = new SqlConnectionStringBuilder
            {
                DataSource = dataSource,
                InitialCatalog = database,
                UserID = user,
                Password = pass,
                MultipleActiveResultSets = true,
                TrustServerCertificate = true,
                Encrypt = false,
                ConnectTimeout = 15
            };


            return sb.ToString();
        }

        private void ClassDoldurma(string connstr , string  sistemcarikod, string altcarikod , string vkn,string kadi)
        {
            //DbHelper.Sessions session = new DbHelper.Sessions();
            //session.Connstr = "Data Source=DESKTOP-ABCD123;Initial Catalog=MyDatabase;

            DbHelper.Sessions session = new DbHelper.Sessions();
            session.Connstr = connstr;
            session.Sistemcarikod = sistemcarikod;
            session.Altcarikod = altcarikod;
            session.Vkn = vkn;  
            session.Kadi = kadi;
        }

        private void Alert(string msg)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('" + msg.Replace("'", "\\'") + "')", true);
        }
    }
}