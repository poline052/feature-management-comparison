using Flagsmith;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace FM.Performance.Com.FlagSmith
{
    public class FSAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _featureFlagName;

        public FSAuthorizeAttribute(string featureFlagName)
        {
            _featureFlagName = featureFlagName;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Get the Flagsmith client from the service provider
            var flagsmithClient = context.HttpContext.RequestServices.GetService<IFlagsmithClient>();

            if (flagsmithClient == null)
            {
                // If Flagsmith client is not available, deny access
                context.Result = new UnauthorizedResult();
                return;
            }

            // Check if the feature is enabled
            var identifier = "fs@personal.co.uk";
            //var traitKey = "car_type";
            //var traitValue = "robin_reliant";
            //var traitList = new List<ITrait> { new Trait(traitKey, traitValue) };

            var flags = await flagsmithClient.GetIdentityFlags(identifier);

            var isFeatureEnabled = await flags.IsFeatureEnabled(_featureFlagName);

            if (!isFeatureEnabled)
            {
                // If the feature is not enabled, deny access
                context.Result = new ForbidResult();
            }
        }
    }
}
