using System;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    public interface IJoinable
    {
        IJoinableEntitySet UseAttribute<T>( Action<T> action, string attribute1, params string[] attributes);
        IJoinableEntitySet UseAttribute<T>( Action<string, T> action, string attribute1, params string[] attributes);
        IJoinableEntitySet UseEntity( Action<EntityWrapper, string> action, string attribute1, params string[] attributes);
        IJoinableEntitySet UseEntity( Action<string, EntityWrapper, string> action, string attribute1, params string[] attributes);
        IJoinableEntitySet Join<T>(Action<IJoinable> target) where T : FluentCRM, new();

        string LogicalName { get; }
        string JoinAttribute(string joinEntity);
        IJoinableEntitySet Outer();

        IJoinable Factory(IOrganizationService service);
        IJoinableNeedsWhereCriteria Where(string attributeName);
    }
}
