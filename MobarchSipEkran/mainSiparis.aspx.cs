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
        private const string SIPARIS_KEY = "SiparisGuid";

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
            if (Session["MobarchUser"] == null)
            {
                Response.Redirect("/mainLogin.aspx");
                return;
            }

            if (!IsPostBack)
            {
                TemizleDetay();
                try
                {
                    txtBelgeNo.Text = GetNextFatirsNoFromSeri();
                }
                catch (Exception ex)
                {
                    alert.AlertMsg("Belge numarası üretilemedi: " + ex.Message, this);
                    txtBelgeNo.Text = "";
                }
                txtTarih.Text = DateTime.Now.ToString("yyyy-MM-dd");
                BindGrid();
                RecalcTotals();
                CariBilgileri();    
                txtGenelToplam.Enabled = false;
                txtFiyat.Enabled = false;
                txtStokAdi.Enabled = false;
                txtBelgeNo.Enabled = false;
             
            }

        }


        private DataTable Stoklar
        {
            get
            {
                if (Session["Stoklar"] == null)
                {
                    var dt = new DataTable();
                    dt.Columns.Add("StokKodu");
                    dt.Columns.Add("StokAdi");
                    dt.Columns.Add("Miktar", typeof(decimal));
                    dt.Columns.Add("Fiyat", typeof(decimal));
                    dt.Columns.Add("Tutar", typeof(decimal));           // net
                    dt.Columns.Add("KdvOran", typeof(decimal));         // 0.01
                    dt.Columns.Add("KdvTutar", typeof(decimal));
                    dt.Columns.Add("KdvDahilTutar", typeof(decimal));   // net + kdv
                    Session["Stoklar"] = dt;
                }
                return (DataTable)Session["Stoklar"];
            }
            set { Session["Stoklar"] = value; }
        }

        private void BindGrid()
        {
            gvStoklar.DataSource = Stoklar;
            gvStoklar.DataBind();
        }

        // --- Stok Seç modali event’i ---------------------------------------
        protected void stokSecModal_StokSecildi(object sender, StokSec.StokSecEventArgs e)
        {
            txtStokKodu.Text = e.StokKodu;
            txtStokAdi.Text = e.StokAdi;

            try
            {
                var row = Db.ExecuteRow("SELECT SATIS_FIAT1 FROM tStokMaster WHERE STOK_KODU=@K", new SqlParameter("@K", e.StokKodu));
                if (row != null && row["SATIS_FIAT1"] != DBNull.Value) txtFiyat.Text = Convert.ToDecimal(row["SATIS_FIAT1"]).ToString("N2");
                else txtFiyat.Text = "0,00";

            }
            catch (Exception ex)
            {

                ScriptManager.RegisterStartupScript(this, GetType(), "hata", "alert('Hata oluştu: " + ex.Message.Replace("'", "\\'") + "');", true);
            }

        }

        // --- EKLE / GÜNCELLE ------------------------------------------------
        protected void btnEkle_Click(object sender, EventArgs e)
        {
            string kod = (txtStokKodu.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(kod))
            {
                ClientScript.RegisterStartupScript(GetType(), "uyari", "alert('Stok kodu zorunlu.');", true);
                return;
            }

            decimal miktar = ParseDec(txtMiktar.Text);
            decimal fiyat = ParseDec(txtFiyat.Text);
            string stokAdi = (txtStokAdi.Text ?? "").Trim();

            // Eksikler varsa DB’den tamamla
            decimal kdvOran = 0m;
            if (string.IsNullOrWhiteSpace(stokAdi) || fiyat <= 0m)
            {
                var row = Db.ExecuteRow(
                    "SELECT STOK_ADI, SATIS_FIAT1, KDV_ORANI FROM tStokMaster WHERE STOK_KODU=@K",
                    new SqlParameter("@K", kod));

                if (row == null)
                {
                    ClientScript.RegisterStartupScript(GetType(), "yok", "alert('Stok bulunamadı.');", true);
                    return;
                }
                if (string.IsNullOrWhiteSpace(stokAdi))
                    stokAdi = Convert.ToString(row["STOK_ADI"]);
                if (fiyat <= 0m && row["SATIS_FIAT1"] != DBNull.Value)
                    fiyat = Convert.ToDecimal(row["SATIS_FIAT1"], tr);

                kdvOran = NormalizeKdv(row["KDV_ORANI"], tr);
            }
            else
            {
                // Kdv çekme
                var row = Db.ExecuteRow(
                    "SELECT KDV_ORANI FROM tStokMaster WHERE STOK_KODU=@K",
                    new SqlParameter("@K", kod));
                kdvOran = NormalizeKdv(row?["KDV_ORANI"], tr);
            }

            // hesaplamalar
            decimal net = Math.Round(miktar * fiyat, 2);
            decimal kdv = Math.Round(net * kdvOran, 2);
            decimal dahil = net + kdv;

            // satir ekleme
            var dt = Stoklar;
            var mevcut = dt.AsEnumerable()
                           .FirstOrDefault(r => string.Equals(r.Field<string>("StokKodu"),
                                                              kod, StringComparison.OrdinalIgnoreCase));
            if (mevcut != null)
            {
                mevcut["StokAdi"] = stokAdi;
                mevcut["Miktar"] = miktar;
                mevcut["Fiyat"] = fiyat;
                mevcut["Tutar"] = net;
                mevcut["KdvOran"] = kdvOran;
                mevcut["KdvTutar"] = kdv;
                mevcut["KdvDahilTutar"] = dahil;
               
            }
            else
            {
                dt.Rows.Add(kod, stokAdi, miktar, fiyat, net, kdvOran, kdv, dahil);
              
            }

            Stoklar = dt;
            BindGrid();
            RefreshTotalsPanel();
            TemizleDetay();


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
                    dt.Rows.Remove(dr);   // ÖNEMLİ: dr.Delete() değil
                }

                Stoklar = dt;
                BindGrid();
                RefreshTotalsPanel();     // RecalcTotals + CariBilgileri
            }
        }
        protected void RowMiktar_TextChanged(object sender, EventArgs e)
        {
            var txt = (TextBox)sender;
            var row = (GridViewRow)txt.NamingContainer;
            int rowIndex = row.RowIndex;
            if (rowIndex < 0) return;

            var dt = Stoklar;
            var kod = Convert.ToString(gvStoklar.DataKeys[rowIndex].Value);
            var dr = dt.AsEnumerable().First(r => r.Field<string>("StokKodu") == kod);

            decimal yeniMiktar = ParseDec(txt.Text);
            decimal fiyat = dr.Field<decimal>("Fiyat");
            decimal kdvOran = dr.Field<decimal?>("KdvOran") ?? 0m;

            decimal net = Math.Round(yeniMiktar * fiyat, 6);
            decimal kdv = Math.Round(net * kdvOran, 6);
            decimal dahil = net + kdv;

            dr["Miktar"] = yeniMiktar;
            dr["Tutar"] = net;
            dr["KdvTutar"] = kdv;
            dr["KdvDahilTutar"] = dahil;

            Stoklar = dt;
            BindGrid();
            RefreshTotalsPanel();
        }

        // --- TOPLAM HESAPLAMA
        private void RecalcTotals()
        {
            var dt = Stoklar;

            decimal brutNet = dt.AsEnumerable().Sum(r => r.Field<decimal?>("Tutar") ?? 0m);
            decimal toplamKdv = dt.AsEnumerable().Sum(r => r.Field<decimal?>("KdvTutar") ?? 0m);

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
                alert.AlertMsg("Cari bakiye bilgileri alınamadı", this);
                return;
            }

            lblCariBakiye.Text = info.Bakiye.ToString("N2", tr);
            lblRiskLimiti.Text = info.RiskLimiti.ToString("N2", tr);

            

            // Carinin kullanılabilir limiti (CariBakiye.Kullanilabilir)
            decimal kullanilabilir = Math.Max(0,info.RiskLimiti - info.Bakiye);

            // Şu anki siparişin genel toplamı
            decimal siparisGenelToplam = ParseDec(txtGenelToplam.Text);
            
           
            // Sipariş sonrası kalan limit
            decimal kalanLimit = kullanilabilir - siparisGenelToplam;

            lblKalanLimit.Text = kalanLimit.ToString("N2", tr);
            lblKalanLimit.ForeColor = kalanLimit < 0 ? Color.Red : Color.Black;
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

            // İlk 3 hane prefix
            string prefix = imps.Length >= 3 ? imps.Substring(0, 3) : imps.PadRight(3, '0');

            // 2) Bu seri ile başlayan en büyük FATIRS_NO'yu çek
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

        private void TemizleDetay()
        {
            txtStokAdi.Text = "";
            txtMiktar.Text = "";
            txtStokKodu.Text = "";
            txtFiyat.Text = "";
        }

    }
}
