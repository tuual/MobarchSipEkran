<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="mainSiparis.aspx.cs" Inherits="MobarchSipEkran.mainSiparis" %>

<%@ Register Src="~/StokSec.ascx" TagPrefix="uc" TagName="StokSec" %>
<asp:Content ID="mainSiparisContent" ContentPlaceHolderID="MainContent" runat="server">

    <asp:HiddenField ID="hfGenelToplam" runat="server" Value="0" />
    <asp:HiddenField ID="hfKullanilabilirLimit" runat="server" Value="0" />
    <asp:HiddenField ID="hfEditIndex" runat="server" />

    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />

    <uc:StokSec ID="stokSecModal" runat="server" OnStokSecildi="stokSecModal_StokSecildi" />

    <style>
        .shadow-soft {
            box-shadow: 0 8px 20px rgba(15, 23, 42, 0.08);
            border-radius: 12px;
            border: 1px solid rgba(148, 163, 184, 0.25);
        }

        .section-title {
            font-size: 1.3rem;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: .5rem;
        }

        .section-title span.badge-pill {
            border-radius: 999px;
            font-size: .75rem;
            padding: .25rem .75rem;
        }
        .grid-wrap {
   
            overflow-x: auto;
        }

        #gvStoklar {
            min-width: 1200px;
        }

        #gvStoklar th,
        #gvStoklar td {
            white-space: nowrap;
        }

        #gvStoklar thead th {
            position: sticky;
            top: 0;
            z-index: 2;
            background: #f8fafc;
        }

        #gvStoklar tr:hover td {
            background-color: #f9fafb;
        }

        .number {
            text-align: right;
        }

        .w-qty {
            max-width: 120px;
        }

        .card-header-small {
            font-size: .85rem;
            text-transform: uppercase;
            letter-spacing: .06em;
        }

        .label-xs {
            font-size: .75rem;
            margin-bottom: 2px;
        }

        .form-control-sm,
        .form-control {
            border-radius: .5rem;
        }

        .pill-info {
            border-radius: 999px;
            background-color: #eff6ff;
            padding: .35rem .9rem;
            font-size: .8rem;
        }

        .badge-soft {
            background-color: #eef2ff;
            color: #4f46e5;
        }
    </style>

  <section class="mt-3">

    <!-- Üst başlık -->
    <div class="d-flex justify-content-between align-items-center mb-3">
        <div>
            <div class="section-title">
                🧾 Müşteri Siparişi
                <span class="badge badge-pill bg-light text-secondary border">
                    Online sipariş giriş ekranı
                </span>
            </div>
            <small class="text-muted">
                Cari için stok seç, satırları doldur, sağ taraftan risk ve toplamları takip et.
            </small>
        </div>
        <div class="pill-info d-none d-md-inline-flex align-items-center gap-1">
            <i class="bi bi-calendar-week"></i>
            <span>Bugün:</span>
            <span><%= DateTime.Now.ToString("dd.MM.yyyy") %></span>
        </div>
    </div>

    <!-- 1. SATIR: SOL (GENEL+STOK) + SAĞ (HESAP+TOPLAM) -->
    <div class="row g-3 align-items-start">

        <!-- SOL BLOK: Genel Bilgiler + Stok Ekle -->
        <div class="col-lg-8">

            <!-- Genel Bilgiler -->
            <div class="card shadow-soft mb-3">
                <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
                    <span class="card-header-small">
                        GENEL BİLGİLER
                    </span>
                    <span class="badge bg-light text-primary">
                        Sipariş başlığı
                    </span>
                </div>
                <div class="card-body">
                    <div class="row g-3">

                        <div class="col-md-4">
                            <label class="form-label label-xs text-muted">Sipariş No</label>
                            <asp:TextBox ID="txtBelgeNo" runat="server" CssClass="form-control form-control-sm" />
                        </div>

                        <div class="col-md-4">
                            <label class="form-label label-xs text-muted">Tarih</label>
                            <asp:TextBox ID="txtTarih" runat="server" TextMode="Date" CssClass="form-control form-control-sm" />
                        </div>

                        <div class="col-md-4">
                            <label class="form-label label-xs text-muted">Açıklama</label>
                            <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control form-control-sm" />
                        </div>

                    </div>
                </div>
            </div>

            <!-- Stok Ekle -->
            <div class="card shadow-soft mb-3">
                <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
                    <span class="card-header-small">
                        STOK EKLE
                    </span>
                    <small class="text-white-50">Stok kodunu seç, miktar & fiyat gir, satıra ekle.</small>
                </div>
                <div class="card-body">
                    <div class="row g-3 align-items-end">

                        <div class="col-md-4">
                            <label class="form-label label-xs text-muted">Stok Kodu</label>
                            <div class="input-group input-group-sm">
                                <asp:TextBox ID="txtStokKodu" runat="server" CssClass="form-control"  EnableViewState="false"/>
                                <button type="button" class="btn btn-outline-light border-0" data-bs-toggle="modal" data-bs-target="#stokModal" title="Stok Seç">
                                    <i class="bi bi-search"></i>
                                </button>
                            </div>
                        </div>

                        <div class="col-md-4">
                            <label class="form-label label-xs text-muted">Stok Adı</label>
                            <asp:TextBox ID="txtStokAdi" runat="server" CssClass="form-control form-control-sm" EnableViewState="false" />
                        </div>

                        <div class="col-md-2">
                            <label class="form-label label-xs text-muted">Miktar</label>
                            <asp:TextBox ID="txtMiktar" runat="server" CssClass="form-control form-control-sm number" EnableViewState="false" />
                        </div>

                        <div class="col-md-2">
                            <label class="form-label label-xs text-muted">Fiyat</label>
                            <asp:TextBox ID="txtFiyat" runat="server" CssClass="form-control form-control-sm number" EnableViewState="false"     />
                        </div>

                        <div class="col-md-2">
                            <asp:Button ID="btnEkle" runat="server" Text="Ekle"
                                CssClass="btn btn-success btn-sm w-100 mt-2 mt-md-4"
                                UseSubmitBehavior="false" OnClick="btnEkle_Click" />
                        </div>

                    </div>
                </div>
            </div>

        </div>

        <!-- SAĞ BLOK: Hesap Bilgisi + Toplamlar -->
        <div class="col-lg-4">
            <asp:UpdatePanel ID="upTotals" runat="server" UpdateMode="Conditional">
                <ContentTemplate>

                    <!-- Hesap Bilgisi -->
                    <div class="card shadow-soft mb-3">
                        <div class="card-header bg-danger text-white d-flex justify-content-between align-items-center">
                            <span class="card-header-small">HESAP BİLGİSİ</span>
                            <span class="badge bg-light text-danger">Cari Durum</span>
                        </div>
                        <div class="card-body">

                            <div class="mb-2">
                                <label class="fw-bold small mb-1 text-muted">Bakiye</label>
                                <asp:Label ID="lblCariBakiye" runat="server"
                                    CssClass="form-control form-control-sm mb-2 bg-light text-end" />
                            </div>

                            <div class="mb-2">
                                <label class="fw-bold small mb-1 text-muted">Risk Limiti</label>
                                <asp:Label ID="lblRiskLimiti" runat="server"
                                    CssClass="form-control form-control-sm mb-2 bg-light text-end" />
                            </div>

                            <div class="mb-2">
                                <label class="fw-bold small mb-1 text-muted">Kullanılabilir Limit</label>
                                <asp:Label ID="lblKalanLimit" runat="server"
                                    CssClass="form-control form-control-sm bg-light text-end fw-bold" />
                            </div>

                        </div>
                    </div>

                    <!-- Toplamlar -->
                    <div class="card shadow-soft">
                        <div class="card-header bg-success text-white d-flex justify-content-between align-items-center">
                            <span class="card-header-small">TOPLAM HESAPLAR</span>
                            <small class="text-white-50">İskonto & KDV dahil</small>
                        </div>
                        <div class="card-body">

                            <label class="form-label label-xs text-muted">Brüt Tutar</label>
                            <asp:TextBox ID="txtBrutTutar" runat="server" ReadOnly="true"
                                CssClass="form-control form-control-sm text-end" />

                            <label class="form-label label-xs text-muted mt-2">İskonto Toplamı</label>
                            <asp:TextBox ID="txtIskonto" runat="server"
                                CssClass="form-control form-control-sm text-end"
                                AutoPostBack="true"
                                OnTextChanged="txtIskonto_TextChanged" />

                            <label class="form-label label-xs text-muted mt-2">Ara Toplam</label>
                            <asp:TextBox ID="txtAraToplam" runat="server" ReadOnly="true"
                                CssClass="form-control form-control-sm text-end" />

                            <label class="form-label label-xs text-muted mt-2">KDV Toplamı</label>
                            <asp:TextBox ID="txtKdvToplam" runat="server" ReadOnly="true"
                                CssClass="form-control form-control-sm text-end" />

                            <label class="form-label label-xs text-muted mt-2 fw-bold">Genel Toplam</label>
                            <asp:TextBox ID="txtGenelToplam" runat="server" ReadOnly="true"
                                CssClass="form-control form-control-sm text-end fw-bold border-2 border-success" />

                        </div>
                    </div>

                </ContentTemplate>

                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnEkle" EventName="Click" />
                    <asp:AsyncPostBackTrigger ControlID="gvStoklar" EventName="RowCommand" />
                    <asp:AsyncPostBackTrigger ControlID="txtIskonto" EventName="TextChanged" />
                </Triggers>
            </asp:UpdatePanel>
        </div>

    </div>

    <!-- 2. SATIR: FULL-WIDTH GRID (SATIR DETAYLARI) -->
    <div class="row mt-3">
        <div class="col-12">
            <asp:UpdatePanel ID="upGrid" runat="server" UpdateMode="Conditional">
                <ContentTemplate>

                    <div class="card shadow-soft mb-3">
                        <div class="card-header bg-light d-flex justify-content-between align-items-center">
                            <span class="card-header-small text-secondary">
                                SATIR DETAYLARI
                            </span>
                            <small class="text-muted">Aşağıdaki satırlar kaydedilecek sipariş satırlarıdır.</small>
                        </div>
                        <div class="card-body p-0">
                            <div class="grid-wrap">

                                <asp:GridView ID="gvStoklar" runat="server" AutoGenerateColumns="False"
                                    CssClass="table table-hover table-sm mb-0 align-middle"
                                    DataKeyNames="StokKodu" ClientIDMode="Static"
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

                                        <asp:TemplateField HeaderText="">
                                            <ItemTemplate>
                                                <asp:LinkButton ID="btnSil" runat="server" Text="Sil"
                                                    CommandName="Sil"
                                                    CommandArgument="<%# ((GridViewRow)Container).RowIndex %>"
                                                    CssClass="btn btn-outline-danger btn-sm" />
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                    </Columns>

                                </asp:GridView>

                            </div>
                        </div>
                    </div>

                </ContentTemplate>

                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnEkle" EventName="Click" />
                </Triggers>

            </asp:UpdatePanel>
        </div>
    </div>

    <!-- Alt Butonlar -->
    <div class="row mt-3">
        <div class="col-12 text-end">
            <asp:Button ID="btnKaydet" runat="server" Text="Kaydet"
                CssClass="btn btn-primary me-2" OnClick="btnKaydet_Click" />
            <asp:Button ID="btnVazgec" runat="server" Text="Vazgeç"
                CssClass="btn btn-outline-secondary" />
        </div>
    </div>

</section>



    <script>
        document.addEventListener('input', function (e) {
            if (e.target.classList.contains('number')) {
                e.target.value = e.target.value
                    .replace(/[^0-9.,-]/g, '')
                    .replace(',', '.');
            }
        });
    </script>

</asp:Content>
