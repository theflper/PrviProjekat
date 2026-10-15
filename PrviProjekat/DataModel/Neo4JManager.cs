using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrviProjekat.DataModel
{
    public static class Neo4jManager
    {
        private static IDriver? _driver;

        public static void Inicijalizuj(string uri, string korisnickoIme, string lozinka)
        {
            try
            {
                _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(korisnickoIme, lozinka));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri povezivanju sa Neo4j bazom: {ex.Message}", "Neo4j Greška", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public static async Task SnimiGrafUNeo4jAsync(List<Ostrvo> ostrva, List<VezaMosta> veze)
        {
            if (_driver == null) return;
            using var session = _driver.AsyncSession(config => config.WithDatabase("NBPprojekat"));
            await session.ExecuteWriteAsync(async tx =>
            {
                // 1. Brišemo stare čvorove i veze
                await tx.RunAsync("MATCH (n:Ostrvo) DETACH DELETE n");
                // 2. Kreiramo čvorove za ostrva
                foreach (var ostrvo in ostrva)
                {
                    string queryOstrvo = "CREATE (o:Ostrvo {id: $id, minX: $minX, minY: $minY, maxX: $maxX, maxY: $maxY})";
                    await tx.RunAsync(queryOstrvo, new
                    {
                        id = ostrvo.Id,
                        minX = ostrvo.MinX,
                        minY = ostrvo.MinY,
                        maxX = ostrvo.MaxX,
                        maxY = ostrvo.MaxY
                    });
                }
                // 3. Kreiramo veze (mostove) između čvorova
                foreach (var veza in veze)
                {
                    string queryVeza = @"
                MATCH (o1:Ostrvo {id: $id1})
                MATCH (o2:Ostrvo {id: $id2})
                CREATE (o1)-[:POVEZAN_MOSTOM]->(o2)";
                    await tx.RunAsync(queryVeza, new
                    {
                        id1 = veza.Ostrvo1Id,
                        id2 = veza.Ostrvo2Id
                    });
                }
            });
        }
        public static void Zatvori()
        {
            _driver?.Dispose();
        }
    }
}
