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
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/checktime")] HttpRequest req,
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

        [FunctionName(nameof(UpdateRegisterTime))]
        public static async Task<IActionResult> UpdateRegisterTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/checktime/{IdClient}")] HttpRequest req,
            [Table("CheckStructure", Connection = "AzureWebJobsStorage")] CloudTable checkStructureTable,
            string IdClient,
            ILogger log)
        {
            log.LogInformation($"Prepare an update for IdClient: {IdClient} for CheckTime Table");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            CheckStructure checkStructure = JsonConvert.DeserializeObject<CheckStructure>(requestBody);

            //Validate IdClient in CheckTime table.
            TableOperation findOperation = TableOperation.Retrieve<CheckEntity>("CHECKTIME", IdClient);
            TableResult findIdClientResult = await checkStructureTable.ExecuteAsync(findOperation);

            if (findIdClientResult.Result == null)
            {
                return new BadRequestObjectResult(new ResponseCheckTime
                {
                    Message = $"The user with id:{checkStructure.IdClient} not found in CheckTime table."
                });
            }

            //Validate registeredTime
            CheckEntity checkEntity = (CheckEntity)findIdClientResult.Result;
            checkEntity.RegisterTime = checkStructure.RegisterTime;
            checkEntity.Type = checkStructure.Type;

            if (string.IsNullOrEmpty(checkStructure.RegisterTime.ToString()))
            {
                return new BadRequestObjectResult(new ResponseCheckTime
                {
                    Message = "The request must have registeredTime YYYY-MM-DD HH:MM:SS"
                });
            }

            //Validate type
            if (string.IsNullOrEmpty(checkStructure.Type.ToString()))
            {
                return new BadRequestObjectResult(new ResponseCheckTime
                {
                    Message = "The request must have type"
                });
            }

            TableOperation replaceOperation = TableOperation.Replace(checkEntity);
            await checkStructureTable.ExecuteAsync(replaceOperation);

            log.LogInformation($"Update the register {IdClient} in table.");

            return new OkObjectResult( new ResponseCheckTime 
            {
                IdClient = checkEntity.IdClient,
                Message = "The register was ejecuted sucessfull"
            });
        }

        [FunctionName(nameof(GetAllRegisters))]
        public static async Task<IActionResult> GetAllRegisters(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/checktime")] HttpRequest req,
            [Table("CheckStructure", Connection = "AzureWebJobsStorage")] CloudTable checkStructureTable,
            ILogger log)
        {
            log.LogInformation("Prepare to get all registers.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            CheckStructure checkStructure = JsonConvert.DeserializeObject<CheckStructure>(requestBody);

            TableQuery<CheckEntity> query = new TableQuery<CheckEntity>();
            TableQuerySegment<CheckEntity> allRegisters = await checkStructureTable.ExecuteQuerySegmentedAsync(query, null);

            log.LogInformation("All registers ready to return.");

            return new OkObjectResult(new ResponseCheckTime
            {
                Message = "Retrieved all register to checktime table.",
                Results = allRegisters
            });
        }

        [FunctionName(nameof(GetRegisterById))]
        public static  IActionResult GetRegisterById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/checktime/{id}")] HttpRequest req,
            [Table("CheckStructure","CHECKTIME","{id}", Connection = "AzureWebJobsStorage")] CheckEntity checkEntity,
            string id,
            ILogger log)
        {
            log.LogInformation($"Prepare to get all register by id: {id}");

            if (checkEntity==null)
            {
                return new BadRequestObjectResult(new ResponseCheckTime
                {
                    Message =$"Id:{id} is not fount in Checktime table."
                });
            }

            log.LogInformation($"Returning register by id: {id}");

            return new OkObjectResult(new ResponseCheckTime
            {
                Message = $"Retrieved all register to checktime table by id:{id}.",
                Results = checkEntity
            });
        }

        [FunctionName(nameof(DeleteRegisterById))]
        public static async Task<IActionResult> DeleteRegisterById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/checktime/{id}")] HttpRequest req,
            [Table("CheckStructure", Connection = "AzureWebJobsStorage")] CloudTable checkStructureTable,
            [Table("CheckStructure", "CHECKTIME", "{id}", Connection = "AzureWebJobsStorage")] CheckEntity checkEntity,
            string id,
            ILogger log)
        {
            log.LogInformation($"Prepare to delete register by id: {id}");

            if (checkEntity == null)
            {
                return new BadRequestObjectResult(new ResponseCheckTime
                {
                    Message = $"Id:{id} is not fount in Checktime table."
                });
            }
            await checkStructureTable.ExecuteAsync(TableOperation.Delete(checkEntity));

            log.LogInformation($"Delete register by id: {id}");

            return new OkObjectResult(new ResponseCheckTime
            {
                Message = "The register was deleted successfully",
                Results = checkEntity
            });
        }
    }
    
}
