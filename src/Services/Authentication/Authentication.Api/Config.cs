using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Models;

namespace Authentication.Api
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Email { Required = true},
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new("characterapi")
                {
                    DisplayName = "Characters", Required = true, Description = "Gives access to control your character and view others."
                },
            };

        private static ICollection<string> AllScopes
        {
            get
            {
                var scopes = ApiScopes.Select(s => s.Name).ToList();
                scopes.AddRange(IdentityResources.Select(r => r.Name));
                return scopes;
            }
        }

        public static IEnumerable<Client> Clients(string gameUrl, string characterApiUrl) =>
            new []
            {
                new Client
                {
                    ClientId = "game",
                    ClientName = "Game",
                    ClientSecrets = { new Secret("39861364-42fc-4147-b3aa-88248a00bc1b".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    
                    PostLogoutRedirectUris = { gameUrl },

                    RedirectUris = { gameUrl + "/auth/signin-callback", gameUrl + "/auth/renew-callback" },
                    
                    AllowedScopes = AllScopes,
                    
                    RequireConsent = false
                },
                new Client
                {
                    ClientId = "docs",
                    ClientName = "Documentation",
                    ClientSecrets = { new Secret("5b7f084b-9eb4-4f0b-a833-e6a32b9a51bd".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,

                    RedirectUris = { characterApiUrl + "/swagger/oauth2-redirect.html" },

                    AllowedScopes = AllScopes,

                    RequireConsent = true
                },
            };
    }
}