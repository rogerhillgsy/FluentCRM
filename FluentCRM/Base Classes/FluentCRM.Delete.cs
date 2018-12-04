using System;
using FluentCRM.Interfaces;

namespace FluentCRM.Base_Classes
{
    public abstract partial class FluentCRM
    {
        #region "Delete selected entity records"

        ICanExecute ICanExecute.Delete()
        {
            throw new NotImplementedException();
        }

        ICanExecute IEntitySet.Delete()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}