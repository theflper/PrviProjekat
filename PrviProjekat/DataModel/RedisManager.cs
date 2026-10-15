namespace PrviProjekat.DataModel
{
    //docker run --name redis-container -p 6379:6379 -d redis
    internal class RedisManager
    {
        private static ConnectionMultiplexer? _connection;
        private static readonly string ConnectionString = "localhost:6379";

        // Inicijalizacija i povezivanje sa Redis-om
        public static void Inicijalizuj()
        {
            try
            {
                // Opciono: Ako želiš da pokreneš Redis preko skripte/procesa pre konekcije, 
                // ovde možeš pozvati metodu za pokretanje (vidi dole).

                _connection = ConnectionMultiplexer.Connect(ConnectionString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri povezivanju sa Redis bazom: {ex.Message}\nProverite da li je Redis server pokrenut.",
                    "Redis Greška", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        // Vraća bazu za rad (standardna baza 0)
        // Dodaj znak ? na IDatabase da označimo da može biti null
        public static IDatabase? GetDatabase()
        {
            if (_connection == null || !_connection.IsConnected)
            {
                Inicijalizuj();
            }

            // Ako je i posle inicijalizacije konekcija null, vraćamo null bezbedno
            if (_connection == null) return null;

            return _connection.GetDatabase();
        }
    }
}
