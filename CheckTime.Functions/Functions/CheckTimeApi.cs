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
using CheckTime.Common.Models;
using CheckTime.Common.Responses;
using CheckTime.Functions.Entities;

namespace CheckTime.Functions.Functions
{
    public static class CheckTimeApi
    {
        [FunctionName(nameof(RegisterTime))]
        public static async Task<IActionResult> RegisterTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CheckTime")] HttpRequest req,
            [Table("CheckStructure", Connection = "AzureWebJobsStorage")] CloudTable checkStructureTable,
            ILogger log)
        {
            log.LogInformation("Recieved a new register");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            CheckStructure checkStructure = JsonConvert.DeserializeObject<CheckStructure>(requestBody);

            if (string.IsNullOrEmpty(checkStructure?.IdClient.ToString()))
            {
                return new BadRequestObjectResult(new ResponseCheckTime
                {
                    Message = "The request must have a IdClient."
                });
            }

            if (string.IsNullOrEmpty(checkStructure?.Type.ToString()))
            {
                return new BadRequestObjectResult(new ResponseCheckTime
                {
                    Message = "The request must have a type to in(0) or out(1)."
                });
            }

            CheckEntity checkEntity = new CheckEntity
            {
                IdClient = checkStructure.IdClient,
                RegisterTime = DateTime.UtcNow,
                Type = checkStructure.Type,
                Consolidated = false,
                PartitionKey = "CHECKTIME",
                RowKey = Guid.NewGuid().ToString(),
                ETag = "*"
            };

            TableOperation addOperation = TableOperation.Insert(checkEntity);
            await checkStructureTable.ExecuteAsync(addOperation);

            log.LogInformation("New register in table CHECKTIME");

            return new OkObjectResult(new ResponseCheckTime
            {
                IdClient = checkEntity.IdClient,
                RegisteredTime = checkEntity.RegisterTime,
                Message = "New register succefull."
            });
        }
    }
}
