using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace OrderFood_SW.Helper;
public static class SessionExtensions
{
    public static void SetObject<T>(this ISession session, string key, T value)
    {
        var json = JsonConvert.SerializeObject(value);
        session.SetString(key, json);
    }

    public static T GetObject<T>(this ISession session, string key)
    {
        var json = session.GetString(key);
        return json == null ? default : JsonConvert.DeserializeObject<T>(json);
    }
}
