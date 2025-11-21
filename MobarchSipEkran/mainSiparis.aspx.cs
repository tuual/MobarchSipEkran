using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MobarchSipEkran
{
    public partial class mainSiparis : System.Web.UI.Page
    {
        private readonly CultureInfo tr = new CultureInfo("tr-TR");


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
            if (!IsPostBack)
            {
                txtTarih.Text = DateTime.Now.ToString("yyyy-MM-dd");
                BindGrid();
                RecalcTotals();
                txtGenelToplam.Enabled = false;
                txtFiyat.Enabled = false;
                txtStokAdi.Enabled = false;

                
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
            var row = Db.ExecuteRow("SELECT SATIS_FIAT1 FROM tStokMaster WHERE STOK_KODU=@K", new SqlParameter("@K", e.StokKodu)); 
            if (row != null && row["SATIS_FIAT1"] != DBNull.Value) txtFiyat.Text = Convert.ToDecimal(row["SATIS_FIAT1"]).ToString("N2"); 
            else txtFiyat.Text = "0,00";

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
            decimal net = Math.Round(miktar * fiyat, 1);
            decimal kdv = Math.Round(net * kdvOran, 1);
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
            RecalcTotals();

            
            Response.Redirect(Request.RawUrl, false);
            Context.ApplicationInstance.CompleteRequest();
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
                if (dr != null) dr.Delete();
                Stoklar = dt;
                BindGrid();
                RecalcTotals();
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

            decimal net = Math.Round(yeniMiktar * fiyat, 2);
            decimal kdv = Math.Round(net * kdvOran, 2);
            decimal dahil = net + kdv;

            dr["Miktar"] = yeniMiktar;
            dr["Tutar"] = net;
            dr["KdvTutar"] = kdv;
            dr["KdvDahilTutar"] = dahil;

            Stoklar = dt;
            BindGrid();
            RecalcTotals();
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
    }
}
