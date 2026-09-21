using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net;
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

    public sealed class ScriptBloxFetchOptions
    {
        public int Page { get; set; } = 1;
        public int Max { get; set; } = 20;
        public string Exclude { get; set; }
        public string Mode { get; set; }
        public bool? Patched { get; set; }
        public bool? Key { get; set; }
        public bool? Universal { get; set; }
        public bool? Verified { get; set; }
        public string SortBy { get; set; } = "updatedAt";
        public string Order { get; set; } = "desc";
        public string Owner { get; set; }
        public long? PlaceId { get; set; }
    }

    public static class BabisWSC
    {
        private const string Endpoint = "https://scriptblox.com/api/script/fetch";
        private const string ScriptBloxBaseUrl = "https://scriptblox.com";
        private static readonly HttpClient Client = CreateClient();

        static BabisWSC()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        public static async Task<ScriptData[]> GetSCData(
            ScriptBloxFetchOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await FetchAsync(Endpoint, BuildQuery(options ?? new ScriptBloxFetchOptions()), cancellationToken)
                .ConfigureAwait(false);
        }

        public static async Task<ScriptData[]> SearchSCData(
            string query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetSCData(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var options = new ScriptBloxFetchOptions { Max = 20, SortBy = "accuracy", Order = "desc" };
            var searchQuery = "q=" + Uri.EscapeDataString(query.Trim())
                + "&page=1&max=" + options.Max.ToString(CultureInfo.InvariantCulture)
                + "&sortBy=accuracy&order=desc&strict=false";
            return await FetchAsync(Endpoint.Replace("/fetch", "/search"), "?" + searchQuery, cancellationToken)
                .ConfigureAwait(false);
        }

        private static async Task<ScriptData[]> FetchAsync(
            string endpoint,
            string query,
            CancellationToken cancellationToken)
        {
            using (var response = await Client.GetAsync(endpoint + query, cancellationToken).ConfigureAwait(false))
            {
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"ScriptBlox returned {(int)response.StatusCode} ({response.ReasonPhrase}): {body}");
                }

                JObject payload;
                try
                {
                    payload = JObject.Parse(body);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("ScriptBlox returned invalid JSON.", ex);
                }

                var message = payload.Value<string>("message");
                var result = payload["result"] as JObject;
                if (result == null)
                {
                    throw new InvalidOperationException(
                        string.IsNullOrWhiteSpace(message)
                            ? "ScriptBlox response did not contain a result."
                            : "ScriptBlox error: " + message);
                }

                var scripts = result["scripts"] as JArray;
                if (scripts == null)
                {
                    throw new InvalidOperationException("ScriptBlox response did not contain a scripts list.");
                }

                return scripts.OfType<JObject>().Select(ParseScript).ToArray();
            }
        }

        private static HttpClient CreateClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(20)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Babis-W Script Hub");
            return client;
        }

        private static string BuildQuery(ScriptBloxFetchOptions options)
        {
            var values = new List<string>
            {
                "page=" + Math.Max(1, options.Page).ToString(CultureInfo.InvariantCulture),
                "max=" + Math.Min(20, Math.Max(1, options.Max)).ToString(CultureInfo.InvariantCulture),
                "sortBy=" + Uri.EscapeDataString(string.IsNullOrWhiteSpace(options.SortBy) ? "updatedAt" : options.SortBy),
                "order=" + Uri.EscapeDataString(string.IsNullOrWhiteSpace(options.Order) ? "desc" : options.Order)
            };

            Add(values, "exclude", options.Exclude);
            Add(values, "mode", options.Mode);
            Add(values, "patched", options.Patched);
            Add(values, "key", options.Key);
            Add(values, "universal", options.Universal);
            Add(values, "verified", options.Verified);
            Add(values, "owner", options.Owner);
            Add(values, "placeId", options.PlaceId);
            return "?" + string.Join("&", values);
        }

        private static void Add(List<string> values, string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                values.Add(name + "=" + Uri.EscapeDataString(value));
            }
        }

        private static void Add(List<string> values, string name, bool? value)
        {
            if (value.HasValue)
            {
                values.Add(name + "=" + (value.Value ? "1" : "0"));
            }
        }

        private static void Add(List<string> values, string name, long? value)
        {
            if (value.HasValue)
            {
                values.Add(name + "=" + value.Value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static ScriptData ParseScript(JObject script)
        {
            var game = script["game"] as JObject;
            return new ScriptData
            {
                Id = script.Value<string>("_id") ?? string.Empty,
                Title = script.Value<string>("title") ?? "Untitled script",
                Script = script.Value<string>("script") ?? string.Empty,
                Desc = game?.Value<string>("name") ?? "Universal script",
                Credits = "ScriptBlox" + (script.Value<bool?>("verified") == true ? " - Verified" : string.Empty),
                ImageURL = NormalizeUrl(script.Value<string>("image")),
                Slug = script.Value<string>("slug") ?? string.Empty,
                GameName = game?.Value<string>("name") ?? string.Empty,
                Verified = script.Value<bool?>("verified") == true,
                HasKey = script.Value<bool?>("key") == true,
                IsUniversal = script.Value<bool?>("isUniversal") == true,
                IsPatched = script.Value<bool?>("isPatched") == true,
                Views = script.Value<int?>("views") ?? 0,
                ScriptType = script.Value<string>("scriptType") ?? string.Empty,
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
                : ScriptBloxBaseUrl + (url.StartsWith("/") ? url : "/" + url);
        }
    }
}
