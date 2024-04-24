namespace JobService.Components
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using MassTransit;
    using Microsoft.Extensions.Logging;


    public class ConvertVideoJobConsumer : IJobConsumer<ConvertVideo>
    {
        readonly ILogger<ConvertVideoJobConsumer> _logger;

        public ConvertVideoJobConsumer(ILogger<ConvertVideoJobConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Run(JobContext<ConvertVideo> context)
        {
            var rng = new Random();
            var variance = TimeSpan.MinValue;
            var stopWatch = Stopwatch.StartNew();

            while (stopWatch.Elapsed.TotalSeconds < 60) // should be less than ConsumerTimeout and StopTimeout
            {
                var cancellation = string.Empty;
                if (context.CancellationToken.IsCancellationRequested)
                    cancellation = " (CancellationRequested)";

                _logger.LogInformation($"Converting Video{cancellation}: {context.Job.GroupId} {context.Job.Path}");

                variance = TimeSpan.FromMilliseconds(rng.Next(1000, 3000));
                await Task.Delay(variance);
            }

            await context.Publish<VideoConverted>(context.Job);

            _logger.LogInformation("Converted Video: {GroupId} {Path}", context.Job.GroupId, context.Job.Path);
        }
    }
}