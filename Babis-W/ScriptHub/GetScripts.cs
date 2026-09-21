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
            return FetchAsync(new ScriptHubFetchOptions
            {
                Query = string.IsNullOrWhiteSpace(query) ? null : query.Trim()
            }, cancellationToken);
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

                var parsed = scripts.OfType<JObject>().Select(ParseScript).ToArray();
                await PopulateScriptBodiesAsync(parsed, cancellationToken).ConfigureAwait(false);
                return parsed;
            }
        }

        private static async Task PopulateScriptBodiesAsync(
            ScriptData[] scripts,
            CancellationToken cancellationToken)
        {
            var tasks = scripts.Select(async script =>
            {
                if (string.IsNullOrWhiteSpace(script.Script))
                {
                    return script;
                }

                try
                {
                    using (var response = await Client.GetAsync(script.Script, cancellationToken).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        script.Script = body;
                    }
                }
                catch
                {
                    script.Script = string.Empty;
                }

                return script;
            });

            var completed = await Task.WhenAll(tasks).ConfigureAwait(false);
            for (var index = 0; index < scripts.Length; index++)
            {
                scripts[index] = completed[index];
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
                Script = NormalizeUrl(rawScript),
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
