using LinqToDB;
using LinqToDB.Data;

namespace MailHeap.Service.Persistence.Model;

public class DbConnection(
    DataOptions dbConnectionOptions
) : DataConnection(dbConnectionOptions)
{
    public ITable<EmailMessage> EmailMessages => this.GetTable<EmailMessage>();
}