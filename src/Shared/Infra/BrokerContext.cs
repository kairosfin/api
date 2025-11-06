using Microsoft.EntityFrameworkCore;

namespace Kairos.Shared.Infra;

public sealed class BrokerContext(DbContextOptions<BrokerContext> options) 
    : DbContext(options);
