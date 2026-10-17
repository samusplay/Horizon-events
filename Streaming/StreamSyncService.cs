using Horizon.Events.Domain;

namespace Horizon.Events.Streaming;

//cada pocos segundos le pregunta al media server qué se está transmitiendo y actualiza el estado de los eventos
internal class StreamSyncService(
    IServiceScopeFactory scopeFactory,
    MediaServerStats mediaServer,
    ILogger<StreamSyncService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    //tiempo sin señal antes de dar el evento por finalizado (cubre cortes de red cortos)
    private static readonly TimeSpan Grace = TimeSpan.FromSeconds(30);

    //evento Live -> desde cuándo no se ve su señal
    private readonly Dictionary<Guid, DateTime> missingSince = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var timer = new PeriodicTimer(Interval);
            do
            {
                try
                {
                    await SyncAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    //nginx caído o sin responder: no se toca ningún estado
                    logger.LogWarning("No se pudo sincronizar las transmisiones: {Message}", ex.Message);
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            //la aplicación se está apagando
        }
    }

    private async Task SyncAsync(CancellationToken ct)
    {
        //si el media server no responde esto lanza excepción y no se cambia nada
        var publishing = await mediaServer.GetPublishingEventIdsAsync(ct);

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventsDbContext>();

        //Scheduled -> Live: apenas hay señal
        var publishingIds = publishing.ToArray();
        if (publishingIds.Length > 0)
        {
            var started = await db.Events
                .Where(e => e.Status == EventStatus.Scheduled && publishingIds.Contains(e.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.Status, EventStatus.Live), ct);

            if (started > 0) logger.LogInformation("{Count} evento(s) pasaron a Live", started);
        }

        //Live -> Finished: cuando la señal lleva Grace sin aparecer
        var liveIds = await db.Events
            .Where(e => e.Status == EventStatus.Live)
            .Select(e => e.Id)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;

        foreach (var id in liveIds)
        {
            if (publishing.Contains(id)) missingSince.Remove(id);
            else missingSince.TryAdd(id, now);
        }

        var expired = liveIds
            .Where(id => missingSince.TryGetValue(id, out var since) && now - since >= Grace)
            .ToArray();

        if (expired.Length > 0)
        {
            var finished = await db.Events
                .Where(e => e.Status == EventStatus.Live && expired.Contains(e.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.Status, EventStatus.Finished), ct);

            if (finished > 0) logger.LogInformation("{Count} evento(s) finalizaron por falta de señal", finished);
        }

        //se olvidan los eventos que ya no están en Live
        foreach (var id in missingSince.Keys.Except(liveIds).ToList())
            missingSince.Remove(id);
    }
}