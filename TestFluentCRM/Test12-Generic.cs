using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FakeItEasy;
using FakeXrmEasy;
using FluentCRM;
using FluentCRM.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace TestFluentCRM
{
    [TestClass]
    public class Test12_Generic
    {
        private XrmFakedContext _context;
        private IOrganizationService _orgService;

        [TestInitialize]
        public void SetUp()
        {
            _context = TestUtilities.TestContext1();
            _orgService = _context.GetOrganizationService();
        }

        /// <summary>
        /// Test Generic FluentCRM access class.
        /// </summary>
        [TestMethod]
        public void TestGeneric()
        {
            var myOwnEntity = new Entity("my_entity") { Id = Guid.NewGuid(), ["name"] = "Hell world" };
            var context = new XrmFakedContext();
            context.Initialize(myOwnEntity);
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();

            FluentCRM.FluentEntity.Entity("my_entity").All()
                .UseAttribute((string s) =>
                {
                    Assert.AreEqual("Hello World", s);
                }, "my_name")
                .Count( c => Assert.AreEqual(1,c))
                .Execute();
        }
    }
}
