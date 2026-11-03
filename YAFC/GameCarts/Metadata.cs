using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace YAFC.GameCarts
{
    [JsonSerializable(typeof(Metadata))]
    internal partial class MetadataJsonContext : JsonSerializerContext
    {
    }

    public sealed partial class Metadata
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("save_size")]
        public int SaveSize { get; set; }

        [JsonIgnore]
        public bool CanSave => SaveSize > 0 && IsValidId(Id);

        public static Metadata Fallback => new()
        {
            Id = string.Empty,
            Name = "YAFC GameCart",
            Description = "Please, make meta.json",
            Version = "0.0.0",
            SaveSize = 0
        };

        private static readonly MetadataJsonContext JsonContext = new(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            MaxDepth = 8
        });

        [GeneratedRegex(@"^[a-z0-9]+([._-][a-z0-9]+)*\z", RegexOptions.CultureInvariant)]
        private static partial Regex IdRegex();

        private static readonly HashSet<string> ReservedNames = new(StringComparer.Ordinal)
        {
            "con", "prn", "aux", "nul",
            "com1", "com2", "com3", "com4", "com5", "com6", "com7", "com8", "com9",
            "lpt1", "lpt2", "lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9"
        };

        public static bool IsValidId(string? id)
        {
            if (string.IsNullOrEmpty(id) || id.Length > 64)
                return false;

            if (!IdRegex().IsMatch(id))
                return false;

            string stem = id.Split('.')[0];
            return !ReservedNames.Contains(stem);
        }

        public static Metadata Parse(string json)
        {
            Metadata? meta;

            try
            {
                meta = JsonSerializer.Deserialize(json, JsonContext.Metadata);
            }
            catch (JsonException e)
            {
                throw new GameCartException("meta.json is broken: " + e.Message);
            }

            if (meta == null)
                throw new GameCartException("meta.json is empty");

            if (meta.Id.Length > 0 && !IsValidId(meta.Id))
            {
                throw new GameCartException("meta.json: invalid id (allowed: a-z, 0-9, '.', '_', '-', up to 64 chars)");
            }

            if (meta.SaveSize < 0 || meta.SaveSize > SaveRam.MaxSize)
            {
                throw new GameCartException($"meta.json: save_size must be 0..{SaveRam.MaxSize}");
            }

            if (meta.SaveSize > 0 && meta.Id.Length == 0)
            {
                throw new GameCartException("meta.json: save_size requires id");
            }

            return meta;
        }
    }
}