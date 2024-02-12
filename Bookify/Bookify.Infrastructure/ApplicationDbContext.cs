using Bookify.Application.Exceptions;
using Bookify.Domain.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure
{
    public sealed class ApplicationDbContext : DbContext, IUnitOfWork
    {
        private readonly IPublisher _publisher;

        public ApplicationDbContext(DbContextOptions options, IPublisher publisher) : base(options)
        {
            _publisher = publisher;

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            /*
             * Caveat: 
             * First we are SavingChanges which is Atomic
             * Second we are publishing events which adds another transaction, the domain envent handlers
             * might interact with the DB but can fail during this. It will cause an Exception indicating 
             * the whole transaction failed but IT'S NOT THE CASE. 
             * We can have a more robust solution by using the Out of the Box pattern
             */
            try
            {
                var result = await base.SaveChangesAsync(cancellationToken);

                // Raises the DomainEvent. Remember an event is a "fact" something that already happened.
                await PublishDomainEventsAsync();

                return result;

            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyException("Concurrency exception occurred.", ex);
            }
        }

        private async Task PublishDomainEventsAsync()
        {
            var domainEvents = ChangeTracker
                // wraps the entity entries implementing Entity class
                .Entries<Entity>()
                .Select(entry => entry.Entity)
                .SelectMany(entity =>
                {
                    // maps the list of events of each entity
                    var domainEvents = entity.GetDomainEvents();

                    /* when we publish domain events, we don't know
                    * what could be happening in the handlers.
                    * There could be another DbContext created, which could 
                    * use same entity and another domain event which causes 
                    * weird behaviors.
                    */
                    entity.ClearDomainEvents();

                    return domainEvents;
                })
                .ToList();

            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent);
            }
        }
    }
}
