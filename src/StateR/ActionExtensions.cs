namespace StateR
{
    public static class ActionExtensions
    {
        public static string GetName(this IAction action)
        {
            var fullName = action.GetType().FullName;
            var lastDot = fullName.LastIndexOf('.');
            return fullName.Substring(lastDot + 1);
        }
    }
}