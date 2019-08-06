using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Orchestrator
{
    public static class Orchestrator
    {
        public class Input
        {
            public DateTimeOffset MyDateTimeOffset { get; set; }
        }

        [FunctionName("Orchestrator")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context, ILogger log)
        {
            var myDateTimeOffset = context.GetInput<Input>().MyDateTimeOffset;

            if (!context.IsReplaying)
            {
                log.LogError($"Orchestrator received input: {myDateTimeOffset}");
                log.LogInformation("Calling Activity");
            }

            await context.CallActivityAsync("Function2_Hello", myDateTimeOffset);
        }

        [FunctionName("Function2_Hello")]
        public static void SayHello([ActivityTrigger] DateTimeOffset myDateTimeOffset, ILogger log)
        {
            log.LogError($"Activity received value with a different offset: {myDateTimeOffset}.");
        }

        [FunctionName("HttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClientBase starter,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var dateTimeOffset = DateTimeOffset.Parse("2019-08-05T00:00:00.23341-07:00");

            log.LogError($"My trigger is generating a DateTimeOffset of value: {dateTimeOffset}");
            log.LogInformation("Calling orchestrator");

            await starter.StartNewAsync("Orchestrator", new Input { MyDateTimeOffset = dateTimeOffset });

            return new OkResult();
        }
    }
}