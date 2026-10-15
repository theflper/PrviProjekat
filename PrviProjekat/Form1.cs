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
            trenutniTeren = new Teren(pictureBoxTeren.Width, pictureBoxTeren.Height);
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

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

        }

        private void pictureBoxTeren_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // 1. Dimenzije PictureBox-a
            int sirinaPB = pictureBoxTeren.Width;
            int visinaPB = pictureBoxTeren.Height;

            // Zaštita od deljenja sa nulom (ako vrednosti nekako budu 0)
            if (data.Visina <= 0 || data.Visina <= 0) return;

            // 3. Izraèunavanje velièine jedne æelije u pikselima
            int celijaSirina = (int)Math.Floor((double)sirinaPB / data.Sirina);
            int celijaVisina = (int)Math.Floor((double)visinaPB / data.Visina);

            for (int y = 0; y < data.Visina; y++)
            {
                for (int x = 0; x < data.Sirina; x++)
                {
                    int xPiksel = x * celijaSirina;
                    int yPiksel = y * celijaVisina;
                    Rectangle rect = new Rectangle(xPiksel, yPiksel, celijaSirina, celijaVisina);

                    // Preuzimamo stanje iz naše instance klase Teren (pretpostavimo da se zove 'trenutniTeren')
                    // Ako još uvek nemaš instanciran teren u formi, za test možeš proveriti preko neke matrice, 
                    // ali ovako izgleda poziv sa tvojom klasom:
                    byte stanje = trenutniTeren.VratiStanjeCelije(x, y);

                    if (stanje == 0)
                    {
                        // Kopno - Zeleno
                        g.FillRectangle(Brushes.SeaGreen, rect);
                    }
                    else if (stanje == 2)
                    {
                        // Voda - Plavo
                        g.FillRectangle(Brushes.LightSkyBlue, rect);
                    }
                    else if (stanje == 1)
                    {
                        // Most - Sivo polje sa crnim X
                        g.FillRectangle(Brushes.Gray, rect);

                        // Crtanje crnog X unutar pravougaonika mosta
                        g.DrawLine(Pens.Black, rect.Left, rect.Top, rect.Right, rect.Bottom);
                        g.DrawLine(Pens.Black, rect.Right, rect.Top, rect.Left, rect.Bottom);
                        // Crne ivice mreže
                        g.DrawRectangle(Pens.Black, rect);
                    }
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
            pictureBoxTeren.Invalidate();
        }
    }
}
