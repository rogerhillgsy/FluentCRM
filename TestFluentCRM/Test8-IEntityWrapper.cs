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

        /// <summary>
        /// Test getting option set values. Note requires access to live CRM. So may have to ignore it
        /// Relies on access to a live CRM system with default test data loaded. (online trial eill do)
        /// </summary>
        [TestMethod]
        public void TestOptionString()
        {
            var cnString = ConfigurationManager.ConnectionStrings["CrmOnline"].ConnectionString;
            cnString = Environment.ExpandEnvironmentVariables(cnString);
            using (var crmSvc = new CrmServiceClient(cnString))
            {
                var accountid = Guid.Empty;
                EntityWrapper ew = null;
                FluentCRM.FluentCRM.StaticService = crmSvc.OrganizationServiceProxy;

                FluentAccount.Account().Where("name").Equals("Alpine Ski House")
                    .UseAttribute((Guid id) => accountid = id, "accountid")
                    .WeakUpdate<OptionSetValue>("customertypecode", new OptionSetValue(1)) // set to "Competitor"
                    .Exists( (e) => Assert.IsTrue(e, "Alpine Ski House account must exist"))
                    .Execute();

                // Fetch the option set back
                FluentAccount.Account(accountid)
                    .UseEntity((e) => ew = e, "customertypecode","name")
                    .Execute();

                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                // Get the string equivalent of the customertypecode.
                var reltype = ew.OptionString("customertypecode");

                stopwatch.Stop();
                Debug.WriteLine($"Stopwatch elapsed time #1 {stopwatch.ElapsedMilliseconds}ms");
                Assert.AreEqual("Competitor", reltype);

                stopwatch.Reset();
                stopwatch.Start();
                var reltype2 = ew.OptionString("customertypecode");

                Assert.AreEqual("Competitor", reltype2);

                stopwatch.Stop();
                Debug.WriteLine($"Stopwatch elapsed time from cache {stopwatch.ElapsedMilliseconds}ms");
                Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100);

                Assert.ThrowsException<ArgumentException>(() => ew.OptionString("name"));
            }
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
    }
}
