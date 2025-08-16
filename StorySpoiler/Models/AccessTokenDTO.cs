using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StorySpoiler.Models
{
    internal class AccessTokenDTO
    {
        [JsonPropertyName("userName")]
        public string? Username { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("accessToken")]
        public string? AccessToken { get; set; }
    }
}
