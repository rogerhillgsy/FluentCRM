using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FakeXrmEasy;
using FluentCRM;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

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
            FluentCRM.FluentCRM.StaticService = _orgService;
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

        /// <summary>
        /// Test use of default value in UseAttribute.
        /// </summary>
        [TestMethod]
        public void TestUseAttribute3()
        {
            var message = string.Empty;
            var message2 = string.Empty;
            FluentAccount.Account(_orgService).Where("name").Equals("Account1")
                .UseAttribute( (string a) => message = $"Name is {a}", "name")
                .UseAttribute( (string a) => message2 = $"Name is {a}", "namemissing")
                .UseAttribute( (string a) => message += "\n Name2 is {a}", "name2").Execute();

            Assert.IsTrue(!string.IsNullOrWhiteSpace(message));
            Assert.IsTrue(message.Contains("Name2"));
            Assert.IsTrue(string.IsNullOrWhiteSpace(message2));


            message = string.Empty;
            message2 = string.Empty;
            FluentAccount.Account(_orgService).Where("name").Equals("Account1")
                .UseAttribute( (string a) => message = $"Name is {a}", "name")
                .UseAttribute( "default", (string a) => message2 = $"Name is {a}", "namemissing")
                .Execute();

            Assert.IsTrue(!string.IsNullOrWhiteSpace(message2));
            Assert.AreEqual("Name is default", message2);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(message));
        }

        [DataTestMethod]
        [DataRow( false, "", "name", "Account1", "name", true )]
        [DataRow( true, "default", "name", "Account1", "name", true )]
        [DataRow( false, "", "namemissing", "", "", false )]
        [DataRow( true, "**Unknown**", "namemissing", "**Unknown**", "", true )]
        public void TestUseAttribute4( bool hasDefault, string defaultval, string attribute, string expectedVal, string expectedAttr, bool callExpected) 
        {
            var called = false;
            Action<string,string> closure = (attr, value) =>
            {
                Debug.WriteLine($"Closure called for attribute {attr} with value {value}");
                Assert.AreEqual(expectedVal, value, "Expected Value");
                Assert.AreEqual(expectedAttr, attr, "Expected Attribute");
                called = true;  
            };

            if (hasDefault)
            {
                FluentAccount.Account(_orgService).Where("name").Equals("Account1")
                    .UseAttribute<int>((s) => Debug.WriteLine($"Option set val {s}"), "statecode")
                    .UseAttribute(defaultval, closure,attribute).Execute();
            }
            else
            {
                FluentAccount.Account(_orgService).Where("name").Equals("Account1")
                    .UseAttribute<int>((s) => Debug.WriteLine($"Option set val {s}"), "statecode")
                    .UseAttribute(closure, attribute).Execute();
            }

            Assert.AreEqual(callExpected, called);
        }

        /// <summary>
        /// Test Case where same attribute is referred to twice in different UseAttribute calls.
        /// Both callbacks shall be invoked.
        /// </summary>
        [TestMethod]
        public void TestUseAttribute5()
        {
            var message = string.Empty;
            var trace = new StringBuilder();
            var name1 = string.Empty;
            var name2 = string.Empty;
            FluentAccount.Account(_orgService).Where("name").Equals("Account1")
                .Trace( s => trace.AppendLine(s))
                .UseAttribute((int a) => message = $"Statecode is {a}", "statecode")
                .UseAttribute((string a) => name1=a, "name")
                .UseAttribute((string a) => name2=a, "name" ).Execute();


            Assert.IsTrue(!string.IsNullOrWhiteSpace(message));
            Assert.AreEqual("Account1", name1);
            Assert.AreEqual("Account1", name2);
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
        public void TestTop()
        {
            var context = TestUtilities.TestContext2();
            var expected = "Account1";
            Entity account = null;

            FluentAccount.Account(context.GetOrganizationService())
                .Where("address1_country").Equals("UK")
                .OrderByAsc("phone1")
                .UseAttribute((string n) =>
                {
                    Assert.AreEqual(expected, n);
                }, "name")
                .Count(c => Assert.AreEqual(1, c))
                .Top(1)
                .Execute();

            expected = "Account";
            FluentAccount.Account(context.GetOrganizationService())
                .Where("address1_country").Equals("UK")
                .OrderByDesc("phone1")
                .UseAttribute((string n) =>
                {
                    Assert.IsTrue(n.StartsWith(expected));
                }, "name")
                .Count(c => Assert.AreEqual(2, c))
                .Top(2)
                .Execute();

            // Try again with no order clause
            var trace = new StringBuilder();

            FluentAccount.Account(context.GetOrganizationService())
                .Where("address1_country").Equals("UK")
                .Trace(s => trace.AppendLine(s))
                .UseAttribute((string n) =>
                {
                    Assert.IsTrue(n.StartsWith(expected));
                }, "name")
                .Count(c => Assert.AreEqual(1, c))
                .Top(1)
                .Execute();

            Assert.IsTrue(trace.ToString().Contains("Warning: Top count"));

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
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();

            // Join account to contact entity via the primary
            FluentAccount.Account(account2.Id, context.GetOrganizationService())
                .Trace(s => Debug.WriteLine(s))
                .UseAttribute((string s) => Debug.WriteLine(s), "name")
                .Join<FluentPrimaryContact>(
                    c => c.UseAttribute<string>(s => Assert.AreEqual("Watson", s), "lastname"))
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

                FluentAccount.Account(account1.Id)
                    .Trace(s => Debug.WriteLine(s))
                    .UseAttribute((string s) => Debug.WriteLine(s), "name")
                    .Join<FluentContact>(c =>
                        c.UseAttribute<string>(s => Assert.IsTrue("Doe" == s || s == "Spade"), "lastname"))
                    .Count(c => Assert.AreEqual(2, c))
                    .Execute();
        }

        [TestMethod]
        public void TestBeforeEachRecord()
        {
            var context = TestUtilities.TestContext2();
            var message = string.Empty;
            FluentCRM.FluentCRM.StaticService = _orgService;
            var accounts = new List<AccountDetails>();
            AccountDetails currentAccount = null;
            var calls = 0;

            // Join account to contact entity via the primary
            FluentAccount.Account()
                .Trace(s => Debug.WriteLine(s))
                .Where("name").BeginsWith("Account")
                .BeforeEachRecord( (e) =>
                {                   
                    currentAccount = new AccountDetails();
                    calls++;
                })
                .UseAttribute((string s) => currentAccount.Name = s , "name")               
                .UseAttribute((string s) => currentAccount.Phone = s , "phone1")   
                .Count(c => Assert.AreEqual(4, c))
                .Execute();

            Assert.AreEqual(4, calls);
        }

        private class AccountDetails
        {
            public string Name { get;set; }
            public string Phone { get; set; }
        }

        [TestMethod]
        public void TestExecute1()
        {
            var context = TestUtilities.TestContext2();
            var message = string.Empty;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();
            var currentAccount = new AccountDetails();
            var lastname = string.Empty;
            var preExecute = false;
            var postExecute = false;

            // Join account to contact entity via the primary
            FluentAccount.Account(context.GetOrganizationService())
                .Where("name").BeginsWith("Account")
                .UseAttribute((string s) =>
                {
                    currentAccount.Name = s;
                    Assert.IsTrue(s.CompareTo(lastname) > 0);
                    lastname = s;
                }, "name")
                .OrderByAsc("name")
                .Count(c => Assert.AreEqual(4, c))
                .Execute(() =>
                    {
                        Assert.IsFalse(preExecute);
                        preExecute = true;
                        Assert.IsTrue(string.IsNullOrEmpty(lastname));
                    },
                    (actionsCalled, updates) =>
                    {
                        Assert.IsTrue(preExecute);
                        Assert.IsFalse(postExecute);
                        postExecute = false;
                        Assert.AreEqual(4,actionsCalled);
                        Assert.AreEqual(0,updates);
                        Assert.IsFalse(string.IsNullOrEmpty(lastname));
                    });

        }

        [TestMethod]
        public void TestExecute2()
        {
            var context = TestUtilities.TestContext2();
            var message = string.Empty;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();
            var currentAccount = new AccountDetails();
            var lastname = "ZZZZZ";
            var preExecute = false;
            var postExecute = false;

            // Join account to contact entity via the primary
            FluentAccount.Account(context.GetOrganizationService())
                .Where("name").BeginsWith("Account")
                .UseAttribute((string s) =>
                {
                    currentAccount.Name = s;
                    Assert.IsTrue(s.CompareTo(lastname) < 0);
                    lastname = s;
                }, "name")
                .OrderByDesc("name")
                .And.Where("name2").IsNotNull
                .WeakUpdate<string>( "name", (v) =>
                {
                    if (v == "Account1")
                    {
                        return v + "--";
                    }

                    return v;
                } )
                .Count(c => Assert.AreEqual(2, c))
                .Execute(() =>
                    {
                        Assert.IsFalse(preExecute);
                        preExecute = true;
                        Assert.IsTrue(lastname == "ZZZZZ");
                    },
                    (actionsCalled, updates) =>
                    {
                        Assert.IsTrue(preExecute);
                        Assert.IsFalse(postExecute);
                        postExecute = false;
                        Assert.AreEqual(4, actionsCalled);
                         // Called for both UseAttribute and WeakUpdate calls.
                        Assert.AreEqual(1, updates);
                        Assert.IsFalse(string.IsNullOrEmpty(lastname));
                    });

        }

        [TestMethod]
        public void TestAfterEachRecord()
        {
            var context = TestUtilities.TestContext2();
            var message = string.Empty;
            FluentCRM.FluentCRM.StaticService = _orgService;
            var accounts = new List<AccountDetails>();
            AccountDetails currentAccount = null;
            var calls = 0;

            // Join account to contact entity via the primary
            FluentAccount.Account(context.GetOrganizationService())
                .Trace(s => Debug.WriteLine(s))
                .Where("name").BeginsWith("Account")
                .BeforeEachRecord( (e) =>
                {                   
                    currentAccount = new AccountDetails();
                })
                .UseAttribute((string s) => currentAccount.Name = s , "name")               
                .UseAttribute((string s) => currentAccount.Phone = s , "phone1")   
                .WeakUpdate<string>("description", (s) => $"{currentAccount.Name} - {currentAccount.Phone}")
                .AfterEachRecord((e) =>
                {
                    accounts.Add(currentAccount);
                    calls++;
                })
                .Count(c => Assert.AreEqual(4, c))
                .Execute();

            Assert.AreEqual(4, calls);

            Assert.AreEqual(4, accounts.Count);

            var resultAccounts = context.CreateQuery("account").ToList();
            Assert.IsTrue( resultAccounts[0].Attributes.ContainsKey("description"));
            Assert.IsFalse( string.IsNullOrEmpty( resultAccounts[0].GetAttributeValue<string>("description")));
            Assert.IsTrue( resultAccounts[1].Attributes.ContainsKey("description"));
            Assert.IsFalse( string.IsNullOrEmpty( resultAccounts[1].GetAttributeValue<string>("description")));
            Assert.IsTrue( resultAccounts[2].Attributes.ContainsKey("description"));
            Assert.IsFalse( string.IsNullOrEmpty( resultAccounts[2].GetAttributeValue<string>("description")));
            Assert.IsTrue( resultAccounts[3].Attributes.ContainsKey("description"));
            Assert.IsFalse( string.IsNullOrEmpty( resultAccounts[3].GetAttributeValue<string>("description")));
        }
    }
}
