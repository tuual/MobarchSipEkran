<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="mainLogin.aspx.cs"
    Inherits="MobarchSipEkran.mainLogin" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>Giriş</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        body { background-color:#EDF2F5; }
        .ubuntu-bold{ font-family:"Ubuntu",sans-serif; font-weight:700; color:#403866; }
        .buton{ background:#827ffe; color:#fff; border:none; border-radius:6px; height:50px }
        .buton:hover{ background:#6b6aff }
        .minimal-input{ background:#e9e9e9; border:none; border-radius:6px; padding:12px 14px; font-size:15px }
        .minimal-input:focus{ background:#f2f2f2; outline:none; box-shadow:0 0 0 2px #a7a5ff70 }
        .rectangle-card{ background:#fff; border-radius:12px; box-shadow:0 2px 16px rgba(64,56,102,.08);
                         padding:40px 32px; max-width:420px; min-width:320px; text-align:center }
        .rectangle-card input{ max-width:300px; width:100%; margin-bottom:10px }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <asp:ScriptManager ID="sm" runat="server" />
    <div class="container d-flex justify-content-center align-items-center" style="min-height:100vh;">
        <div class="rectangle-card">
            <h1 class="ubuntu-bold mb-3">Giriş</h1>
            <p class="text-muted">Kullanıcı adınızı ve şifrenizi girin.</p>

            <asp:TextBox ID="txtVKN" runat="server"
                CssClass="form-control minimal-input"
                Placeholder="VKN/TC" MaxLength="11" />
            <asp:TextBox ID="txtKadi" runat="server"
                CssClass="form-control minimal-input"
                Placeholder="Kullanıcı Adı" />
            <asp:TextBox ID="txtParola" runat="server"
                CssClass="form-control minimal-input"
                TextMode="Password" Placeholder="Parola" />

            <asp:Button ID="btnGiris" runat="server"
                CssClass="form-control buton ubuntu-bold mt-2"
                Text="Giriş Yap"
                OnClick="btnGiris_Click" />
        </div>
    </div>
</form>
</body>
</html>
