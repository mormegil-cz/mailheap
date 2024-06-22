using LinqToDB;
using LinqToDB.Data;

namespace MailHeap.Service.Persistence.Model;

public class DbConnection() : DataConnection("dbConnection")
{
    public ITable<EmailMessage> EmailMessages => this.GetTable<EmailMessage>();
}