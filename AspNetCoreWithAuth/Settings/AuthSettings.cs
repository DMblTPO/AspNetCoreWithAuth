using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AspNetCoreWithAuth.Settings
{
    public class AuthSettings
    {
        public string Issuer { get; set; } // token publisher
        public string Audience { get; set; } // token consumer
        public int Lifetime { get; set; } // token life time in minutes
        public string Secret { get; set; } // security key to encode

        public SymmetricSecurityKey GetSymmetricSecurityKey()
            => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Secret));
    }
}