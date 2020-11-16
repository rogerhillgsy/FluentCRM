using System;

namespace TestFluentCRM.Helpers
{
    public class PluginStep
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string MessageName { get; set; }
        public string Description { get; set; }
        public string CustomConfiguration { get; set; }
        public string FilteringAttributes { get; set; }
        public string ImpersonatingUserFullname { get; set; }
        public string Mode { get; set; }
        public string PrimaryEntityName { get; set; }
        public int Rank { get; set; }
        public string Stage { get; set; }
        public string SupportedDeployment { get; set; }
        public bool AsyncAutoDelete { get; set; }
        public string StateCode { get; set; }
        public PluginImages[] Images { get; set; }
    }

}
