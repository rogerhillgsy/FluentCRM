using System;
using FluentCRM.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FluentCRM.Base_Classes
{
    public abstract partial class FluentCRM : IJoinable
    {
        #region "Join functions"

        ICanExecute IEntitySet.Join<T>(Action<IJoinable> target)
        {
            throw new NotImplementedException();
        }

        ICanExecute IUnknownEntity.Join<T>(Action<IJoinable> target)
        {
            throw new NotImplementedException();
        }

        ICanExecute ICanExecute.Join<T>(Action<IJoinable> target)
        {
            throw new NotImplementedException();
        }

        public IJoinable UseAttribute<T>(Action<string, T> action, string attribute1, params string[] attributes)
        {
            throw new NotImplementedException();
        }

        public IJoinable UseEntity(Action<EntityWrapper> action, string attribute1, params string[] attributes)
        {
            throw new NotImplementedException();
        }

        public IJoinable UseAttribute<T>(Action<T> action, string attribute1, params string[] attributes)
        {
            throw new NotImplementedException();
        }

        public ICanExecute Join<T>(Action<IJoinable> target) where T : IJoinable, new()
        {
            throw new NotImplementedException();
        }

        public virtual string JoinAttribute(string JoinEntity)
        {
            throw new NotImplementedException();
        }

        public IJoinable Outer { get; }

        #endregion

        protected FluentCRM()
        {
            GetAlias = () => $"a.{AliasCount++}";
        }

        protected int AliasCount = 1;

        protected Func<string> _getAlias;
        protected Func<string> GetAlias
        {
            get { return _getAlias ?? (_getAlias = () => $"a{AliasCount++}"); }
            set { _getAlias = value; }
        }

        public abstract IJoinable Factory(IOrganizationService service);

        private LinkEntity _linkEntity;
        protected LinkEntity LinkEntity
        {
            get
            {
                if (_linkEntity == null)
                {
                    _linkEntity = new LinkEntity
                    {
                        Columns = new ColumnSet(true),
                        JoinOperator = JoinOperator.Inner,
                        LinkFromEntityName = LogicalName
                    };
                }

                return _linkEntity;
            }
        }
    }
}