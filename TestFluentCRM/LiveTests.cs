using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using FluentCRM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using TestFluentCRM;
using Guid = System.Guid;


namespace TestFluentCRMLive
{
    /// <summary>
    /// Tests that rely on fresh a Dynamics CRM trial instance with test data.
    /// </summary>
    [TestClass]
    public class LiveTests
    {
        private string _connectionString;

        [TestInitialize]
        public void Setup()
        {
            // Obtain connection string from app.config and expand any environment variables e.g. for Passwords, usernames)
            _connectionString = ConfigurationManager.ConnectionStrings["CrmAppConnection"].ConnectionString;
            _connectionString = Environment.ExpandEnvironmentVariables(_connectionString);
        }

        /// <summary>
        /// Have a look at what settings values are returned by a live CRM system.
        /// </summary>
        [TestMethod]
        [TestCategory("LiveTest")]
        public void Test10_LiveUserSettings()
        {
            using (var crmSvc = new CrmServiceClient(_connectionString))
            {
                var orgService = crmSvc.OrganizationServiceProxy;

                FluentUserSettings.CurrentUserSettings(orgService)
                    .UseEntity((e) =>
                    {
                        foreach (var kv in e.Entity.Attributes.OrderBy((k) => k.Key))
                        {
                            // Debug.WriteLine($"{kv.Key, -40} - {kv.Value.ToString()}");
                            var val = kv.Value.ToString();
                            if (kv.Value is OptionSetValue)
                            {
                                val =
                                    $"new OptionSetValue({e.GetAttributeValue<OptionSetValue>(kv.Key).Value}), // {e.OptionString(kv.Key)}";
                            }
                            else if (kv.Value is DateTime)
                            {
                                val = $"new DateTime(\"{val}\"),";
                            }
                            else if (!(kv.Value is int || kv.Value is bool))
                            {
                                val = $"\"{val}\",";
                            }
                            else
                            {
                                val = $"{val},";
                            }

                            Debug.WriteLine($"[\"{kv.Key}\"] = {val}");
                        }
                    }, FluentCRM.FluentCRM.AllColumns)
                    .Count((c) => Assert.AreEqual(1, c))
                    .Execute();
            }

            // Usersettings read from live system.
            //var userSettings = new Entity("usersettings")
            //{
            //    ["addressbooksyncinterval"] = 86400000,
            //    ["advancedfindstartupmode"] = 1,
            //    ["allowemailcredentials"] = false,
            //    ["amdesignator"] = "AM",
            //    ["autocreatecontactonpromote"] = 1,
            //    ["businessunitid"] = "a61573a6-983d-e911-a81e-000d3a11e7a9",
            //    ["calendartype"] = 0,
            //    ["createdon"] = "03/03/2019 12:47:45",
            //    ["currencydecimalprecision"] = 2,
            //    ["currencyformatcode"] = 0,
            //    ["currencysymbol"] = "£",
            //    ["datavalidationmodeforexporttoexcel"] = new OptionSetValue(1), // None
            //    ["dateformatcode"] = 2,
            //    ["dateformatstring"] = "dd/MM/yyyy",
            //    ["dateseparator"] = "/",
            //    ["decimalsymbol"] = ".",
            //    ["defaultcalendarview"] = 0,
            //    ["entityformmode"] = new OptionSetValue(0), // Organization default
            //    ["fullnameconventioncode"] = 1,
            //    ["getstartedpanecontentenabled"] = true,
            //    ["helplanguageid"] = 1033,
            //    ["homepagearea"] = "<Default>",
            //    ["homepagesubarea"] = "",
            //    ["ignoreunsolicitedemail"] = true,
            //    ["incomingemailfilteringmethod"] =
            //        new OptionSetValue(1), // Email messages in response to Dynamics 365 email
            //    ["isappsforcrmalertdismissed"] = false,
            //    ["isautodatacaptureenabled"] = true,
            //    ["isdefaultcountrycodecheckenabled"] = false,
            //    ["isduplicatedetectionenabledwhengoingonline"] = false,
            //    ["isguidedhelpenabled"] = true,
            //    ["isresourcebookingexchangesyncenabled"] = false,
            //    ["issendasallowed"] = false,
            //    ["lastalertsviewedtime"] = "01/01/1900 00:00:00",
            //    ["localeid"] = 2057,
            //    ["longdateformatcode"] = 1,
            //    ["modifiedby"] = "Microsoft.Xrm.Sdk.EntityReference",
            //    ["modifiedon"] = "05/03/2019 10:27:51",
            //    ["negativecurrencyformatcode"] = 1,
            //    ["negativeformatcode"] = 1,
            //    ["numbergroupformat"] = "3",
            //    ["numberseparator"] = ",",
            //    ["offlinesyncinterval"] = 900000,
            //    ["outlooksyncinterval"] = 900000,
            //    ["paginglimit"] = 50,
            //    ["pmdesignator"] = "PM",
            //    ["pricingdecimalprecision"] = 2,
            //    ["reportscripterrors"] =
            //        new OptionSetValue(1), // Ask me for permission to send an error report to Microsoft
            //    ["resourcebookingexchangesyncversion"] = "0",
            //    ["showweeknumber"] = false,
            //    ["splitviewstate"] = true,
            //    ["synccontactcompany"] = true,
            //    ["systemuserid"] = "8c140611-11f7-4aeb-b55f-83a6261982ff",
            //    ["timeformatcode"] = 0,
            //    ["timeformatstring"] = "HH:mm",
            //    ["timeseparator"] = ":",
            //    ["timezonebias"] = 0,
            //    ["timezonecode"] = 92,
            //    ["timezonedaylightbias"] = 0,
            //    ["timezonedaylightday"] = 0,
            //    ["timezonedaylightdayofweek"] = 0,
            //    ["timezonedaylighthour"] = 0,
            //    ["timezonedaylightminute"] = 0,
            //    ["timezonedaylightmonth"] = 0,
            //    ["timezonedaylightsecond"] = 0,
            //    ["timezonedaylightyear"] = 0,
            //    ["timezonestandardbias"] = 0,
            //    ["timezonestandardday"] = 0,
            //    ["timezonestandarddayofweek"] = 0,
            //    ["timezonestandardhour"] = 0,
            //    ["timezonestandardminute"] = 0,
            //    ["timezonestandardmonth"] = 0,
            //    ["timezonestandardsecond"] = 0,
            //    ["timezonestandardyear"] = 0,
            //    ["trackingtokenid"] = 9,
            //    ["uilanguageid"] = 1033,
            //    ["usecrmformforappointment"] = false,
            //    ["usecrmformforcontact"] = true,
            //    ["usecrmformforemail"] = false,
            //    ["usecrmformfortask"] = true,
            //    ["useimagestrips"] = true,
            //    ["visualizationpanelayout"] = new OptionSetValue(1), // Side-by-side
            //    ["workdaystarttime"] = "08:00",
            //    ["workdaystoptime"] = "17:00",
            //};
        }

        [TestMethod]
        [TestCategory("LiveTest")]
        public void Test4_JoinLive()
        {
            // This kind of join seems not to work in FakeXrmEasy. Does work with a real CRM system.

            using (var crmSvc = new CrmServiceClient(_connectionString))
            {
                var orgService = crmSvc.OrganizationServiceProxy;

                //var fetchXmlQuery = new QueryExpressionToFetchXmlRequest {Query = qe};
                //var response = (QueryExpressionToFetchXmlResponse)orgService.Execute(fetchXmlQuery);

                //Debug.WriteLine( response.FetchXml);
                var accountId = Guid.Empty;
                FluentAccount.Account(orgService).Where("name").Equals("Alpine Ski House")
                    .UseAttribute((Guid id) => accountId = id, "accountid").Execute();

                //var accountId = new Guid("AAA19CDD-88DF-E311-B8E5-6C3BE5A8B200");
                FluentAccount.Account(accountId, orgService)
                    .Trace(s => Debug.WriteLine(s))
                    .Join<FluentPrimaryContact>(
                        c => c.UseAttribute<string>(s => Assert.AreEqual("Cook", s), "lastname"))
                    .UseAttribute((string s) => Debug.WriteLine(s), "name")
                    .Count(c => Assert.AreEqual(1, c))
                    .Execute();

                FluentAccount.Account(accountId, orgService)
                    .Trace(s => Debug.WriteLine(s))
                    .UseAttribute((string s) => Debug.WriteLine(s), "name")
                    .Join<FluentContact>(c => c.UseAttribute<string>(s => Assert.IsNotNull(s), "lastname"))
                    .Count(c => Assert.AreEqual(5, c))
                    .Execute();
            }

            var context = TestUtilities.TestContext2();
            var account2 = context.Data["account"]
                .Where(a => a.Value.GetAttributeValue<string>("name").Equals("Account2")).First().Value;

            FluentAccount.Account(account2.Id, context.GetOrganizationService())
                .Trace(s => Debug.WriteLine(s))
                .Join<FluentPrimaryContact>(
                    c => c.UseAttribute<string>(s => Assert.AreEqual("Watson", s), "lastname"))
                .UseAttribute((string s) => Debug.WriteLine(s), "name")
                .Count(c => Assert.AreEqual(1, c))
                .Execute();
        }

        [TestCategory("LiveTest")] // Not convinced that FetchXrmEasy supports linked entity order by clauses....
        [TestMethod]
        public void TestJoinOrderByDesc()
        {
            // Tests the mechanics of the join call
            var context = TestUtilities.TestContext2();
            var account1 = context.Data["account"].Where(a => a.Value.GetAttributeValue<string>("name") .Equals("Account1")).First().Value;
            var called = false;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();
            var expected = new[] { "Sam","John"};
            var current = 0;

            Debug.WriteLine($"Account1 Id is {account1.Id}");
            
            // Join account to contact entity via the primary
            FluentAccount.Account(account1.Id )
                .Trace(s => Debug.WriteLine(s))
                .UseAttribute((string s) => Debug.WriteLine(s), "name")
                .Join<FluentPrimaryContact>(
                    c => c.UseAttribute<string>(s =>
                        {
                            called = true;
                            Assert.AreEqual(expected[current++], s);
                        }, "firstname")
                        .OrderByDesc("firstname"))
                .Count(c => Assert.AreEqual(1, c))
                .Execute();

            Assert.IsTrue(called);
        }


        [TestMethod]
        [TestCategory("LiveTest")]
        public void SetPrimaryContact()
        {
            // Set the primary contact on alpine ski house if it is not set in the test org.
            using (var crmSvc = new CrmServiceClient(_connectionString))
            {
                var orgService = crmSvc.OrganizationServiceProxy;
                FluentCRM.FluentCRM.StaticService = orgService;

                var accountId = Guid.Empty;
                var contactId = Guid.Empty;
                FluentContact.Contact().Where("fullname").Equals("Cathan Cook").UseAttribute((Guid id) => contactId= id, "contactid" ).Execute();

                FluentAccount.Account().Where("name").Equals("Alpine Ski House")
                    .WeakUpdate<EntityReference>( "primarycontactid", new EntityReference("contact", contactId)).Execute();
            }
        }

        /// <summary>
        /// Test getting option set values. Note requires access to live CRM. So may have to ignore it
        /// Relies on access to a live CRM system with default test data loaded. (online trial eill do)
        /// </summary>
        [TestMethod]
        [TestCategory("LiveTest")]
        public void Test4_OptionString()
        {

            using (var crmSvc = new CrmServiceClient(_connectionString))
            {
                var accountid = Guid.Empty;
                EntityWrapper ew = null;
                FluentCRM.FluentCRM.StaticService = crmSvc.OrganizationServiceProxy;

                FluentAccount.Account().Where("name").Equals("Alpine Ski House")
                    .UseAttribute((Guid id) => accountid = id, "accountid")
                    .WeakUpdate<OptionSetValue>("customertypecode", new OptionSetValue(1)) // set to "Competitor"
                    .Exists((e) => Assert.IsTrue(e, "Alpine Ski House account must exist"))
                    .Execute();

                // Fetch the option set back
                FluentAccount.Account(accountid)
                    .UseEntity((e) => ew = e, "customertypecode", "name")
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

        /// <summary>
        /// Test that exposes the differences between FakeXrmEasy and a real CRM system
        /// In RealCRM, setting an attribute to null, clears that attribute value.
        /// In FakeXrmEasy it appears to have no effect.
        /// </summary>
        [TestMethod]
        [TestCategory("LiveTest")]
        public void Test4_ClearLive()
        {
            using (var crmSvc = new CrmServiceClient(_connectionString))
            {
                EntityWrapper entity1 = null;
                FluentContact.Contact(crmSvc.OrganizationServiceProxy)
                    .Where("fullname").Equals("Cathan Cook")
                    .UseEntity(e => entity1 = e, "contactid", "telephone1", "fullname")
                    .Execute();

                FluentContact.Contact(crmSvc.OrganizationServiceProxy)
                    .Where("fullname").Equals("Cathan Cook")
                    .Clear("telephone1")
                    .Execute();

                EntityWrapper entity2 = null;
                FluentContact.Contact(entity1.Id, crmSvc.OrganizationServiceProxy)
                    .UseEntity(e => entity2 = e, "contactid", "telephone1", "fullname")
                    .Execute();

                Assert.IsTrue(entity2.Contains("contactid"));
                Assert.IsTrue(entity2.Contains("fullname"));
                Assert.IsFalse(entity2.Contains("telephone1"));
            }
        }

        /// <summary>
        /// Test an issue where Top appears to cause an error
        /// </summary>
        [TestMethod]
        [TestCategory("LiveTest")]
        public void Test5_TopUse()
        {
            using (var crmSvc = new CrmServiceClient(_connectionString))
            {
                DateTime? nextMeetingDate = null;
                var nextMeetingType = string.Empty;
                Guid? nextMeetingId = Guid.Empty;
                EntityReference nextMeeting = null;
                var meetingType = string.Empty;

                FluentActivityParty.ActivityParty(crmSvc.OrganizationWebProxyClient)
                    .Trace(s => Debug.WriteLine(s))
                    .Where("partyid").Equals(Guid.Parse("e5621966-7435-eb11-a813-002248003e25"))
                    .And
                    .Where("participationtypemask").In(new int[] {1, 2, 5, 8})
                    .And
                    .Where("scheduledstart").GreaterThan(DateTime.Today)
                    .Join<FluentActivity>(
                        a => a.Where("activitytypecode").In("phonecall", "appointment")
                            .UseAttribute(string.Empty, (s) =>  {/*Read activitytypecode, use below */},"activitytypecode")
                    )
                    .UseEntity((ew) =>
                    {
                        if (!nextMeetingDate.HasValue)
                        {
                            // Using "Top" on complex CRM queries can return more than one record, but they are in the correct order,
                            // So just take the first one and ignore the rest.
                            nextMeetingDate = ew.GetAttributeValue<DateTime>("scheduledstart");
                            nextMeetingType = ew.GetAttributeValue<string>("a1.activitytypecode");
                            nextMeeting = ew.GetAttributeValue<EntityReference>("activityid");
                            nextMeetingId = nextMeeting?.Id ;
                        }
                    },"partyid","participationtypemask","scheduledstart","activityid")
                    .OrderByAsc("scheduledstart")
                    .Top(1)
                    .Execute();

                
                Assert.IsFalse(string.IsNullOrEmpty(nextMeetingType));
                Assert.IsNotNull(nextMeeting);
                Assert.IsNotNull(nextMeetingId);
            }
        }
    }
}
