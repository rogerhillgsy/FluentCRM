using System;

namespace TestFluentCRM.Helpers
{
    public class PluginType
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public string TypeName { get; set; }
        public PluginStep[] Steps { get; set; }
        public string WorkflowActivityGroupName { get; set; }
    }
}
