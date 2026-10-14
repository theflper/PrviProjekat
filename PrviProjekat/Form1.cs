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
        private void Form1_Load(object sender, EventArgs e)
        {
            labelN.Text = "N = " + data.Visina;
            labelM.Text = "M = " + data.Sirina;
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

        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {

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

            // Provera da li je klik unutar granica grida
            if (kliknutX >= 0 && kliknutX < data.Sirina && kliknutY >= 0 && kliknutY < data.Visina)
            {
                // Proveravamo koji taster miša je kliknut
                if (e.Button == MouseButtons.Left)
                {
                    // LEVI KLIK = VODA (stanje 2)
                    trenutniTeren.PostaviStanjeCelije(kliknutX, kliknutY, 2);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    // DESNI KLIK = MOST (stanje 1)
                    trenutniTeren.PostaviStanjeCelije(kliknutX, kliknutY, 1);
                }
                //Pozivamo Invalidate() da se PictureBox odmah ponovo iscrta sa novom promenom!
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
