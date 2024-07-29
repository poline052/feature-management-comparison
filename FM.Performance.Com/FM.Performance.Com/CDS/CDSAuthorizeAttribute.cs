using Flagsmith;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace FM.Performance.Com.FlagSmith
{
    public class CDSAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _featureFlagName;

        public CDSAuthorizeAttribute(string featureFlagName)
        {
            _featureFlagName = featureFlagName;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Get the Flagsmith client from the service provider
            var redisClient = context.HttpContext.RequestServices.GetService<IConnectionMultiplexer>();

            if (redisClient == null)
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

            var database = redisClient.GetDatabase(12426776);

            var features = await database.StringGetAsync(identifier);
            UserPermission permission;
            if (string.IsNullOrEmpty(features))
            {
                //Pull from DB
                permission = new UserPermission { Features = new List<string> { "cds-feature" } };
                await database.StringSetAsync(identifier, JsonConvert.SerializeObject(permission));
            }
            else
            {
                permission = JsonConvert.DeserializeObject<UserPermission>(features);
            }
            if (!IsFeatureEnabled(permission, _featureFlagName))
            {
                // If the feature is not enabled, deny access
                context.Result = new ForbidResult();
            }
        }
        private bool IsFeatureEnabled(UserPermission permission, string feature)
        {
            return permission.Features.Any(f => f.Equals(feature));
        }
    }
    public class UserPermission
    {
        public List<string> Features { get; set; } = new List<string>();
    }
}
