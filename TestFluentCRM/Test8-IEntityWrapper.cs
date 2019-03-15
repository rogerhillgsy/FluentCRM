using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FakeXrmEasy;
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
            EntityWrapper ewa1 = null;
            EntityWrapper ewa2 = null;
            EntityWrapper ewc = null;
            var ewcAlias = string.Empty;
            FluentAccount.Account()
                .Where("name").Equals("Account1")
                .UseEntity((e) => ewa1 = e, "name" )
                .Join<FluentPrimaryContact>(
                    c => c.UseEntity((e,a) =>
                    {
                        ewc = e;
                    },  "firstname", "lastname"))
                .UseEntity((e) => ewa2 = e, "name" )
                .Exists((e) => Assert.IsTrue(e))
                .Execute();

            Assert.IsNotNull(ewa1);
            Assert.IsNotNull(ewa2);
            Assert.IsNotNull(ewc);

            Assert.IsTrue(string.IsNullOrEmpty(ewa1.Alias));
            Assert.IsTrue(string.IsNullOrEmpty(ewa2.Alias));
            Assert.IsFalse( string.IsNullOrEmpty(ewc.Alias));

            Assert.IsTrue( ewa1.Contains( "name" ));
            Assert.IsTrue( ewa1.Contains( ewa1.Alias +  "name" ));
            Assert.IsTrue( ewa2.Contains( "name" ));
            Assert.IsTrue( ewa2.Contains( ewa1.Alias +  "name" ));
            Assert.IsFalse( ewc.Contains( "firstname" ));
            Assert.IsTrue( ewc.Contains( ewc.Alias +  "firstname" ));
            Assert.IsFalse( ewc.Contains( "lastname" ));
            Assert.IsTrue( ewc.Contains( ewc.Alias +  "lastname" ));
        }
    }
}
