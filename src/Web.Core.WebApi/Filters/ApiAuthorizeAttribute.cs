using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Core.WebApi.Filters
{
    /// <summary>
    /// Authenication Filter for use in API's.
    /// Returns a 401 statuscode instead of redirecting to login page.
    /// </summary>
    public class ApiAuthorizeAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Method to check if user is authenicated.
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext?.User?.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
