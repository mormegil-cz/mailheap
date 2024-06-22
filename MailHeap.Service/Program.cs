using MailHeap.Service;
using MailHeap.Service.Ingress;
using MailHeap.Service.Persistence;
using MailHeap.Service.Rules;
using MailHeap.Service.Settings;
using SmtpServer.Storage;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<MailHeapSettingsSection>(builder.Configuration.GetSection("MailHeap"));
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<MailHeapSettings>();
builder.Services.AddSingleton<SmtpServerHost>();
builder.Services.AddSingleton<IMailboxFilter, MailboxFilterAdapter>();
builder.Services.AddSingleton<IMessageStore, MessageStoreAdapter>();
builder.Services.AddSingleton<IMailStorage, SqliteStorage>();
builder.Services.AddSingleton<IDecisionEngine, SimpleDecisionEngine>();

var host = builder.Build();
host.Run();