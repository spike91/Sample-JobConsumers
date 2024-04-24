# Job Consumer Sample

Configuration was changed a bit: Redis repository is used for saga instead of Postgres.

Run both JobService.Producer and JobService.Service

Navigate to `http://localhost:23074` to post a video to convert.
Navigate to `http://localhost:37590` to stop the bus/application.

Test №1 (stopping the bus):
Start the application (JobService.Producer and JobService.Service)
Add job #1 - http://localhost:23074/ConvertVideo/{path}
Call stop bus endpoint - http://localhost:37590/Stop/Bus
Add job #2 while bus is stopping - http://localhost:23074/ConvertVideo/{path}
Add job #3 when bus is stopped - http://localhost:23074/ConvertVideo/{path}
Call stop application endpoint - http://localhost:37590/Stop/Application

The results should be:
Job #1 cancellation token requested (OK)
Job #2 started with cancellation token requested (Not OK)
Job #2 finished when the bus was already stopped
Job #2 faulted to publish due to stopped send transport
Job #3 was added queued but hasn't been started (OK)

JobService.Service console logs: console_logs_test1.txt

Test №2 (stopping the application):
Start the application (JobService.Producer and JobService.Service)
Add job #1 - http://localhost:23074/ConvertVideo/{path}
Call stop application endpoint - http://localhost:37590/Stop/Application
Add job #2 while application is stopping - http://localhost:23074/ConvertVideo/{path}

Results:
Job #1 cancellation token requested (OK)
Job #2 started with cancellation token requested (Not OK)
Job #2 not finished or cancelled, the application was shut down in the middle of job execution

JobService.Service console logs: console_logs_test2.txt