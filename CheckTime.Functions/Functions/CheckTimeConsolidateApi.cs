using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using CheckTime.Common.Responses;
using CheckTime.Functions.Entities;

namespace CheckTime.Functions.Functions
{
    public static class CheckTimeConsolidateApi
    {
        [FunctionName(nameof(GetConsolidateByDate))]
        public static async Task<IActionResult> GetConsolidateByDate(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/consolidate/{dateRequest}")] HttpRequest req,
            [Table("CheckConsolidate", Connection = "AzureWebJobsStorage")]
            CloudTable checkStructureConsolidateTable,
            DateTime dateRequest,
            ILogger log)
        {
            log.LogInformation("Prepare get register by date from CheckConsolidate table");


            TableQuery<CheckConsolidateEntity> query = new TableQuery<CheckConsolidateEntity>().Where(TableQuery.GenerateFilterConditionForDate("Date","eq",dateRequest));
            TableQuerySegment<CheckConsolidateEntity> results = await checkStructureConsolidateTable.ExecuteQuerySegmentedAsync(query,null);

            if (results == null)
            {
                return new BadRequestObjectResult( new ResponseConsolidated{
                    Message = "Object returning null."
                });
            }

            log.LogInformation("Getting results by date from ChecConsolidate table");

            return new OkObjectResult(new ResponseConsolidated
            {
                Message = "",
                Result = ""
            });
        }
    }
}
