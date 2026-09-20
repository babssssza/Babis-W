using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BabisW.ScriptHub
{
    // We will declare these 4 stuff first
    public struct ScriptData
    {
        public string Title;
        public string Script;
        public string Desc;
        public string Credits;
        public string ImageURL;

    }
    public static class BabisWSC
    {

        private static readonly WebClient Web = new WebClient();

        // We want to supress this warning just to keep the error list clean
        // should really be ran asynchronously

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public static async Task<ScriptData[]> GetSCData()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        
        {
            // Now we parse the json, making use of Newtonsoft
            var json = Web.DownloadString("https://raw.githubusercontent.com/babssssza/Babis-W/main/UpdateStuff/Scripts.json");
            var arrays = JArray.Parse(json);

            // Then we return each of it
            return arrays.Values<JObject>()
                .Select(wow => new ScriptData
                {
                    Title = wow.Value<string>("title") ?? string.Empty,
                    Credits = wow.Value<string>("credits") ?? string.Empty,
                    Desc = wow.Value<string>("desc") ?? string.Empty,
                    Script = wow.Value<string>("script") ?? string.Empty,
                    ImageURL = wow.Value<string>("imgurl") ?? string.Empty
                }).ToArray();
        }
    }
}
