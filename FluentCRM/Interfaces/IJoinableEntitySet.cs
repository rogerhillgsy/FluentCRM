using System;

namespace FluentCRM
{
    public interface IJoinableEntitySet
    {
        IJoinableEntitySet  UseAttribute<T>( Action<T> action, string attribute1, params string[] attributes);
        IJoinableEntitySet UseAttribute<T>( Action<string, T> action, string attribute1, params string[] attributes);
        IJoinableEntitySet UseEntity( Action<EntityWrapper,string> action, string attribute1, params string[] attributes);
        IJoinableEntitySet UseEntity( Action<string, EntityWrapper,string> action, string attribute1, params string[] attributes);

        IJoinableEntitySet WeakUpdate<T>(string attributeToUpdate, T updateValue);
        IJoinableEntitySet WeakUpdate<T>(string attributeToUpdate, Func<T,T> getUpdateValue);

        IJoinableAnotherWhere And { get; }

        IJoinableEntitySet Clear(string attributeToClear, params string[] additionalAttributesToClear);
        IJoinableEntitySet Delete();
        IJoinableEntitySet Join<T>(Action<IJoinable> target) where T : FluentCRM, new();
    }
}