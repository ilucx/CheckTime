using System;
using System.Threading.Tasks;
using CheckTime.Functions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace CheckTime.Functions.Functions
{
    public static class TriggerConsolidate
    {
        [FunctionName(nameof(TriggerConsolidateFunction))]
        public static async Task TriggerConsolidateFunction(
        [TimerTrigger("0 */2 * * * *")] TimerInfo myTimer,
        [Table("CheckStructure", Connection = "AzureWebJobsStorage")]CloudTable checkStructureTable, 
        ILogger log)
        {
            // CheckEntity = Tabla 1
            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<CheckEntity> query = new TableQuery<CheckEntity>().Where(filter);
            TableQuerySegment<CheckEntity> allCheckEntity = await checkStructureTable.ExecuteQuerySegmentedAsync(query, null);

            /*//CheckConsolidateEntity = Tabla 2
            TableQuery<CheckConsolidateEntity> queryConsolidate = new TableQuery<CheckConsolidateEntity>();
            TableQuerySegment<CheckConsolidateEntity> allCheckConsolidateEntity = await checkStructureTable.ExecuteQuerySegmentedAsync(queryConsolidate, null);*/
            /*
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
            }*/
        }
    }
}
