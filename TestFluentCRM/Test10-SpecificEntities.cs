using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using FakeXrmEasy;
using FluentCRM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

namespace TestFluentCRM
{
    [TestClass]
    public class Test10_SpecificEntities
    {
        private XrmFakedContext _context;
        private IOrganizationService _orgService;

        [TestInitialize]
        public void SetUp()
        {
            _context = TestUtilities.TestContext1();
            _orgService = _context.GetOrganizationService();
        }

        [TestMethod]
        public void TestSystemUser()
        {
            var userId = Guid.NewGuid();
            var context = TestUtilities.TestContext3(new Entity("systemuser") {Id = userId, ["fullname"] = "roger"});
            context.CallerId = new EntityReference("systemuser", userId);
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();

            FluentSystemUser.CurrentUser().UseAttribute((string n) => Assert.AreEqual("roger", n), "fullname")
                .Count((c) => Assert.AreEqual(1, c))
                .Execute();

        }

        [TestMethod]
        public void TestCurrentUserSettings()
        {
            var userId = Guid.NewGuid();
            var context = TestUtilities.TestContext3(new Entity("systemuser") {Id = userId, ["fullname"] = "roger"},
                new Entity("usersettings") {Id = Guid.NewGuid(), ["systemuserid"] = userId, ["numberseparator"] = ","});
            context.CallerId = new EntityReference("systemuser", userId);
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();

            FluentUserSettings.CurrentUserSettings()
                .UseAttribute((string n) => Assert.AreEqual(",", n), "numberseparator")
                .Count((c) => Assert.AreEqual(1, c))
                .Execute();

        }

   
}
}
