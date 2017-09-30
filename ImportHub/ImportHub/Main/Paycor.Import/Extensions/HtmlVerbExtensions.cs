using System.Collections.Generic;
using Paycor.Import.Messaging;
using System.Net.Http;
using System;
//TODO: No unit tests
namespace Paycor.Import.Extensions
{
    public static class HtmlVerbExtensions
    {
        public static KeyValuePair<HtmlVerb, string> GetEndPointWithVerb(this HtmlVerb verb, IReadOnlyDictionary<HtmlVerb, string> endpoints)
        {
            var apiEndpointWithVerb = new KeyValuePair<HtmlVerb, string>(verb, null);

            if (verb == HtmlVerb.Post)
            {
                apiEndpointWithVerb = new KeyValuePair<HtmlVerb, string>(HtmlVerb.Post, endpoints[HtmlVerb.Post]);
            }

            if (verb == HtmlVerb.Put)
            {
                if (endpoints[HtmlVerb.Patch] != null)
                {
                    apiEndpointWithVerb = new KeyValuePair<HtmlVerb, string>(HtmlVerb.Patch, endpoints[HtmlVerb.Patch]);
                }
                else if (endpoints[HtmlVerb.Put] != null)
                {
                    apiEndpointWithVerb = new KeyValuePair<HtmlVerb, string>(HtmlVerb.Put, endpoints[HtmlVerb.Put]);
                }
            }
            if (verb == HtmlVerb.Delete && endpoints[HtmlVerb.Delete] != null)
            {
                apiEndpointWithVerb = new KeyValuePair<HtmlVerb, string>(HtmlVerb.Delete, endpoints[HtmlVerb.Delete]);
            }
            if (verb == HtmlVerb.Upsert)
            {
                apiEndpointWithVerb = endpoints.GetEndpointsWithVerbForUpsert();
            }
            return apiEndpointWithVerb;
        }

        public static string GetActionFromVerb(this HtmlVerb input)
        {
            switch (input)
            {
                case HtmlVerb.Post:
                    return "Add";
                case HtmlVerb.Patch:
                case HtmlVerb.Put:
                    return "Update";
                case HtmlVerb.Delete:
                    return "Delete";
                case HtmlVerb.Upsert:
                    return "Upsert";
                default:
                    return "Unknown";
            }
        }

        public static HttpMethod ToMethod(this HtmlVerb verb)
        {
            switch (verb)
            {
                case HtmlVerb.Post:
                    return HttpMethod.Post;
                case HtmlVerb.Patch:
                    return new HttpMethod("PATCH");
                case HtmlVerb.Put:
                    return HttpMethod.Put;
                case HtmlVerb.Delete:
                    return HttpMethod.Delete;
                case HtmlVerb.Get:
                    return HttpMethod.Get;
                default:
                    throw new Exception($"Unsupported verb detected in mapping: {verb}");
            }
        }

    }
}
