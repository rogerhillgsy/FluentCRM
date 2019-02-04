using System;
using System.Diagnostics;
using System.Linq;
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
        public void TestTrace()
        {
            var message = string.Empty;
            var account1 = _context.Data["account"].First().Value;
            var accountId = account1.Id;

            FluentAccount.Account(accountId).UseAttribute((string a) => Debug.Write($"Name is {a}"), "name").Trace(m =>
            {
                message = m;
                Debug.WriteLine(m);
            }).Execute();

            Assert.IsTrue(!string.IsNullOrWhiteSpace(message));
        }

        [TestMethod]
        public void TestTimer()
        {
            var message = string.Empty;
            FluentAccount.Account().Where("name").Equals("Account1").UseAttribute( (string a) => Debug.Write($"Name is {a}"), "name").
                Timer( m =>
            {
                message = m;
                Debug.WriteLine($@"Timer message: {m}");
            }).Execute();

            Assert.IsTrue(!string.IsNullOrWhiteSpace(message));
        }

        [TestMethod]
        public void TestUseAttribute1()
        {
            var message = string.Empty;
            FluentAccount.Account(_orgService).Where("name").Equals("Account1")
                .UseAttribute( (string a) => message = $"Name is {a}", "name")
                .UseAttribute( (string a) => message += "\n Name2 is {a}", "name2").Execute();

            Assert.IsTrue(!string.IsNullOrWhiteSpace(message));
            Assert.IsTrue(message.Contains("Name2"));
        }

        [TestMethod]
        public void TestUseAttribute2()
        {
            var message = string.Empty;
            FluentAccount.Account(_orgService).Where("name").Equals("Account1")
                .UseAttribute( (int a) => message = $"Statecode is {a}", "statecode")
                .UseAttribute( (string a) => message += "\nName2 is {a}", "name4", "name2").Execute();


            Assert.IsTrue(!string.IsNullOrWhiteSpace(message));
            Assert.IsTrue(message.Contains("Name2"));
        }

        [TestMethod]
        public void TestWeakUpdate1()
        {
            var message = string.Empty;
            var acc1 = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account1");
            Assert.AreEqual("123456", acc1[ "phone1"]);

            FluentAccount.Account(acc1.Id, _orgService)
                .Trace( (s => Debug.WriteLine(s)))
                .UseAttribute((string s ) => message += $"name= {s}", "name" )
                .WeakUpdate("phone1", "234567")
                .Execute();

            Assert.AreEqual("234567", acc1[ "phone1"]);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(message));
        }

        [TestMethod]
        public void TestWeakUpdate2()
        {
        }

        [TestMethod]
        public void TestCount()
        {
        }

        [TestMethod]
        public void TestExists1()
        {
        }

        [TestMethod]
        public void TestExists2()
        {
        }

        [TestMethod]
        public void TestDistinct()
        {
        }


        [TestMethod]
        public void TestOrderByAsc()
        {
        }

        [TestMethod]
        public void TestOrderByDesc()
        {
        }

        [TestMethod]
        public void TestAnd()
        {
        }

        [TestMethod]
        public void TestClear()
        {
        }

        [TestMethod]
        public void TestDelete()
        {
        }

        [TestMethod]
        public void TestJoin()
        {
        }

        [TestMethod]
        public void TestBeforeEachRecord()
        {
        }

        [TestMethod]
        public void TestAfterEachRecord()
        {
        }
    }
}
