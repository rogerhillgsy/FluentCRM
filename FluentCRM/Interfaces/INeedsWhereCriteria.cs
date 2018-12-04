using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCRM.Interfaces
{
    public interface INeedsWhereCriteria
    {
        IEntitySet Equals<T>(T value);
        IEntitySet IsNotNull { get; }
        IEntitySet IsNull { get; }
        IEntitySet In<T>(params T[] inVals);
        IEntitySet GreaterThan<T>(T value);
        IEntitySet LessThan<T>(T value);
    }
}
