using PrviProjekat.DataModel;

namespace PrviProjekat
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        PodaciGrida data = new PodaciGrida();
        Teren trenutniTeren;
        private string trenutniNazivNivoa = "";
        private async void SnimiNivoURedis(string naziv)
        {
            var db = RedisManager.GetDatabase();
            if (db == null) return;
            string kljuc = $"nivo:{naziv}";
            if (db.KeyExists(kljuc))
            {
                var rezultat = MessageBox.Show(
                    $"Nivo pod nazivom '{naziv}' veæ postoji! Da li želite da ga prepišete?",
                    "Upozorenje - Postojeæi nivo",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (rezultat == DialogResult.No) return;
            }
            // 1. Sinhronizujemo bit memoriju iz terena
            data.GridBitMemorija = trenutniTeren.DajSvuMemoriju();
            // 2. OBAVEZNO DETEKTUJEMO OSTRVA
            trenutniTeren.DetektujOstrva();
            data.Ostrva = trenutniTeren.Ostrva;
            // 3. OBAVEZNO NALAZIMO VEZE (MOSTOVE) IZMEÐU OSTRVA
            data.Veze = trenutniTeren.PronadjiVezeMostova();
            // Sada data.Veze više nisu prazne ukoliko su mostovi spojili dva ostrva!
            // 4. Èuvamo u Redis (JSON sada sadrži i veze)
            string jsonNivoa = JsonSerializer.Serialize(data);
            db.StringSet(kljuc, jsonNivoa);
            // 5. Èuvamo u Neo4j
            await Neo4jManager.SnimiGrafZaNivoAsync(naziv, data.Ostrva, data.Veze);
            // 6. Odmah osvežavamo prikaz na ekranu da se brojevi pojave èim se klikne saèuvaj!
            pictureBoxTeren.Invalidate();
            MessageBox.Show($"Nivo '{naziv}' je uspešno saèuvan (sa {data.Ostrva.Count} ostrva i {data.Veze.Count} veza)!", "Uspeh", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SacuvajStanjeZaUndo()
        {
            var db = RedisManager.GetDatabase();
            if (db == null) return;
            // Serijalizujemo trenutni objekat data (PodaciGrida) u JSON
            string jsonStanje = JsonSerializer.Serialize(data);
            // Ubacujemo na poèetak liste u Redisu (LPUSH)
            db.ListLeftPush("undo:stack", jsonStanje);
            // Ogranièavamo listu na taèno 15 elemenata (indeksi od 0 do 14)
            db.ListTrim("undo:stack", 0, 14);
            // Kada korisnik napravi novu akciju, Redo lanac se prekida i briše
            db.KeyDelete("redo:stack");
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            labelN.Text = "N = " + data.Visina;
            labelM.Text = "M = " + data.Sirina;
            RedisManager.Inicijalizuj();
            Neo4jManager.Inicijalizuj("neo4j://127.0.0.1:7687", "neo4j", "graf1234");
            trenutniTeren = new Teren(pictureBoxTeren.Width, pictureBoxTeren.Height);
            label2.Text= "Pritiskom na dugme proveravamo \nda li postoji hamiltonov put u grafu\n" +
                "tj. da li je moguæe obiæi sva ostrva\ntaèno jednom bez ponavljanja.\n";
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(trenutniNazivNivoa))
            {
                saveAsToolStripMenuItem_Click(sender, e);
                return;
            }
            SnimiNivoURedis(trenutniNazivNivoa);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dijalog = new FormUnosImena("", true, true))
            {
                dijalog.Text = "Open - Unesite naziv nivoa za uèitavanje";
                if (dijalog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dijalog.UnetoIme))
                {
                    string trazeniNaziv = dijalog.UnetoIme;
                    var db = RedisManager.GetDatabase();
                    if (db == null) return;
                    string jsonNivoa = db.StringGet($"nivo:{trazeniNaziv}");
                    if (string.IsNullOrEmpty(jsonNivoa))
                    {
                        MessageBox.Show($"Nivo pod nazivom '{trazeniNaziv}' nije pronaðen u Redis bazi!", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    // Deserijalizujemo podatke
                    data = JsonSerializer.Deserialize<PodaciGrida>(jsonNivoa) ?? new PodaciGrida();
                    trenutniNazivNivoa = trazeniNaziv;
                    // Rekonstruišemo teren na formi
                    trenutniTeren = new Teren(data.Sirina, data.Visina);
                    trenutniTeren.UcitajMemoriju(data.GridBitMemorija, data.Ostrva);
                    // Ažuriramo kontrole
                    trackBarM.Value = data.Sirina;
                    trackBarN.Value = data.Visina;
                    labelM.Text = "M = " + data.Sirina;
                    labelN.Text = "N = " + data.Visina;
                    pictureBoxTeren.Invalidate();
                    MessageBox.Show($"Nivo '{trazeniNaziv}' je uspešno uèitan iz Redisa!", "Uspeh", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var db = RedisManager.GetDatabase();
            if (db == null) return;
            // 1. Vadimo poslednje stanje iz undo steka (LPOP)
            string prethodnoStanjeJson = db.ListLeftPop("undo:stack");
            if (string.IsNullOrEmpty(prethodnoStanjeJson))
            {
                MessageBox.Show("Nema više koraka za Undo!", "Obaveštenje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // 2. Trenutno stanje prebacujemo u redo stek pre nego što ga prepišemo
            string trenutnoStanjeJson = JsonSerializer.Serialize(data);
            db.ListLeftPush("redo:stack", trenutnoStanjeJson);
            db.ListTrim("redo:stack", 0, 14);
            // 3. Deserijalizujemo prethodno stanje u naš data objekat
            data = JsonSerializer.Deserialize<PodaciGrida>(prethodnoStanjeJson) ?? new PodaciGrida();
            // 4. Rekonstruišemo teren na osnovu uèitanih podataka
            trenutniTeren = new Teren(data.Sirina, data.Visina);
            trenutniTeren.UcitajMemoriju(data.GridBitMemorija, data.Ostrva);
            // 5. Ažuriramo interfejs (trackbar-ove i labele)
            trackBarM.Value = data.Sirina;
            trackBarN.Value = data.Visina;
            labelM.Text = "M = " + data.Sirina;
            labelN.Text = "N = " + data.Visina;

            pictureBoxTeren.Invalidate();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var db = RedisManager.GetDatabase();
            if (db == null) return;
            // 1. Vadimo stanje iz redo steka
            string sledeceStanjeJson = db.ListLeftPop("redo:stack");
            if (string.IsNullOrEmpty(sledeceStanjeJson))
            {
                MessageBox.Show("Nema više koraka za Redo!", "Obaveštenje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // 2. Trenutno stanje vraæamo nazad u undo stek
            string trenutnoStanjeJson = JsonSerializer.Serialize(data);
            db.ListLeftPush("undo:stack", trenutnoStanjeJson);
            db.ListTrim("undo:stack", 0, 14);
            // 3. Deserijalizujemo sledeæe stanje
            data = JsonSerializer.Deserialize<PodaciGrida>(sledeceStanjeJson) ?? new PodaciGrida();
            // 4. Rekonstruišemo teren
            trenutniTeren = new Teren(data.Sirina, data.Visina);
            trenutniTeren.UcitajMemoriju(data.GridBitMemorija, data.Ostrva);
            // 5. Ažuriramo labele i prikaz
            trackBarM.Value = data.Sirina;
            trackBarN.Value = data.Visina;
            labelM.Text = "M = " + data.Sirina;
            labelN.Text = "N = " + data.Visina;
            pictureBoxTeren.Invalidate();
        }

        private void trackBarN_Scroll(object sender, EventArgs e)
        {
            data.Visina = trackBarN.Value;
            labelN.Text = "N = " + data.Visina;
        }

        private void trackBarM_Scroll(object sender, EventArgs e)
        {
            data.Sirina = trackBarM.Value;
            labelM.Text = "M = " + data.Sirina;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dijalog = new FormUnosImena(trenutniNazivNivoa))
            {
                dijalog.Text = "Save As - Unesite naziv nivoa";
                if (dijalog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dijalog.UnetoIme))
                {
                    trenutniNazivNivoa = dijalog.UnetoIme;
                    SnimiNivoURedis(trenutniNazivNivoa);
                }
            }
        }

        private void pictureBoxTeren_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int sirinaPB = pictureBoxTeren.Width;
            int visinaPB = pictureBoxTeren.Height;
            if (data.Visina <= 0 || data.Sirina <= 0) return;
            int celijaSirina = (int)Math.Floor((double)sirinaPB / data.Sirina);
            int celijaVisina = (int)Math.Floor((double)visinaPB / data.Visina);
            // 1. Crtanje grid æelija (kopno, voda, mostovi)
            for (int y = 0; y < data.Visina; y++)
            {
                for (int x = 0; x < data.Sirina; x++)
                {
                    int xPiksel = x * celijaSirina;
                    int yPiksel = y * celijaVisina;
                    Rectangle rect = new Rectangle(xPiksel, yPiksel, celijaSirina, celijaVisina);

                    byte stanje = trenutniTeren.VratiStanjeCelije(x, y);
                    if (stanje == 0)
                    {
                        g.FillRectangle(Brushes.SeaGreen, rect);
                    }
                    else if (stanje == 2)
                    {
                        g.FillRectangle(Brushes.LightSkyBlue, rect);
                    }
                    else if (stanje == 1)
                    {
                        g.FillRectangle(Brushes.Gray, rect);
                        g.DrawLine(Pens.Black, rect.Left, rect.Top, rect.Right, rect.Bottom);
                        g.DrawLine(Pens.Black, rect.Right, rect.Top, rect.Left, rect.Bottom);
                        g.DrawRectangle(Pens.Black, rect);
                    }
                }
            }
            // 2. Crtanje brojeva (ID-jeva) preko detektovanih ostrva
            if (data.Ostrva != null)
            {
                using Font fontOstrva = new Font("Segoe UI", 10, FontStyle.Bold);
                using StringFormat format = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                foreach (var ostrvo in data.Ostrva)
                {
                    // Izraèunavamo centar bounding box-a ostrva u pikselima
                    int centarX = (ostrvo.MinX + ostrvo.MaxX + 1) * celijaSirina / 2;
                    int centarY = (ostrvo.MinY + ostrvo.MaxY + 1) * celijaVisina / 2;
                    // Malo pomeramo pravougaonik za ispis teksta
                    Rectangle rectTekst = new Rectangle(centarX - 15, centarY - 15, 30, 30);
                    // Crtamo belu pozadinu kružiæa/kvadratiæa da se broj lepo vidi preko zelene boje
                    g.FillEllipse(Brushes.White, rectTekst);
                    g.DrawEllipse(Pens.Black, rectTekst);
                    // Ispisujemo ID ostrva
                    g.DrawString(ostrvo.Id.ToString(), fontOstrva, Brushes.Black, rectTekst, format);
                }
            }
        }

        private void pictureBoxTeren_MouseClick(object sender, MouseEventArgs e)
        {
            // 1. Opet raèunamo dimenzije æelije na osnovu trenutne velièine PictureBox-a i TrackBar-ova
            int sirinaPB = pictureBoxTeren.Width;
            int visinaPB = pictureBoxTeren.Height;
            if (data.Sirina <= 0 || data.Visina <= 0) return;
            int celijaSirina = sirinaPB / data.Sirina;
            int celijaVisina = visinaPB / data.Visina;
            //Izraèunavamo na koju (x, y) æeliju u gridu je korisnik kliknuo na osnovu piksela miša (e.X, e.Y)
            int kliknutX = e.X / celijaSirina;
            int kliknutY = e.Y / celijaVisina;
            if (kliknutX >= 0 && kliknutX < data.Sirina && kliknutY >= 0 && kliknutY < data.Visina)
            {
                // PRVO èuvamo staro stanje pre nego što ga izmenimo!
                SacuvajStanjeZaUndo();

                if (e.Button == MouseButtons.Left)
                {
                    trenutniTeren.PostaviStanjeCelije(kliknutX, kliknutY, 2); // Voda
                }
                else if (e.Button == MouseButtons.Right)
                {
                    trenutniTeren.PostaviStanjeCelije(kliknutX, kliknutY, 1); // Most
                }
                // Sinhronizujemo bit memoriju nazad u data objekat
                data.GridBitMemorija = trenutniTeren.DajSvuMemoriju();
                pictureBoxTeren.Invalidate();
            }
        }

        private void TerenBtn_Click(object sender, EventArgs e)
        {
            trenutniTeren = new Teren(data.Sirina, data.Visina);
            trenutniNazivNivoa = "";
            pictureBoxTeren.Invalidate();
        }

        private async void btnPut_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(trenutniNazivNivoa))
            {
                MessageBox.Show("Molimo vas saèuvajte ili uèitajte nivo pre analize!", "Obaveštenje", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Pozivamo Neo4j analizu
            List<int> putanja = await Neo4jManager.PronadjiHamiltonovPutAsync(trenutniNazivNivoa);
            if (putanja.Count > 0)
            {
                string redosled = string.Join(" -> ", putanja);
                MessageBox.Show($"Uspeh! Pronaðen je Hamiltonov put.\n\nRedosled obilaska ostrva:\n{redosled}",
                    "Topološka analiza", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Nije moguæe obiæi sva ostrva taèno jednom (Hamiltonov put ne postoji za trenutne veze).",
                    "Topološka analiza", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
