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
        public static async Task SnimiGrafZaNivoAsync(string nazivNivoa, List<Ostrvo> ostrva, List<VezaMosta> veze)
        {
            if (_driver == null) return;
            using var session = _driver.AsyncSession(config => config.WithDatabase("neo4j"));
            await session.ExecuteWriteAsync(async tx =>
            {
                // 1. Brišemo stare čvorove ovog nivoa da ne bi došlo do preklapanja pri izmenama
                string queryDelete = "MATCH (o:Ostrvo {nivo: $naziv}) DETACH DELETE o";
                await tx.RunAsync(queryDelete, new { naziv = nazivNivoa });
                // 2. Kreiramo čvorove - čuvamo samo nivo i lokalni ID ostrva (npr. 1 do k)
                foreach (var ostrvo in ostrva)
                {
                    string queryOstrvo = "CREATE (o:Ostrvo {nivo: $nivo, id: $id})";

                    await tx.RunAsync(queryOstrvo, new
                    {
                        nivo = nazivNivoa,
                        id = ostrvo.Id
                    });
                }
                // 3. Kreiramo relacije (mostove) između ostrva unutar istog nivoa
                foreach (var veza in veze)
                {
                    string queryVeza = @"
                MATCH (o1:Ostrvo {nivo: $nivo, id: $id1})
                MATCH (o2:Ostrvo {nivo: $nivo, id: $id2})
                CREATE (o1)-[:POVEZAN_MOSTOM]->(o2)";

                    await tx.RunAsync(queryVeza, new
                    {
                        nivo = nazivNivoa,
                        id1 = veza.Ostrvo1Id,
                        id2 = veza.Ostrvo2Id
                    });
                }
            });
        }
        public static async Task<List<int>> PronadjiHamiltonovPutAsync(string nazivNivoa)
        {
            if (_driver == null) return new List<int>();
            using var session = _driver.AsyncSession(config => config.WithDatabase("neo4j"));

            return await session.ExecuteReadAsync(async tx =>
            {
                // Pronalazimo putanju koja počinje od bilo kog čvora, ide preko relacija (u oba smera) 
                // i poseti tačno onoliko čvorova koliko ih ukupno ima u tom nivou, bez ponavljanja.
                string query = @"
            MATCH (o:Ostrvo {nivo: $nivo})
            WITH count(o) as totalNodes
            MATCH path = (start:Ostrvo {nivo: $nivo})-[r:POVEZAN_MOSTOM*]-(end:Ostrvo {nivo: $nivo})
            WHERE length(path) = totalNodes - 1
            WITH path, nodes(path) as nodesInPath, totalNodes
            UNWIND nodesInPath as node
            WITH path, totalNodes, collect(DISTINCT node) as uniqueNodes
            WHERE size(uniqueNodes) = totalNodes
            RETURN [nd IN nodes(path) | nd.id] as putanja
            LIMIT 1";

                var cursor = await tx.RunAsync(query, new { nivo = nazivNivoa });
                if (await cursor.FetchAsync())
                {
                    var putanjaList = cursor.Current["putanja"].As<List<object>>();
                    return putanjaList.Select(x => Convert.ToInt32(x)).ToList();
                }

                // Alternativni upit ako osnovni vrati prazno (provera preko single-direction match-a sa tolikim brojem skokova)
                string fallbackQuery = @"
            MATCH (o:Ostrvo {nivo: $nivo})
            WITH count(o) as totalNodes
            MATCH p = (c:Ostrvo {nivo: $nivo})
            WHERE all(i in range(0, size(nodes(p)) - 2) WHERE (nodes(p)[i]) -[:POVEZAN_MOSTOM]- (nodes(p)[i+1]))
              AND length(p) = totalNodes - 1
              AND size(apoc.coll.toSet(nodes(p))) = totalNodes
            RETURN [nd IN nodes(p) | nd.id] as putanja
            LIMIT 1";

                return new List<int>();
            });
        }
        public static void Zatvori()
        {
            _driver?.Dispose();
        }
    }
}
