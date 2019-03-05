using System;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    /// <summary>
    ///  Used where the entity is in a state where it can potentially be executed to carry out the deferred actions etc that have been set up in previous calls on the Fluent CRM object.
    /// </summary>
    public interface ICanExecute
    {
        ICanExecute Trace(Action<string> action);
        ICanExecute Timer(Action<string> timerFunction );
        IOrganizationService Service { get; }

        ICanExecute UseAttribute<T>( Action<T> action, string attribute1, params string[] attributes);
        ICanExecute UseAttribute<T>( Action<string, T> action, string attribute1, params string[] attributes);
        ICanExecute UseEntity( Action<EntityWrapper> action, string attribute1, params string[] attributes);
        ICanExecute UseEntity( Action<string, EntityWrapper> action, string attribute1, params string[] attributes);

        ICanExecute WeakUpdate<T>(string attributeToUpdate, T updateValue);
        ICanExecute WeakUpdate<T>(string attributeToUpdate, Func<T,T> getUpdateValue);
        [Obsolete]
        ICanExecute WeakUpdateEntity<T>(string attributesToUpdate, Func<EntityWrapper,T> getUpdateValue, params string[] additionalAttributes);

        ICanExecute Count(Action<int?> action);
        ICanExecute Exists(Action<bool> action);
        ICanExecute Exists(Action whenTrue, Action whenFalse = null);
        ICanExecute PageSize(int i); 
        ICanExecute Distinct();
        ICanExecute OrderByAsc(string attribute);
        ICanExecute OrderByDesc(string attribute);
        IAnotherWhere And { get; }

        ICanExecute Clear(string attributeToClear, params string[] additionalAttributesToClear);
        ICanExecute Delete();
        ICanExecute Join<T>(Action<IJoinable> target) where T : FluentCRM, new();

        ICanExecute AfterEachEntity(Action<EntityWrapper> action);

        void Execute( Action preExecute = null, Action<int,int> postExecute = null );
    }
}
