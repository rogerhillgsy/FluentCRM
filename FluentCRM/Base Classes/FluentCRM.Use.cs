using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    public abstract partial class FluentCRM
    {
        #region "UseAttribute"

        /// <summary>
        /// Use the given attribute to call the given action function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="attribute1"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        ICanExecute IEntitySet.UseAttribute<T>(Action<T> action, string attribute1, params string[] attributes)
        {
            return (ICanExecute) UseAttribute( default(T), false, action, attribute1, attributes);
        }

        ICanExecute IEntitySet.UseAttribute<T>(Action<string, T> action, string attribute1, params string[] attributes)
        {
            return (ICanExecute) UseAttribute(default(T), false, action, attribute1, attributes);
        }

        ICanExecute IEntitySet.UseAttribute<T>(T defaultValue, Action<T> action, string attribute1, params string[] attributes)
        {
            return (ICanExecute) UseAttribute(defaultValue, true, action, attribute1, attributes);
        }

        ICanExecute IEntitySet.UseAttribute<T>(T defaultValue, Action<string, T> action, string attribute1, params string[] attributes)
        {
            return (ICanExecute) UseAttribute(defaultValue, true, action, attribute1, attributes);
        }

        /// <summary>
        /// Add method to process an entity attribute after it has been retreived.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="attribute1"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        ICanExecute ICanExecute.UseAttribute<T>(Action<string, T> action,
            string attribute1, params string[] attributes)
        {
            return (ICanExecute) UseAttribute<T>(default(T), false, action, attribute1, attributes);
        }

        ICanExecute ICanExecute.UseAttribute<T>(Action<T> action, string attribute1, params string[] attributes)
        {
            return (ICanExecute) UseAttribute( default(T), false, action, attribute1, attributes);
        }

                ICanExecute ICanExecute.UseAttribute<T>(T defaultValue, Action<string, T> action,
            string attribute1, params string[] attributes)
        {
            return (ICanExecute) UseAttribute<T>(defaultValue, true, action, attribute1, attributes);
        }

        ICanExecute ICanExecute.UseAttribute<T>(T defaultValue, Action<T> action, string attribute1, params string[] attributes)
        {
            return (ICanExecute) UseAttribute( defaultValue, true, action, attribute1, attributes);
        }

        private FluentCRM UseAttribute<T>(T defaultValue, bool hasDefault, Action<T> action, string attribute1, params string[] attributes)
        {
            return UseAttribute( defaultValue, hasDefault, (string c, T val) => action(val), attribute1, attributes);
        }

        /// <summary>
        /// Add method to process an entity attribute after it has been retreived.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hasDefault">Indicates if default value has been specified</param>
        /// <param name="action">Closure to be called when non-null attribute value is found, or a default value has been specified.</param>
        /// <param name="attribute1">Mandatory candidate attribute</param>
        /// <param name="attributes">Set of optional candidate attributes</param>
        /// <param name="defaultValue">optional Default value to be use to call the closure if none of the candidate attributes are non-null</param>
        /// <returns></returns>
        private FluentCRM UseAttribute<T>(T defaultValue, bool hasDefault, Action<string, T> action, string attribute1, params string[] attributes)
            {
            var allAttributes = new List<string> {attribute1};
            allAttributes.AddRange(attributes);
            if (allAttributes.Exists(s => s == AllColumns)) throw new ArgumentException("Cannot specify AllColumns here" );
            Trace($"Adding columns [{String.Join(",", allAttributes) }] to column set");

            var alias = LinkEntity?.EntityAlias;
            if (alias != null) alias += ".";

            _actionList.Add(
                new Tuple<string[], Func<EntityWrapper, string, bool?>>(
                    allAttributes.ToArray(),
                    (entity, c) =>
                    {
                        foreach (var c1 in allAttributes)
                        {
                            var column = alias + c1;
                            if (entity != null &&
                                entity.Contains(column))
                            {
                                if (entity.Entity[column] is T )
                                {
                                    action(column, entity.GetAttributeValue<T>(column));
                                    return true;
                                }
                                else
                                {
                                    if (alias != null &&
                                        entity.Entity.Attributes[column] is AliasedValue &&
                                        (entity.Entity.Attributes[column] as AliasedValue)?.Value is T)
                                    {
                                        var aliasValue = entity.Entity.Attributes[column] as AliasedValue;
                                        var columnOnly = column.Substring(alias.Length);
                                        action(columnOnly, (T) aliasValue.Value);
                                        return true;
                                    }
                                    else
                                    {
                                         var error =
                                            $"For {column} returned type {(entity[column] as AliasedValue)?.Value?.GetType().ToString() ??( entity[column]?.GetType().ToString() ?? "*Unknown*")} but expected type {typeof(T)}";
                                        Trace(error);
                                        TopEntity._allArgExceptions.AppendLine(error);
                                        return true;
                                    }
                                }
                            }
                        }

                        if (hasDefault)
                        {
                            Trace($"Calling closure with default value: {defaultValue}");
                            action("", defaultValue);
                            return true;
                        }
                        else
                        {
                            Trace($"Columns not found so no action taken: {string.Join(",", allAttributes)}");
                        }

                        return true;
                    }
                ));
            return this;
        }
        #endregion
        
        #region "UseEntity

        /// <summary>
        /// Call closure with Entity wrapper having loaded specified attributes from CRM.
        /// </summary>
        /// <param name="action">Closure to be called with EntityWrapper function</param>
        /// <param name="attribute1">Mandatory attribute to fetch into the EntityWrapper</param>
        /// <param name="attributes">Optional attributes to fetch into the EntityWrapper</param>
        /// <returns></returns>
        ICanExecute ICanExecute.UseEntity(Action<EntityWrapper> action, string attribute1, params string[] attributes)
        {
            return UseEntity( (EntityWrapper w, string alias ) => action(w), attribute1, attributes);
        }

        ICanExecute IEntitySet.UseEntity(Action<EntityWrapper> action, string attribute1,
            params string[] attributes)
        {
            return UseEntity( (EntityWrapper w, string alias ) => action(w), attribute1, attributes);
        }

        ICanExecute UseEntity(Action<EntityWrapper,string> action, string attribute1, params string[] attributes)
        {
            var allAttributes = new List<string> {attribute1};
            allAttributes.AddRange(attributes);
            Trace($"Adding columns [{String.Join(",", allAttributes)}] to column set");

            var alias = LinkEntity?.EntityAlias;
            if (alias != null) alias += ".";

            _actionList.Add(
                new Tuple<string[], Func<EntityWrapper, string, bool?>>(
                    allAttributes.ToArray(),
                    (entity, c) =>
                    {
                        entity.Alias = alias;
                        action(entity, alias);
                        return true;
                    }
                ));

            return this;
        }

        ICanExecute ICanExecute.UseEntity(Action<string, EntityWrapper> action, string attribute1,
            params string[] attributes)
        {
            return ((IEntitySet) this).UseEntity(action, attribute1, attributes);
        }

        ICanExecute IEntitySet.UseEntity(Action<string, EntityWrapper> action, string attribute1,
            params string[] attributes)
        {
            return UseEntity((string field, EntityWrapper w, string alias) => action(field, w), attribute1, attributes);
        }

        ICanExecute UseEntity(Action<string, EntityWrapper, string> action, string attribute1,
            params string[] attributes)
        {
            var allAttributes = new List<string> {attribute1};
            allAttributes.AddRange(attributes);
            if (allAttributes.Exists(s => s == AllColumns)) throw new ArgumentException("Cannot specify AllColumns here" );
            Trace($"Adding columns [{String.Join(",", allAttributes) }] to column set");

            var alias = LinkEntity?.EntityAlias;
            if (alias != null) alias += ".";

            _actionList.Add(
                new Tuple<string[], Func<EntityWrapper, string, bool?>>(
                    allAttributes.ToArray(),
                    (entity, c) =>
                    {
                        entity.Alias = alias;
                        foreach (var c1 in allAttributes)
                        {
                            var column = alias + c1;
                            entity.Alias = alias;
                            if (entity != null &&
                                entity.Contains(column))
                            {
                                action(column, entity, alias);
                                return true;
                            }
                        }

                        Trace($"Columns not found so no action taken: {string.Join(",", allAttributes)}");
                        return false;
                    }
                ));

            return this;
        }
        #endregion
    }
}