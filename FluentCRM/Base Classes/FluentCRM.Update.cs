using System;
using FluentCRM.Interfaces;

namespace FluentCRM.Base_Classes
{
    public abstract partial class FluentCRM
    {
        #region "Update entity attributes"

        ICanExecute ICanExecute.WeakUpdate<T>(string attributeToUpdate, T updateValue,
            params string[] additionalAttributes)
        {
            throw new NotImplementedException();
        }

        ICanExecute ICanExecute.WeakUpdate<T>(string attributeToUpdate, Func<EntityWrapper, T> getUpdateValue,
            params string[] additionalAttributes)
        {
            throw new NotImplementedException();
        }

        ICanExecute ICanExecute.WeakUpdateEntity<T>(string attributesToUpdate, Func<EntityWrapper, T> getUpdateValue,
            params string[] additionalAttributes)
        {
            throw new NotImplementedException();
        }

        ICanExecute IEntitySet.WeakUpdate<T>(string attributeToUpdate, T updateValue,
            params string[] additionalAttributes)
        {
            throw new NotImplementedException();
        }

        ICanExecute IEntitySet.WeakUpdate<T>(string attributeToUpdate, Func<EntityWrapper, T> getUpdateValue,
            params string[] additionalAttributes)
        {
            throw new NotImplementedException();
        }

        ICanExecute ICanExecute.Clear(string attributeToClear, params string[] additionalAttributesToClear)
        {
            throw new NotImplementedException();
        }

        public ICanExecute Clear(string attributeToClear, params string[] additionalAttributesToClear)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}