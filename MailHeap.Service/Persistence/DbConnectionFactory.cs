using LinqToDB;
using MailHeap.Service.Persistence.Model;

namespace MailHeap.Service.Persistence;

public class DbConnectionFactory(
    DataOptions dbConnectionOptions
)
{
    public DbConnection Create() => new(dbConnectionOptions);
}