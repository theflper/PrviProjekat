using PrviProjekat.DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrviProjekat
{
    public partial class FormUnosImena : Form
    {
        public string UnetoIme { get; private set; } = "";

        private TextBox txtIme;
        private ListBox lstPostojeciNivoi;
        private Button btnOk;
        private Button btnCancel;
        private Label lblInfo;
        // Prazan konstruktor koji prosleđuje podrazumevane vrednosti u glavni konstruktor
        public FormUnosImena() : this("")
        {
        }
        // Lista koja čuva sva imena učitana iz Redisa radi lakšeg filtriranja
        private List<string> sviNivoi = new List<string>();
        // Glavni konstruktor sa opcionalnim parametrima
        public FormUnosImena(string trenutniNaziv, bool prikaziListuNivoa = true, bool jeOtvaranje = false)
        {
            InitializeComponent();
            Text = jeOtvaranje ? "Učitaj nivo iz Redisa" : "Sačuvaj nivo u Redis";
            Width = 380;
            Height = 340;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            var lbl = new Label { Text = "Naziv nivoa:", Left = 20, Top = 15, Width = 320 };
            Controls.Add(lbl);
            txtIme = new TextBox { Text = trenutniNaziv, Left = 20, Top = 35, Width = 325, Font = new Font("Segoe UI", 10F) };
            // Događaj koji osvežava/filtrira listu čim korisnik krene da kuca u TextBox
            txtIme.TextChanged += TxtIme_TextChanged;
            Controls.Add(txtIme);
            if (prikaziListuNivoa)
            {
                lblInfo = new Label { Text = "Postojeći nivoi u bazi:", Left = 20, Top = 70, Width = 320 };
                Controls.Add(lblInfo);
                lstPostojeciNivoi = new ListBox { Left = 20, Top = 90, Width = 325, Height = 130, Font = new Font("Segoe UI", 9.5F) };
                lstPostojeciNivoi.SelectedIndexChanged += (s, e) =>
                {
                    if (lstPostojeciNivoi.SelectedItem != null)
                    {
                        // Privremeno skidamo TextChanged event da nam ne poremeti selekciju
                        txtIme.TextChanged -= TxtIme_TextChanged;
                        txtIme.Text = lstPostojeciNivoi.SelectedItem.ToString();
                        txtIme.TextChanged += TxtIme_TextChanged;
                    }
                };
                UcitajPostojeceNivoeIzRedisa();
                Controls.Add(lstPostojeciNivoi);
            }
            string tekstDugmeta = jeOtvaranje ? "Otvori" : "Sačuvaj";
            btnOk = new Button { Text = tekstDugmeta, Left = 95, Top = 240, DialogResult = DialogResult.OK, Width = 120, Height = 38, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            btnCancel = new Button { Text = "Otkaži", Left = 225, Top = 240, DialogResult = DialogResult.Cancel, Width = 120, Height = 38, Font = new Font("Segoe UI", 9.5F) };
            btnOk.Click += (s, e) => { UnetoIme = txtIme.Text.Trim(); };
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
        private void UcitajPostojeceNivoeIzRedisa()
        {
            try
            {
                var db = RedisManager.GetDatabase();
                if (db == null || RedisManager.Connection == null) return;
                var endpoints = RedisManager.Connection.GetEndPoints();
                if (endpoints.Length == 0) return;
                var server = RedisManager.Connection.GetServer(endpoints[0]);
                var kljucevi = server.Keys(pattern: "nivo:*");
                // Obavezno čistimo obe liste pre učitavanja
                sviNivoi.Clear();
                if (lstPostojeciNivoi != null)
                {
                    lstPostojeciNivoi.Items.Clear();
                }
                foreach (var kljuc in kljucevi)
                {
                    string naziv = kljuc.ToString().Replace("nivo:", "");
                    sviNivoi.Add(naziv); // <-- OVO JE NEDOSTAJALO! Punimo glavnu memorijsku listu
                }
                // Pozivamo osvežavanje da prikaže početno stanje
                OsveziPrikazListe("");
            }
            catch (Exception)
            {
                // Ako server nije dostupan, lista ostaje prazna
            }
        }
        // Metoda koja filtrira listu na osnovu unetog teksta (ignoriše velika/mala slova)
        private void TxtIme_TextChanged(object? sender, EventArgs e)
        {
            OsveziPrikazListe(txtIme.Text.Trim());
        }

        private void OsveziPrikazListe(string filter)
        {
            if (lstPostojeciNivoi == null) return;

            lstPostojeciNivoi.Items.Clear();

            var filtriraniNivoi = string.IsNullOrEmpty(filter)
                ? sviNivoi
                : sviNivoi.Where(n => n.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var nivo in filtriraniNivoi)
            {
                lstPostojeciNivoi.Items.Add(nivo);
            }
        }
        private void FormUnosImena_Load(object sender, EventArgs e)
        {

        }
    }
}
