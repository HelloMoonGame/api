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
                new ApiScope("characterapi"),
            };

        public static IEnumerable<Client> Clients(string gameUrl) =>
            new Client[]
            {
                // interactive client using code flow + pkce
                new Client
                {
                    ClientId = "interactive",
                    ClientSecrets = { new Secret("39861364-42fc-4147-b3aa-88248a00bc1b".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = { gameUrl + "/signin-oidc" },
                    FrontChannelLogoutUri = gameUrl + "/signout-oidc",
                    PostLogoutRedirectUris = { gameUrl + "/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "characterapi" },
                    
                    RequireConsent = false
                },
            };
    }
}