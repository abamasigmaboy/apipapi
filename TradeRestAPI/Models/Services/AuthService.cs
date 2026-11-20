using TradeRestAPI.Models;
using TradeRestAPI.Models.Data;

namespace TradeRestAPI.Models.Services
{
    public class AuthService
    {
        private readonly TradeDbContext db;

        public AuthService()
        {
            db = new TradeDbContext();
        }

        public AuthService(TradeDbContext _db)
        {
            db = _db;
        }

        public Client? Login(string login, string password)
        {
            var client = db.Clients.FirstOrDefault(x =>
                x.Login == login && (x.Password == password || x.Passport == password));
            return client;
        }

        public bool Register(Client regClient)
        {
            var existing = db.Clients.FirstOrDefault(p => p.Login == regClient.Login);
            if (existing == null)
            {
                db.Clients.Add(regClient);
                db.SaveChanges();
                return true;
            }
            return false;
        }
    }
}
