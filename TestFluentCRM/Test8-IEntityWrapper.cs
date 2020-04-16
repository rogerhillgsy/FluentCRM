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
using Microsoft.Xrm.Sdk.Metadata;
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

        private static object _testLock = new object();

        [TestMethod]
        public void TestIndexing()
        {
            EntityWrapper ew = null;

            FluentAccount.Account()
                .Where("name").Equals("Account1")
                .UseEntity((w) => ew = w, FluentCRM.FluentCRM.AllColumns)
                .Execute();

            Assert.IsNotNull(ew);

            Assert.AreEqual("Account1", ew["name"]);
            Assert.AreEqual("Account name 2", ew["name2"]);
            Assert.AreEqual("name3", ew["name3"]);
            Assert.AreEqual("123456", ew["phone1"]);
            Assert.AreEqual("UK", ew["address1_country"]);
            Assert.AreEqual(0, ew["statecode"]);
            Assert.AreEqual("", ew["description"]);
            Assert.IsNull(ew["missing"]);
        }

        [TestMethod]
        public void TestGetAttributeValue()
        {
            EntityWrapper ew = null;

            FluentAccount.Account()
                .Where("name").Equals("Account1")
                .UseEntity((w) => ew = w, FluentCRM.FluentCRM.AllColumns)
                .Execute();

            Assert.IsNotNull(ew);

            Assert.AreEqual("Account1", ew.GetAttributeValue<string>("name"));
            Assert.AreEqual("Account name 2", ew.GetAttributeValue<string>("name2"));
            Assert.AreEqual("name3", ew.GetAttributeValue<string>("name3"));
            Assert.AreEqual("123456", ew.GetAttributeValue<string>("phone1"));
            Assert.AreEqual("UK", ew.GetAttributeValue<string>("address1_country"));
            Assert.AreEqual(0, ew.GetAttributeValue<int>("statecode"));
            Assert.AreEqual("", ew.GetAttributeValue<string>("description"));
            Assert.ThrowsException<InvalidCastException>(() => ew.GetAttributeValue<int>("description"));
            Assert.IsNull(ew.GetAttributeValue<string>("missing"));
        }

        [TestMethod]
        public void TestContains()
        {
            EntityWrapper ew = null;

            FluentContact.Contact()
                .Where("firstname").Equals("Sam")
                .UseEntity((w) => ew = w, "firstname", "lastname", "noname")
                .Execute();

            Assert.IsNotNull(ew);

            Assert.IsTrue(ew.Contains("firstname"));
            Assert.IsTrue(ew.Contains("lastname"));
            Assert.IsFalse(ew.Contains("noname"));
            Assert.IsFalse(ew.Contains("mobilephone"));
            Assert.IsFalse(ew.Contains("phone"));
            Assert.IsFalse(ew.Contains("parentcustomerid"));
        }

        [TestMethod]
        public void TestTrace()
        {
            lock (_testLock)
            {
                EntityWrapper ew = null;
                var traceText = new StringBuilder();
                EntityWrapper.SetTracing(null);
                FluentAccount.Account().Where("name").Equals("Account1")
                    .Trace((s) => traceText.Append(s))
                    .UseEntity((a) => ew = a, "name")
                    .Execute();

                Assert.IsNotNull(ew);

                var t1 = traceText.ToString();

                var missingAttr = ew["notpresent"];

                Assert.AreNotEqual(t1, traceText.ToString());
                Assert.IsTrue(traceText.ToString().Contains("Attribute not found: notpresent"));
            }
        }

        [TestMethod]
        public void TestId()
        {
            var account1 = _context.Data["account"].First(a => a.Value.GetAttributeValue<string>("name") == "Account1")
                .Value;

            EntityWrapper ew = null;
            FluentAccount.Account().Where("name").Equals("Account1")
                .UseEntity((a) => ew = a, "name")
                .Execute();

            Assert.IsNotNull(ew);

            Assert.AreEqual(account1.Id, ew.Id);
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
                }, "name")
                .Join<FluentPrimaryContact>(
                    c => c.UseEntity((e, a) =>
                    {
                        ew = e;
                        ewcAlias = e.Alias;
                    }, "firstname", "lastname"))
                .UseEntity((e) =>
                {
                    ewa2Alias = e.Alias;
                    ewa2 = e;
                }, "name")
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
            Assert.IsFalse(string.IsNullOrEmpty(ewcAlias));

            Assert.IsTrue(ew.Contains("name"));
            Assert.IsTrue(ew.Contains(ewa1Alias + "name"));
            Assert.IsTrue(ew.Contains("name"));
            Assert.IsTrue(ew.Contains(ewa1Alias + "name"));
            Assert.IsFalse(ew.Contains("firstname"));
            Assert.IsTrue(ew.Contains(ewcAlias + "firstname"));
            Assert.IsFalse(ew.Contains("lastname"));
            Assert.IsTrue(ew.Contains(ewcAlias + "lastname"));
        }

        [TestMethod]
        public void TestEntityWrapperTypeMismatch()
        {
            lock (_testLock)
            {
                // Test that when there are multiple typing errors, we process all attributes before throwing an exception
                var calls = 0;
                var log = new StringBuilder();
                EntityWrapper.SetTracing(null);
                float fv = 0;
                Assert.ThrowsException<InvalidCastException>(() =>
                    FluentAccount.Account()
                        .Trace(s => log.AppendLine(s))
                        .Where("name").Equals("Account1")
                        .UseEntity(ew => { fv = ew.GetAttributeValue<float>("doubleWidth"); }, "doubleWidth")
                        .UseAttribute((string n) => Console.WriteLine($"Name {n}"), "name")
                        .Count((c) => Assert.AreEqual(0, c))
                        .Execute()
                );

                Console.WriteLine(log.ToString());
                Assert.AreEqual(0, calls);
                Assert.IsTrue(
                    log.ToString()
                        .Contains("For doubleWidth returned type System.Double but expected type System.Single"),
                    $"Expected message for doubleWidth not found in ${log.ToString()}");
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
                Assert.IsTrue(
                    log.ToString()
                        .Contains("For doubleWidth returned type System.Double but expected type System.Single"),
                    $"Expected message for doubleWidth not found in ${log.ToString()}");
            }
        }

        [TestMethod]
        public void TestEntityWrapperOptionSetSeeding1()
        {
            var log = new StringBuilder();
            var label = string.Empty;

            EntityWrapper.Testing.AddToOptionSetCache("account/type", "Test Option", 5);
            EntityWrapper.Testing.AddToOptionSetCache("account/type", "Test Option 2", 100000);

            FluentAccount.Account()
                .Trace(s => log.AppendLine(s))
                .Where("name").Equals("Account1")
                .UseEntity(ew =>
                {
                    // Get label for current option set value.
                    label = ew.OptionString("type");
                }, "type")
                .Count((c) => Assert.AreEqual(1, c))
                .Execute();

            Assert.AreEqual("Test Option", label);

        }

        [TestMethod]
        public void TestEntityWrapperOptionSetSeeding2()
        {
            var log = new StringBuilder();
            var label = string.Empty;

            EntityWrapper.Testing.AddToOptionSetCache("account/type", "Test Option", 5);
            EntityWrapper.Testing.AddToOptionSetCache("account/type", "Test Option 2", 100000);


            /// Try again with a non-existent value
            var account1 = _context.Data["account"].First(a => a.Value.GetAttributeValue<string>("name") == "Account1")
                .Value;
            account1["type"] = new OptionSetValue(6);
            FluentAccount.Account()
                .Trace(s => log.AppendLine(s))
                .Where("name").Equals("Account1")
                .UseEntity(ew =>
                {
                    // Get label for current option set value.
                    label = ew.OptionString("type");
                }, "type")
                .Count((c) => Assert.AreEqual(1, c))
                .Execute();
        }

        [TestMethod]
        public void TestEntityWrapperOptionSetSeeding3()
        {
            var log = new StringBuilder();
            var label = string.Empty;

            EntityWrapper.Testing.AddToOptionSetCache("account/type", "Test Option", 5);
            EntityWrapper.Testing.AddToOptionSetCache("account/type", "Test Option 2", 100000);


            // With an attribute that is not a string.
            Assert.ThrowsException<ArgumentException>(() =>
                FluentAccount.Account()
                    .Trace(s => log.AppendLine(s))
                    .Where("name").Equals("Account1")
                    .UseEntity(ew =>
                    {
                        // Get label for current option set value.
                        label = ew.OptionString("name2");
                    }, "type", "name2")
                    .Count((c) => Assert.AreEqual(1, c))
                    .Execute());
        }

        [TestMethod]
        public void TestEntityWrapperOptionSetSeeding4()
        {
            var log = new StringBuilder();
            var label = string.Empty;

            EntityWrapper.Testing.AddToOptionSetCache("account/type", "Test Option", 5);
            EntityWrapper.Testing.AddToOptionSetCache("account/type", "Test Option 2", 100000);

            Assert.ThrowsException<ArgumentException>(() =>
                FluentAccount.Account()
                    .Trace(s => log.AppendLine(s))
                    .Where("name").Equals("Account1")
                    .UseEntity(ew =>
                    {
                        // Get label for current option set value.
                        label = ew.OptionString("name2");
                    }, "type", "name2")
                    .Count((c) => Assert.AreEqual(1, c))
                    .Execute());

        }

        [TestMethod]
        public void TestEntityWrapperSetTracing()
        {
            lock (_testLock)
            {

                var sb1 = new StringBuilder();
                Action<string> trace1 = (s) => sb1.AppendLine(s);
                var ew = new EntityWrapper(new Entity("account", Guid.NewGuid()) {["opts"] = new OptionSetValue(1)},
                    _orgService, trace1);
                EntityWrapper.SetTracing(trace1);

                EntityWrapper.Testing.AddToOptionSetCache("account/opts", "Option 1", 1);
                var opt = ew.OptionString("opts");
                Assert.IsFalse(string.IsNullOrEmpty(sb1.ToString()));

                sb1.Clear();
                var sb2 = new StringBuilder();
                Action<string> trace2 = (s) => sb2.AppendLine(s);
                EntityWrapper.SetTracing(trace2);

                opt = ew.OptionString("opts");
                Assert.IsTrue(string.IsNullOrEmpty(sb1.ToString()));
                Assert.IsFalse(string.IsNullOrEmpty(sb2.ToString()));
            }
        }

        [TestMethod]
        public void TestEntityWrapperSetMetadata()
        {
            var meta = new EntityNameAttributeMetadata("optionsetattr")
            {
                OptionSet = new OptionSetMetadata()
                {Options =
                    {
                        new OptionMetadata( new Label() { UserLocalizedLabel = new LocalizedLabel("Option 1", 1033)}, 1),
                        new OptionMetadata( new Label() { UserLocalizedLabel = new LocalizedLabel("Option 2", 1033)}, 2)
                    }
                }
            };

            EntityWrapper.Testing.AddToOptionSetCache("account/optionsetattr", meta);

            var ew = new EntityWrapper(new Entity("account", Guid.NewGuid()) { ["optionsetattr"] = new OptionSetValue(1) }, _orgService, null);
            var opt = ew.OptionString("optionsetattr");

            Assert.AreEqual("Option 1", opt);
        }

        [TestMethod]
        public void TestEntityWrapperDump()
        {
            var meta = new EntityNameAttributeMetadata("optionsetattr")
            {
                OptionSet = new OptionSetMetadata()
                {
                    Options =
                    {
                        new OptionMetadata( new Label() { UserLocalizedLabel = new LocalizedLabel("Option 1", 1033)}, 1),
                        new OptionMetadata( new Label() { UserLocalizedLabel = new LocalizedLabel("Option 2", 1033)}, 2)
                    }
                }
            };

            EntityWrapper.Testing.AddToOptionSetCache("account/optionsetattr", meta);

            var sb = new StringBuilder();
            EntityWrapper.Testing.Dump(s => sb.AppendLine(s));

            var output = sb.ToString();
            Assert.IsFalse(string.IsNullOrEmpty(output));
            Assert.IsTrue( output.Contains( "Option 1"));
            Assert.IsTrue(output.Contains("Option 2"));
        }

    }
    }
