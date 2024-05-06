using Microsoft.AspNetCore.Mvc;
using System;
using System.Dynamic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace Octokit.Bot
{
    [ModelBinder(BinderType = typeof(WebhookEventBinder))]
    public class WebHookEvent
    {
        private JsonNode _payload = null;
        private string _payloadRaw = null;
        private GitHubOption _gitHubOption;

        internal WebHookEvent(GitHubOption gitHubOption)
        {
            _gitHubOption = gitHubOption;
        }

        public string GitHubEvent { get; internal set; }

        public string GitHubDelivery { get; internal set; }

        public string HubSignature { get; internal set; }

        public string PayloadRaw
        {
            get { return _payloadRaw; }
            internal set
            {
                _payloadRaw = value;
                ParsePayload();
            }
        }

        public JsonNode JsonPayload => _payload;

        private void ParsePayload()
        {
            if (_payload == null && PayloadRaw != null)
            {
                _payload = JsonNode.Parse(PayloadRaw);
            }
        }

        public long? GetInstallationId()
        {
            _payload.AsObject().TryGetPropertyValue("installation", out var installation);

            if (installation != null)
            {
                return installation.AsObject()["id"]!.GetValue<long>();
            }

            return null;
        }

        public bool IsMessageAuthenticated
        {
            get
            {
                var key = Encoding.UTF8.GetBytes(_gitHubOption.WebHookSecret);
                var msg = Encoding.UTF8.GetBytes(_payloadRaw);

                using (var hmac = new HMACSHA1(key))
                {
                    var hashValue = hmac.ComputeHash(msg);
                    var calcHashString = "sha1=" + BitConverter.ToString(hashValue).Replace("-", "").ToLowerInvariant();
                    return calcHashString == HubSignature;
                }
            }
        }
    }
}