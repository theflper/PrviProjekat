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
        public PodaciGrida()
        {
            Sirina = 4; // Primer širine
            Visina = 4; // Primer visine
            int ukupnoCelija = 16;
            int velicinaNizaBajtova = (ukupnoCelija + 3) / 4;
            GridBitMemorija = new byte[velicinaNizaBajtova];
        }
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

    public Teren(int sirina, int visina)
    {
        _sirina = sirina;
        _visina = visina;

        // Računamo veličinu niza bajtova (2 bita po ćeliji -> 4 ćelije staju u 1 bajt)
        int ukupnoCelija = sirina * visina;
        int velicinaNizaBajtova = (ukupnoCelija + 3) / 4;
        _gridBitMemorija = new byte[velicinaNizaBajtova];
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
}
}
