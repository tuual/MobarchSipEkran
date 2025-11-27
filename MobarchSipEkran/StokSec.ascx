<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StokSec.ascx.cs" Inherits="MobarchSipEkran.StokSec" %>

<!-- Sadece modal içerik, UpdatePanel yok -->
<div class="modal fade" id="stokModal" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-xl modal-dialog-scrollable">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">Stok Seç</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
      </div>

      <div class="modal-body">
        <div class="row g-2 mb-2">
          <div class="col-md-6">
            <asp:TextBox ID="txtAra" runat="server" CssClass="form-control" placeholder="Kod / Ad ile ara..." />
          </div>
          <div class="col-md-3">
            <asp:Button ID="btnAra" runat="server" CssClass="btn btn-primary w-100" Text="Ara" OnClick="btnAra_Click" />
          </div>
        </div>

        <asp:GridView ID="gv" runat="server"
            AutoGenerateColumns="False"
            DataKeyNames="STOK_KODU,STOK_ADI"
            OnRowCommand="gv_RowCommand"
            OnRowDataBound="gv_RowDataBound"
            CssClass="table table-sm table-striped">

          <Columns>
            <asp:BoundField DataField="STOK_KODU" HeaderText="Kod" />
            <asp:BoundField DataField="STOK_ADI"  HeaderText="Ad" />
            <asp:TemplateField HeaderText="Seç">
              <ItemTemplate>
                <asp:LinkButton ID="btnSec" runat="server"
                    Text="Seç"
                    CommandName="Sec"
                    CausesValidation="false"
                    CssClass="btn btn-primary btn-sm" />
              </ItemTemplate>
            </asp:TemplateField>
          </Columns>
        </asp:GridView>
      </div>

      <div class="modal-footer">
        <button class="btn btn-secondary" data-bs-dismiss="modal">Kapat</button>
      </div>
    </div>
  </div>
</div>
