using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BabisW.ScriptHub
{
    public struct ScriptData
    {
        public string Id;
        public string Title;
        public string Script;
        public string RawScriptUrl;
        public string Desc;
        public string Credits;
        public string ImageURL;
        public string Slug;
        public string GameName;
        public bool Verified;
        public bool HasKey;
        public bool IsUniversal;
        public bool IsPatched;
        public int Views;
        public string ScriptType;
        public string CreatedAt;
    }

    public sealed class ScriptHubFetchOptions
    {
        public int Page { get; set; } = 1;
        public string Query { get; set; }
        public string OrderBy { get; set; } = "date";
        public string Sort { get; set; } = "desc";
    }

    public static class BabisWSC
    {
        private const string Endpoint = "https://rscripts.net/api/v2/scripts";
        private const string BaseUrl = "https://rscripts.net";
        private static readonly HttpClient Client = CreateClient();

        static BabisWSC()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        public static Task<ScriptData[]> GetSCData(
            ScriptHubFetchOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return FetchAsync(options ?? new ScriptHubFetchOptions(), cancellationToken);
        }

        public static Task<ScriptData[]> SearchSCData(
            string query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return SearchAsync(query, cancellationToken);
        }

        private static async Task<ScriptData[]> SearchAsync(
            string query,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await FetchAsync(new ScriptHubFetchOptions(), cancellationToken)
                    .ConfigureAwait(false);
            }

            var normalizedQuery = query.Trim();
            var results = await FetchAsync(new ScriptHubFetchOptions
            {
                Query = normalizedQuery
            }, cancellationToken).ConfigureAwait(false);
            if (results.Length > 0)
            {
                return results;
            }

            // Some catalogue mirrors intermittently ignore q. Search a few
            // pages locally so a valid query does not appear empty.
            var fallback = new List<ScriptData>();
            for (var page = 1; page <= 3; page++)
            {
                var pageResults = await FetchAsync(new ScriptHubFetchOptions
                {
                    Page = page
                }, cancellationToken).ConfigureAwait(false);
                fallback.AddRange(pageResults.Where(script =>
                    Contains(script.Title, normalizedQuery) ||
                    Contains(script.Desc, normalizedQuery) ||
                    Contains(script.GameName, normalizedQuery) ||
                    Contains(script.Credits, normalizedQuery)));
            }

            return fallback
                .GroupBy(script => string.IsNullOrWhiteSpace(script.Id) ? script.Title : script.Id)
                .Select(group => group.First())
                .ToArray();
        }

        private static bool Contains(string value, string query)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static async Task<ScriptData[]> FetchAsync(
            ScriptHubFetchOptions options,
            CancellationToken cancellationToken)
        {
            var values = new List<string>
            {
                "page=" + Math.Max(1, options.Page).ToString(CultureInfo.InvariantCulture),
                "orderBy=" + Uri.EscapeDataString(options.OrderBy ?? "date"),
                "sort=" + Uri.EscapeDataString(options.Sort ?? "desc")
            };
            if (!string.IsNullOrWhiteSpace(options.Query))
            {
                values.Add("q=" + Uri.EscapeDataString(options.Query));
            }

            using (var response = await Client.GetAsync(
                Endpoint + "?" + string.Join("&", values), cancellationToken).ConfigureAwait(false))
            {
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"RScripts returned {(int)response.StatusCode} ({response.ReasonPhrase}): {body}");
                }

                JObject payload;
                try
                {
                    payload = JObject.Parse(body);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("RScripts returned invalid JSON.", ex);
                }

                var scripts = payload["scripts"] as JArray;
                if (scripts == null)
                {
                    throw new InvalidOperationException("RScripts response did not contain a scripts list.");
                }

                return scripts.OfType<JObject>()
                    .Select(ParseScript)
                    .Where(script => !string.IsNullOrWhiteSpace(script.Title))
                    .ToArray();
            }
        }

        public static async Task<string> DownloadScriptAsync(
            string rawScriptUrl,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(rawScriptUrl))
            {
                throw new InvalidOperationException("This catalogue entry does not contain a raw script URL.");
            }

            using (var response = await Client.GetAsync(rawScriptUrl, cancellationToken).ConfigureAwait(false))
            {
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(body))
                {
                    throw new HttpRequestException(
                        $"RScripts raw script request failed with {(int)response.StatusCode} ({response.ReasonPhrase}).");
                }

                return body;
            }
        }

        private static HttpClient CreateClient()
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Babis-W Script Hub");
            return client;
        }

        private static ScriptData ParseScript(JObject script)
        {
            var game = script["game"] as JObject;
            var user = script["user"] as JObject;
            var rawScript = script.Value<string>("rawScript");
            return new ScriptData
            {
                Id = script.Value<string>("_id") ?? string.Empty,
                Title = script.Value<string>("title") ?? "Untitled script",
                Script = string.Empty,
                RawScriptUrl = NormalizeUrl(rawScript),
                Desc = script.Value<string>("description") ?? game?.Value<string>("title") ?? "Roblox script",
                Credits = user?.Value<string>("username") ?? "RScripts",
                ImageURL = NormalizeUrl(script.Value<string>("image") ?? game?.Value<string>("imgurl")),
                Slug = script.Value<string>("slug") ?? string.Empty,
                GameName = game?.Value<string>("title") ?? string.Empty,
                Verified = user?.Value<bool?>("verified") == true,
                HasKey = script.Value<bool?>("keySystem") == true,
                IsUniversal = string.IsNullOrWhiteSpace(game?.Value<string>("placeId")),
                IsPatched = false,
                Views = script.Value<int?>("views") ?? 0,
                ScriptType = script.Value<bool?>("paid") == true ? "paid" : "free",
                CreatedAt = script.Value<string>("createdAt") ?? string.Empty
            };
        }

        private static string NormalizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            return Uri.TryCreate(url, UriKind.Absolute, out Uri absolute)
                ? absolute.ToString()
                : BaseUrl + (url.StartsWith("/") ? url : "/" + url);
        }
    }
}
