using System.Reflection;

namespace SimplePM.WebAPI.Library
{
    public static class MethodInfoExtensions
    {
        public static string GetParamName(this MethodInfo method, int index)
        {
            string retVal = string.Empty;
            if (method != null && method.GetParameters().Length > index)
            {
                retVal = method.GetParameters()[index].Name;
            }
            return retVal;
        }
    }
}
