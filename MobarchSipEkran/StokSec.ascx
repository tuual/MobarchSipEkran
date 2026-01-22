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
          <asp:UpdatePanel ID="upStok" runat="server" UpdateMode="Always">
              <ContentTemplate>
        <div class="row g-2 mb-2">
          <div class="col-md-6">
            <asp:TextBox ID="txtAra" OnTextChanged="txtAra_TextChanged" AutoPostBack="true" onkeydown="return (event.keyCode !== 13);" runat="server" CssClass="form-control" placeholder="Kod / Ad ile ara..." />
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
            <asp:BoundField DataField="SATIS_FIAT1"  HeaderText="Fiyat" />
            <asp:TemplateField HeaderText="Miktar" ControlStyle-Width="100px">
                <ItemTemplate>

                    <asp:TextBox ID="txtMiktar" runat="server" Text='<%# Eval("KayitliMiktar").ToString() == "0" ? "" : Eval("KayitliMiktar")%>' Width="100px" CssClass="form-control form-control-sm"  ></asp:TextBox>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField>
              <ItemTemplate>
                <asp:LinkButton ID="btnEkle" runat="server" CommandName="Ekle" Text="Ürünü Ekle" 
                    CommandArgument='<%# Container.DataItemIndex %>'
                    CssClass="btn btn-sm btn-primary">

                </asp:LinkButton>
                  
              </ItemTemplate>
          
            </asp:TemplateField>
              <asp:TemplateField>
                    <ItemTemplate>
        <asp:LinkButton ID="btnSil" runat="server" CommandName="Sil" Text="Kaldır" 
         CommandArgument='<%# Container.DataItemIndex %>'
         CssClass="btn btn-sm btn-danger"></asp:LinkButton>
  </ItemTemplate>
              </asp:TemplateField>
          </Columns>
        </asp:GridView>
                      </ContentTemplate>
          </asp:UpdatePanel>
      </div>

    </div>
  </div>
</div>
<script>
    
</script>