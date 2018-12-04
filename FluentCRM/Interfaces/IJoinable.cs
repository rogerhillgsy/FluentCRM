using System;
using System.Collections.Generic;
using System.Text;
using FluentCRM.Base_Classes;

namespace FluentCRM.Interfaces
{
    public interface IJoinable
    {
        IJoinable UseAttribute<T>( Action<T> action, string attribute1, params string[] attributes);
        IJoinable UseAttribute<T>( Action<string, T> action, string attribute1, params string[] attributes);
        IJoinable UseEntity( Action<EntityWrapper> action, string attribute1, params string[] attributes);
        ICanExecute UseEntity( Action<string, EntityWrapper> action, string attribute1, params string[] attributes);
        ICanExecute Join<T>(Action<IJoinable> target) where T : IJoinable, new();

        string LogicalName { get; }
        string JoinAttribute(string JoinEntity);
        IJoinable Outer { get; }
    }
}
