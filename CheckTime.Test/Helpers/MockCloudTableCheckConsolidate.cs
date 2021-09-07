using CheckTime.Test.Helpers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckTime.Tests.Helpers
{
    public class MockCloudTableCheckConsolidate : CloudTable
    {
        public MockCloudTableCheckConsolidate(Uri tableAddress) : base(tableAddress)
        {
        }

        public MockCloudTableCheckConsolidate(Uri tableAbsoluteUri, StorageCredentials credentials) : base(tableAbsoluteUri, credentials)
        {
        }

        public MockCloudTableCheckConsolidate(StorageUri tableAddress, StorageCredentials credentials) : base(tableAddress, credentials)
        {
        }

        public override async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            return await Task.FromResult(new TableResult
            {
                HttpStatusCode = 200,
                Result = TestFactory.GetCheckConsolidateEntity()
            });
        }
    }
}
