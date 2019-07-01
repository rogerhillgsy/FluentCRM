using System.Diagnostics;
using System.Linq;
using System.Text;
using FakeXrmEasy;
using FluentCRM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace TestFluentCRM
{

    // Test miscellaneous issues encountered "in the wild"
    [TestClass]
    public class TestIssues
    {
        private XrmFakedContext _context;
        private IOrganizationService _orgService;

        [TestInitialize]
        public void SetUp()
        {
            _context = TestUtilities.TestContext1();
            _orgService = _context.GetOrganizationService();
            FluentCRM.FluentCRM.StaticService = _orgService;
        }
        /// <summary>
        /// Issue where if the value of a string being updated contained formatting characters such as "{}" this caused a FormatException
        /// </summary>
        [TestMethod]
        public void TestUpdateWithFormat()
        {
            var acc1 = _context.Data["account"].Values.Single(n => (string)n["name"] == "Account1");
            Assert.AreEqual("123456", acc1["phone1"]);

            var traceString = new StringBuilder();

            FluentAccount.Account(acc1.Id)
                .Trace((s => traceString.Append(s)))
                .WeakUpdate("phone1", "Some update with format {blah}")
                .Execute();

            Assert.IsTrue(traceString.ToString().Contains(@"Some update with format {blah}"));
            Debug.WriteLine("If we get don't throw a FormatException then we're OK.");
        }
    }
}

