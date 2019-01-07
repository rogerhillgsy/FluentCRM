using Microsoft.Xrm.Sdk.Query;

namespace FluentCRM
{
    public interface INeedsWhereCriteria
    {
        IEntitySet Equals<T>(T value);
        IEntitySet NotEqual<T>(T value);
        IEntitySet IsNotNull { get; }
        IEntitySet IsNull { get; }
        IEntitySet In<T>(params T[] inVals);
        IEntitySet GreaterThan<T>(T value);
        IEntitySet LessThan<T>(T value);
        IEntitySet Condition<T>(ConditionOperator op, T value);
    }
}
