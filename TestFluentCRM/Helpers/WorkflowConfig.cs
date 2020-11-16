using System;

namespace TestFluentCRM.Helpers
{
    public class WorkflowConfig
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string IsolationMode { get; set; }
        public string SourceType { get; set; }
        public string Version { get; set; }
        public PluginType[] PluginTypes { get; set; }
    }
}
