using Horizon.Events.Domain;
using Microsoft.EntityFrameworkCore;

namespace Horizon.Events.Persistence;

// Conexión a la base de datos PostgreSQL (horizon_events)
public class EventsDbContext : DbContext
{
    public EventsDbContext(DbContextOptions<EventsDbContext> options) : base(options) { }

    // Definimos la tabla Events en la base de datos
    public DbSet<Event> Events => Set<Event>();
}