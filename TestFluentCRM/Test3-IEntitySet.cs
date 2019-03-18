using System;
using System.Activities.Expressions;
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
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;

namespace TestFluentCRM
{
    [TestClass]
    public class Test3_IEntitySet
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
        public void TestService1()
        {
            var f = FluentAccount.Account();

            var service2 = f.Service;

            Assert.AreSame(_orgService, service2);
        }

        [TestMethod]
        public void TestTrace1()
        {
            var message = string.Empty;
            var account1 = _context.Data["account"].First().Value;
            var accountId = account1.Id;

            FluentAccount.Account(accountId).Trace(m =>
            {
                message = m;
                Debug.WriteLine(m);
            }).UseAttribute( (string a) => Debug.Write($"Name is {a}"), "name").Execute();

            Assert.IsTrue(!string.IsNullOrWhiteSpace(message));
        }

        [TestMethod]
        public void TestTimer1()
        {
            var message = string.Empty;
            FluentAccount.Account().Where("name").Equals("Account1").Timer( m =>
            {
                message = m;
                Debug.WriteLine($@"Timer message: {m}");
            }).UseAttribute( (string a) => Debug.Write($"Name is {a}"), "name").Execute();

            Assert.IsTrue(!string.IsNullOrWhiteSpace(message));
        }

        [TestMethod]
        public void TestUseAttribute1()
        {
            var message = string.Empty;
            FluentAccount.Account().Where("name").Equals("Account1")
                .UseAttribute( (string a) => message = $"Name is {a}", "name").Execute();

            Assert.IsTrue(!string.IsNullOrWhiteSpace(message));
        }

        [TestMethod]
        public void TestUseAttribute2()
        {
            var message = string.Empty;
            FluentAccount.Account().Where("name").Equals("Account1")
                .UseAttribute( (int a) => message = $"Statecode is {a}", "statecode").Execute();

            Assert.IsTrue(!string.IsNullOrWhiteSpace(message));
        }

        [DataRow( new string[] {"name", "name1", "name2"}, "name" )]
        [DataRow( new string[] {"name1", "name", "name2"}, "name" )]
        [DataRow( new string[] {"name3", "name1", "name2"}, "name2" )]
        [DataRow( new string[] {"name2", "name", "name", "name2"}, "name" )]
        [DataTestMethod]
        public void TestUseAttribute3(string[] nameList, string resultAttribute)
        {
            var account3 = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account3");
            var name = string.Empty;
            var nameListRest = nameList.Skip(1).ToArray();

            FluentAccount.Account().Where("name").Equals("Account3")
                .UseAttribute( (string a) => name = a, nameList[0], nameListRest).Execute();

            Assert.AreEqual(account3.GetAttributeValue<string>(resultAttribute), name == "" ? null : name);
        }


        [DataRow( new string[] {"name", "name1", "name2"}, "name" )]
        [DataRow( new string[] {"name1", "name", "name2"}, "name" )]
        [DataRow( new string[] {"name3", "name1", "name2"}, "name2" )]
        [DataRow( new string[] {"name2", "name", "name", "name2"}, "name" )]
        [DataTestMethod]
        public void TestUseAttribute4(string[] nameList, string resultAttribute)
        {
            var account3 = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account3");
            var name = string.Empty;
            var nameListRest = nameList.Skip(1).ToArray();

            FluentAccount.Account().Where("name").Equals("Account3")
                .UseAttribute( (string attr, string a) =>
                {
                    Assert.AreEqual(resultAttribute, attr);
                    name = a;
                }, nameList[0], nameListRest).Execute();

            Assert.AreEqual(account3.GetAttributeValue<string>(resultAttribute), name == "" ? null : name);
        }

        [DataRow( new string[] {"name", "name1", "name2"}, "name" )]
        [DataRow( new string[] {"name1", "name", "name2"}, "name" )]
        [DataRow( new string[] {"name3", "name1", "name2"}, "name2" )]
        [DataRow( new string[] {"name2", "name", "name", "name2"}, "name" )]
        [DataTestMethod]
        public void TestUseEntity1(string[] nameList, string resultAttribute)
        {
            var account3 = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account3");
            var name = string.Empty;
            var nameListRest = nameList.Skip(1).ToArray();

            FluentAccount.Account().Where("name").Equals("Account3")
                .UseEntity( a =>
                {
                    Assert.AreEqual(account3.Id, a.Entity.Id);
                    name = a.GetAttributeValue<string>("name");
                }, nameList[0], nameListRest).Execute();
            
            Assert.AreEqual(account3.GetAttributeValue<string>(resultAttribute), name == "" ? null : name);
        }

        [DataRow( new string[] {"name", "name1", "name2"}, "name" )]
        [DataRow( new string[] {"name1", "name", "name2"}, "name" )]
        [DataRow( new string[] {"name3", "name1", "name2"}, "name2" )]
        [DataRow( new string[] {"name2", "name", "name", "name2"}, "name" )]
        [DataTestMethod]
        public void TestUseEntity2(string[] nameList, string resultAttribute)
        {
            var account3 = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account3");
            var name = string.Empty;
            var nameListRest = nameList.Skip(1).ToArray();

            FluentAccount.Account().Where("name").Equals("Account3")
                .UseEntity( (string attr, EntityWrapper a) =>
                {
                    Assert.AreEqual(account3.Id, a.Entity.Id);
                    Assert.AreEqual(resultAttribute, attr);
                    name = a.GetAttributeValue<string>("name");
                }, nameList[0], nameListRest).Execute();
            
            Assert.AreEqual(account3.GetAttributeValue<string>(resultAttribute), name == "" ? null : name);
        }

        [TestMethod]
        public void TestWeakUpdate1()
        {
            var acc1 = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account1");
            Assert.AreEqual("123456", acc1[ "phone1"]);

            FluentAccount.Account(acc1.Id)
                .Trace( (s => Debug.WriteLine(s)))
                .WeakUpdate("phone1", "234567")
                .Execute();

            Assert.AreEqual("234567", acc1[ "phone1"]);

        }

        [TestMethod]
        public void TestWeakUpdate2()
        {
            var acc1 = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account1");
            Assert.AreEqual("123456", acc1[ "phone1"]);

            FluentAccount.Account(acc1.Id)
                .Trace( (s => Debug.WriteLine(s)))
                .WeakUpdate("phone1", (string t) =>  t + "7")
                .WeakUpdate("phone2", (string s) => "878787")
                .WeakUpdate("phone3", "959595959")
                .Execute();

            Assert.AreEqual("1234567", acc1[ "phone1"]);
            Assert.AreEqual("878787", acc1[ "phone2"]);
            Assert.AreEqual("959595959", acc1[ "phone3"]);

        }

        [TestMethod]
        public void TestWeakUpdate3()
        {
            var acc = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account1");
            Assert.AreEqual("123456", acc[ "phone1"]);

            var phone = String.Empty;

            FluentAccount.Account()
                .Trace( (s => Debug.WriteLine(s)))
                .Where("name").Condition(ConditionOperator.BeginsWith,"Account")
                .UseAttribute( (string s) => phone= s, "phone1")
                .WeakUpdate("phone2", (string t) => phone + "2")
                .WeakUpdate("phone3", (string s) => phone + "3")
                .Execute();

            Assert.AreEqual("123456", acc[ "phone1"]);
            Assert.AreEqual("1234562", acc[ "phone2"]);
            Assert.AreEqual("1234563", acc[ "phone3"]);
            acc = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account2");
            Assert.AreEqual("654321", acc[ "phone1"]);
            Assert.AreEqual("6543212", acc[ "phone2"]);
            Assert.AreEqual("6543213", acc[ "phone3"]);
            acc = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account3");
            Assert.AreEqual("222333", acc[ "phone1"]);
            Assert.AreEqual("2223332", acc[ "phone2"]);
            Assert.AreEqual("2223333", acc[ "phone3"]);
            acc = _context.Data["account"].Values.Single( n => (string) n["name"] == "Account4");
            Assert.AreEqual("222333", acc[ "phone1"]);
            Assert.AreEqual("2223332", acc[ "phone2"]);
            Assert.AreEqual("2223333", acc[ "phone3"]);
        }

        [TestMethod]
        public void TestWeakUpdate4()
        {
            var acc1 = _context.Data["account"].Values.Single(n => (string) n["name"] == "Account1");
            Assert.AreEqual("123456", acc1["phone1"]);
            var b = new StringBuilder();

            FluentAccount.Account(acc1.Id)
                .Trace((s => b.AppendLine(s)))
                .WeakUpdate("phone1", (string t) => t)
                .WeakUpdate("phone2", (string t) => "2222222")
                .WeakUpdate("name3", (string n) => "name33")
                .Execute();

            Assert.AreEqual("123456", acc1["phone1"]);
            Assert.AreEqual("2222222", acc1["phone2"]);
            Assert.AreEqual("name33", acc1["name3"]);

            var result = b.ToString();
            Assert.IsFalse( result.Contains("Updating column phone1") );
            Assert.IsTrue( result.Contains("Updating column phone2") );
            Assert.IsTrue( result.Contains("Updating column name3") );
            Debug.WriteLine(result);
        }

        /// <summary>
        /// Ensure that if we Update with the same value, no changes are written back to CRM
        /// </summary>
        [TestMethod]
        public void TestWeakUpdate5()
        {
            var acc1 = _context.Data["account"].Values.Single(n => (string) n["name"] == "Account1");
            Assert.AreEqual("123456", acc1["phone1"]);
            var b = new StringBuilder();

            FluentAccount.Account(acc1.Id)
                .Trace((s => b.AppendLine(s)))
                .WeakUpdate("phone1", (string t) => t)
                .Execute();

            Assert.AreEqual("123456", acc1["phone1"]);

            var result = b.ToString();
            Assert.IsFalse( result.Contains("Updating column phone1") );
            Assert.IsFalse( result.Contains("Updating entity account/") );
            Debug.WriteLine(result);
        }
        /// <summary>
        /// Ensure that if we Update with the null, no changes are written back to CRM
        /// </summary>
        [TestMethod]
        public void TestWeakUpdate6()
        {
            var acc1 = _context.Data["account"].Values.Single(n => (string) n["name"] == "Account1");
            Assert.AreEqual("123456", acc1["phone1"]);
            var b = new StringBuilder();

            FluentAccount.Account(acc1.Id)
                .Trace((s => b.AppendLine(s)))
                .WeakUpdate("phone1", (string t) => null)
                .Execute();

            Assert.AreEqual("123456", acc1["phone1"]);

            var result = b.ToString();
            Assert.IsFalse( result.Contains("Updating column phone1") );
            Assert.IsFalse( result.Contains("Updating entity account/") );
            Debug.WriteLine(result);
        }

        /// <summary>
        /// Ensure that weak update of constant value off IEntitySet works.
        /// </summary>
        [TestMethod]
        public void TestWeakUpdate7()
        {
            var acc1 = _context.Data["account"].Values.Single(n => (string) n["name"] == "Account1");
            Assert.AreEqual("123456", acc1["phone1"]);
            var b = new StringBuilder();
            var newPhone = "43432423423";

            FluentAccount.Account()
                .Where("name").Equals("Account1")
                .Trace((s => b.AppendLine(s)))
                .Distinct()
                .WeakUpdate("phone1", newPhone)
                .UseAttribute((string n) => newPhone="999999", "name")
                .Execute();

            Assert.AreEqual("43432423423", acc1["phone1"]);

            var result = b.ToString();
            Assert.IsTrue( result.Contains("Updating column phone1") );
            Assert.IsTrue( result.Contains("Updating entity account/") );
            Debug.WriteLine(result);
        }

        [TestMethod]
        public void TestCount1()
        {
            int? count = -1;

            FluentAccount.Account().Where("name").Equals("Account1")
                .Count( c => count  = c)
                .Execute();

            Assert.AreEqual(1, count);
        }

        [DataRow("Account1", 1)]
        [DataRow("Account", 4)]
        [DataTestMethod]
        public void TestCount2( string searchFor, int expectedCount )
        {
            int? count = -1;

            FluentAccount.Account().Where("name").BeginsWith(searchFor)
                .Count( c => count  = c)
                .Execute();

            Assert.AreEqual( expectedCount, count);
        }

        [DataRow("Account1",true)]
        [DataRow("Account",true)]
        [DataRow("NotAnAccount",false)]
        [DataTestMethod]
        public void TestExists1( string search, bool expected)
        {
            var called = false;

            FluentAccount.Account().Where("name").BeginsWith(search)
                .Exists(b =>
                {
                    Assert.AreEqual(expected, b);
                    called = true;
                })
                .Execute();

            Assert.IsTrue(called);
        }

        [DataRow("Account1",true)]
        [DataRow("Account",true)]
        [DataRow("Accountxx",false)]
        [DataTestMethod]
        public void TestExists2( string search, bool expected)
        {
            var called = 0;

            FluentAccount.Account().Where("name").BeginsWith(search)
                .Exists( () =>
                {
                    called++;
                    if ( !expected ) 
                        Assert.Fail();
                })
                .Execute();

            if (expected )
                Assert.AreEqual(1, called);
            else 
                Assert.AreEqual(0, called);
        }

        [DataRow("Account1",true)]
        [DataRow("Account",true)]
        [DataRow("Accountxx",false)]
        [DataTestMethod]
        public void TestExists3( string search, bool expected)
        {
            var called = 0;

            FluentAccount.Account().Where("name").BeginsWith(search)
                .Exists(() =>
                    {
                        called++;
                        if (!expected)
                            Assert.Fail();
                    },
                    () =>
                    {
                        called++;
                        if (expected)
                            Assert.Fail();
                    })
                .Execute();

            Assert.AreEqual(1, called);
        }

        [TestMethod]
        public void TestDistinct()
        {
            var name = string.Empty;
            int? count = 0;
            int calls = 0;
            var b = new StringBuilder();

            var context = TestUtilities.TestContext2();
            

            FluentAccount.Account(context.GetOrganizationService() ).Where("name").BeginsWith("Account")
                .Trace( s =>b.AppendLine(s))
                .UseAttribute((string n) =>
                {
                    calls++;
                    name = n;
                }, "name" )
                .PageSize(2)
                .Distinct()
                .Count( c => count = c )
                .Execute();

            Assert.AreEqual(4, calls);
            Assert.AreEqual(4, count);

            Debug.WriteLine(b.ToString());
        }

        [TestMethod]
        public void TestOrderByAsc()
        {
            var context = TestUtilities.TestContext2();
            var expected = new List<string> {"123456", "222333", "222333", "654321"};

            FluentAccount.Account(context.GetOrganizationService()).Where("name").IsNotNull
                .OrderByAsc("phone1")
                .UseAttribute((string n) =>
                {
                    var next = expected.First(); 
                    Assert.AreEqual( n, next);
                    expected.RemoveAt(0);
                }, "phone1")
                .Execute();

            Assert.AreEqual(0, expected.Count);
        }

        [TestMethod]
        public void TestOrderByDesc()
        {
            var context = TestUtilities.TestContext2();
            var expected = new List<string> {"123456", "222333", "222333", "654321"};
            expected.Reverse();

            FluentAccount.Account(context.GetOrganizationService()).Where("name").IsNotNull
                .OrderByDesc("phone1")
                .UseAttribute((string n) =>
                {
                    var next = expected.First(); 
                    Assert.AreEqual( n, next);
                    expected.RemoveAt(0);
                }, "phone1")
                .Execute();

            Assert.AreEqual(0, expected.Count);
        }

        [TestMethod]
        public void TestAnd()
        {
            var context = TestUtilities.TestContext2();

            FluentContact.Contact(context.GetOrganizationService())
                .Where("firstname").Equals("John")
                .Count( c => Assert.AreEqual(2, c))
                .Execute();

            FluentContact.Contact(context.GetOrganizationService())
                .Where("firstname").Equals("John")
                .And
                .Where("lastname").Equals("Doe")
                .Count( c => Assert.AreEqual(1, c))
                .Execute();

            FluentContact.Contact(context.GetOrganizationService())
                .Where("firstname").Equals("John")
                .And
                .Where("lastname").Equals("Smith")
                .Count( c => Assert.AreEqual(0, c))
                .Execute();
        }

        /// <summary>
        /// Clear appears to be broken in FakeXrmEasy
        /// Setting an entity field to null and updating does not appear to clear that field in FakeXrmEasy.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void TestClear1()
        {
            var context = TestUtilities.TestContext2();

            FluentContact.Contact(context.GetOrganizationService())
                .Where("firstname").Equals("John")
                .UseAttribute( (string t) => Assert.IsNotNull(t), "telephone1")
                .Count( c => Assert.AreEqual(2, c))
                .Execute();

            FluentContact.Contact(context.GetOrganizationService())
                .Trace( s => Debug.WriteLine(s))
                .Where("firstname").Equals("John")
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
            var context = TestUtilities.TestContext2();

            FluentContact.Contact(context.GetOrganizationService())
                .Where("firstname").Equals("John")
                .And
                .Where("lastname").Equals("Doe")
                .Count( c => Assert.AreEqual(1, c))
                .Execute();

            FluentContact.Contact(context.GetOrganizationService())
                .Where("firstname").Equals("John")
                .And
                .Where("lastname").Equals("Doe")
                .Delete()
                .Execute();

            FluentContact.Contact(context.GetOrganizationService())
                .Where("firstname").Equals("John")
                .And
                .Where("lastname").Equals("Doe")
                .Count( c => Assert.AreEqual(0, c))
                .Execute();
        }

        [TestMethod]
        public void TestJoin1()
        {
            var context = TestUtilities.TestContext2();
            var account1 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account1")).First().Value;
            var account2 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account2")).First().Value;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();

            // Join account to contact entity via the primary
            FluentAccount.Account(account2.Id)
                .Trace( s => Debug.WriteLine(s))
                .Join<FluentContact>( c => c.UseAttribute<string>(s => Assert.AreEqual("Watson", s) , "lastname"))
                .Execute();

            var fa1 = FluentAccount.Account(account1.Id)
                    .Trace(s => Debug.WriteLine(s))
                    .Join<FluentPrimaryContact>(c => c.UseAttribute<string>(s => Assert.AreEqual("Doe", s), "lastname"))
                    .UseAttribute((string s) => Debug.WriteLine(s), "name")
                    .Count(c => Assert.AreEqual(1, c))
                ; //.Execute();

            var qe = ((FluentCRM.FluentCRM) fa1)._queryExpression;

            // This kind of join seems not to work in FakeXrmEasy. Does work with a real CRM system.
            //fa1.Execute();

            // Ensure Environment variable "Password" is set.
            var cnString = ConfigurationManager.ConnectionStrings["CrmOnline"].ConnectionString;
            cnString = Environment.ExpandEnvironmentVariables(cnString);
            using (var crmSvc = new CrmServiceClient(cnString))
            {
                var orgService = crmSvc.OrganizationServiceProxy;

                var fetchXmlQuery = new QueryExpressionToFetchXmlRequest {Query = qe};
                var response = (QueryExpressionToFetchXmlResponse)orgService.Execute(fetchXmlQuery);

                Debug.WriteLine( response.FetchXml);
                var accountId = Guid.Empty;
                FluentAccount.Account(orgService).Where("name").Equals("Alpine Ski House").UseAttribute((Guid id) => accountId= id, "accountid").Execute();

                //var accountId = new Guid("AAA19CDD-88DF-E311-B8E5-6C3BE5A8B200");
                FluentAccount.Account(accountId, orgService)
                        .Trace(s => Debug.WriteLine(s))
                        .Join<FluentPrimaryContact>(c => c.UseAttribute<string>(s => Assert.AreEqual("Cook", s), "lastname"))
                        .UseAttribute((string s) => Debug.WriteLine(s), "name")
                        .Count(c => Assert.AreEqual(1, c))
                        .Execute();
            }
        }

    }
}
