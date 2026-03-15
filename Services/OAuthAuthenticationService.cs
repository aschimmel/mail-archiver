using MailArchiver.Auth.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Claims;
using MailArchiver.Models;

namespace MailArchiver.Services
{
    public class OAuthAuthenticationService
    {
        private readonly ILogger<OAuthAuthenticationService> _logger;
        private readonly IOptions<OAuthOptions> _oAuthOptions;

        public OAuthAuthenticationService(
            ILogger<OAuthAuthenticationService> logger
            , IOptions<OAuthOptions> oAuthOptions)
        {
            _logger = logger;
            _oAuthOptions = oAuthOptions;
        }

        public async Task HandleLoginAsync(
            UserInformationReceivedContext ctx
            , CancellationToken cancellationToken = default) { 
        }
    }

    public class M365OAuthTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<M365OAuthOptions> _options;
        private readonly ILogger<M365OAuthTokenService> _logger;

        public M365OAuthTokenService(
            HttpClient httpClient,
            IOptions<M365OAuthOptions> options,
            ILogger<M365OAuthTokenService> logger)
        {
            _httpClient = httpClient;
            _options = options;
            _logger = logger;
        }

        public string? BuildAuthorizationUrl(string redirectUri, string state, string[]? scopes = null, string? loginHint = null)
        {
            if (string.IsNullOrWhiteSpace(redirectUri) || string.IsNullOrWhiteSpace(state))
            {
                return null;
            }

            var authorizationEndpoint = ResolveAuthorizationEndpoint();
            if (string.IsNullOrWhiteSpace(authorizationEndpoint) || string.IsNullOrWhiteSpace(_options.Value.ClientId))
            {
                _logger.LogWarning("M365 OAuth options are incomplete for authorization URL generation.");
                return null;
            }

            var requestedScopes = scopes ?? _options.Value.Scopes ?? Array.Empty<string>();
            var scopeValue = string.Join(" ", requestedScopes);
            var query = new Dictionary<string, string?>
            {
                ["client_id"] = _options.Value.ClientId,
                ["response_type"] = "code",
                ["redirect_uri"] = redirectUri,
                ["response_mode"] = "query",
                ["scope"] = scopeValue,
                ["state"] = state
            };

            if (!string.IsNullOrWhiteSpace(loginHint))
            {
                query["login_hint"] = loginHint;
            }

            return QueryHelpers.AddQueryString(authorizationEndpoint, query);
        }

        public Task<M365TokenResponse?> RequestTokenWithAuthorizationCodeAsync(
            string code,
            string redirectUri,
            string[]? scopes = null,
            CancellationToken cancellationToken = default)
        {
            var values = new Dictionary<string, string>
            {
                ["client_id"] = _options.Value.ClientId,
                ["client_secret"] = _options.Value.ClientSecret,
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = redirectUri,
                ["scope"] = string.Join(" ", scopes ?? _options.Value.Scopes ?? Array.Empty<string>())
            };

            return RequestTokenAsync(values, cancellationToken);
        }

        public Task<M365TokenResponse?> RefreshAccessTokenAsync(
            string refreshToken,
            string[]? scopes = null,
            CancellationToken cancellationToken = default)
        {
            var values = new Dictionary<string, string>
            {
                ["client_id"] = _options.Value.ClientId,
                ["client_secret"] = _options.Value.ClientSecret,
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["scope"] = string.Join(" ", scopes ?? _options.Value.Scopes ?? Array.Empty<string>())
            };

            return RequestTokenAsync(values, cancellationToken);
        }

        private async Task<M365TokenResponse?> RequestTokenAsync(
            Dictionary<string, string> formValues,
            CancellationToken cancellationToken)
        {
            var tokenEndpoint = ResolveTokenEndpoint();

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(formValues)
            };

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "M365 token request failed with status {StatusCode}. Response: {ResponseBody}",
                    (int)response.StatusCode,
                    body);
                return null;
            }

            var tokenResponse = JsonSerializer.Deserialize<M365TokenResponse>(
                body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (tokenResponse == null)
            {
                _logger.LogWarning("M365 token request succeeded but response could not be parsed.");
            }

            return tokenResponse;
        }

        private string ResolveAuthorizationEndpoint()
        {
            if (!string.IsNullOrWhiteSpace(_options.Value.AuthorizationEndpoint))
            {
                return _options.Value.AuthorizationEndpoint;
            }

            var authority = (_options.Value.Authority ?? string.Empty).TrimEnd('/');
            return string.IsNullOrWhiteSpace(authority)
                ? string.Empty
                : $"{authority}/oauth2/v2.0/authorize";
        }

        private string ResolveTokenEndpoint()
        {
            if (!string.IsNullOrWhiteSpace(_options.Value.TokenEndpoint))
            {
                return _options.Value.TokenEndpoint;
            }

            var authority = (_options.Value.Authority ?? string.Empty).TrimEnd('/');
            return string.IsNullOrWhiteSpace(authority)
                ? string.Empty
                : $"{authority}/oauth2/v2.0/token";
        }
    }

    public class M365TokenResponse
    {
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("ext_expires_in")]
        public int ExtExpiresIn { get; set; }

        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
    }
}
