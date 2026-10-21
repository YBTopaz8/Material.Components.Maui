
namespace Material.Components.Maui.Extensions;

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder UseMaterialComponents(this MauiAppBuilder builder)
    {
        return builder.ConfigureMauiHandlers(
            (handlers) =>
            {

            }
        );
    }
}
