<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="mainLogin.aspx.cs" Inherits="MobarchSipEkran.mainLogin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        body {
            background-color: #EDF2F5;
        }

        .ubuntu-bold {
            font-family: "Ubuntu", sans-serif;
            font-weight: 700;
            font-style: normal;
            color: #403866;
        }

        .buton {
            background-color: #827ffe;
            color: #fff;
            border: none;
            border-radius: 6px;
            height: 50px;
            transition: background-color 0.2s ease-in-out;
        }

        .buton:hover {
            background-color: #6b6aff;
        }

        .minimal-input {
            background-color: #e9e9e9;
            border: none;
            border-radius: 6px;
            padding: 12px 14px;
            font-size: 15px;
            color: #333;
            font-weight: 500;
            transition: all 0.2s ease-in-out;
        }

        .minimal-input::placeholder {
            color: #8c8c8c;
            font-weight: 500;
        }

        .minimal-input:focus {
            background-color: #f2f2f2;
            outline: none;
            box-shadow: 0 0 0 2px #a7a5ff70;
        }

        .form-control.minimal-input {
            box-shadow: none !important;
        }

        /* Dikdörtgen beyaz card */
        .rectangle-card {
            background: #fff;
            border-radius: 12px;
            box-shadow: 0 2px 16px rgba(64, 56, 102, 0.08);
            padding: 40px 32px;
            max-width: 420px;
            min-width: 320px;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            text-align: center;
        }

        .rectangle-card input {
            max-width: 300px;
            width: 100%;
            margin-bottom: 10px;
        }

        p.text-muted {
            margin-bottom: 20px;
        }
    </style>

    <div class="container d-flex justify-content-center align-items-center" style="min-height: 100vh;">
        <div class="rectangle-card">
            <h1 class="ubuntu-bold mb-3" id="baslik">Giriş</h1>
            <p class="text-muted">Kullanıcı adınızı ve şifrenizi girip sisteme devam edin.</p>
            <asp:TextBox ID="txtVKN" runat="server" CssClass="form-control minimal-input" Placeholder="VKN/TC" MaxLength="11"></asp:TextBox>

<asp:TextBox ID="txtKadi" runat="server" CssClass="form-control minimal-input" Placeholder="Kullanıcı Adı"></asp:TextBox>
<asp:TextBox ID="txtParola" runat="server" CssClass="form-control minimal-input" TextMode="Password" Placeholder="Parola"></asp:TextBox>
<asp:Button ID="btnGiris" runat="server" CssClass="form-control buton ubuntu-bold mt-2" Text="Giriş Yap" OnClick="btnGiris_Click" />
        </div>
    </div>
</asp:Content>
