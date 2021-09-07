using CheckTime.Functions.Functions;
using CheckTime.Test.Helpers;
using CheckTime.Tests.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CheckTime.Test.Tests
{
    public class TriggerConsolidateTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();
        [Fact]
        public void TriggerConsolidateTest_Should_Log_Message()
        {
            //Arange
            MockCloudTableCheck mockCloudTableCheck = new MockCloudTableCheck(new Uri("http://127.0.0.1:10002/devstoreaccount1/reposts"));
            MockCloudTableCheckConsolidate mockCloudTableCheckConsolidate = new MockCloudTableCheckConsolidate(new Uri("http://127.0.0.1:10002/devstoreaccount1/reposts"));
            ListLogger logger = (ListLogger)TestFactory.CreateLogger();

            //Act
            TriggerConsolidate.TriggerConsolidateFunction(null, mockCloudTableCheck, mockCloudTableCheckConsolidate, logger);
            string message = logger.Logs[0];

            //Assert
            Assert.Contains("Prepare to consolidate all registers", message);
        }
    }
}
