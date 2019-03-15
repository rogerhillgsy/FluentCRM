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
    public class Test7_IJoinable
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

        [TestMethod]
        public void TestUseAttribute1()
        {
            // Tests the mechanics of the join call
            var context = TestUtilities.TestContext2();
            var account2 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account2")).First().Value;
            var called = false;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();

            Debug.WriteLine($"Account2 Id is {account2.Id}");
            
            // Join account to contact entity via the primary
            FluentAccount.Account(account2.Id )
                .Trace(s => Debug.WriteLine(s))
                .UseAttribute((string s) => Debug.WriteLine(s), "name")
                .Join<FluentPrimaryContact>(
                    c => c.UseAttribute<string>(s =>
                    {
                        called = true;
                        Assert.AreEqual("Watson", s);
                    }, "lastname"))
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

            Assert.IsTrue(called);
        }

        [TestMethod]
        public void TestUseAttribute2()
        {
            var context = TestUtilities.TestContext2();
            var account2 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account2")).First().Value;
            var called = false;

            Debug.WriteLine($"Account2 Id is {account2.Id}");

            FluentAccount.Account(account2.Id, context.GetOrganizationService())
                .Trace(s => Debug.WriteLine(s))
                .Join<FluentPrimaryContact>(
                    c => c.UseAttribute<string>(s =>
                    {
                        called = true;
                        Assert.AreEqual("Watson", s);
                    }, "lastname"))
                .UseAttribute((string s) => Debug.WriteLine(s), "name")
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

            Assert.IsTrue(called);
        }


        [TestMethod]
        public void TestUseAttribute3()
        {
            var context = TestUtilities.TestContext2();
            var account2 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account2")).First().Value;
            var called = false;

            Debug.WriteLine($"Account2 Id is {account2.Id}");

            FluentAccount.Account(account2.Id, context.GetOrganizationService())
                .Trace(s => Debug.WriteLine(s))
                .Join<FluentPrimaryContact>(
                    c => c.UseAttribute<string>(s =>
                    {
                        called = true;
                        Assert.AreEqual("Watson", s);
                    }, "lastname"))
                .UseAttribute((string name, string s) => Debug.WriteLine($"Called with attribute: {name} = {s}"), "name")
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

            Assert.IsTrue(called);
        }

        [TestMethod]
        public void TestUseEntity()
        {
            // Tests the mechanics of the join call
            var context = TestUtilities.TestContext2();
            var account2 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account2")).First().Value;
            var called = false;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();

            Debug.WriteLine($"Account2 Id is {account2.Id}");
            
            // Join account to contact entity via the primary
            FluentAccount.Account(account2.Id )
                .Trace(s => Debug.WriteLine(s))
                .UseEntity((e) => Debug.WriteLine(e.Entity.LogicalName), "name")
                .Join<FluentPrimaryContact>(
                    c => c.UseAttribute<string>(s =>
                    {
                        called = true;
                        Assert.AreEqual("Watson", s);
                    }, "lastname").
                        UseEntity((e, a) => Debug.WriteLine($"Joined entity called with element logical name {e.Entity.LogicalName}, alias {a}"), "firstname"))
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

            Assert.IsTrue(called);
        }

        [TestMethod]
        public void TestUseEntity2()
        {
            // Tests the mechanics of the join call
            var context = TestUtilities.TestContext2();
            var account2 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account2")).First().Value;
            var called = false;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();

            Debug.WriteLine($"Account2 Id is {account2.Id}");
            
            // Join account to contact entity via the primary
            FluentAccount.Account(account2.Id )
                .Trace(s => Debug.WriteLine(s))
                .Join<FluentPrimaryContact>(
                    c => c.UseAttribute<string>(s =>
                        {
                            called = true;
                            Assert.AreEqual("Watson", s);
                        }, "lastname").
                        UseEntity((name, e, alias) =>
                        {
                            Assert.AreEqual(e.Alias + "firstname", name );
                            Assert.AreEqual(alias + "firstname", name );
                            Debug.WriteLine(
                                    $"Joined entity called with element logical name {e.Entity.LogicalName}, alias {alias}");
                        }, "firstname"))
                .UseEntity((name, e) =>
                {
                    Assert.AreEqual("name", name);
                    Debug.WriteLine(e.Entity.LogicalName);
                }, "name")
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

            Assert.IsTrue(called);
        }

        [TestMethod]
        public void TestJoin()
        {
            // Look for Contact2, parent account, primary contact (which should be contact 1)
            FluentContact.Contact()
                .Trace( (s) => Debug.WriteLine(s))
                .Where("firstname").Equals("Sam")
                .And
                .Where("lastname").Equals("Spade")
                .Join<FluentAccount>(
                    a => a.Join<FluentPrimaryContact>(
                        pc => pc.UseAttribute((string fn) => Assert.AreEqual("John", fn), "firstname")
                            .UseAttribute((string ln) => Assert.AreEqual("Doe", ln), "lastname"))
                        .UseAttribute<string>((attr, n) =>
                        {
                            Assert.AreEqual( "name", attr);
                            Assert.AreEqual( "Account1", n);
                        }, "name"))
                .UseAttribute((string attr, string t2) =>
                {
                    Assert.AreEqual("telephone2", attr);
                    Assert.AreEqual( "3456789",t2);
                }, "telephone", "telephone2")
                .Exists( (e) => Assert.IsTrue(e))
                .Execute();
        }

        [TestMethod]
        public void TestJoin2()
        {
            var calls = 0;
              Action<string, string> processAttribute = (string attr, string val) =>
              {
                  Debug.WriteLine($"ProcessAttribute :{attr}/{val}");
                  calls++;
              };

            // Look for contacts of account
            FluentAccount.Account()
                .Where("name").Equals("Account1")
                .Join<FluentContact>(
                    c => c.UseAttribute(processAttribute, "firstname")
                        .UseAttribute(processAttribute, "lastname")
                        .UseAttribute(processAttribute, "firstname")
                    )
                .Exists( (e) => Assert.IsTrue(e))
                .Count((c) => Assert.AreEqual(2, c))
                .Execute();

            Assert.AreEqual(6 , calls);
        }

        [TestMethod]
        public void TestLogicalName()
        {
            var calls = 0;
            Action<string, string> processAttribute = (string attr, string val) =>
            {
                Debug.WriteLine($"ProcessAttribute :{attr}/{val}");
                calls++;
            };

             FluentAccount.Account()
                .Where("name").Equals("Account1")
                .Join<FluentContact>(
                    c =>
                    {
                        Assert.AreEqual("contact", c.LogicalName);
                        c.UseAttribute(processAttribute, "firstname")
                             .UseAttribute(processAttribute, "lastname")
                             .UseAttribute(processAttribute, "firstname");
                    })
                .Exists((e) => Assert.IsTrue(e))
                .Count((c) => Assert.AreEqual(2, c));
        }

        [TestMethod]
        public void TestJoinAttribute()
        {
            var calls = 0;
            Action<string, string> processAttribute = (string attr, string val) =>
            {
                Debug.WriteLine($"ProcessAttribute :{attr}/{val}");
                calls++;
            };

            FluentAccount.Account()
                .Where("name").Equals("Account1")
                .Join<FluentContact>(
                    c =>
                    {
                        Assert.AreEqual( "parentcustomerid", c.JoinAttribute("account"));
                        Assert.AreEqual( "contactid", c.JoinAttribute("annotation"));
                        c.UseAttribute(processAttribute, "firstname")
                            .UseAttribute(processAttribute, "firstname");
                    })
                .Exists((e) => Assert.IsTrue(e))
                .Count((c) => Assert.AreEqual(2, c));
        }

        [TestMethod]
        public void TestOuter1()
        {
            // return join of account1 and primary contact 
            FluentAccount.Account()
                .Where("name").Equals("Account1")
                .Join<FluentPrimaryContact>(
                    c => c.UseAttribute((string n) =>
                    {
                        Assert.AreEqual("Doe", n);
                    }, "lastname"))
                .Count((c) => Assert.AreEqual(1, c))
                .Execute();
        }

        [TestMethod]
        public void TestOuter2()
        {
            // return outer join of account1 and primary contact 
            FluentAccount.Account()
                .Where("name").Equals("Account1")
                .Join<FluentPrimaryContact>(
                    c => c.Outer().UseAttribute((string n) =>
                    {
                        Assert.AreEqual("Doe", n);
                    }, "lastname"))
                .Count((c) => Assert.AreEqual(1, c))
                .Execute();
        }

        [TestMethod]
        public void TestOuter3()
        {
            // return outer join of account3 and primary contact 
            var calls = 0;

            FluentAccount.Account()
                .Where("name").Equals("Account3")
                .Join<FluentPrimaryContact>(
                    c => c.Outer().UseAttribute((string n) => calls++, "lastname"))
                .Count((c) => Assert.AreEqual(1, c))
                .Execute();

            Assert.AreEqual(0 ,calls);
        }

        [TestMethod]
        public void TestOuter4()
        {
            // return join of account3 and primary contact 
            var calls = 0;

            FluentAccount.Account()
                .Where("name").Equals("Account3")
                .Join<FluentPrimaryContact>(
                    c => c.UseAttribute((string n) => calls++, "lastname"))
                .Count((c) => Assert.AreEqual(0, c))
                .Execute();

            Assert.AreEqual(0 ,calls);
        }

    }
}
