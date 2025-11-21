<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="mainSiparis.aspx.cs" Inherits="MobarchSipEkran.mainSiparis" %>
<%@ Register Src="~/StokSec.ascx" TagPrefix="uc" TagName="StokSec" %>

<asp:Content ID="mainLogin" ContentPlaceHolderID="MainContent" runat="server">

    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />

    <uc:StokSec ID="stokSecModal" runat="server" OnStokSecildi="stokSecModal_StokSecildi" />
    <asp:HiddenField ID="hfEditIndex" runat="server" />

    <style>
        .number { text-align: right }
        .card { border-radius: 8px }
        .w-qty { max-width: 120px }

        .grid-wrap {
            max-height: 42vh;
            overflow: auto;
            border: 1px solid #dee2e6;
            border-radius: .5rem;
        }

        #gvStoklar { min-width: 1200px; }

        #gvStoklar th,
        #gvStoklar td {
            white-space: nowrap;
        }

        #gvStoklar thead th {
            position: sticky;
            top: 0;
            z-index: 2;
            background: #fff;
        }

        .number { text-align: right; }
        .w-qty { max-width: 120px; }
    </style>

    <section class="mt-3">
        <h4 class="mb-3">🧾 Müşteri Siparişi</h4>

        <div class="row">

            <!-- SOL -->
            <div class="col-lg-12   ">

                <!-- Genel Bilgiler -->
                <div class="card shadow-sm mb-3">
                    <div class="card-header bg-primary text-white">Genel Bilgiler</div>
                    <div class="card-body">
                        <div class="row g-3">

                            <div class="col-md-4">
                                <label class="form-label">Belge No</label>
                                <asp:TextBox ID="txtBelgeNo" runat="server" CssClass="form-control" />
                            </div>

                            <div class="col-md-4">
                                <label class="form-label">Tarih</label>
                                <asp:TextBox ID="txtTarih" runat="server" TextMode="Date" CssClass="form-control" />
                            </div>

                            <div class="col-md-4">
                                <label class="form-label">Teslim Tarihi</label>
                                <asp:TextBox ID="txtTeslimTarihi" runat="server" TextMode="Date" CssClass="form-control" />
                            </div>

                            <div class="col-12">
                                <label class="form-label">Açıklama</label>
                                <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control" />
                            </div>

                        </div>
                    </div>
                </div>

                <!-- Stok Ekle -->
                <div class="card shadow-sm mb-3">
                    <div class="card-header bg-primary text-white">Stok Ekle</div>
                    <div class="card-body">
                        <div class="row g-10 align-items-end">

                            <div class="col-md-3">
                                <label class="form-label">Stok Kodu</label>
                                <div class="input-group">
                                    <asp:TextBox ID="txtStokKodu" runat="server" CssClass="form-control" />
                                    <button type="button" class="btn btn-outline-primary" data-bs-toggle="modal" data-bs-target="#stokModal">
                                        <i class="bi bi-search"></i>
                                    </button>
                                </div>
                            </div>

                            <div class="col-md-3">
                                <label class="form-label">Stok Adı</label>
                                <asp:TextBox ID="txtStokAdi" runat="server" CssClass="form-control" />
                            </div>

                            <div class="col-md-2">
                                <label class="form-label">Miktar</label>
                                <asp:TextBox ID="txtMiktar" runat="server" CssClass="form-control number" />
                            </div>

                            <div class="col-md-2">
                                <label class="form-label">Fiyat</label>
                                <asp:TextBox ID="txtFiyat" runat="server" CssClass="form-control number" />
                            </div>

                            <div class="col-md-2">
                                <asp:Button ID="btnEkle" runat="server" Text="Ekle" CssClass="btn btn-success w-100"
                                    UseSubmitBehavior="false" OnClick="btnEkle_Click" />
                            </div>

                        </div>
                    </div>
                </div>

                <!-- Grid -->
                <asp:UpdatePanel ID="upGrid" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>

                        <div class="grid-wrap">

                            <asp:GridView ID="gvStoklar" runat="server" AutoGenerateColumns="False"
                                CssClass="table table-bordered table-striped table-sm mb-0"
                                DataKeyNames="StokKodu,StokAdi" ClientIDMode="Static"
                                RowStyle-Wrap="false"
                                OnRowCommand="gvStoklar_RowCommand"
                                OnPreRender="gvStoklar_PreRender">

                                <Columns>

                                    <asp:BoundField DataField="StokKodu" HeaderText="Stok Kodu" />
                                    <asp:BoundField DataField="StokAdi" HeaderText="Stok Adı" />

                                    <asp:TemplateField HeaderText="Miktar">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtGridMiktar" runat="server"
                                                Text='<%# Eval("Miktar","{0:0.##}") %>'
                                                CssClass="form-control form-control-sm number w-qty"
                                                AutoPostBack="true"
                                                OnTextChanged="RowMiktar_TextChanged" />
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:BoundField DataField="Fiyat" HeaderText="Birim Fiyat"
                                        DataFormatString="{0:N2}" ItemStyle-CssClass="number" />

                                    <asp:BoundField DataField="Tutar" HeaderText="Net Tutar"
                                        DataFormatString="{0:N2}" ItemStyle-CssClass="number" />

                                    <asp:BoundField DataField="KdvOran" HeaderText="KDV %"
                                        DataFormatString="{0:P0}" HtmlEncode="false" ItemStyle-CssClass="number" />

                                    <asp:BoundField DataField="KdvTutar" HeaderText="KDV Tutar"
                                        DataFormatString="{0:N2}" ItemStyle-CssClass="number" />

                                    <asp:BoundField DataField="KdvDahilTutar" HeaderText="KDV Dâhil"
                                        DataFormatString="{0:N2}" ItemStyle-CssClass="number" />

                                    <asp:TemplateField HeaderText="Sil">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="btnSil" runat="server" Text="Sil"
                                                CommandName="Sil"
                                                CommandArgument="<%# ((GridViewRow)Container).RowIndex %>"
                                                CssClass="btn btn-danger btn-sm" />
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                </Columns>

                            </asp:GridView>

                        </div>

                    </ContentTemplate>

                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="btnEkle" EventName="Click" />
                    </Triggers>

                </asp:UpdatePanel>

            </div>

            <!-- SAĞ -->


                    <!-- Hesap Bilgisi -->
                    <div class="card shadow-sm mb-3">
                        <div class="card-header bg-danger text-white">Hesap Bilgisi</div>
                        <div class="card-body">

                            <label class="fw-bold">Bakiye</label>
                            <asp:Label ID="lblCariBakiye" runat="server"
                                CssClass="form-control mb-2 bg-light text-end" />

                            <label class="fw-bold">Risk Limiti</label>
                            <asp:Label ID="lblRiskLimiti" runat="server"
                                CssClass="form-control mb-2 bg-light text-end" />

                            <label class="fw-bold">Kullanılabilir Limit</label>
                            <asp:Label ID="lblKullanilabilir" runat="server"
                                CssClass="form-control bg-light text-end" />

                        </div>


                    <!-- Toplamlar -->
                   

                </div>

            </div>

            <!-- Toplam Kart -->
            <div class="col-md-3 mt-5 position-relative float-end">

                <asp:UpdatePanel ID="upTotals" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>

                        <div class="card shadow-sm mb-3">
                            <div class="card-header bg-success text-white">Toplamlar</div>
                            <div class="card-body">

                                <label>Brüt Tutar</label>
                                <asp:TextBox ID="txtBrutTutar" runat="server" ReadOnly="true"
                                    CssClass="form-control text-end" />

                                <label class="mt-2">İskonto Toplamı</label>
                                <asp:TextBox ID="txtIskonto" runat="server"
                                    CssClass="form-control text-end"
                                    AutoPostBack="true"
                                    OnTextChanged="RowMiktar_TextChanged" />

                                <label class="mt-2">Ara Toplam</label>
                                <asp:TextBox ID="txtAraToplam" runat="server" ReadOnly="true"
                                    CssClass="form-control text-end" />

                                <label class="mt-2">KDV Toplamı</label>
                                <asp:TextBox ID="txtKdvToplam" runat="server" ReadOnly="true"
                                    CssClass="form-control text-end" />

                                <label class="mt-2 fw-bold">Genel Toplam</label>
                                <asp:TextBox ID="txtGenelToplam" runat="server" ReadOnly="true"
                                    CssClass="form-control text-end fw-bold" />

                            </div>
                        </div>

                    </ContentTemplate>
                </asp:UpdatePanel>
                 <div class="text-end">
     <asp:Button ID="btnKaydet" runat="server" Text="Kaydet" CssClass="btn btn-primary"
         OnClick="btnKaydet_Click" />

     <asp:Button ID="btnVazgec" runat="server" Text="Vazgeç"
         CssClass="btn btn-secondary ms-2" />
 </div>

        

    </section>

    <script>
        // sadece sayı ve virgül/nokta
        document.addEventListener('input', function (e) {
            if (e.target.classList.contains('number')) {
                e.target.value = e.target.value.replace(/[^0-9.,-]/g, '').replace(',', '.');
            }
        });
    </script>

</asp:Content>
