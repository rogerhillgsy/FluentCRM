using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    public abstract partial class FluentCRM
    {
        #region "Use attributes read from CRM"

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
            return ((ICanExecute) this).UseAttribute(action, attribute1, attributes);
        }

        ICanExecute ICanExecute.UseAttribute<T>(Action<T> action, string attribute1, params string[] attributes)
        {
            return ((IEntitySet) this).UseAttribute((string c, T val) => action(val), attribute1, attributes);
        }

        ICanExecute IEntitySet.UseAttribute<T>(Action<string, T> action, string attribute1, params string[] attributes)
        {
            return ((ICanExecute) this).UseAttribute(action, attribute1, attributes);
        }

        /// <summary>
        /// Add method to process an entity attribute after it has been retreived.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="attribute1"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        ICanExecute ICanExecute.UseAttribute<T>(Action<string, T> action, string attribute1, params string[] attributes)
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
                                if (entity[column] is T)
                                {
                                    action(column, entity.GetAttributeValue<T>(column));
                                    return true;
                                }
                                else
                                {
                                    if (alias != null &&
                                        entity.GetAttributeValue<AliasedValue>(column).Value is T)
                                    {
                                        action(column, (T) entity.GetAttributeValue<AliasedValue>(column).Value);
                                        return true;
                                    }
                                    else
                                    {
                                        var error =
                                            $"For {column} expected type {entity[column].GetType()} found type {typeof(T)}";
                                        Trace(error);
                                        throw new ArgumentException(error);
                                    }
                                }
                            }
                        }

                        Trace($"Columns not found so no action taken: {string.Join(",", allAttributes)}");
                        return false;
                    }
                ));
            return this;
        }
        public ICanExecute UseEntity(Action<string, EntityWrapper> action, string attribute1,
            params string[] attributes)
        {
            return ((ICanExecute) this).UseAttribute(action, attribute1, attributes);
        }

        ICanExecute ICanExecute.UseEntity(Action<EntityWrapper> action, string attribute1, params string[] attributes)
        {
            return ((IEntitySet) this).UseEntity(action, attribute1, attributes);
        }

        ICanExecute IEntitySet.UseEntity(Action<EntityWrapper> action, string attribute1, params string[] attributes)
        {
            var allAttributes = new List<string> {attribute1};
            allAttributes.AddRange(attributes);
            if (allAttributes.Exists(s => s == AllColumns))
                throw new ArgumentException("Cannot specify AllColumns here");
            Trace($"Adding columns [{String.Join(",", allAttributes)}] to column set");

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
                                ( column == AllColumns ||
                                entity.Contains(column)))
                            {
                                action(entity);
                                return true;
                            }
                        }

                        Trace($"Columns not found so no action taken: {string.Join(",", allAttributes)}");
                        return false;
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
                                action(column, entity);
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