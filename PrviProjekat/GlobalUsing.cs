// Osnovni .NET imenski prostori
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
// Windows Forms i grafika
global using System.Drawing;
global using System.Windows.Forms;
// Serijalizacija (za JSON i Redis čuvanje stanja)
global using System.Text.Json;
global using System.Text.Json.Serialization;
// Baze podataka
global using Neo4j.Driver;          // Za rad sa Neo4j bazom
global using StackExchange.Redis;    // Za rad sa Redis bazom (Undo/Redo i keširanje)