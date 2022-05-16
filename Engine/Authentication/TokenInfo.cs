using System;
using System.Collections.Generic;

namespace OpenTap.Authentication
{
    /// <summary> Represents stored information about a token. </summary>
    public class TokenInfo
    {
        /// <summary> Raw token string. </summary>
        public string TokenData { get; set; }
        /// <summary> Expiration date of the token. </summary>
        public DateTime Expiration { get; set; }
        /// <summary> The site this token activates. </summary>
        public string Domain { get; set; }
        /// <summary> The type of token. </summary>
        public TokenType Type { get; set; }

        /// <summary> Gets if the token is expired.</summary>
        public bool Expired => Expiration < DateTime.Now;

        System.Text.Json.JsonDocument payload;

        System.Text.Json.JsonDocument GetPayload()
        {
            if (payload != null) return payload;
            var payloadData = TokenData.Split('.')[1];
            payload = System.Text.Json.JsonDocument.Parse(Convert.FromBase64String(payloadData));
            return payload;
        }
        /// <summary> Gets the client id. </summary>
        public string GetClientId()
        {
            if (GetPayload().RootElement.TryGetProperty("azp", out var id))
                return id.GetString();
            return null;
        }

        /// <summary> Gets the auth URL. </summary>
        public string GetAuthority()
        {
            if (GetPayload().RootElement.TryGetProperty("iss", out var id))
                return id.GetString();
            return null;
        }

        static readonly TimeSpan refreshSlack = TimeSpan.FromSeconds(30);
        /// <summary> Parses tokens from oauth response string (json format). </summary>

        public static List<TokenInfo> ParseTokens(string responseString, string domain)
        {
            var json = System.Text.Json.JsonDocument.Parse(responseString);

            //"expires_in":300,"refresh_expires_in":1800
            var accessExp = DateTime.Now.AddSeconds(300);
            var refreshExp = DateTime.Now.AddSeconds(1800);
            if (json.RootElement.TryGetProperty("expires_in", out var exp1Str) && exp1Str.TryGetInt32(out var accessAdd))
                accessExp = DateTime.Now.AddSeconds(accessAdd).Subtract(refreshSlack);
            if (json.RootElement.TryGetProperty("refresh_expires_in", out exp1Str) && exp1Str.TryGetInt32(out var refreshAdd))
                refreshExp = DateTime.Now.AddSeconds(refreshAdd).Subtract(refreshSlack);
            List<TokenInfo> tokens = new List<TokenInfo>();

            if (json.RootElement.TryGetProperty("access_token", out var accessTokenData))
                tokens.Add(new TokenInfo { Domain = domain, Expiration = accessExp, Type = TokenType.AccessToken, TokenData = accessTokenData.GetString() });

            if (json.RootElement.TryGetProperty("refresh_token", out var refreshTokenData))
                tokens.Add(new TokenInfo { Domain = domain, Expiration = refreshExp, Type = TokenType.RefreshToken, TokenData = refreshTokenData.GetString() });

            if(json.RootElement.TryGetProperty("id_token", out var idTokenData))
                tokens.Add(new TokenInfo { Domain = domain, Expiration = accessExp, Type = TokenType.IdentityToken, TokenData = idTokenData.GetString() });

            return tokens;
        }
    }
}