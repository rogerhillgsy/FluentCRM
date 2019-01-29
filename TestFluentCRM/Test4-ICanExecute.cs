using System;
using System.Diagnostics;
using FakeXrmEasy;
using FluentCRM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace TestFluentCRM
{
    [TestClass]
    public class Test4_ICanExecute
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
        public void TestMethod1()
        {
        }


        [TestMethod]
        public void TestJoin1()
        {
            var message = string.Empty;
            FluentAccount.Account(_orgService).Where("name").Equals("Account2").Trace(s => Debug.WriteLine(s))
                .Join<FluentContact>(c => c.UseAttribute((string n) =>
                {
                    Debug.WriteLine($"Contact name is {n}");
                    message = "Join succeeded";
                }, "firstname")).UseAttribute((string a) => Debug.Write($"Account Name is {a}"), "name").Execute();

            Assert.IsTrue(!string.IsNullOrWhiteSpace(message));
        }
    }
}
