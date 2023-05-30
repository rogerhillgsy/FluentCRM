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
        
        /// <summary>
        /// Test different generic constructors.
        /// </summary>
        [TestMethod]
        public void TestGeneric2()
        {
            var myOwnEntity = new Entity("my_entity") { Id = Guid.NewGuid(), ["name"] = "Hell world" };
            var context = new XrmFakedContext();
            context.Initialize(myOwnEntity);
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();

            var entity1 = new FluentEntity("my_entity");

            entity1.All()
                .UseAttribute((string s) =>
                {
                    Assert.AreEqual("Hello World", s);
                }, "my_name")
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

            var entity2 = new FluentEntity( context.GetOrganizationService(), "my_entity");

            entity2.All()
                .UseAttribute((string s) =>
                {
                    Assert.AreEqual("Hello World", s);
                }, "my_name")
                .Count(c => Assert.AreEqual(1, c))
                .Execute(); 
            
            var entity3 = new FluentEntity( myOwnEntity.Id, context.GetOrganizationService(), "my_entity");

            entity3.All()
                .UseAttribute((string s) =>
                {
                    Assert.AreEqual("Hello World", s);
                }, "my_name")
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

            var entity4 = new FluentEntity( myOwnEntity.Id, "my_entity");

            entity4.All()
                .UseAttribute((string s) =>
                {
                    Assert.AreEqual("Hello World", s);
                }, "my_name")
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

        }
    }
}
