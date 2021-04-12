using IdentityServer4.Models;
using System.Collections.Generic;

namespace AuthenticationApi
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
                   new IdentityResource[]
                   {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                   };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ("characterapi"),
            };

        public static IEnumerable<Client> Clients(string gameUrl) =>
            new []
            {
                new Client
                {
                    ClientId = "game",
                    ClientSecrets = { new Secret("39861364-42fc-4147-b3aa-88248a00bc1b".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,

                    RedirectUris = { gameUrl + "/auth/signin-callback", gameUrl + "/auth/renew-callback" },
                    
                    AllowedScopes = { "openid", "profile", "characterapi" },
                    
                    RequireConsent = false
                },
            };
    }
}