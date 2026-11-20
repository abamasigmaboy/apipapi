namespace TradeRestAPI.Models
{
    public class Client
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string? Middlename { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Passport { get; set; }
        public string? Type { get; set; } = "Manager";
    }
}
