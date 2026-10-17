using System.Xml.Linq;

namespace Horizon.Events.Streaming;

//lee las estadísticas de nginx-rtmp (XML) para saber quién está transmitiendo
public class MediaServerStats(HttpClient http, IConfiguration config)
{
    //ids de los eventos que tienen una señal publicándose ahora mismo
    public async Task<HashSet<Guid>> GetPublishingEventIdsAsync(CancellationToken ct)
    {
        var url = config["MediaServer:StatUrl"]
            ?? throw new InvalidOperationException("Falta la configuración MediaServer:StatUrl");

        var doc = XDocument.Parse(await http.GetStringAsync(url, ct));

        //una transmisión activa es un <stream> con <publishing/> como hijo directo
        //(el <publishing/> que está dentro de <client> no cuenta)
        return doc.Descendants("application")
            .Where(app => (string?)app.Element("name") == "live")
            .Descendants("stream")
            .Where(stream => stream.Element("publishing") is not null)
            .Select(stream => Guid.TryParse((string?)stream.Element("name"), out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToHashSet();
    }
}