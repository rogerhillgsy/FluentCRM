using System;
using System.Collections.Generic;
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
            var message = string.Empty;
            var acc1 = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account1");
            Assert.AreEqual("123456", acc1[ "phone1"]);
            var newPhone = string.Empty;

            FluentAccount.Account(acc1.Id, _orgService)
                .Trace( (s => Debug.WriteLine(s)))
                .UseAttribute((string s ) =>
                {
                    message += $"name= {s}";
                    newPhone = "234567";
                }, "name" )
                .WeakUpdate("phone1", (string s) => newPhone)
                .Execute();

            Assert.AreEqual("234567", acc1[ "phone1"]);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(message));

            message = string.Empty;
            newPhone = "345678";
            FluentAccount.Account(acc1.Id, _orgService)
                .Trace( (s => Debug.WriteLine(s)))
                .UseAttribute((string s ) =>
                {
                    message += $"name= {s}";
                    newPhone = "234567";
                }, "name" )
                .WeakUpdate("phone1", newPhone)
                .Execute();

            Assert.AreEqual("345678", acc1[ "phone1"]);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(message));

        }

        [TestMethod]
        public void TestCount()
        {
            var message = string.Empty;
            var acc1 = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account1");
            Assert.AreEqual("123456", acc1[ "phone1"]);
            var count = 0;

            FluentAccount.Account(acc1.Id, _orgService)
                .Trace( (s => Debug.WriteLine(s)))
                .UseAttribute((string s ) =>
                {
                    message += $"name= {s}";
                }, "name" )
                .Count( (c) => count = c ?? 0)
                .Execute();

            Assert.AreEqual(1, count);

            FluentAccount.Account(_orgService)
                .Where("name").BeginsWith("Account")
                .UseAttribute((string s ) =>
                {
                    message += $"name= {s}";
                }, "name" )
                .Count( (c) => count = c ?? 0)
                .Execute();

            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public void TestExists1()
        {
            var message = string.Empty;
            var acc1 = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account1");
            Assert.AreEqual("123456", acc1[ "phone1"]);
            var existsCalled = false;

            FluentAccount.Account(_orgService)
                .Where("name").BeginsWith("Account")
                .UseAttribute((string s ) =>
                {
                    message += $"name= {s}";
                }, "name" )
                .Exists((e) =>
                {
                    Assert.IsTrue(e);
                    existsCalled = true;
                })
                .Execute();

            Assert.IsTrue(existsCalled);

            FluentAccount.Account(_orgService)
                .Where("name").BeginsWith("NotAccount")
                .UseAttribute((string s ) =>
                {
                    message += $"name= {s}";
                }, "name" )
                .Exists((e) =>
                {
                    Assert.IsFalse(e);
                    existsCalled = true;
                })
                .Execute();

            Assert.IsTrue(existsCalled);

        }

        [TestMethod]
        public void TestExists2()
        {
            var message = string.Empty;
            var acc1 = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account1");
            Assert.AreEqual("123456", acc1[ "phone1"]);
            var existsCalled = false;

            FluentAccount.Account(_orgService)
                .Where("name").BeginsWith("Account")
                .UseAttribute((string s ) =>
                {
                    message += $"name= {s}";
                }, "name" )
                .Exists(() =>
                {
                    existsCalled = true;
                },
                () =>
                {
                    Assert.Fail("account should have existed");
                })
                .Execute();

            Assert.IsTrue(existsCalled);

            FluentAccount.Account(_orgService)
                .Where("name").BeginsWith("NotAccount")
                .UseAttribute((string s ) =>
                {
                    message += $"name= {s}";
                }, "name" )
                .Exists(() =>
                    {
                        Assert.Fail("account should have existed");
                    },
                    () =>
                    {
                        existsCalled = true;
                    })
                .Execute();

            Assert.IsTrue(existsCalled);

        }

        [TestMethod]
        public void TestDistinct()
        {
            var message = string.Empty;
            var acc1 = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account1");
            
            Assert.AreEqual("123456", acc1[ "phone1"]);
            var existsCalled = false;

            FluentAccount.Account(_orgService)
                .Where("name").BeginsWith("Account")
                .UseAttribute((string s ) =>
                {
                    message += $"name= {s}";
                }, "name" )
                .Distinct()
                .Count((c) => Assert.AreEqual(4, c))
                .Execute();
        }


        [TestMethod]
        public void TestOrderByAsc()
        {
            var message = string.Empty;
            var acc1 = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account1");
            var expectedOrder = new List<string>{"Account1", "Account2", "Account3", "Account4"};
            var useCalled = false;
            
            FluentAccount.Account(_orgService)
                .Where("name").BeginsWith("Account")
                .OrderByAsc("name")
                .UseAttribute((string s ) =>
                {
                    Assert.AreEqual(expectedOrder.First(), s);
                    useCalled = true;
                    expectedOrder.RemoveAt(0);
                }, "name" )
                .Execute();

            Assert.IsTrue(useCalled);
        }

        [TestMethod]
        public void TestOrderByDesc()
        {
            var expectedOrder = new List<string>{"Account4", "Account3", "Account2", "Account1"};
            var useCalled = false;
            
            FluentAccount.Account(_orgService)
                .Where("name").BeginsWith("Account")
                .OrderByDesc("name")
                .UseAttribute((string s ) =>
                {
                    Assert.AreEqual(expectedOrder.First(), s);
                    useCalled = true;
                    expectedOrder.RemoveAt(0);
                }, "name" )
                .Execute();

            Assert.IsTrue(useCalled);
        }

        [TestMethod]
        public void TestAnd()
        {
            var useCalled = false;
            
            FluentAccount.Account(_orgService)
                .Where("name").BeginsWith("Account")
                .And
                .Where("address1_country").Equals("UK")
                .UseAttribute((string s ) =>
                {
                    useCalled = true;
                }, "name" )
                .Count((c) => Assert.AreEqual(2, c))
                .Execute();

            Assert.IsTrue(useCalled);

            useCalled = false;
            FluentAccount.Account(_orgService)
                .Where("name").BeginsWith("Account")
                .And
                .Where("address1_country").Equals("UK")
                .And
                .Where("phone1").Equals("987654321")
                .UseAttribute((string s ) =>
                {
                    useCalled = true;
                }, "name" )
                .Count((c) => Assert.AreEqual(0, c))
                .Execute();

            Assert.IsFalse(useCalled);

        }

        /// <summary>
        /// Clear appears to be broken in FakeXrmEasy
        /// Setting an entity field to null and updating does not appear to clear that field in FakeXrmEasy.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void TestClear1()
        {
            var message = string.Empty;
            var context = TestUtilities.TestContext2();

            FluentContact.Contact(context.GetOrganizationService())
                .Where("firstname").Equals("John")
                .UseAttribute( (string t) => Assert.IsNotNull(t), "telephone1")
                .Count( c => Assert.AreEqual(2, c))
                .Execute();

            FluentContact.Contact(context.GetOrganizationService())
                .Trace( s => Debug.WriteLine(s))
                .Where("firstname").Equals("John")
                .UseAttribute((string s) => message += "s", "name" )
                .Clear("telephone1")
                .Count( c => Assert.AreEqual(2, c))
                .Execute();

            FluentContact.Contact(context.GetOrganizationService())
                .Where("firstname").Equals("John")
                .UseAttribute( (string t) => Assert.Fail($"unexpected {t}"), "telephone1")
                .Count( c => Assert.AreEqual(2, c))
                .Execute();

            FluentContact.Contact(context.GetOrganizationService())
                .Where("firstname").NotEqual("John")
                .UseAttribute( (string t) => Assert.IsNotNull(t), "telephone1")
                .Count( c => Assert.AreEqual(2, c))
                .Execute();

        }

        [TestMethod]
        public void TestDelete()
        {
            var message = string.Empty;
            var acc1 = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account1");
            var expectedOrder = new List<string>{"Account4", "Account3", "Account2", "Account1"};
            Assert.AreEqual(4, _context.Data["account"].Count);

            FluentAccount.Account(_orgService).Where("name").Equals("Account1")
                .Delete()
                .Execute();

            Assert.AreEqual(3, _context.Data["account"].Count);
            acc1 = _context.Data["account"].Values.FirstOrDefault( n => (string) n["name"] == "Account1");
            Assert.IsNull(acc1);

            FluentAccount.Account(_orgService).Where("name").Equals("Account1")
                .Exists((e) => Assert.IsFalse(e))
                .Execute();
        }
        [TestMethod]
        public void TestJoin()
        {
            var context = TestUtilities.TestContext2();
            var account1 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account1")).First().Value;
            var account2 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account2")).First().Value;
            var message = string.Empty;
            FluentCRM.FluentCRM.StaticService = _orgService;

            // Join account to contact entity via the primary
            FluentAccount.Account(account2.Id)
                .Trace( s => Debug.WriteLine(s))
                .UseAttribute<string>((s) => message += s, "name")
                .Join<FluentContact>( c => c.UseAttribute<string>(s => Assert.AreEqual("Watson", s) , "lastname"))
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

            FluentAccount.Account(account1.Id)
                    .Trace(s => Debug.WriteLine(s))
                    .UseAttribute((string s) => Debug.WriteLine(s), "name")
                    .Join<FluentPrimaryContact>(c => c.UseAttribute<string>(s => Assert.AreEqual("Doe", s), "lastname"))
                    .Count(c => Assert.AreEqual(1, c))
                    .Execute();
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
