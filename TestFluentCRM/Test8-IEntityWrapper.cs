using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using FluentCRM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

namespace TestFluentCRM
{
    [TestClass]
    public class Test8_IEntityWrapper
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
        public void TestIndexing()
        {
            EntityWrapper ew = null;

            FluentAccount.Account()
                .Where("name").Equals("Account1")
                .UseEntity((w) => ew = w, FluentCRM.FluentCRM.AllColumns)
                .Execute();

            Assert.IsNotNull( ew );

            Assert.AreEqual( "Account1", ew["name"] );
            Assert.AreEqual( "Account name 2", ew["name2"] );
            Assert.AreEqual( "name3", ew["name3"] );
            Assert.AreEqual( "123456", ew["phone1"] );
            Assert.AreEqual( "UK", ew["address1_country"] );
            Assert.AreEqual( 0, ew["statecode"] );
            Assert.AreEqual( "", ew["description"] );
            Assert.IsNull( ew["missing"] );
        }

        [TestMethod]
        public void TestGetAttributeValue()
        {
            EntityWrapper ew = null;

            FluentAccount.Account()
                .Where("name").Equals("Account1")
                .UseEntity((w) => ew = w, FluentCRM.FluentCRM.AllColumns )
                .Execute();

            Assert.IsNotNull( ew );

            Assert.AreEqual( "Account1", ew.GetAttributeValue<string>("name") );
            Assert.AreEqual( "Account name 2", ew.GetAttributeValue<string>("name2"));
            Assert.AreEqual( "name3", ew.GetAttributeValue<string>("name3") );
            Assert.AreEqual( "123456", ew.GetAttributeValue<string>("phone1") );
            Assert.AreEqual( "UK", ew.GetAttributeValue<string>("address1_country") );
            Assert.AreEqual( 0, ew.GetAttributeValue<int>("statecode") );
            Assert.AreEqual( "", ew.GetAttributeValue<string>("description") );
            Assert.ThrowsException<InvalidCastException>(() => ew.GetAttributeValue<int>("description"));
            Assert.IsNull(  ew.GetAttributeValue<string>("missing") );
        }

        [TestMethod]
        public void TestContains()
        {
            EntityWrapper ew = null;

            FluentContact.Contact()
                .Where("firstname").Equals("Sam")
                .UseEntity((w) => ew = w, "firstname", "lastname", "noname" )
                .Execute();

            Assert.IsNotNull( ew );

            Assert.IsTrue( ew.Contains("firstname"));
            Assert.IsTrue( ew.Contains("lastname"));
            Assert.IsFalse( ew.Contains("noname"));
            Assert.IsFalse( ew.Contains("mobilephone"));
            Assert.IsFalse( ew.Contains("phone"));
            Assert.IsFalse( ew.Contains("parentcustomerid"));
        }

        [TestMethod]
        public void TestTrace()
        {
            EntityWrapper ew = null;
            var traceText = new StringBuilder();
            FluentAccount.Account().Where("name").Equals("Account1")
                .Trace( (s) => traceText.Append(s))
                .UseEntity((a) => ew = a, "name")
                .Execute();

            Assert.IsNotNull(ew);

            var t1 = traceText.ToString();

            var missingAttr = ew["notpresent"];

            Assert.AreNotEqual(t1, traceText.ToString());
            Assert.IsTrue(traceText.ToString().Contains("Attribute not found: notpresent"));
        }

        [TestMethod]
        public void TestId()
        {
            var account1 = _context.Data["account"].First(a => a.Value.GetAttributeValue<string>("name") == "Account1").Value;

            EntityWrapper ew = null;
            FluentAccount.Account().Where("name").Equals("Account1")
                .UseEntity((a) => ew = a, "name")
                .Execute();

            Assert.IsNotNull(ew);

            Assert.AreEqual( account1.Id, ew.Id);
        }

        [TestMethod]
        public void TestJoin1()
        {
            EntityWrapper ew = null;
            EntityWrapper ewa1 = null;
            EntityWrapper ewa2 = null;
            var ewa1Alias = string.Empty;
            var ewa2Alias = string.Empty;
            var ewcAlias = string.Empty;
            FluentAccount.Account()
                .Where("name").Equals("Account1")
                .UseEntity((e) =>
                {
                    ewa1Alias = e.Alias;
                    ewa1 = e;
                }, "name" )
                .Join<FluentPrimaryContact>(
                    c => c.UseEntity((e, a) =>
                    {
                        ew = e;
                        ewcAlias = e.Alias;
                    },  "firstname", "lastname"))
                .UseEntity((e) =>
                {
                     ewa2Alias = e.Alias;
                    ewa2 = e;
                }, "name" )
                .Exists((e) => Assert.IsTrue(e))
                .Execute();

            // Note that the same EntityWrapper is passed to all of the closures above.
            // The only thing that should vary between them is the value of the entity.Alias attribute.
            Assert.IsNotNull(ew);
            Assert.IsNotNull(ewa1);
            Assert.IsNotNull(ewa2);

            Assert.AreSame(ew, ewa1);
            Assert.AreSame(ew, ewa2);

            Assert.IsTrue(string.IsNullOrEmpty(ewa1Alias));
            Assert.IsTrue(string.IsNullOrEmpty(ewa2Alias));
            Assert.IsFalse( string.IsNullOrEmpty(ewcAlias));

            Assert.IsTrue( ew.Contains( "name" ));
            Assert.IsTrue( ew.Contains( ewa1Alias +  "name" ));
            Assert.IsTrue( ew.Contains( "name" ));
            Assert.IsTrue( ew.Contains( ewa1Alias +  "name" ));
            Assert.IsFalse( ew.Contains( "firstname" ));
            Assert.IsTrue( ew.Contains( ewcAlias +  "firstname" ));
            Assert.IsFalse( ew.Contains( "lastname" ));
            Assert.IsTrue( ew.Contains( ewcAlias +  "lastname" ));
        }

        [TestMethod]
        public void TestEntityWrapperTypeMismatch()
        {
            // Test that when there are multiple typing errors, we process all attributes before throwing an exception
            var calls = 0;
            var log = new StringBuilder();
            float fv = 0;
            Assert.ThrowsException<InvalidCastException>(() =>
                FluentAccount.Account()
                    .Trace(s => log.AppendLine(s))
                    .Where("name").Equals("Account1")
                    .UseEntity(ew => { fv = ew.GetAttributeValue<float>("doubleWidth"); },"doubleWidth")
                    .UseAttribute((string n) => Console.WriteLine($"Name {n}"), "name")
                    .Count((c) => Assert.AreEqual(0, c))
                    .Execute()
            );

            Console.WriteLine(log.ToString());
            Assert.AreEqual(0, calls);
            Assert.IsTrue(log.ToString().Contains("For doubleWidth returned type System.Double but expected type System.Single"), $"Expected message for doubleWidth not found in ${log.ToString()}");
            log.Clear();
            fv = 10;

            FluentAccount.Account()
                .Trace(s => log.AppendLine(s))
                .Where("name").Equals("Account1")
                .UseEntity(ew => { fv = ew.GetAttributeValue<float>("doubleWidth", false); }, "doubleWidth")
                .UseAttribute((string n) => Console.WriteLine($"Name {n}"), "name")
                .Count((c) => Assert.AreEqual(1, c))
                .Execute();

            Console.WriteLine(log.ToString());
            Assert.AreEqual(0, fv);
            Assert.AreEqual(0, calls);
            Assert.IsTrue(log.ToString().Contains("For doubleWidth returned type System.Double but expected type System.Single"), $"Expected message for doubleWidth not found in ${log.ToString()}");

        }

    }
}
