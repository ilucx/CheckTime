using System;
using System.Threading.Tasks;
using CheckTime.Functions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using CheckTime.Common.Models;
using CheckTime.Common.Responses;
using CheckTime.Functions.Entities;

namespace CheckTime.Functions.Functions
{
    public static class TriggerConsolidate
    {
        [FunctionName(nameof(TriggerConsolidateFunction))]
        public static async Task TriggerConsolidateFunction(
        [TimerTrigger("0 */2 * * * *")] TimerInfo myTimer,
        [Table("CheckStructure", Connection = "AzureWebJobsStorage")] CloudTable checkStructureTable,
        [Table("CheckStructureConsolidate", Connection = "AzureWebJobsStorage")] CloudTable checkConsolidateStructureTable,
        ILogger log)
        {
            log.LogInformation($"Prepare to consolidate all registers. Time: {DateTime.Now}");

            // CheckEntity
            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<CheckEntity> query = new TableQuery<CheckEntity>().Where(filter);
            TableQuerySegment<CheckEntity> allCheckEntity = await checkStructureTable.ExecuteQuerySegmentedAsync(query, null);

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
                                RegisterTime = itemtwo.RegisterTime,
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

                            //CheckConsolidateEntity
                            string filterConsolidate = TableQuery.GenerateFilterConditionForInt("IdClient", QueryComparisons.Equal, item.IdClient);
                            TableQuery<CheckConsolidateEntity> queryConsolidate = new TableQuery<CheckConsolidateEntity>().Where(filterConsolidate);
                            TableQuerySegment<CheckConsolidateEntity> allCheckConsolidateEntity = await checkConsolidateStructureTable.ExecuteQuerySegmentedAsync(queryConsolidate, null);

                            if (allCheckConsolidateEntity == null || allCheckConsolidateEntity.Results.Count.Equals(0))
                            {
                                string tem = item.RegisterTime.ToShortDateString();
                                DateTime tempTwo = Convert.ToDateTime(tem);
                                CheckConsolidateEntity checkConsolidateInsert = new CheckConsolidateEntity
                                {
                                    IdClient = item.IdClient,
                                    DateClient = item.RegisterTime,
                                    MinWorked = timeCalculated.TotalMinutes,
                                    PartitionKey = "CHECKCONSOLIDATE",
                                    RowKey = Guid.NewGuid().ToString(),
                                    ETag = "*"
                                };
                                TableOperation insertCheckConsolidate = TableOperation.Insert(checkConsolidateInsert);
                                await checkConsolidateStructureTable.ExecuteAsync(insertCheckConsolidate);
                            }
                            else
                            {
                                foreach (CheckConsolidateEntity itemConsolidate in allCheckConsolidateEntity)
                                {
                                    CheckConsolidateEntity checkConsolidateReplace = new CheckConsolidateEntity
                                    {
                                        IdClient = itemConsolidate.IdClient,
                                        DateClient = itemConsolidate.DateClient.Date,
                                        MinWorked = (double)(itemConsolidate.MinWorked + timeCalculated.TotalMinutes),
                                        PartitionKey = "CHECKCONSOLIDATE",
                                        RowKey = itemConsolidate.RowKey,
                                        ETag = "*"
                                    };

                                    TableOperation replaceCheckConsolidate = TableOperation.Replace(checkConsolidateReplace);
                                    await checkConsolidateStructureTable.ExecuteAsync(replaceCheckConsolidate);
                                }
                            }

                        }
                    }
                }
            }
        }
    }
}
