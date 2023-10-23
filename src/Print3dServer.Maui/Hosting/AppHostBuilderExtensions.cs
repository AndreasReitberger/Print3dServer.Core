using AndreasReitberger.Shared.Syncfusion.Hosting;

namespace AndreasReitberger.API.Print3dServer.Maui.Hosting
{
    public static class AppHostBuilderExtensions
    {
        public static MauiAppBuilder InitializeSharedStyles(this MauiAppBuilder builder)
        {
            builder
                .InitializeSharedSyncfusionStyles()
                ;
            return builder;
        }
    }
}
