using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Exceptions;
using Bookify.Domain.Abstractions;
using Bookify.Infrastructure.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bookify.Infrastructure
{
    public sealed class ApplicationDbContext : DbContext, IUnitOfWork
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.All,
        };

        public ApplicationDbContext(DbContextOptions options, IDateTimeProvider dateTimeProvider) : base(options)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            /*
             * Raises the DomainEvent. Remember an event is a "fact" something that already happened.
             * When using PublishDomainEvenstAsync()
             * Caveat: 
             * First we are SavingChanges which is Atomic
             * Second we are publishing events which adds another transaction, the domain envent handlers
             * might interact with the DB but can fail during this. It will cause an Exception indicating 
             * the whole transaction failed but IT'S NOT THE CASE. 
             * We can have a more robust solution by using the Out of the Box pattern
             */
            try
            {
                /*
                 * Will be persisting everything in a single transaction which gives us atomic
                 * guarantees because we are using SQL database.
                 * So either all of the outbox messages are persisted together as part of our transaction
                 * or nothing is persisted.
                 */
                AddDomainEventsAsOutboxMessages();

                var result = await base.SaveChangesAsync(cancellationToken);

                return result;

            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyException("Concurrency exception occurred.", ex);
            }
        }

        private void AddDomainEventsAsOutboxMessages()
        {
            var outboxMessages = ChangeTracker
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
                .Select(domainEvent => new OutboxMessage(
                    Guid.NewGuid(),
                    _dateTimeProvider.UtcNow,
                    domainEvent.GetType().Name,
                    JsonConvert.SerializeObject(domainEvent, _jsonSerializerSettings)
                    ))
                .ToList();

            AddRange(outboxMessages);
        }
    }
}
