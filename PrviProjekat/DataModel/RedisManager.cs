namespace PrviProjekat.DataModel
{
    //docker run --name redis-container -p 6379:6379 -d redis
    //sudo service redis-server start
    public static class RedisManager
    {
        private static ConnectionMultiplexer? _connection;
        private static readonly string ConnectionString = "localhost:6379,abortConnect=false";

        // Javni property da forma može da pristupi konekciji za pretragu ključeva
        public static ConnectionMultiplexer? Connection => _connection;

        public static void Inicijalizuj()
        {
            try
            {
                if (_connection == null || !_connection.IsConnected)
                {
                    _connection = ConnectionMultiplexer.Connect(ConnectionString);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri povezivanju sa Redis bazom: {ex.Message}\nProverite da li je Redis server pokrenut.",
                    "Redis Greška", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public static IDatabase? GetDatabase()
        {
            if (_connection == null || !_connection.IsConnected)
            {
                Inicijalizuj();
            }

            if (_connection == null) return null;

            return _connection.GetDatabase();
        }
    }
}
