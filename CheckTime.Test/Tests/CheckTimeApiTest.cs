using CheckTime.Common.Models;
using CheckTime.Functions.Entities;
using CheckTime.Functions.Functions;
using CheckTime.Test.Helpers;
using CheckTime.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CheckTime.Test.Tests
{
    public class CheckTimeApiTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void RegisterTime_Should_Return_200()
        {
            //Arrenge
            MockCloudTableCheck mockCloudTableCheck = new MockCloudTableCheck(new Uri("http://127.0.0.1:10002/devstoreaccount1/reposts"));
            CheckStructure checkStructure = TestFactory.GetCheckStructureRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(checkStructure);

            //Act
            IActionResult reponse = await CheckTimeApi.RegisterTime(request, mockCloudTableCheck, logger);

            //Assert
            OkObjectResult result = (OkObjectResult)reponse;
            Assert.Equal(StatusCodes.Status200OK,result.StatusCode);
        }

        [Fact]
        public async void UpdateRegisterTime_Should_Return_200()
        {
            //Arrenge
            MockCloudTableCheck mockCloudTableCheck = new MockCloudTableCheck(new Uri("http://127.0.0.1:10002/devstoreaccount1/reposts"));
            CheckStructure checkStructure = TestFactory.GetCheckStructureRequest();
            Guid idClient = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(idClient,checkStructure);

            //Act
            IActionResult reponse = await CheckTimeApi.UpdateRegisterTime(request, mockCloudTableCheck, idClient.ToString(),logger);

            //Assert
            OkObjectResult result = (OkObjectResult)reponse;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void GetAllRegisters_Should_Return_200()
        {
            //Arrenge
            MockCloudTableCheck mockCloudTableCheck = new MockCloudTableCheck(new Uri("http://127.0.0.1:10002/devstoreaccount1/reposts"));
            DefaultHttpRequest request = TestFactory.CreateHttpRequest();

            //Act
            IActionResult reponse = await CheckTimeApi.GetAllRegisters(request, mockCloudTableCheck, logger);

            //Assert
            OkObjectResult result = (OkObjectResult)reponse;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public void GetRegisterById_Should_Return_200()
        {
            //Arrenge
            //MockCloudTableCheck mockCloudTableCheck = new MockCloudTableCheck(new Uri("http://127.0.0.1:10002/devstoreaccount1/reposts"));
            Guid IdClient = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(IdClient);
            CheckEntity checkEntity = TestFactory.GetCheckEntity();
            
            //Act
            IActionResult reponse = CheckTimeApi.GetRegisterById(request, checkEntity, IdClient.ToString(),logger);

            //Assert
            OkObjectResult result = (OkObjectResult)reponse;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void DeleteRegisterById_Should_Return_200()
        {
            //Arrenge
            MockCloudTableCheck mockCloudTableCheck = new MockCloudTableCheck(new Uri("http://127.0.0.1:10002/devstoreaccount1/reposts"));
            Guid IdClient = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(IdClient);
            CheckEntity checkEntity = TestFactory.GetCheckEntity();

            //Act
            IActionResult reponse = await CheckTimeApi.DeleteRegisterById(request, mockCloudTableCheck, checkEntity, IdClient.ToString(), logger);

            //Assert
            OkObjectResult result = (OkObjectResult)reponse;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

    }
}
