using System;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    public interface IJoinable
    {
        IJoinable UseAttribute<T>( Action<T> action, string attribute1, params string[] attributes);
        IJoinable UseAttribute<T>( Action<string, T> action, string attribute1, params string[] attributes);
        IJoinable UseEntity( Action<EntityWrapper> action, string attribute1, params string[] attributes);
        ICanExecute UseEntity( Action<string, EntityWrapper> action, string attribute1, params string[] attributes);
        ICanExecute Join<T>(Action<IJoinable> target) where T : FluentCRM, new();

        string LogicalName { get; }
        string JoinAttribute(string JoinEntity);
        IJoinable Outer { get; }

        IJoinable Factory(IOrganizationService service);

    }
}
