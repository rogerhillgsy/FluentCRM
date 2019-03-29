using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using FakeXrmEasy;
using FluentCRM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

namespace TestFluentCRM
{
    [TestClass]
    public class Test10_SpecificEntities
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
        public void TestSystemUser()
        {
            var userId = Guid.NewGuid();
            var context = TestUtilities.TestContext3(new Entity("systemuser") {Id = userId, ["fullname"] = "roger"});
            context.CallerId = new EntityReference("systemuser", userId);
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();

            FluentSystemUser.CurrentUser().UseAttribute((string n) => Assert.AreEqual("roger", n), "fullname")
                .Count((c) => Assert.AreEqual(1, c))
                .Execute();

        }

        [TestMethod]
        public void TestCurrentUserSettings()
        {
            var userId = Guid.NewGuid();
            var context = TestUtilities.TestContext3(new Entity("systemuser") {Id = userId, ["fullname"] = "roger"},
                new Entity("usersettings") {Id = Guid.NewGuid(), ["systemuserid"] = userId, ["numberseparator"] = ","});
            context.CallerId = new EntityReference("systemuser", userId);
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();

            FluentUserSettings.CurrentUserSettings()
                .UseAttribute((string n) => Assert.AreEqual(",", n), "numberseparator")
                .Count((c) => Assert.AreEqual(1, c))
                .Execute();

        }

        /// <summary>
        /// Have a look at what settings values are returned by a live CRM system.
        /// </summary>
        [TestMethod]
        public void TestLiveUserSettings()
        {
            var cnString = ConfigurationManager.ConnectionStrings["CrmOnline"].ConnectionString;
            cnString = Environment.ExpandEnvironmentVariables(cnString);
            using (var crmSvc = new CrmServiceClient(cnString))
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
}
}
