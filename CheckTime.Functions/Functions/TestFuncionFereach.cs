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
using CheckTime.Functions.Entities;
using CheckTime.Common.Responses;

namespace CheckTime.Functions.Functions
{
    public static class TestFuncionFereach
    {
        [FunctionName(nameof(FunTest))]
        public static async Task<IActionResult> FunTest(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "otra")] HttpRequest req,
            [Table("CheckConsolidate", Connection = "AzureWebJobsStorage")]
            CloudTable checkStructureTable,
            ILogger log)
        {
            log.LogInformation($"Prepare to consolidate all registers. Time: {DateTime.Now} CONSOLIDATE");

            // CheckEntity = Tabla 1
            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<CheckEntity> query = new TableQuery<CheckEntity>().Where(filter);
            TableQuerySegment<CheckEntity> allCheckEntity = await checkStructureTable.ExecuteQuerySegmentedAsync(query, null);


            //CheckConsolidateEntity = Tabla 2
            TableQuery<CheckConsolidateEntity> queryConsolidate = new TableQuery<CheckConsolidateEntity>();
            TableQuerySegment<CheckConsolidateEntity> allCheckConsolidateEntity = await checkStructureTable.ExecuteQuerySegmentedAsync(queryConsolidate, null);

            bool correctUpdate = false;

            log.LogInformation($"Entrando al primer foreach");
            foreach (CheckEntity item in allCheckEntity)
            {
                log.LogInformation($"Este es el primer if");
                if (!string.IsNullOrEmpty(item.IdClient.ToString()) && item.Type == 0)
                {
                    log.LogInformation($"Este es el segundo foreach");
                    foreach (CheckEntity itemtwo in allCheckEntity)
                    {
                        TimeSpan dateCalculated = (itemtwo.RegisterTime - item.RegisterTime);
                        log.LogInformation($"Este es el tercer foreach");
                        if (itemtwo.IdClient.Equals(item.IdClient) && itemtwo.Type == 1)
                        {
                            log.LogInformation($"Este es el IDRowKey, {item.RowKey}, {itemtwo.RowKey}");

                            CheckEntity check = new CheckEntity
                            {
                                IdClient = itemtwo.IdClient,
                                RegisterTime = Convert.ToDateTime(dateCalculated.ToString()),
                                Type = itemtwo.Type,
                                Consolidated = true,
                                PartitionKey = "WORKINGTIME",
                                RowKey = itemtwo.RowKey,
                                ETag = "*"
                            };

                            log.LogInformation($"Este es el cálculo, {dateCalculated}");
                            TableOperation updateCheckEntity = TableOperation.Replace(check);
                            await checkStructureTable.ExecuteAsync(updateCheckEntity);
                            correctUpdate = true;
                        }

                        log.LogInformation($"He estado aquí, {item.RowKey}");
                        if (correctUpdate == true)
                        {
                            CheckEntity check = new CheckEntity
                            {
                                IdClient = item.IdClient,
                                RegisterTime = Convert.ToDateTime(dateCalculated.ToString()),
                                Type = item.Type,
                                Consolidated = true,
                                PartitionKey = "WORKINGTIME",
                                RowKey = item.RowKey,
                                ETag = "*"
                            };
                            TableOperation updateCheckEntity = TableOperation.Replace(check);
                            await checkStructureTable.ExecuteAsync(updateCheckEntity);
                        }
                    }
                }
            }

            return new OkObjectResult(new ResponseCheckTime
            {
                Message = "Table",
                Results = allCheckEntity
            });
        }
    }
}
