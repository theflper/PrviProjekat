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
            // Pomoćna funkcija koja nalazi ID ostrva na osnovu (x, y) koordinate kopna
            int VratiIdOstrvaNaKoordinati(int x, int y)
            {
                foreach (var ostrvo in Ostrva)
                {
                    // Proveravamo da li tačka (x,y) spada u bounding box ostrva
                    // (Ako su ostrva nepravilnog oblika, ovde možemo preciznije proveriti, 
                    // ali pošto koristiš bounding box-ove, ovo je standardna provera)
                    if (x >= ostrvo.MinX && x <= ostrvo.MaxX && y >= ostrvo.MinY && y <= ostrvo.MaxY)
                    {
                        // Dodatna provera da li je zaista kopno na toj tački
                        if (VratiStanjeCelije(x, y) == 0)
                        {
                            return ostrvo.Id;
                        }
                    }
                }
                return -1;
            }
            for (int x = 0; x < Sirina; x++)
            {
                for (int y = 0; y < Visina; y++)
                {
                    if (VratiStanjeCelije(x, y) == 1) // Ako je polje most
                    {
                        // Gledamo 4 suseda mosta (gore, dole, levo, desno)
                        int[] dx = { 0, 0, -1, 1 };
                        int[] dy = { -1, 1, 0, 0 };

                        var susednaOstrvaIds = new List<int>();

                        for (int i = 0; i < 4; i++)
                        {
                            int susedX = x + dx[i];
                            int susedY = y + dy[i];

                            int idOstrva = VratiIdOstrvaNaKoordinati(susedX, susedY);
                            if (idOstrva != -1)
                            {
                                susednaOstrvaIds.Add(idOstrva);
                            }
                        }
                        // Uzimamo jedinstvene ID-jeve (da ne dupliramo ako most graniči sa istim ostrvom više puta)
                        var jedinstvenaOstrva = susednaOstrvaIds.Distinct().ToList();

                        // Ako most spaja tačno dva različita ostrva, to je validna veza
                        if (jedinstvenaOstrva.Count == 2)
                        {
                            veze.Add(new VezaMosta { Ostrvo1Id = jedinstvenaOstrva[0], Ostrvo2Id = jedinstvenaOstrva[1] });
                        }
                    }
                }
            }
            // Uklanjamo duplikate veza
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
