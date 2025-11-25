using System.Text.Json;

namespace myapp.Utils
{ 
public static class JsonFileLoader
    {
        public static T? Load<T>(string path)
        {
            var txt = File.ReadAllText(path);
            var opts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            return JsonSerializer.Deserialize<T>(txt, opts);
        }
    }
}