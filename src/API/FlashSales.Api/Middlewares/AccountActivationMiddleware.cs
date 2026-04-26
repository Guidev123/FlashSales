namespace FlashSales.Api.Middlewares
{
    internal sealed class AccountActivationMiddleware : IMiddleware
    {
        private const string ActivateCustomerRoute = "/api/v1/users/customer/activate";

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.User.Identity is not null
                && context.User.Identity.IsAuthenticated)
            {
                if (context.Request.Path.StartsWithSegments(ActivateCustomerRoute))
                {
                    await next(context);
                    return;
                }

                var isActivated = context.User.HasClaim("activated", "true");

                if (!isActivated)
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        type = "account_not_activated",
                        redirectTo = "/activate"
                    });

                    return;
                }
            }

            await next(context);
        }
    }
}