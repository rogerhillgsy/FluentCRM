using System;

namespace FluentCRM
{
    public interface IJoinableEntitySet
    {
        ICanExecute UseAttribute<T>( Action<T> action, string attribute1, params string[] attributes);
        ICanExecute UseAttribute<T>( Action<string, T> action, string attribute1, params string[] attributes);
        ICanExecute UseEntity( Action<EntityWrapper> action, string attribute1, params string[] attributes);
        ICanExecute UseEntity( Action<string, EntityWrapper> action, string attribute1, params string[] attributes);

        ICanExecute WeakUpdate<T>(string attributeToUpdate, T updateValue);
        ICanExecute WeakUpdate<T>(string attributeToUpdate, Func<T,T> getUpdateValue);

        IJoinableAnotherWhere And { get; }

        ICanExecute Clear(string attributeToClear, params string[] additionalAttributesToClear);
        ICanExecute Delete();
        ICanExecute Join<T>(Action<IJoinable> target) where T : FluentCRM, new();
    }
}