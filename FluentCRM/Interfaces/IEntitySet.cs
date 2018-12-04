using System;
using System.Collections.Generic;
using System.Text;
using FluentCRM.Base_Classes;
using Microsoft.Xrm.Sdk;

namespace FluentCRM.Interfaces
{
    /// <summary>
    /// Interface that refers to an Fluent CRM instance that potentially refers to a set of one or more Entities, but is not ready to execute as no actions have been defined on it.
    /// </summary>
    public interface IEntitySet
    {
        ICanExecute Trace(Action<string> action);
        IEntitySet Timer(Action<string> timerFunction );
        IOrganizationService Service { get; }

        ICanExecute UseAttribute<T>( Action<T> action, string attribute1, params string[] attributes);
        ICanExecute UseAttribute<T>( Action<string, T> action, string attribute1, params string[] attributes);
        ICanExecute UseEntity( Action<EntityWrapper> action, string attribute1, params string[] attributes);
        ICanExecute UseEntity( Action<string, EntityWrapper> action, string attribute1, params string[] attributes);

        ICanExecute WeakUpdate<T>(string attributeToUpdate, T updateValue, params string[] additionalAttributes);
        ICanExecute WeakUpdate<T>(string attributeToUpdate, Func<EntityWrapper,T> getUpdateValue, params string[] additionalAttributes);
        [Obsolete]
        ICanExecute WeakUpdateEntity<T>(string attributesToUpdate, Func<EntityWrapper,T> getUpdateValue, params string[] additionalAttributes);

        ICanExecute Count(Action<int?> action);
        ICanExecute Exists(Action<bool> action);
        ICanExecute Exists(Action whenTrue, Action whenFalse = null);
        IEntitySet Distinct();
        IEntitySet OrderByAsc(string attribute);
        IEntitySet OrderByDesc(string attribute);
        IAnotherWhere And { get; }

        ICanExecute Clear(string attributeToClear, params string[] additionalAttributesToClear);
        ICanExecute Delete();
        ICanExecute Join<T>(Action<IJoinable> target) where T : Base_Classes.FluentCRM, new();
    }
}
