using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentCRM
{
    public abstract partial class FluentCRM
    {
        #region "Update entity attributes"

        ICanExecute ICanExecute.WeakUpdate<T>(string attributeToUpdate, T updateValue)
        {
            return ((IEntitySet) this).WeakUpdate<T>(attributeToUpdate, f => updateValue);
        }

        ICanExecute ICanExecute.WeakUpdate<T>(string attributeToUpdate, Func<T, T> getUpdateValue)
        {
            return ((IEntitySet) this).WeakUpdate<T>(attributeToUpdate, getUpdateValue);
        }

        ICanExecute IEntitySet.WeakUpdate<T>(string attributeToUpdate, T updateValue)
        {
            return ((IEntitySet) this).WeakUpdate<T>(attributeToUpdate, f => updateValue);
        }

        ICanExecute IEntitySet.WeakUpdate<T>(string attributeToUpdate, Func<T, T> getUpdateValue)
        {
            // Avoid problems where getUpdateValue takes an EntityWrapper - hard to pick up with static type checking.
            if (getUpdateValue is Func<EntityWrapper, T> && typeof(T) != typeof(Object))
            {
                throw new ArgumentException("Invalid invocation of WeakUpdate - use WeakUpdateEntity()");
            }
            Trace($"Update {attributeToUpdate} ");

            // Add the action to the first column
            _actionList.Add(
                new Tuple<string[], Func<EntityWrapper, string, bool?>>(
                    new string[] { attributeToUpdate },
                    (entity, c) =>
                    {
                        try
                        {
                            var oldVal = entity.GetAttributeValue<T>(attributeToUpdate);
                            var newVal = getUpdateValue(oldVal);
                            if (newVal == null || newVal.Equals(oldVal))
                            {
                                // Do nothing if value has not changed or null was returned.
                            }
                            else
                            {
                                Trace($"Updating column {attributeToUpdate} = {newVal}");
                                _update.Attributes[attributeToUpdate] = newVal;
                                _updateRequired = true;
                            }
                        }
                        catch (InvalidCastException)
                        {
                            Trace($"For update of {attributeToUpdate} in {LogicalName} expected type: {entity.Entity.Attributes[attributeToUpdate].GetType()} type supplied: {typeof(T)}");
                            throw;
                        }
                        return true;
                    }
                ));

            return this;
        }

        ICanExecute ICanExecute.WeakUpdateEntity<T>(string attributeToUpdate, Func<EntityWrapper, T> getUpdateValue,
            params string[] additionalAttributes)
        {
            return ((IEntitySet) this).WeakUpdateEntity(attributeToUpdate, getUpdateValue, additionalAttributes);
        }

        ICanExecute IEntitySet.WeakUpdateEntity<T>(string attributeToUpdate, Func<EntityWrapper, T> getUpdateValue,
            params string[] additionalAttributes)
        {
            Trace($"Update Entity {attributeToUpdate} ");
            if (additionalAttributes.Length > 0) Trace($"Adding column {additionalAttributes[0]} to column set");

            // Add the action to the first column
            _actionList.Add(
                new Tuple<string[], Func<EntityWrapper, string, bool?>>(
                    (new string[] { attributeToUpdate }).Concat(additionalAttributes).ToArray(),
                    (entity, c) =>
                    {
                        var oldVal = entity.GetAttributeValue<T>(attributeToUpdate);
                        var newVal = getUpdateValue(entity);
                        if (newVal == null || newVal.Equals(oldVal))
                        {
                            // Do nothing if value has not changed or null was returned.
                        }
                        else
                        {
                            Trace($"Updating column {attributeToUpdate} = {newVal}");
                            _update.Attributes[attributeToUpdate] = newVal;
                            _updateRequired = true;
                        }
                        return true;
                    }
                ));

            return this;
        }

        ICanExecute ICanExecute.Clear(string attributeToClear, params string[] additionalAttributesToClear)
        {
            return ((ICanExecute) this).Clear(attributeToClear, additionalAttributesToClear);
        }

        public ICanExecute Clear(string attributeToClear, params string[] additionalAttributesToClear)
        {
            Trace($"Update Entity {attributeToClear} ");
            if (additionalAttributesToClear.Length > 0) Trace("Also clearing:", String.Join(",", additionalAttributesToClear));

            var toClear = new List<string>() { attributeToClear };
            toClear.AddRange(additionalAttributesToClear);

            // Add the action to the first column
            _actionList.Add(
                new Tuple<string[], Func<EntityWrapper, string, bool?>>(
                    (toClear.ToArray()),
                    //(new string[] {updateAttribute}).Concat(additionalAttributes).ToArray(),
                    (entity, colToClear) =>
                    {
                        Trace($"Clearing {colToClear}");
                        _update.Attributes[colToClear] = null;
                        _updateRequired = true;
                        return false;
                    }
                ));

            return this;
        }

        #endregion
    }
}