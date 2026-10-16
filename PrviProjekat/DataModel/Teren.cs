namespace PrviProjekat.DataModel
{
    /// <summary>
    /// Predstavlja jedno ostrvo sa svojim bounding box-om i identifikatorom.
    /// </summary>
    public class Ostrvo
    {
        public int Id { get; set; }
        public int MinX { get; set; }
        public int MinY { get; set; }
        public int MaxX { get; set; }
        public int MaxY { get; set; }
    }
    public class PodaciGrida
    {
        public int Sirina { get; set; }
        public int Visina { get; set; }
        public byte[] GridBitMemorija { get; set; }
        public List<Ostrvo> Ostrva { get; set; } = new();
        public List<VezaMosta> Veze { get; set; } = new();
        public PodaciGrida()
        {
            Sirina = 4; // Primer širine
            Visina = 4; // Primer visine
            int ukupnoCelija = 16;
            int velicinaNizaBajtova = (ukupnoCelija + 3) / 4;
            GridBitMemorija = new byte[velicinaNizaBajtova];
        }
    }
    public class VezaMosta
    {
        public int Ostrvo1Id { get; set; }
        public int Ostrvo2Id { get; set; }
    }
    /// <summary>
    /// Upravlja gridom terena, memorijom i bit-mapom stanja.
    /// </summary>
    public class Teren
    {
            private readonly int _sirina;
            private readonly int _visina;

            // Matrica ili niz bitova za čuvanje stanja terena (00-kopno, 01-most, 10-voda, 11-neiskorišćeno)
            // Svaka ćelija čuva 2 bita, pa možemo koristiti niz bajtova ili interno pakovanje.
            private readonly byte[] _gridBitMemorija;

            public List<Ostrvo> Ostrva { get; set; } = new ();
            public int Sirina => _sirina;
            public int Visina => _visina;

            // Vraća direktno referencu ili kopiju niza bajtova
            public byte[] DajSvuMemoriju() => _gridBitMemorija;

            // Učitava spoljašnju memoriju i ostrva u teren
            public void UcitajMemoriju(byte[] memorija, List<Ostrvo> ostrva)
            {
                if (memorija != null && memorija.Length == _gridBitMemorija.Length)
                {
                    Array.Copy(memorija, _gridBitMemorija, memorija.Length);
                }
                Ostrva = ostrva ?? new List<Ostrvo>();
            }

        public Teren(int sirina, int visina)
        {
            _sirina = sirina;
            _visina = visina;

            // Računamo veličinu niza bajtova (2 bita po ćeliji -> 4 ćelije staju u 1 bajt)
            int ukupnoCelija = sirina * visina;
            int velicinaNizaBajtova = (ukupnoCelija + 3) / 4;
            _gridBitMemorija = new byte[velicinaNizaBajtova];
        }
        public List<VezaMosta> PronadjiVezeMostova()
        {
            var veze = new List<VezaMosta>();
            if (Ostrva == null || Ostrva.Count < 2) return veze;

            // Pomoćna funkcija koja proverava da li je tačka (x, y) unutar nekog ostrva i vraća njegov ID
            int VratiIdOstrva(int x, int y)
            {
                foreach (var ostrvo in Ostrva)
                {
                    if (x >= ostrvo.MinX && x <= ostrvo.MaxX && y >= ostrvo.MinY && y <= ostrvo.MaxY)
                    {
                        if (VratiStanjeCelije(x, y) == 0) return ostrvo.Id;
                    }
                }
                return -1;
            }

            // Za svako ostrvo tražimo direktne veze prateći mostove (1)
            foreach (var polaznoOstrvo in Ostrva)
            {
                // Nalazimo sve obodne ćelije ovog ostrva (kopno koje graniči sa vodom/mostom)
                var obodneCelije = new List<(int X, int Y)>();

                for (int y = polaznoOstrvo.MinY; y <= polaznoOstrvo.MaxY; y++)
                {
                    for (int x = polaznoOstrvo.MinX; x <= polaznoOstrvo.MaxX; x++)
                    {
                        if (VratiStanjeCelije(x, y) == 0) // Ako je kopno ovog ostrva
                        {
                            int[] dx = { 0, 0, -1, 1 };
                            int[] dy = { -1, 1, 0, 0 };
                            for (int i = 0; i < 4; i++)
                            {
                                int nx = x + dx[i];
                                int ny = y + dy[i];
                                // Ako susedno polje postoji i nije kopno (započinje most)
                                if (VratiStanjeCelije(nx, ny) == 1)
                                {
                                    obodneCelije.Add((nx, ny));
                                }
                            }
                        }
                    }
                }

                // Od svake obodne ćelije širimo se kroz mostove (1) da vidimo do kog drugog ostrva vodi
                foreach (var pocetakMosta in obodneCelije.Distinct())
                {
                    bool[,] posecenoMost = new bool[Sirina, Visina];
                    Queue<(int X, int Y)> red = new Queue<(int, int)>();

                    red.Enqueue(pocetakMosta);
                    posecenoMost[pocetakMosta.X, pocetakMosta.Y] = true;

                    while (red.Count > 0)
                    {
                        var (trenutniX, trenutniY) = red.Dequeue();

                        int[] dx = { 0, 0, -1, 1 };
                        int[] dy = { -1, 1, 0, 0 };

                        for (int i = 0; i < 4; i++)
                        {
                            int susedX = trenutniX + dx[i];
                            int susedY = trenutniY + dy[i];

                            if (susedX >= 0 && susedX < Sirina && susedY >= 0 && susedY < Visina)
                            {
                                byte stanje = VratiStanjeCelije(susedX, susedY);

                                if (stanje == 1 && !posecenoMost[susedX, susedY]) // Nastavljamo kroz most
                                {
                                    posecenoMost[susedX, susedY] = true;
                                    red.Enqueue((susedX, susedY));
                                }
                                else if (stanje == 0) // Stigli smo do nekog kopna!
                                {
                                    int ciljniIdOstrva = VratiIdOstrva(susedX, susedY);
                                    if (ciljniIdOstrva != -1 && ciljniIdOstrva != polaznoOstrvo.Id)
                                    {
                                        // Našli smo vezu između ova dva ostrva!
                                        veze.Add(new VezaMosta
                                        {
                                            Ostrvo1Id = Math.Min(polaznoOstrvo.Id, ciljniIdOstrva),
                                            Ostrvo2Id = Math.Max(polaznoOstrvo.Id, ciljniIdOstrva)
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // Uklanjamo duplikate i vraćamo čiste jedinstvene veze
            return veze.DistinctBy(v => new { v.Ostrvo1Id, v.Ostrvo2Id }).ToList();
        }
        /// <summary>
        /// Postavlja stanje za određenu ćeliju (x, y).
        /// stanje: 0 (kopno), 1 (most), 2 (voda), 3 (neiskorišćeno)
        /// </summary>
        public void PostaviStanjeCelije(int x, int y, byte stanje)
        {
            if (x < 0 || x >= _sirina || y < 0 || y >= _visina) return;
            if (stanje > 3) throw new ArgumentOutOfRangeException(nameof(stanje), "Stanje mora biti između 0 i 3.");

            int indeksCelije = y * _sirina + x;
            int indeksBajta = indeksCelije / 4;
            int pozicijaBitova = (indeksCelije % 4) * 2;

            // Očisti stare bitove i upiši nove
            _gridBitMemorija[indeksBajta] &= (byte)~(0b11 << pozicijaBitova);
            _gridBitMemorija[indeksBajta] |= (byte)(stanje << pozicijaBitova);
        }

        /// <summary>
        /// Vraća stanje ćelije na datim koorinatam.
        /// </summary>
        public byte VratiStanjeCelije(int x, int y)
        {
            if (x < 0 || x >= _sirina || y < 0 || y >= _visina) return 3; // neiskorišćeno van granica

            int indeksCelije = y * _sirina + x;
            int indeksBajta = indeksCelije / 4;
            int pozicijaBitova = (indeksCelije % 4) * 2;

            return (byte)((_gridBitMemorija[indeksBajta] >> pozicijaBitova) & 0b11);
        }

        /// <summary>
        /// Metoda za učitavanje ili generisanje početnog terena i ostrva.
        /// </summary>
        public void InicijalizujPrimer()
        {
            // Primer dodavanja ostrva sa bounding box-om
            Ostrva.Add(new Ostrvo { Id = 1, MinX = 2, MinY = 2, MaxX = 5, MaxY = 5 });
            Ostrva.Add(new Ostrvo { Id = 2, MinX = 10, MinY = 2, MaxX = 14, MaxY = 6 });

            // Ovde možeš popuniti grid vodom, kopnom itd.
        }
        /// <summary>
        /// Detektuje sva ostrva na terenu koristeći Flood Fill (BFS) algoritam 
        /// i automatski popunjava listu ostrva sa njihovim bounding box-ovima.
        /// </summary>
        public void DetektujOstrva()
        {
            Ostrva.Clear();
            bool[,] poseceno = new bool[_sirina, _visina];
            int idCounter = 1;
            for (int y = 0; y < _visina; y++)
            {
                for (int x = 0; x < _sirina; x++)
                {
                    // Ako je ćelija kopno (0) i nije već posećena, našli smo novo ostrvo!
                    if (VratiStanjeCelije(x, y) == 0 && !poseceno[x, y])
                    {
                        Ostrvo novoOstrvo = new Ostrvo
                        {
                            Id = idCounter++,
                            MinX = x,
                            MinY = y,
                            MaxX = x,
                            MaxY = y
                        };
                        // BFS red za obilazak povezanih delova kopna
                        Queue<(int X, int Y)> red = new Queue<(int, int)>();
                        red.Enqueue((x, y));
                        poseceno[x, y] = true;
                        while (red.Count > 0)
                        {
                            var (trenutniX, trenutniY) = red.Dequeue();
                            // Ažuriramo granice bounding box-a
                            if (trenutniX < novoOstrvo.MinX) novoOstrvo.MinX = trenutniX;
                            if (trenutniX > novoOstrvo.MaxX) novoOstrvo.MaxX = trenutniX;
                            if (trenutniY < novoOstrvo.MinY) novoOstrvo.MinY = trenutniY;
                            if (trenutniY > novoOstrvo.MaxY) novoOstrvo.MaxY = trenutniY;
                            // Provera 4 suseda (gore, dole, levo, desno)
                            int[] dx = { 0, 0, -1, 1 };
                            int[] dy = { -1, 1, 0, 0 };

                            for (int i = 0; i < 4; i++)
                            {
                                int susedX = trenutniX + dx[i];
                                int susedY = trenutniY + dy[i];

                                if (susedX >= 0 && susedX < _sirina && susedY >= 0 && susedY < _visina)
                                {
                                    if (VratiStanjeCelije(susedX, susedY) == 0 && !poseceno[susedX, susedY])
                                    {
                                        poseceno[susedX, susedY] = true;
                                        red.Enqueue((susedX, susedY));
                                    }
                                }
                            }
                        }
                        Ostrva.Add(novoOstrvo);
                    }
                }
            }
        }
    }
}
