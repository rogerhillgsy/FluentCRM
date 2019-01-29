using Microsoft.Xrm.Sdk.Query;

namespace FluentCRM
{
    public interface IJoinableNeedsWhereCriteria
    {
        IJoinableEntitySet Equals<T>(T value);
        IJoinableEntitySet NotEqual<T>(T value);
        IJoinableEntitySet IsNotNull { get; }
        IJoinableEntitySet IsNull { get; }
        IJoinableEntitySet In<T>(params T[] inVals);
        IJoinableEntitySet GreaterThan<T>(T value);
        IJoinableEntitySet LessThan<T>(T value);
        IJoinableEntitySet BeginsWith(string s);
        IJoinableEntitySet Condition<T>(ConditionOperator op, T value);    }
}