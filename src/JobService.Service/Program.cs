using System;
using System.Threading.Tasks;
using JobService.Components;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using NSwag;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddOpenApiDocument(cfg => cfg.PostProcess = d =>
{
    d.Info.Title = "Job Consumer Sample";
    d.Info.Contact = new OpenApiContact
    {
        Name = "Job Consumer Sample using MassTransit"
    };
});

/*AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDbContext<JobServiceSagaDbContext>(optionsBuilder =>
{
    var connectionString = builder.Configuration.GetConnectionString("JobService");

    optionsBuilder.UseNpgsql(connectionString, m =>
    {
        m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
        m.MigrationsHistoryTable($"__{nameof(JobServiceSagaDbContext)}");
    });
});

builder.Services.AddHostedService<MigrationHostedService<JobServiceSagaDbContext>>();*/

builder.Services.AddMassTransit(x =>
{
    x.AddDelayedMessageScheduler();

    x.AddConsumer<ConvertVideoJobConsumer, ConvertVideoJobConsumerDefinition>()
        .Endpoint(e => e.Name = "convert-job-queue");


    x.SetJobConsumerOptions();
    x.AddJobSagaStateMachines(options => options.FinalizeCompleted = false)
    .RedisRepository(r =>
    {
        var redisUrl = "localhost:32768";
        var configuration = new ConfigurationOptions
        {
            EndPoints = { redisUrl ?? string.Empty },
            AbortOnConnectFail = false,
            Password = "redispw"
        };
        r.DatabaseConfiguration(configuration);
    });
    /*x.AddJobSagaStateMachines(options => options.FinalizeCompleted = false)
        .EntityFrameworkRepository(r =>
        {
            r.ExistingDbContext<JobServiceSagaDbContext>();
            r.UsePostgres();
        });*/

    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseDelayedMessageScheduler();

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddOptions<MassTransitHostOptions>()
    .Configure(options =>
    {
        options.WaitUntilStarted = true;
        options.StartTimeout = TimeSpan.FromMinutes(1);
        options.StopTimeout = TimeSpan.FromMinutes(2);
        options.ConsumerStopTimeout = TimeSpan.FromSeconds(90);
    });

builder.Services.AddOptions<HostOptions>()
    .Configure(options => options.ShutdownTimeout = TimeSpan.FromMinutes(3));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseOpenApi();
app.UseSwaggerUi3();

app.UseRouting();
app.UseAuthorization();

static Task HealthCheckResponseWriter(HttpContext context, HealthReport result)
{
    context.Response.ContentType = "application/json";

    return context.Response.WriteAsync(result.ToJsonString());
}

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter
});

app.MapHealthChecks("/health/live", new HealthCheckOptions { ResponseWriter = HealthCheckResponseWriter });

app.MapControllers();

await app.RunAsync();