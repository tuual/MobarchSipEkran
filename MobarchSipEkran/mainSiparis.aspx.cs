using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Ajax.Utilities;
using MobarchSipEkran.Class;
using MobarchSipEkran.DbHelper;

namespace MobarchSipEkran
{
    public partial class mainSiparis : System.Web.UI.Page
    {
        private readonly CultureInfo tr = new CultureInfo("tr-TR");

        Alert alert = new Alert();
        // kdv hesaplama gpt den aldım
        private static decimal NormalizeKdv(object val, CultureInfo culture)
        {
            if (val == null || val == DBNull.Value) return 0m;
            var k = Convert.ToDecimal(val, culture); // 1  veya 0.01 gelebilir
            if (k >= 1m) k /= 100m;                   // 1 -> 0.01
            if (k < 0m) k = 0m;
            return Math.Round(k, 4);
        }

        private decimal ParseDec(string s)
        {
            return decimal.TryParse(s ?? "0", NumberStyles.Any, tr, out var d) ? d : 0m;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var conn = Db.ResolveConnStr();
            if (string.IsNullOrEmpty(conn))
            {
                ClientScript.RegisterStartupScript(GetType(), "yok", "alert('Bağlantı bulunamadı.');", true);
                Response.Redirect("/mainLogin.aspx");
            }
            if (!IsPostBack)
            {
               

                try
                {
                    txtBelgeNo.Text = GetNextFatirsNoFromSeri();
                }
                catch (Exception ex)
                {
                    alert.AlertMsg("Belge numarası üretilemedi: " + ex.Message, this,"belgeNoUyari");
                    txtBelgeNo.Text = "";
                }
                txtTarih.Text = DateTime.Now.ToString("yyyy-MM-dd");
                BindGrid();
                RecalcTotals();
              
                CariBilgileri();


             
            }

        }

        private DataTable CreateStoklarTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("StokKodu");
            dt.Columns.Add("StokAdi");
            dt.Columns.Add("Miktar", typeof(decimal));
            dt.Columns.Add("Fiyat", typeof(decimal));
            dt.Columns.Add("Tutar", typeof(decimal));
            dt.Columns.Add("KdvOran", typeof(decimal));
            dt.Columns.Add("KdvTutar", typeof(decimal));
            dt.Columns.Add("KdvDahilTutar", typeof(decimal));
            return dt;
        }
        private DataTable Stoklar
        {
            get
            {
                if (Session["Stoklar"] == null)
                    Session["Stoklar"] = CreateStoklarTable();

                return (DataTable)Session["Stoklar"];
            }
            set
            {
                Session["Stoklar"] = value;
            }
        }

        private void BindGrid()
        {
           
                if (Session["ConnStr"] == null)
                {
                    return;
                }
                else
                {
                    string sessionID = Session["SID"].ToString();
                    string sql = @"SELECT T.StokKodu, S.STOK_ADI as StokAdi, T.Miktar, S.SATIS_FIAT1 as Fiyat, 
                   (T.Miktar * S.SATIS_FIAT1) as Tutar, (S.KDV_ORANI / 100.0) as KdvOran,
                    ((T.Miktar * S.SATIS_FIAT1)/100)*S.KDV_ORANI AS 'KdvTutar',
                    (((T.Miktar * S.SATIS_FIAT1)*S.KDV_ORANI)/100)+T.Miktar * S.SATIS_FIAT1 AS 'KdvDahilTutar'
                   FROM tWebSiparis T 
                   INNER JOIN tStokMaster S ON T.StokKodu = S.STOK_KODU
                   WHERE T.SessionID = @SID";
                    DataTable dt = Db.ExecuteDataTable(sql, new SqlParameter("@SID", sessionID));
                    Session["Stoklar"] = dt;
                    gvStoklar.DataSource = dt;
                    gvStoklar.DataBind();

                }
            }
           

            
            
             
        

        protected void stokSecModal_StokSecildi(object sender, StokSec.StokSecEventArgs e)
        {
          

            try
            {
                BindGrid();
                RefreshTotalsPanel();
                upGrid.Update();
                upTotals.Update();


            }
            catch (Exception ex)
            {

                ScriptManager.RegisterStartupScript(this, GetType(), "hata", "alert('Hata oluştu: " + ex.Message.Replace("'", "\\'") + "');", true);
            }

        }

        protected void btnEkle_Click(object sender, EventArgs e)
        {
        }



        protected void gvStoklar_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out var rowIndex)) return;
            if (rowIndex < 0 || rowIndex >= gvStoklar.Rows.Count) return;

            var kod = Convert.ToString(gvStoklar.DataKeys[rowIndex].Value);

            if (e.CommandName == "Sil")
            {
                var dt = Stoklar;
                var dr = dt.AsEnumerable().FirstOrDefault(r => r.Field<string>("StokKodu") == kod);
                if (dr != null)
                {
                    var varmi = Db.ExecuteNonQuery("SELECT COUNT(*) FROM tWebSiparis WHERE StokKodu = @SK AND SessionId = @SID", new SqlParameter("@SK", kod),
                        new SqlParameter("@SID", Session.SessionID));
                    if (varmi == 0)
                    {
                        BildirimHelper.MesajGoster(this, "Ürün tWebSiparis tablosunda SessionId ve StokKodu bulunamadı", false);
                    }
                    Db.ExecuteNonQuery("Delete from tWebSiparis WHERE StokKodu = @SK and SessionId = @SID", new SqlParameter("@SK", kod),
                        new SqlParameter("@SID", Session.SessionID));
                    
                    dt.Rows.Remove(dr);   
                }

                Stoklar = dt;
                BindGrid();
                RefreshTotalsPanel();    
            }
        }
      /*  protected void RowMiktar_TextChanged(object sender, EventArgs e)
        {
            var txt = (TextBox)sender;
            var row = (GridViewRow)txt.NamingContainer;
            int rowIndex = row.RowIndex;
            if (rowIndex < 0) return;

            var dt = Stoklar;
            var kod = Convert.ToString(gvStoklar.DataKeys[rowIndex].Value);
            var dr = dt.AsEnumerable().First(r => r.Field<string>("StokKodu") == kod);

            decimal yeniMiktar = ParseDec(txt.Text);
            decimal fiyat = dr["Fiyat"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["Fiyat"]);
            decimal kdvOran = dr["KdvOran"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["KdvOran"]);
            Db.ExecuteNonQuery("UPDATE tWebSiparisDetayTemp SET Miktar = @Miktar WHERE StokKodu = @SK and SessionId = @SID ",
                new SqlParameter("@Miktar", yeniMiktar), new SqlParameter("@SK", kod),
                new SqlParameter("@SID", Session.SessionID.ToString()));
            decimal net = Math.Round(yeniMiktar * fiyat, 6);
            decimal kdv = Math.Round(net * kdvOran, 6);
            decimal dahil = net + kdv;

            

            Stoklar = dt;
            BindGrid();
            RefreshTotalsPanel();
        }

      */
        private void RecalcTotals()
        {
            var dt = Stoklar;

            decimal brutNet = dt.AsEnumerable().Sum(r => Convert.ToDecimal(r["Tutar"] == DBNull.Value ? 0 : r["Tutar"]));
            decimal toplamKdv = dt.AsEnumerable().Sum(r => Convert.ToDecimal(r["KdvTutar"] == DBNull.Value ? 0 : r["KdvTutar"]));

            decimal iskonto = ParseDec(txtIskonto.Text);
            if (iskonto < 0m) iskonto = 0m;
            if (iskonto > brutNet) iskonto = brutNet;


            // işlemler
            decimal araToplam = brutNet - iskonto;
            decimal factor = brutNet > 0m ? araToplam / brutNet : 1m;
            decimal kdvAfter = Math.Round(toplamKdv * factor, 2);

          

            txtBrutTutar.Text = brutNet.ToString("N2", tr);
            txtAraToplam.Text = araToplam.ToString("N2", tr);
            txtKdvToplam.Text = kdvAfter.ToString("N2", tr);
            txtGenelToplam.Text = (araToplam + kdvAfter).ToString("N2", tr);

        }


        protected void btnKaydet_Click(object sender, EventArgs e)
        {
        }

        protected void gvStoklar_PreRender(object sender, EventArgs e)
        {
            if (gvStoklar.HeaderRow != null)
            {
                gvStoklar.UseAccessibleHeader = true;
                gvStoklar.HeaderRow.TableSection = TableRowSection.TableHeader;
            }
        }

        protected void txtIskonto_TextChanged(object sender, EventArgs e)
        {

        }

        private void RefreshTotalsPanel()
        {
            RecalcTotals();
            CariBilgileri();
            upTotals.Update();
        }


        private void CariBilgileri()
        {
            var info = Services.CariService.GetFromSession();
            
            if (info == null)
            {
                lblCariBakiye.Text = "0,00";
                lblRiskLimiti.Text = "0,00";
                lblKalanLimit.Text = "0,00";
                lblKalanLimit.ForeColor = Color.Black;
                alert.AlertMsg("Cari bakiye bilgileri alınamadı", Page,"cariBakiyeUyari");
                return;
            }
            if (info.RiskLimiti == 0)
            {
                lblCariBakiye.Text = info.Bakiye.ToString("N2", tr);
                decimal kullanilabilir = Math.Max(0, info.RiskLimiti - info.Bakiye);

                decimal siparisGenelToplam = ParseDec(txtGenelToplam.Text);


                decimal kalanLimit = kullanilabilir - siparisGenelToplam;
            }
            else
            {
                lblCariBakiye.Text = info.Bakiye.ToString("N2", tr);
                lblRiskLimiti.Text = info.RiskLimiti.ToString("N2", tr);



                decimal kullanilabilir = Math.Max(0, info.RiskLimiti - info.Bakiye);

                decimal siparisGenelToplam = ParseDec(txtGenelToplam.Text);


                decimal kalanLimit = kullanilabilir - siparisGenelToplam;

                lblKalanLimit.Text = kalanLimit.ToString("N2", tr);
                lblKalanLimit.ForeColor = kalanLimit < 0 ? Color.Red : Color.Black;
            }

             
        }

        private string GetNextFatirsNoFromSeri()
        {
            // 1) Önce IMPSERI'den seri prefix (ilk 3 hane) al
            var seriRow = Db.ExecuteRow(
                "SELECT IMPSERI FROM dbo.tMainEvrakSerileri WHERE KULID = 0");

            if (seriRow == null || seriRow["IMPSERI"] == DBNull.Value)
                throw new Exception("IMPSERI bulunamadı (tMainEvrakSerileri.KULID=0).");

            string imps = Convert.ToString(seriRow["IMPSERI"]).Trim();
            if (string.IsNullOrEmpty(imps))
                throw new Exception("IMPSERI boş geldi.");

            string prefix = imps.Length >= 3 ? imps.Substring(0, 3) : imps.PadRight(3, '0');

            const string sql = @"
        SELECT MAX(FATIRS_NO) AS SONNO
        FROM dbo.tFatura
        WHERE FATIRS_NO LIKE @P + '%';
    ";

            var row = Db.ExecuteRow(sql, new SqlParameter("@P", prefix));

            string last = (row == null || row["SONNO"] == DBNull.Value)
                ? null
                : Convert.ToString(row["SONNO"]);

            int totalLen = 15;                       
            int numericLen = totalLen - prefix.Length;

            if (numericLen <= 0)
                throw new Exception("Belge numarası uzunluğu hatalı (prefix çok uzun).");

            if (string.IsNullOrWhiteSpace(last))
            {
                string firstNumber = 1.ToString().PadLeft(numericLen, '0');
                return prefix + firstNumber;
            }

            if (!last.StartsWith(prefix))
                throw new Exception("Son FATIRS_NO beklenen seri ile başlamıyor.");

            string numericPart = last.Substring(prefix.Length);
            if (!numericPart.All(char.IsDigit))
                throw new Exception("FATIRS_NO sayısal olmayan karakter içeriyor.");

            long n = long.Parse(numericPart);
            n++; 

            string nextNumeric = n.ToString().PadLeft(numericPart.Length, '0');

            // 5) Yeni belge numarası
            return prefix + nextNumeric;
        }

       
    }
}
