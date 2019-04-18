using System;
using System.Collections.Generic;
using FakeXrmEasy;
using FluentCRM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace TestFluentCRM
{
    [TestClass]
    public class Test6_ICreateEntity
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
        public void TestService()
        {
            var f = FluentAccount.Account(_orgService);

            var g = f.Create(new Dictionary<string, object>{{"name", "New Account"}});


            var service2 = g.Service;

            Assert.AreSame(_orgService, service2);

        }

        [TestMethod]
        public void TestCreate()
        {
            const string name = "New Account";
            EntityReference requestRef = null;

FluentAccount.Account(_orgService)
    .Create(new Dictionary<string, object>
    {
        {"name", name},
        {"emptyAttribute1", "" },
        {"emptyAttribute2", null },
    })
    .Id((id) => requestRef = id)
    .Create( new Dictionary<string, object>
    {
        {"address1_city", "New York"}
    })
    .Execute();

            Assert.IsNotNull(requestRef);
            Assert.IsNotNull(_context.Data["account"]);
            Assert.IsTrue(_context.Data["account"].ContainsKey(requestRef.Id));

            var newEntity = _context.Data["account"][requestRef.Id];
            Assert.IsTrue(newEntity.Contains("name"));
            Assert.AreEqual(name, newEntity["name"]);
            Assert.IsTrue(newEntity.Contains("address1_city"));
            Assert.AreEqual("New York", newEntity["address1_city"]);

            Assert.IsFalse(newEntity.Contains("emptyAttribute1"));
            Assert.IsFalse(newEntity.Contains("emptyAttribute2"));        }

        [TestMethod]
        public void TestId()
        {
            const string name = "New Account";
            EntityReference requestRef = null;

            FluentAccount.Account(_orgService)
                .Create(new Dictionary<string, object>
                {
                    {"name", name},
                    {"emptyAttribute1", "" },
                    {"emptyAttribute2", null },
                })
                .Id((id) => requestRef = id)
                .Execute();

            Assert.IsNotNull(requestRef);
            Assert.IsNotNull(_context.Data["account"]);
            Assert.IsTrue(_context.Data["account"].ContainsKey(requestRef.Id));
            var newEntity = _context.Data["account"][requestRef.Id];
        }

        [TestMethod]
        public void TestCreateOptionSets()
        {
            const string name = "New Account";
            EntityReference requestRef = null;

            FluentAccount.Account(_context.GetOrganizationService())
                .Create(new Dictionary<string, object>
                {
                    {"name", name},
                })
                .CreateOptionSets(new Dictionary<string, string>
                {
                    {"option1", "12345"},
                    {"option2", "a12345"},
                    {"option3", ""},
                    {"option4", null},
                })
                .Id((id) => requestRef = id)
                .Execute();

            Assert.IsNotNull(requestRef);
            Assert.IsNotNull(_context.Data["account"]);
            Assert.IsTrue(_context.Data["account"].ContainsKey(requestRef.Id));

            var newEntity = _context.Data["account"][requestRef.Id];
            Assert.IsTrue(newEntity.Contains("name"));
            Assert.AreEqual(name, newEntity["name"]);

            Assert.IsTrue(newEntity.Contains("option1"));
            Assert.AreEqual(new OptionSetValue(12345), newEntity["option1"]);
        }
    }
}
