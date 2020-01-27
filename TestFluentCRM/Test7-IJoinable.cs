using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FakeXrmEasy;
using FluentCRM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

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

        /// <summary>
        /// Test use of default values
        /// </summary>
        [TestMethod]
        public void TestUseAttribute4()
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
                    c => c.UseAttribute<string>("Smith", s =>
                    {
                        called = true;
                        Assert.AreEqual("Smith", s);
                    }, "lastnamemissing"))
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

        // Test form of UseEntity where actual entity name is passed to closure.
        [TestMethod]
        public void TestUseEntity3()
        {
            // Tests the mechanics of the join call
            var context = TestUtilities.TestContext2();
            var account2 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account2")).First().Value;
            var called = false;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();
            
            // Join account to contact entity via the primary
            FluentAccount.Account(account2.Id)
                .Trace(s => Debug.WriteLine(s))
                .Join<FluentPrimaryContact>(
                    c => c.UseEntity((name, e, alias) =>
                        {
                            Assert.AreEqual(e.Alias + "firstname", name);
                            Assert.AreEqual(alias + "firstname", name);
                            Debug.WriteLine(
                                $"Joined entity called with element logical name {e.Entity.LogicalName}, alias {alias}");
                        }, "firstname")
                        .UseAttribute<string>(s =>
                        {
                            called = true;
                            Assert.AreEqual("Watson", s);
                        }, "lastname"))
                .UseEntity((name, e) =>
                {
                    Assert.AreEqual("name", name);
                    Debug.WriteLine(e.Entity.LogicalName);
                }, "name")
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

            Assert.IsTrue(called);
        }

        // Test form of UseEntity where actual entity name is passed to closure.
        [TestMethod]
        public void TestUseEntity4()
        {
            // Tests the mechanics of the join call
            var context = TestUtilities.TestContext2();
            var account2 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account2")).First().Value;
            var called = false;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();
            
            // Join account to contact entity via the primary
            FluentAccount.Account(account2.Id)
                .Trace(s => Debug.WriteLine(s))
                .Join<FluentPrimaryContact>(
                    c => c.Where("firstname").IsNotNull.And.Where("lastname").IsNotNull.UseEntity((name, e, alias) =>
                        {
                            Assert.AreEqual(e.Alias + "firstname", name);
                            Assert.AreEqual(alias + "firstname", name);
                            Debug.WriteLine(
                                $"Joined entity called with element logical name {e.Entity.LogicalName}, alias {alias}");
                        }, "firstname")
                        .UseAttribute<string>(s =>
                        {
                            called = true;
                            Assert.AreEqual("Watson", s);
                        }, "lastname")
                )
                .UseEntity((name, e) =>
                {
                    Assert.AreEqual("name", name);
                    Debug.WriteLine(e.Entity.LogicalName);
                }, "name")
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

            Assert.IsTrue(called);
        }

        // Test form of UseEntity where actual entity name is passed to closure.
        [TestMethod]
        public void TestJoinWhereCriteria()
        {
            // Tests the mechanics of the join call
            var context = TestUtilities.TestContext2();
            var account2 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account2")).First().Value;
            var called = false;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();
            
            // Join account to contact entity via the primary
            FluentAccount.Account(account2.Id)
                .Trace(s => Debug.WriteLine(s))
                .Join<FluentPrimaryContact>(
                    c => c.Where("firstname").Equals("John").And.Where("lastname").Equals("Watson").UseEntity((name, e, alias) =>
                        {
                            Assert.AreEqual(e.Alias + "firstname", name);
                            Assert.AreEqual(alias + "firstname", name);
                            Debug.WriteLine(
                                $"Joined entity called with element logical name {e.Entity.LogicalName}, alias {alias}");
                        }, "firstname")
                        .UseAttribute<string>(s =>
                        {
                            called = true;
                            Assert.AreEqual("Watson", s);
                        }, "lastname"))
                .UseEntity((name, e) =>
                {
                    Assert.AreEqual("name", name);
                    Debug.WriteLine(e.Entity.LogicalName);
                }, "name")
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

            Assert.IsTrue(called);
        }

        // Test form of UseEntity where actual entity name is passed to closure.
        [TestMethod]
        public void TestJoinWhereCriteria2()
        {
            // Tests the mechanics of the join call
            var context = TestUtilities.TestContext2();
            var account1 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account1")).First().Value;
            var called = false;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();
            
            // Join account to contact entity via the primary
            FluentAccount.Account(account1.Id)
                .Trace(s => Debug.WriteLine(s))
                .Join<FluentContact>(
                    c => c.Where("firstname").NotEqual("John").And.Where("telephone1").IsNull.UseEntity((name, e, alias) =>
                        {
                            Assert.AreEqual(e.Alias + "firstname", name);
                            Assert.AreEqual(alias + "firstname", name);
                            Debug.WriteLine(
                                $"Joined entity called with element logical name {e.Entity.LogicalName}, alias {alias}");
                        }, "firstname")
                        .UseAttribute<string>(s =>
                        {
                            called = true;
                            Assert.AreEqual("Spade", s);
                        }, "lastname"))
                .UseEntity((name, e) =>
                {
                    Assert.AreEqual("name", name);
                    Debug.WriteLine(e.Entity.LogicalName);
                }, "name")
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

            Assert.IsTrue(called);


            // Join account to contact entity via the primary
FluentAccount.Account(account1.Id)
.Join<FluentContact>(
    c => c.UseAttribute<string>(s =>
        {
            Debug.WriteLine(s);
        }, "lastname"))
.Execute();

FluentAccount.Account(account1.Id)
    .Join<FluentPrimaryContact>(
        c => c.UseAttribute<string>(s =>
        {
            Debug.WriteLine(s);
        }, "lastname"))
    .Execute();


        }

        // Test form of UseEntity where actual entity name is passed to closure.
        [TestMethod]
        public void TestJoinWhereCriteriaGreaterThan()
        {
            // Tests the mechanics of the join call
            var context = TestUtilities.TestContext2();
            var account1 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account1")).First().Value;
            var called = false;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();
            
            // Join account to contact entity via the primary
            FluentAccount.Account(account1.Id)
                .Trace(s => Debug.WriteLine(s))
                .Join<FluentContact>(
                    c => c.Where("size").GreaterThan(5).UseEntity((name, e, alias) =>
                        {
                            Assert.AreEqual(e.Alias + "firstname", name);
                            Assert.AreEqual(alias + "firstname", name);
                            Debug.WriteLine(
                                $"Joined entity called with element logical name {e.Entity.LogicalName}, alias {alias}");
                        }, "firstname")
                        .UseAttribute<string>(s =>
                        {
                            called = true;
                            Assert.AreEqual("Spade", s);
                        }, "lastname"))
                .UseEntity((name, e) =>
                {
                    Assert.AreEqual("name", name);
                    Debug.WriteLine(e.Entity.LogicalName);
                }, "name")
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

            Assert.IsTrue(called);
        }

        // Test form of UseEntity where actual entity name is passed to closure.
        [TestMethod]
        public void TestJoinWhereCriteriaLessThan()
        {
            // Tests the mechanics of the join call
            var context = TestUtilities.TestContext2();
            var account1 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account1")).First().Value;
            var called = false;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();
            
            // Join account to contact entity via the primary
            FluentAccount.Account(account1.Id)
                .Trace(s => Debug.WriteLine(s))
                .Join<FluentContact>(
                    c => c.Where("size").LessThan(6).UseEntity((name, e, alias) =>
                        {
                            Assert.AreEqual(e.Alias + "firstname", name);
                            Assert.AreEqual(alias + "firstname", name);
                            Debug.WriteLine(
                                $"Joined entity called with element logical name {e.Entity.LogicalName}, alias {alias}");
                        }, "firstname")
                        .UseAttribute<string>(s =>
                        {
                            called = true;
                            Assert.AreEqual("Doe", s);
                        }, "lastname"))
                .UseEntity((name, e) =>
                {
                    Assert.AreEqual("name", name);
                    Debug.WriteLine(e.Entity.LogicalName);
                }, "name")
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

            Assert.IsTrue(called);
        }

        // Test form of UseEntity where actual entity name is passed to closure.
        [TestMethod]
        public void TestJoinWhereCriteriaBeginsWith()
        {
            // Tests the mechanics of the join call
            var context = TestUtilities.TestContext2();
            var account1 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account1")).First().Value;
            var called = false;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();
            
            // Join account to contact entity via the primary
            FluentAccount.Account(account1.Id)
                .Trace(s => Debug.WriteLine(s))
                .Join<FluentContact>(
                    c => c.Where("telephone2").BeginsWith("3456").UseEntity((name, e, alias) =>
                        {
                            Assert.AreEqual(e.Alias + "firstname", name);
                            Assert.AreEqual(alias + "firstname", name);
                            Debug.WriteLine(
                                $"Joined entity called with element logical name {e.Entity.LogicalName}, alias {alias}");
                        }, "firstname")
                        .UseAttribute<string>(s =>
                        {
                            called = true;
                            Assert.AreEqual("Spade", s);
                        }, "lastname"))
                .UseEntity((name, e) =>
                {
                    Assert.AreEqual("name", name);
                    Debug.WriteLine(e.Entity.LogicalName);
                }, "name")
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

            Assert.IsTrue(called);
        }

        // Test form of UseEntity where actual entity name is passed to closure.
        [TestMethod]
        public void TestJoinWhereCriteriaCondition()
        {
            // Tests the mechanics of the join call
            var context = TestUtilities.TestContext2();
            var account1 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account1")).First().Value;
            var called = false;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();
            
            // Join account to contact entity via the primary
            FluentAccount.Account(account1.Id)
                .Trace(s => Debug.WriteLine(s))
                .Join<FluentContact>(
                    c => c.Where("lastname").Condition( ConditionOperator.EndsWith, "de").UseEntity((name, e, alias) =>
                        {
                            Assert.AreEqual(e.Alias + "firstname", name);
                            Assert.AreEqual(alias + "firstname", name);
                            Debug.WriteLine(
                                $"Joined entity called with element logical name {e.Entity.LogicalName}, alias {alias}");
                        }, "firstname")
                        .UseAttribute<string>(s =>
                        {
                            called = true;
                            Assert.AreEqual("Spade", s);
                        }, "lastname"))
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

            void ProcessAttribute(string attr, string val)
            {
                Debug.WriteLine($"ProcessAttribute :{attr}/{val}");
                calls++;
            }

            // Look for contacts of account
            FluentAccount.Account()
                .Where("name").Equals("Account1")
                .Join<FluentContact>(
                    c => c.UseAttribute((Action<string, string>) ProcessAttribute, "firstname")
                        .UseAttribute((Action<string, string>) ProcessAttribute, "lastname")
                        .UseAttribute((Action<string, string>) ProcessAttribute, "firstname")
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

            void ProcessAttribute(string attr, string val)
            {
                Debug.WriteLine($"ProcessAttribute :{attr}/{val}");
                calls++;
            }

            FluentAccount.Account()
                .Where("name").Equals("Account1")
                .Join<FluentContact>(
                    c =>
                    {
                        Assert.AreEqual("contact", c.LogicalName);
                        c.UseAttribute((Action<string, string>) ProcessAttribute, "firstname")
                             .UseAttribute((Action<string, string>) ProcessAttribute, "lastname")
                             .UseAttribute((Action<string, string>) ProcessAttribute, "firstname");
                    })
                .Exists((e) => Assert.IsTrue(e))
                .Count((c) => Assert.AreEqual(2, c))
                .Execute();
        }

        [TestMethod]
        public void TestJoinAttribute()
        {
            var calls = 0;

            void ProcessAttribute(string attr, string val)
            {
                Debug.WriteLine($"ProcessAttribute :{attr}/{val}");
                calls++;
            }

            FluentAccount.Account()
                .Where("name").Equals("Account1")
                .Join<FluentContact>(
                    c =>
                    {
                        Assert.AreEqual( "parentcustomerid", c.JoinAttribute("account"));
                        Assert.AreEqual( "contactid", c.JoinAttribute("annotation"));
                        c.UseAttribute((Action<string, string>) ProcessAttribute, "firstname")
                            .UseAttribute((Action<string, string>) ProcessAttribute, "firstname");
                    })
                .Exists((e) => Assert.IsTrue(e))
                .Count((c) => Assert.AreEqual(2, c))
                .Execute();
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

        /// <summary>
        /// Test issue #3 When getting joined attribute, typing errors cause an exception.
        /// </summary>
        [TestMethod]
        public void TestOuterUseAttributeTypeMismatch()
        {
            // return join of account3 and primary contact 
            var calls = 0;
            var log = new StringBuilder();
            Assert.ThrowsException<InvalidCastException>(() =>
                FluentAccount.Account()
                    .Trace( s => log.AppendLine(s))
                    .Where("name").Equals("Account1")
                    .Join<FluentPrimaryContact>(
                        c => c.UseAttribute((int n) => calls++, "phone"))
                    .Count((c) => Assert.AreEqual(0, c))
                    .Execute()
            );

            Console.WriteLine(log.ToString());
            Assert.AreEqual(0, calls);
            Assert.IsTrue(log.ToString().Contains("For a1.phone returned type System.String but expected type System.Int32"));
        }

        [TestMethod]
        public void TestOuterUseAttributeTypeMismatch2()
        {
            // Test typing error in UseAttribute after join
            var calls = 0;
            var log = new StringBuilder();
            float  fv = 0;
            double dv = 0;
            Assert.ThrowsException<InvalidCastException>(() =>
                FluentAccount.Account()
                    .Trace(s => log.AppendLine(s))
                    .Where("name").Equals("Account1")
                    .Join<FluentPrimaryContact>(
                        c => c.UseAttribute((double n) => dv = n, "doubleHeight"))
                    .UseAttribute( (float f ) => fv = f, "doubleWidth")
                    .Count((c) => Assert.AreEqual(0, c))
                    .Execute()
            );

            Assert.AreEqual(0, calls);
            Assert.IsTrue(log.ToString().Contains("For doubleWidth returned type System.Double but expected type System.Single"), $"Expected message not found in ${log.ToString()}");
            Console.WriteLine(log.ToString());
        }

        [TestMethod]
        public void TestOuterUseAttributeTypeMismatch3()
        {
            // Test that when there are multiple typing errors, we process all attributes before throwing an exception
            var calls = 0;
            var log = new StringBuilder();
            float fv = 0;
            int iv = 0;
            Assert.ThrowsException<InvalidCastException>(() =>
                FluentAccount.Account()
                    .Trace(s => log.AppendLine(s))
                    .Where("name").Equals("Account1")
                    .UseAttribute((float f) => fv = f, "doubleWidth")
                    .UseAttribute((int f) => iv = f, "phone1")
                    .UseAttribute( (string n) => Console.WriteLine($"Name {n}"), "name")
                    .Count((c) => Assert.AreEqual(0, c))
                    .Execute()
            );

            Console.WriteLine(log.ToString());
            Assert.AreEqual(0, calls);
            Assert.IsTrue(log.ToString().Contains("For doubleWidth returned type System.Double but expected type System.Single"), $"Expected message for doubleWidth not found in ${log.ToString()}");
            Assert.IsTrue(log.ToString().Contains("For phone1 returned type System.String but expected type System.Int32"), $"Expected message for phone1 not found in ${log.ToString()}");
        }

    }
}
