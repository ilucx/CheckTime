using CheckTime.Common.Models;
using CheckTime.Functions.Entities;
using CheckTime.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CheckTime.Test.Helpers
{
    public class TestFactory
    {
        public static CheckEntity GetCheckEntity()
        {
            return new CheckEntity
            {
                ETag = "*",
                PartitionKey = "CHECKTIME",
                RowKey = Guid.NewGuid().ToString(),
                IdClient = 1,
                RegisterTime = DateTime.UtcNow,
                Type = 0,
                Consolidated = false
            };
        }

        public static CheckConsolidateEntity GetCheckConsolidateEntity()
        {
            return new CheckConsolidateEntity
            {
                ETag = "*",
                PartitionKey = "CHECKCONSOLIDATE",
                RowKey = Guid.NewGuid().ToString(),
                IdClient = 1,
                DateClient = DateTime.UtcNow,
                MinWorked = 300
            };
        }

        //Update registers
        public static DefaultHttpRequest CreateHttpRequest(Guid IdClient, CheckStructure checkStructure)
        {
            string request = JsonConvert.SerializeObject(checkStructure);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateSteamFromString(request),
                Path = $"/{IdClient}",
            };
        }

        // To deleted registers and select
        public static DefaultHttpRequest CreateHttpRequest(Guid IdClient)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{IdClient}",
            };
        }

        //to create a new register
        public static DefaultHttpRequest CreateHttpRequest(CheckStructure checkStructure)
        {
            string request = JsonConvert.SerializeObject(checkStructure);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateSteamFromString(request),
            };
        }

        //get all registers
        public static DefaultHttpRequest CreateHttpRequest()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());
        }

        public static CheckStructure GetCheckStructureRequest()
        {
            return new CheckStructure
            {
                IdClient = 1,
                Type = 0,
            };
        }

        public static Stream GenerateSteamFromString(string stringToConvert)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringToConvert);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;
            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }
            return logger;
        }
    }
}
