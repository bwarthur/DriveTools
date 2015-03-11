using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DriveToolsSql.Model
{
    public class NoSqlContext
    {
        public NoSqlContext()
        {
            
        }

        public List<string> GetRemoveList()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var server = client.GetServer();
            var database = server.GetDatabase("Drive2");

            var removeCollection = database.GetCollection<BsonDocument>("remove");
            var removeList =
                removeCollection.FindAll()
                    .Select(remove => remove.GetValue("removeId").AsString)
                    .OrderBy(r => r)
                    .ToList();

            return removeList;
        }
    }
}
