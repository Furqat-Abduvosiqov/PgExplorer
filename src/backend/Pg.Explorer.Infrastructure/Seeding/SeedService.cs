using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pg.Explorer.Domain.ConnectionConfigurations;
using Pg.Explorer.Domain.Queries.Entities;
using Pg.Explorer.Infrastructure.Database;
using Pg.Explorer.Shared.Encryptions;

namespace Pg.Explorer.Infrastructure.Seeding;

public class SeedService
{
    private readonly PgExplorerDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<SeedService> _logger;

    public SeedService(ILogger<SeedService> logger, PgExplorerDbContext context, IEncryptionService encryptionService)
    {
        _logger = logger;
        _context = context;
        _encryptionService = encryptionService;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding started");

        await _context.Database.MigrateAsync(cancellationToken);

        if (!await _context.ConnectionConfigs.AnyAsync(cancellationToken))
        {
            var defaultConnection = new ConnectionConfig
            {
                Name = "Localhost",
                Host = "localhost",
                Port = 5432,
                DatabaseName = "postgres",
                Username = "postgres",
                EncryptedPassword = _encryptionService.EncryptPassword("password"),
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _context.ConnectionConfigs.AddAsync(defaultConnection, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded default connection 'Localhost'.");
        }

        if (!await _context.Queries.AnyAsync(cancellationToken))
        {
            var connectionId = await _context.ConnectionConfigs
                .OrderBy(c => c.Id)
                .Select(c => c.Id)
                .FirstAsync(cancellationToken);

            var sampleQuery = new QueryEntity
            {
                Id = Guid.NewGuid(),
                ConnectionId = connectionId,
                QueryBody = "SELECT version();",
                QueryType = QueryType.Read,
                ExecutedAt = DateTimeOffset.UtcNow,
                ExecutionTime = TimeSpan.Zero,
                QueryStatus = QueryStatus.Success,
                AffectedRows = 0
            };

            await _context.Queries.AddAsync(sampleQuery, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded sample query 'SELECT version();'.");
        }

        _logger.LogInformation("Seeding completed");
    }
}