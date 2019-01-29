namespace FluentCRM
{
    public interface IJoinableAnotherWhere
    {
        IJoinableNeedsWhereCriteria Where(string attributeName);
    }
}