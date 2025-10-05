using Backend.Application.Data;
using Backend.Domain.Users;
using Backend.Domain.Primitive;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence
{
    public class AppDbContext : DbContext, IAppDbContext, IUnitOfWork
    {
        private readonly IPublisher _publisher;

        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options, IPublisher publisher)
            : base(options)
        {
            _publisher = publisher;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken() )
        {
            var domainEvents = ChangeTracker.Entries<Entity>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .SelectMany(e => e.DomainEvents)
                .ToList();
            /*
                Before saving: domain events are just intentions, not facts; failures could roll back changes unintentionally.
                After saving: changes are persisted, events represent actual facts; failures affect only the events, not the saved data.
            */
            var result = await base.SaveChangesAsync(cancellationToken);

            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }

            // Clear domain events from tracked entities after publishing
            foreach (var entry in ChangeTracker.Entries<Entity>())
            {
                entry.Entity.ClearDomainEvents();
            }
            return result;
        }
    }
}
