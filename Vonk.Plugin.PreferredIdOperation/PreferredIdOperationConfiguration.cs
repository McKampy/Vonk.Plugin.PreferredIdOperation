using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vonk.Core.Context;
using Vonk.Core.Metadata;
using Vonk.Core.Pluggability;
using Vonk.Core.Pluggability.ContextAware;

namespace Vonk.Plugin.PreferredIdOperation
{
    [VonkConfiguration(order: 4600)]
    public class PreferredIdOperationConfiguration
    {
        public static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.TryAddScoped<PreferredIdService>();
            services.TryAddContextAware<ICapabilityStatementContributor, PreferredIdOperationConformanceContributor>(ServiceLifetime.Transient);
            
            return services;
        }

        public static IApplicationBuilder Configure(IApplicationBuilder builder)
        {
            builder
                .OnCustomInteraction(VonkInteraction.type_custom, "preferred-id")
                .AndMethod("GET")
                .HandleAsyncWith<PreferredIdService>((svc, context) => svc.GetPreferredId(context));

            builder
                .OnCustomInteraction(VonkInteraction.type_custom, "preferred-id")
                .AndMethod("POST")
                .HandleAsyncWith<PreferredIdService>((svc, context) => svc.PostPreferredId(context));

            return builder;
        }
    }
}
