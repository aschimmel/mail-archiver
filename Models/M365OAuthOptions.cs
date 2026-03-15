namespace MailArchiver.Models
{
    public class M365OAuthOptions
    {
        public const string M365OAuth = "M365OAuth";

        public string Authority { get; set; } = "https://login.live.com";
        public string AuthorizationEndpoint { get; set; } = "https://login.live.com/oauth20_authorize.srf";
        public string TokenEndpoint { get; set; } = "https://login.live.com/oauth20_token.srf";
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string[] Scopes { get; set; } = new[] { "offline_access", "https://outlook.office.com/IMAP.AccessAsUser.All" };
    }
}
