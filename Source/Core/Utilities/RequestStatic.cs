using System;
using System.Linq.Expressions;
using System.Web;

namespace PHP.Core.Utilities
{
    /// <summary>
    /// Class to allow for variables that are static to a web request, or thread in non-web code
    /// </summary>
    public class RequestStatic<T>
    {
        // Thread static variable to store the value for non-web code. 
#if !SILVERLIGHT
        // TODO: Silverlight does not support ThreadStatic, so just use regular static variables for now
        [ThreadStatic]
#endif
		private static T threadStatic;

        // Define the key we will use to look up this value
        private string contextKey;

        // The default value for this type
        private T defaultValue;

        /// <summary>
        /// Constructor to create a request static variable of a specific type. We pass in an expression
        /// to represent the variable being wrapped so there is a level of type safety and automation
        /// to create the HttpContext.Items dictionary entry so it will be unqiue to this variable.
        /// </summary>
        /// <param name="expression">Expression to represent the variable being wrapped</param>
        public RequestStatic(
            Expression<Func<T>> expression)
        {
            var member = ((MemberExpression)((MemberExpression)expression.Body).Expression).Member;
            contextKey = "_rqs_" + member.DeclaringType.FullName + member.Name;
        }

        /// <summary>
        /// Constructor to create a request static variable of a specific type
        /// </summary>
        /// <param name="expression">Expression to represent the variable being wrapped</param>
        /// <param name="defaultValue">Default value to assign to the variable</param>
        public RequestStatic(
            Expression<Func<T>> expression,
            T defaultValue)
            : this(expression)
        {
            threadStatic = this.defaultValue = defaultValue;
        }

        /// <summary>
        /// Property to return the value of the RequestStatic variable
        /// </summary>
        public T Value
        {
            get
            {
                var httpContext = HttpContext.Current;
                if (httpContext != null)
                {
                    var items = httpContext.Items;
                    return items.Contains(contextKey) ? (T)items[contextKey] : defaultValue;
                }
                return threadStatic;
            }
            set
            {
                var httpContext = HttpContext.Current;
                if (httpContext != null)
                    httpContext.Items[contextKey] = value;
                else
                    threadStatic = value;
            }
        }
    }
}
