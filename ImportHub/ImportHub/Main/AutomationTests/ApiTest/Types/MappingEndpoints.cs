using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Newtonsoft.Json;
using Paycor.Import.ImportHubTest.Common;


namespace Paycor.Import.ImportHubTest.ApiTest.Types
{
    public class MappingEndpoints
    {
        [JsonProperty(PropertyName = "post")]
        public string Post { get; set; }

        [JsonProperty(PropertyName = "put")]
        public string Put { get; set; }

        [JsonProperty(PropertyName = "delete")]
        public string Delete { get; set; }

        [JsonProperty(PropertyName = "patch")]
        public string Patch { get; set; }

        public Uri GetUriByVerb(Verb verb)
        {
            switch (verb)
            {
                case Verb.Post:
                    return new Uri(Post);
                case Verb.Put:
                    return new Uri(Put);
                case Verb.Delete:
                    return new Uri(Delete);
                case Verb.Patch:
                    return new Uri(Patch);
                default:
                    return null;
            }
        }

        bool CompareEndpoint(Verb verb, string expected)
        {
            return string.Compare(GetUriByVerb(verb).ToString(), expected, StringComparison.OrdinalIgnoreCase) == 0 ;
        }

        public bool Validate()
        {
            List<bool> result = new List<bool>();
            result.Add(!string.IsNullOrEmpty(Post) && (new Uri(Post)).GetLeftPart(UriPartial.Authority) == ConfigurationManager.AppSettings["BaseURL"]);
            result.Add(string.IsNullOrEmpty(Put) && (new Uri(Put)).GetLeftPart(UriPartial.Authority) == ConfigurationManager.AppSettings["BaseURL"]);
            result.Add(string.IsNullOrEmpty(Delete) && (new Uri(Delete)).GetLeftPart(UriPartial.Authority) == ConfigurationManager.AppSettings["BaseURL"]);
            result.Add(string.IsNullOrEmpty(Patch) && (new Uri(Patch)).GetLeftPart(UriPartial.Authority) == ConfigurationManager.AppSettings["BaseURL"]);
            return result.All(x => x);
        }
    }
}
