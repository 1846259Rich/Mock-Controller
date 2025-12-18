using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using mock.depart.Controllers;
using mock.depart.Data;
using mock.depart.Models;
using mock.depart.Services;
using Moq;

namespace mock.depart.Controllers.Tests
{
    [TestClass()]
    public class CatsControllerTests
    {
        private Mock<CatsController> _catsController;
        private Mock<CatsService> _catsService;
        private Cat testCat;

        [TestInitialize]
        public void Init()
        {
            _catsService = new Mock<CatsService>();
            testCat = new Cat { Id = 1, Name = "test", CuteLevel = Cuteness.BarelyOk, CatOwner = new CatOwner { Id = "2" } };

            _catsController = new Mock<CatsController>(_catsService.Object) { CallBase = true };
            _catsController.Setup(t => t.UserId).Returns("2");
        }

        [TestMethod()]
        public void DeleteCatTestWork()
        {
            _catsService.Setup(x => x.Get(It.IsAny<int>())).Returns(testCat);
            _catsService.Setup(x => x.Delete(It.IsAny<int>())).Returns(testCat);
            var actionResult = _catsController.Object.DeleteCat(3);
            var result = actionResult.Result as OkObjectResult;
            Assert.IsNotNull(result);

            Cat? cat = result.Value as Cat;

            Assert.IsNotNull(cat);
            Assert.AreEqual(testCat, cat);
        }

        [TestMethod()]
        public void DeleteCatTestFailNoCatFound()
        {
            // No Cat found
            _catsService.Setup(x => x.Get(It.IsAny<int>())).Returns(value: null);

            var actionResult = _catsController.Object.DeleteCat(3);
            var resultNotFound = actionResult.Result as NotFoundResult;

            Assert.AreEqual(resultNotFound.StatusCode, StatusCodes.Status404NotFound);
        }

        [TestMethod()]
        public void DeleteCatTestFailCatIsNotYour()
        {
            // The Cat is not yours
            Cat catNotOwned = new Cat { Id = 1, Name = "test", CuteLevel = Cuteness.BarelyOk, CatOwner = new CatOwner { Id = "3" } };
            _catsService.Setup(x => x.Get(It.IsAny<int>())).Returns(catNotOwned);

            var actionResult = _catsController.Object.DeleteCat(3);
            var resultBadRequest = actionResult.Result as BadRequestObjectResult;

            Assert.AreEqual(resultBadRequest.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual(resultBadRequest.Value, "Cat is not yours");
        }

        [TestMethod()]
        public void DeleteCatTestFailCatIsTooCute()
        {
            // The Cat is too cute
            Cat catTooCute = new Cat { Id = 1, Name = "test", CuteLevel = Cuteness.Amazing, CatOwner = new CatOwner { Id = "2" } };
            _catsService.Setup(x => x.Get(It.IsAny<int>())).Returns(catTooCute);

            var actionResult = _catsController.Object.DeleteCat(3);
            var resultBadRequest = actionResult.Result as BadRequestObjectResult;

            Assert.AreEqual(resultBadRequest.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual(resultBadRequest.Value, "Cat is too cute");
        }
    }
}