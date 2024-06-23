using LinqToDB;
using MailHeap.Service;
using MailHeap.Service.Forwarding;
using MailHeap.Service.Ingress;
using MailHeap.Service.Persistence;
using MailHeap.Service.Rules;
using MailHeap.Service.Settings;
using Microsoft.Extensions.Configuration.Json;
using SmtpServer.Storage;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", true);
builder.Services.Configure<MailHeapSettingsSection>(builder.Configuration.GetSection("MailHeap"));
builder.Services.AddSingleton(new DataOptions().UseSQLite(builder.Configuration.GetConnectionString("dbConnection") ?? throw new InvalidOperationException("Connection string configuration missing")));
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddSingleton<MailHeapSettings>();
builder.Services.AddSingleton<SmtpServerHost>();
builder.Services.AddSingleton<IMailboxFilter, MailboxFilterAdapter>();
builder.Services.AddSingleton<IMessageStore, MessageStoreAdapter>();
builder.Services.AddSingleton<IMailStorage, DatabaseStorage>();
builder.Services.AddSingleton<IDecisionEngine, SimpleDecisionEngine>();
builder.Services.AddSingleton<IMailProcessor, MailProcessor>();
builder.Services.AddSingleton<IMailSender, SmtpEmailSender>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<MailProcessingWorker>();

var host = builder.Build();
host.Run();