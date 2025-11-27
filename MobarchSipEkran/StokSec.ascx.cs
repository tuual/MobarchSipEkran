using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MobarchSipEkran
{
    public partial class StokSec : UserControl
    {
        public class StokSecEventArgs : EventArgs
        {
            public string StokKodu { get; set; }
            public string StokAdi { get; set; }
        }

        public event EventHandler<StokSecEventArgs> StokSecildi;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            // Her postback'te DataKeys’in sağlam kalması için arama kriterini koruyarak bind edeceğiz.
            // İstersen bu satırı kaldırıp Page_Load’ta çağırabilirsin; önemli olan postback’te DataKeys’in hazır olması.
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) Bind("");
        }

        private void Bind(string term)
        {
            string sql = @"
                SELECT TOP 50 STOK_KODU, STOK_ADI
                FROM tStokMaster
                WHERE (@T='' OR STOK_KODU LIKE @KOD OR STOK_ADI LIKE @AD)
                ORDER BY STOK_KODU";
            var dt = Db.ExecuteDataTable(sql,
                new SqlParameter("@T", term ?? string.Empty),
                new SqlParameter("@KOD", (term ?? string.Empty) + "%"),
                new SqlParameter("@AD", "%" + (term ?? string.Empty) + "%"));

            gv.DataSource = dt;
            gv.DataBind();
        }

        protected void btnAra_Click(object sender, EventArgs e)
        {
            Bind(txtAra.Text?.Trim());
        }

        protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var btn = (LinkButton)e.Row.FindControl("btnSec");
                if (btn != null)
                {
                    // FULL POSTBACK: UpdatePanel yerine klasik postback
                    ScriptManager.GetCurrent(Page).RegisterPostBackControl(btn);
                }
            }
        }

        protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Sec") return;

            try
            {
                var row = (GridViewRow)((Control)e.CommandSource).NamingContainer;
                int idx = row.RowIndex;

                if (gv.DataKeys == null || gv.DataKeys.Count <= idx)
                    throw new InvalidOperationException("Seçimde DataKeys bulunamadı. ViewState/Bind kontrol edin.");

                string kod = Convert.ToString(gv.DataKeys[idx].Values["STOK_KODU"]);
                string ad = Convert.ToString(gv.DataKeys[idx].Values["STOK_ADI"]);

                if (string.IsNullOrEmpty(kod))
                    throw new InvalidOperationException("Geçersiz satır: STOK_KODU boş.");

                StokSecildi?.Invoke(this, new StokSecEventArgs { StokKodu = kod, StokAdi = ad });
            }
            catch (Exception ex)
            {
                // 500 yerine kullanıcıya göster; PRM çakılmasın.
                ScriptManager.RegisterStartupScript(this, GetType(), "secErr",
                    "alert('Seç işleminde hata: " + HttpUtility.JavaScriptStringEncode(ex.Message) + "');", true);
            }
        }
    }
}
