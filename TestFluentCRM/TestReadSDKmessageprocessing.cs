using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Diagnostics;
using FluentCRM;
using Microsoft.Xrm.Tooling.Connector;

namespace TestFluentCRM
{
    [TestClass]
    public class TestReadSDKmessageprocessing
    {
        private string _connectionString;

        [TestInitialize]
        public void Setup()
        {
            // Obtain connection string from app.config and expand any environment variables e.g. for Passwords, usernames)
            _connectionString = ConfigurationManager.ConnectionStrings["CrmOnline"].ConnectionString;
            _connectionString = Environment.ExpandEnvironmentVariables(_connectionString);
        }

        /// <summary>
        /// Poc to demonstrate that we can access the Plugin Config stack.
        /// </summary>
        [TestMethod]
        [TestCategory("ConfigGeneration")]
        public void ArupCRMPluginConfig1()
        {
            var count = 0;
            using (var crmSvc = new CrmServiceClient(_connectionString))
            {
                FluentPluginAssembly.PluginAssembly(crmSvc)
                    .Trace(s => Debug.WriteLine(s))
                    .All()
                    .UseEntity(ew =>
                    {
                        Console.WriteLine($"Read plugin Assembly {ew.GetAttributeValue<string>("name")} id: {ew.Id}");
                        
                    }, "pluginassemblyid", "name", "isolationmode", "sourcetype", "version", "path")
                    .Join<FluentPluginType>(pt => pt.UseEntity((ptw, pta) =>
                    {
                        Console.WriteLine($"Read plugin Type {ptw.GetAttributeValue<string>(pta + "typename")} id: {ptw.Id}");
                    }, "isworkflowactivity", "name", "plugintypeid", "description", "friendlyname", "typename",
                        "workflowactivitygroupname")
                        .Join<FluentSdkMessageProcessingStep>(ps => ps.Outer().UseEntity((psw, psa) =>
                        {
                            Console.WriteLine($"Read plugin Step {psw.GetAttributeValue<string>(psa + "name")} id: {psw.Id}");
                        }, "sdkmessageprocessingstepid", "name", "sdkmessageid", "description", "configuration", "filteringattributes", "impersonatinguserid", "mode",/* "primary entity name", */
                           "rank", "stage", "supporteddeployment", "asyncautodelete", "statecode")
                            .Join<FluentSdkMessageProcessingStepImage>( im => im.UseEntity((imw, ima) =>
                    {
                        Console.WriteLine($"Read image {imw.GetAttributeValue<string>(ima + "attributes")} id: {imw.Id}");
                    }, "sdkmessageprocessingstepimageid","attributes","entityalias","messagepropertyname","imagetype"))))
                           .Count( c => { count = c ?? 0; Console.WriteLine($"Read {c} records"); })
                    .Execute();

                Assert.IsTrue(count > 0);
            }
        }
    }
}
