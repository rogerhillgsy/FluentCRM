using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using FluentCRM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using TestFluentCRM.Helpers;

namespace TestFluentCRM
{
    /// <summary>
    /// Generate plugin and workflow config files for use in Devops pipeline components from Wael Hamze (Power Devops... components)
    /// In particular the Plugin Registration and Workflow registration tasks.
    /// </summary>
    [TestClass]
    [TestCategory("ConfigGeneration")]
    public class GeneratePluginConfigFiles
    {
        private string _connectionString;
        private const String TargetPluginAssemblyName = "Arup.CRM.Plugins";
        private const String TargetWorkflowAssemblyName = "Arup.CRM.Workflows";
        private const String OutputFilePrefix = "Arup";

        [TestInitialize]
        public void Setup()
        {
            // Obtain connection string from app.config and expand any environment variables e.g. for Passwords, usernames)
            _connectionString = ConfigurationManager.ConnectionStrings["CrmOnline"].ConnectionString;
            _connectionString = Environment.ExpandEnvironmentVariables(_connectionString);
        }

        [TestMethod]
        public void ArupCRMPluginConfig()
        {
            using (var crmSvc = new CrmServiceClient(_connectionString))
            {
                var orgService = crmSvc.OrganizationServiceProxy;
                FluentCRM.FluentCRM.StaticService = orgService;

                WorkflowConfig workflowConfig = null;
                var pluginTypes = new Dictionary<Guid, PluginType>();
                var pluginSteps = new Dictionary<Guid, Dictionary<Guid, PluginStep>>();
                var pluginImages = new Dictionary<Guid, Dictionary<Guid, PluginImages>>();

                FluentPluginAssembly.PluginAssembly()
                    .Trace(s => Debug.WriteLine(s))
                    .Where("name").Equals(TargetPluginAssemblyName)
                    .UseEntity(ew =>
                    {
                        if (workflowConfig == null)
                        {
                            workflowConfig = new WorkflowConfig
                            {
                                Id = ew.GetAttributeValue<Guid>("pluginassemblyid"),
                                IsolationMode = ew.OptionString("isolationmode"),
                                Name = ew.GetAttributeValue<string>("name") + ".dll",
                                SourceType = ew.OptionString("sourcetype"),
                                Version = ew.GetAttributeValue<string>("version"),
                            };
                        }
                    }, "pluginassemblyid", "name", "isolationmode", "sourcetype", "version", "path")
                    .Join<FluentPluginType>(pt => pt.UseEntity((ptw, s) =>
                            {
                                if (!pluginTypes.ContainsKey(ptw.Id))
                                {
                                    pluginTypes.Add(ptw.Id, new PluginType
                                    {
                                        Id = ptw.GetAttributeValue<Guid>(s + "plugintypeid"),
                                        Description = ptw.GetAttributeValue<string>(s + "description"),
                                        Name = ptw.GetAttributeValue<string>(s + "name"),
                                        FriendlyName = ptw.GetAttributeValue<string>(s + "friendlyname"),
                                        TypeName = ptw.GetAttributeValue<string>(s + "typename"),
                                        WorkflowActivityGroupName =
                                            ptw.GetAttributeValue<string>(s + "workflowactivitygroupname"),
                                        Steps = new PluginStep[] { }
                                    });
                                }
                            }, "isworkflowactivity", "name", "plugintypeid", "description", "friendlyname",
                            "typename",
                            "workflowactivitygroupname", "pluginassemblyid")
                        .Join<FluentSdkMessageProcessingStep>(ps =>
                        {
                            var primaryEntity = string.Empty;

                            ps.Outer().Join<FluentSdkMessageFilter>(
                                    mf => mf.Outer().UseAttribute((string s) => { primaryEntity = s; },
                                        "primaryobjecttypecode")).UseEntity((psw, psa) =>
                                    {
                                        if (!Guid.Empty.Equals(psw.Id))
                                        {
                                            if (!psw.Contains(psa + "plugintypeid"))
                                            {
                                                throw new ArgumentException(
                                                    $"SDK message processing step {psw.Id} does not contain a reference to plugintype.");
                                            }
                                            else
                                            {
                                                var typeId =
                                                    psw.GetAttributeValue<EntityReference>(psa + "plugintypeid");

                                                if (!pluginSteps.ContainsKey(typeId.Id))
                                                {
                                                    pluginSteps.Add(typeId.Id, new Dictionary<Guid, PluginStep>());
                                                }

                                                var Id = psw.Id;
                                                if (!pluginSteps[typeId.Id].ContainsKey(Id))
                                                {
                                                    pluginSteps[typeId.Id].Add(Id, new PluginStep
                                                    {
                                                        Id = psw.GetAttributeValue<Guid>(
                                                            psa + "sdkmessageprocessingstepid"),
                                                        Name = psw.GetAttributeValue<string>(psa + "name"),
                                                        MessageName =
                                                            psw.GetAttributeValue<EntityReference>(
                                                                    psa + "sdkmessageid")
                                                                ?.Name,
                                                        Description =
                                                            psw.GetAttributeValue<string>(psa + "description"),
                                                        CustomConfiguration =
                                                            psw.GetAttributeValue<string>(psa + "configuration"),
                                                        FilteringAttributes =
                                                            psw.GetAttributeValue<string>(
                                                                psa + "filteringattributes"),
                                                        ImpersonatingUserFullname =
                                                            psw.GetAttributeValue<EntityReference>(
                                                                    psa + "impersonatinguserid")
                                                                ?.Name ?? "",
                                                        Mode = psw.OptionString(psa + "mode"),
                                                        PrimaryEntityName = primaryEntity,
                                                        Rank = psw.GetAttributeValue<int>(psa + "rank"),
                                                        Stage = stageMapping.Mapping(
                                                            psw.OptionString(psa + "stage")),
                                                        SupportedDeployment =
                                                            deploymentMapping.Mapping(
                                                                psw.OptionString(psa + "supporteddeployment")),
                                                        AsyncAutoDelete =
                                                            psw.GetAttributeValue<bool>(psa + "asyncautodelete"),
                                                        StateCode = psw.OptionString(psa + "statecode"),
                                                        Images = new PluginImages[] { }
                                                    });
                                                }
                                            }
                                        }
                                    }, "sdkmessageprocessingstepid", "name", "sdkmessageid", "description",
                                    "configuration", "filteringattributes", "impersonatinguserid",
                                    "mode", /* "primary entity name", */
                                    "rank", "stage", "supporteddeployment", "asyncautodelete", "statecode",
                                    "plugintypeid")
                                .Join<FluentSdkMessageProcessingStepImage>(im => im.Outer()
                                    .UseEntity((imw, ima) =>
                                        {
                                            if (!Guid.Empty.Equals(imw.Id))
                                            {
                                                if (!imw.Contains(ima + "sdkmessageprocessingstepid"))
                                                {
                                                    throw new ArgumentException(
                                                        $"plugin step image {imw.Id} does not contain a reference to sdkmessageprocessingstepid.");
                                                }
                                                else
                                                {
                                                    var stepId =
                                                        imw.GetAttributeValue<EntityReference>(
                                                            ima + "sdkmessageprocessingstepid");

                                                    if (!pluginImages.ContainsKey(stepId.Id))
                                                    {
                                                        pluginImages.Add(stepId.Id,
                                                            new Dictionary<Guid, PluginImages>());
                                                    }

                                                    var Id = imw.Id;
                                                    if (!pluginImages[stepId.Id].ContainsKey(Id))
                                                    {
                                                        pluginImages[stepId.Id].Add(Id, new PluginImages
                                                        {
                                                            Id = imw.GetAttributeValue<Guid>(
                                                                ima + "sdkmessageprocessingstepimageid"),
                                                            Attributes =
                                                                imw.GetAttributeValue<string>(ima + "attributes"),
                                                            EntityAlias =
                                                                imw.GetAttributeValue<string>(ima + "entityalias"),
                                                            MessagePropertyName =
                                                                imw.GetAttributeValue<string>(
                                                                    ima + "messagepropertyname"),
                                                            ImageType = imw.OptionString(ima + "imagetype")
                                                        });
                                                    }
                                                }
                                            }
                                        }, "sdkmessageprocessingstepimageid", "attributes", "entityalias",
                                        "messagepropertyname", "imagetype", "sdkmessageprocessingstepid")
                                );
                        })
                    )
                    .Count(c => Debug.WriteLine($"Read {c} records"))
                    .Execute();

                workflowConfig.PluginTypes = pluginTypes.OrderBy(t => t.Value.Name).Select(t => t.Value).ToArray();
                foreach (var type in workflowConfig.PluginTypes)
                {
                    if (pluginSteps.ContainsKey(type.Id))
                    {
                        type.Steps = pluginSteps[type.Id].OrderBy(s => s.Value.Name).Select(s => s.Value).ToArray();
                    }
                }

                foreach (var pluginStep in workflowConfig.PluginTypes.SelectMany(t => t.Steps))
                {
                    if (pluginImages.ContainsKey(pluginStep.Id))
                    {
                        pluginStep.Images = pluginImages[pluginStep.Id].OrderBy(i => i.Value.MessagePropertyName)
                            .Select(i => i.Value).ToArray();
                    }
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,

                };
                var json = JsonSerializer.Serialize(workflowConfig, options);
                File.WriteAllText($"{OutputFilePrefix}CRMPluginsConfig.json", json, Encoding.UTF8);
            }
        }

        /// <summary>
        /// Generate config file for devops deployment
        /// Copy to ../../../Workflows
        /// </summary>
        [TestMethod]
        public void ArupCRMWorkflowsConfig()
        {
            using (var crmSvc = new CrmServiceClient(_connectionString))
            {
                var orgService = crmSvc.OrganizationServiceProxy;
                FluentCRM.FluentCRM.StaticService = orgService;

                WorkflowConfig workflowConfig = null;
                var pluginTypes = new List<PluginType>();
                FluentPluginAssembly.PluginAssembly()
                    .Trace(s => Debug.WriteLine(s))
                    .Where("name").Equals(TargetWorkflowAssemblyName)
                    .UseEntity(ew =>
                    {
                        if (workflowConfig == null)
                        {
                            workflowConfig = new WorkflowConfig
                            {
                                Id = ew.GetAttributeValue<Guid>("pluginassemblyid"),
                                IsolationMode = ew.OptionString("isolationmode"),
                                Name = ew.GetAttributeValue<string>("name") + ".dll",
                                SourceType = ew.OptionString("sourcetype"),
                                Version = ew.GetAttributeValue<string>("version"),
                            };
                        }
                    }, "pluginassemblyid", "name", "isolationmode", "sourcetype", "version", "path")
                    .Join<FluentPluginType>(pt => pt.UseEntity((ptw, s) =>
                        {
                            pluginTypes.Add(new PluginType
                            {
                                Id = ptw.GetAttributeValue<Guid>(s + "plugintypeid"),
                                Description = ptw.GetAttributeValue<string>(s + "description"),
                                Name = ptw.GetAttributeValue<string>(s + "name"),
                                FriendlyName = ptw.GetAttributeValue<string>(s + "friendlyname"),
                                TypeName = ptw.GetAttributeValue<string>(s + "typename"),
                                WorkflowActivityGroupName =
                                    ptw.GetAttributeValue<string>(s + "workflowactivitygroupname"),
                                Steps = new PluginStep[] { }
                            });
                        }, "isworkflowactivity", "name", "plugintypeid", "description", "friendlyname", "typename",
                        "workflowactivitygroupname"))
                    .Execute();

                workflowConfig.PluginTypes = pluginTypes.OrderBy(t => t.TypeName).ToArray();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,

                };
                var json = JsonSerializer.Serialize(workflowConfig, options);
                File.WriteAllText($"{OutputFilePrefix}CRMWorkflowsConfig.json", json, Encoding.UTF8);
            }
        }

        /// <summary>
        /// Example of how to get details of all plugin steps
        /// </summary>
        [TestMethod]
        public void GetAllSteps()
        {
            using (var crmSvc = new CrmServiceClient(_connectionString))
            {
                var orgService = crmSvc.OrganizationServiceProxy;
                FluentCRM.FluentCRM.StaticService = orgService;

                FluentSdkMessageProcessingStep.SDKMessageProcessingStep()
                    .Where("name").BeginsWith(TargetPluginAssemblyName)
                    .UseEntity(ew => { Console.WriteLine(ew.Id); }, FluentCRM.FluentCRM.AllColumns)
                    .Count(c => Console.WriteLine($"Read {c} records"))
                    .Count(c => Assert.IsTrue(c > 0,
                        "Failed to read any SDK message Processing step records for Arup.CRM.Plugins."))
                    .Execute();
            }
        }

        [TestMethod]
        public void GetAllTypes()
        {
            using (var crmSvc = new CrmServiceClient(_connectionString))
            {
                var orgService = crmSvc.OrganizationServiceProxy;
                FluentCRM.FluentCRM.StaticService = orgService;

                FluentPluginType.PluginType()
                    .Where("name").BeginsWith(TargetPluginAssemblyName)
                    .UseEntity(ew => { Console.WriteLine(ew.Id); }, FluentCRM.FluentCRM.AllColumns)
                    .Count(c => Console.WriteLine($"Read {c} records"))
                    .Count(c => Assert.IsTrue(c > 0, "Failed to read any Plugin type records for Arup.CRM.Plugins."))
                    .Execute();
            }
        }

        [TestMethod]
        public void GetMessageFilters()
        {
            using (var crmSvc = new CrmServiceClient(_connectionString))
            {
                var orgService = crmSvc.OrganizationServiceProxy;
                FluentCRM.FluentCRM.StaticService = orgService;

                FluentSdkMessageFilter.SDKMessageFilter()
                    .Trace(s => Debug.WriteLine(s))
                    .All() // .Where("name").BeginsWith(TargetPluginAssemblyName)
                    .UseEntity(ew => { Console.WriteLine(ew.Id); }, FluentCRM.FluentCRM.AllColumns)
                    .Count(c => Console.WriteLine($"Read {c} records"))
                    .Count(c => Assert.IsTrue(c > 0,
                        "Failed to read any SDK message filter records for Arup.CRM.Plugins."))
                    .Execute();
            }
        }

        /// <summary>
        ///  Class to simplify mapping of optionset values that seem not to correspond with values expected by Devops tools.
        /// </summary>
        public class SoftMapping : Dictionary<string, string>
        {
            /// <summary>
            /// Return mapping if available, otherwise original value
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public string Mapping(string index)
            {
                if (ContainsKey(index))
                {
                    return this[index];
                }

                return index;
            }
        }

        private readonly SoftMapping stageMapping = new SoftMapping
        {
            {"Pre-validation", "Prevalidation"},
            {"Pre-operation", "Preoperation"},
            {"Post-operation", "Postoperation"},
        };

        private readonly SoftMapping deploymentMapping = new SoftMapping
        {
            {"Server Only", "ServerOnly"},
        };
    }
}
