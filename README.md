# Job Consumer Sample

Configuration was changed a bit: Redis repository is used for saga instead of Postgres.

Run both JobService.Producer and JobService.Service

Navigate to `http://localhost:23074` to post a video to convert.<br />
Navigate to `http://localhost:37590` to stop the bus/application.

Test №1 (stopping the bus):<br />
1. Start the application (JobService.Producer and JobService.Service)
2. Add job #1 - `http://localhost:23074/ConvertVideo/{path}`
3. Call stop bus endpoint - `http://localhost:37590/Stop/Bus`
4. Add job #2 while bus is stopping - `http://localhost:23074/ConvertVideo/{path}`
5. Add job #3 when bus is stopped - `http://localhost:23074/ConvertVideo/{path}`
6. Call stop application endpoint - `http://localhost:37590/Stop/Application`

The results:<br />
Job #1 cancellation token requested (OK)<br />
Job #2 started (Not OK)<br />
Job #2 finished when the bus was already stopped<br />
Job #2 faulted to publish due to stopped send transport<br />
Job #3 was added queued but hasn't been started (OK)<br />


Test №2 (stopping the application):<br />
1. Start the application (JobService.Producer and JobService.Service)
2. Add job #1 - `http://localhost:23074/ConvertVideo/{path}`
3. Call stop application endpoint - `http://localhost:37590/Stop/Application`
4. Add job #2 while application is stopping - `http://localhost:23074/ConvertVideo/{path}`

The results:<br />
Job #1 cancellation token requested (OK)<br />
Job #2 started (Not OK)<br />
Job #2 neither finished nor canceled, the application was shut down in the middle of the job execution


JobService.Service console logs: <br />
- [console_logs_test1.txt](https://github.com/spike91/Sample-JobConsumers/blob/main/console_logs_test1.txt)
- [console_logs_test2.txt](https://github.com/spike91/Sample-JobConsumers/blob/main/console_logs_test2.txt)
