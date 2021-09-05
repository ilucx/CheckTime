using CheckTime.Common.Responses;
using CheckTime.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace CheckTime.Functions.Functions
{
    public static class TestFuncionFereach
    {
        [FunctionName(nameof(FunTest))]
        public static async Task<IActionResult> FunTest(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "otra")] HttpRequest req,
            [Table("CheckStructure", Connection = "AzureWebJobsStorage")] CloudTable checkStructureTable,
            [Table("CheckConsolidate", Connection = "AzureWebJobsStorage")] CloudTable checkConsolidateStructureTable,
            ILogger log)
        {
            log.LogInformation($"Prepare to consolidate all registers. Time: {DateTime.Now}");

            // CheckEntity = Tabla 1
            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<CheckEntity> query = new TableQuery<CheckEntity>().Where(filter);
            TableQuerySegment<CheckEntity> allCheckEntity = await checkStructureTable.ExecuteQuerySegmentedAsync(query, null);

            //CheckConsolidateEntity = Tabla 2
            TableQuery<CheckConsolidateEntity> queryConsolidate = new TableQuery<CheckConsolidateEntity>();
            TableQuerySegment<CheckConsolidateEntity> allCheckConsolidateEntity = await checkConsolidateStructureTable.ExecuteQuerySegmentedAsync(queryConsolidate, null);

            foreach (CheckEntity item in allCheckEntity)
            {
                if (!string.IsNullOrEmpty(item.IdClient.ToString()) && item.Type == 0)
                {
                    foreach (CheckEntity itemtwo in allCheckEntity)
                    {
                        TimeSpan timeCalculated = (itemtwo.RegisterTime - item.RegisterTime);

                        if (item.IdClient == itemtwo.IdClient && itemtwo.Type == 1)
                        {
                            log.LogInformation("Prepare to update CheckTime Table");

                            CheckEntity check = new CheckEntity
                            {
                                IdClient = itemtwo.IdClient,
                                RegisterTime = itemtwo.RegisterTime, //Convert.ToDateTime(dateCalculated.ToString()),
                                Type = itemtwo.Type,
                                Consolidated = true,
                                PartitionKey = itemtwo.PartitionKey,
                                RowKey = itemtwo.RowKey,
                                ETag = "*"
                            };

                            CheckEntity checkTwo = new CheckEntity
                            {
                                IdClient = item.IdClient,
                                RegisterTime = item.RegisterTime,
                                Type = item.Type,
                                Consolidated = true,
                                PartitionKey = item.PartitionKey,
                                RowKey = item.RowKey,
                                ETag = "*"
                            };

                            TableOperation updateCheckEntity = TableOperation.Replace(check);
                            await checkStructureTable.ExecuteAsync(updateCheckEntity);

                            TableOperation updateCheckEntityTwo = TableOperation.Replace(checkTwo);
                            await checkStructureTable.ExecuteAsync(updateCheckEntityTwo);

                            log.LogInformation("Prepare to update Consolidate Table");

                            CheckConsolidateEntity checkConsolidate = new CheckConsolidateEntity
                            {
                                IdClient = item.IdClient,
                                DateClient = item.RegisterTime,
                                MinWorked = timeCalculated.TotalMinutes,
                                PartitionKey = "CHECKCONSOLIDATE",
                                RowKey = Guid.NewGuid().ToString(),
                                ETag = "*"
                            };

                            TableOperation insertCheckConsolidate = TableOperation.Insert(checkConsolidate);
                            await checkConsolidateStructureTable.ExecuteAsync(insertCheckConsolidate);

                            foreach (CheckConsolidateEntity itemConsolidate in allCheckConsolidateEntity)
                            {
                                log.LogInformation("Actualizando consolidado segunda tabla");
                                if (itemConsolidate.IdClient == item.IdClient)
                                {
                                    CheckConsolidateEntity checkConsolidateFor = new CheckConsolidateEntity
                                    {
                                        IdClient = itemConsolidate.IdClient,
                                        DateClient = itemConsolidate.DateClient,
                                        MinWorked = itemConsolidate.MinWorked + timeCalculated.TotalMinutes,
                                        PartitionKey = itemConsolidate.PartitionKey,
                                        RowKey = itemConsolidate.RowKey,
                                        ETag = "*"
                                    };

                                    TableOperation insertConsolidate = TableOperation.Replace(checkConsolidateFor);
                                    await checkConsolidateStructureTable.ExecuteAsync(insertConsolidate);
                                }
                                else
                                {
                                    CheckConsolidateEntity checkConsolidateFor = new CheckConsolidateEntity
                                    {
                                        IdClient = item.IdClient,
                                        DateClient = item.RegisterTime,
                                        MinWorked = timeCalculated.TotalMinutes,
                                        PartitionKey = "CHECKCONSOLIDATE",
                                        RowKey = Guid.NewGuid().ToString(),
                                        ETag = "*"
                                    };

                                    TableOperation insertConsolidate = TableOperation.Insert(checkConsolidateFor);
                                    await checkConsolidateStructureTable.ExecuteAsync(insertConsolidate);
                                }
                            }
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
