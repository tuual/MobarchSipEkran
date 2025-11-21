using System;
using System.Data;
using System.Data.SqlClient;
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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) Bind("");
        }

        void Bind(string term)
        {
            string sql = @"
                SELECT  STOK_KODU, STOK_ADI, SATIS_FIAT1
                FROM tStokMaster
                WHERE (@TERM = '' OR STOK_KODU LIKE @KOD OR STOK_ADI LIKE @ADI)
                ORDER BY STOK_KODU";

            var dt = Db.ExecuteDataTable(sql,
                new SqlParameter("@TERM", term ?? ""),
                new SqlParameter("@KOD", (term ?? "") + "%"),
                new SqlParameter("@ADI", "%" + (term ?? "") + "%"));

            gv.DataSource = dt;
            gv.DataBind();
        }

        protected void btnAra_Click(object sender, EventArgs e) => Bind(txtAra.Text.Trim());

        protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Sec")
            {
                return;
            }
            else
            {
                var row = (GridViewRow)((Control)e.CommandSource).NamingContainer;
                int rowIndex = row.RowIndex;

                string stokKodu = gv.DataKeys[rowIndex].Values["STOK_KODU"].ToString();
                string stokAdi = gv.DataKeys[rowIndex].Values["STOK_ADI"].ToString();

                StokSecildi?.Invoke(this, new StokSecEventArgs { StokKodu = stokKodu, StokAdi = stokAdi });
            }

        }

        protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var btn = (LinkButton)e.Row.FindControl("btnSec");
                ScriptManager.GetCurrent(Page).RegisterPostBackControl(btn);
            }

        }
    }
}
