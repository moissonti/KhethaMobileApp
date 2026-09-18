using Microsoft.Extensions.Logging;

namespace NCAP
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIconsOutlined-Regular.otf", "MaterialOutlined");
                    fonts.AddFont("MaterialIconsRound-Regular.otf", "MaterialRound");
                });

#if ANDROID
            Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("EnableJavaScript", (handler, view) =>
            {
                handler.PlatformView.Settings.JavaScriptEnabled = true;
                handler.PlatformView.Settings.DomStorageEnabled = true;
                handler.PlatformView.Settings.LoadWithOverviewMode = true;
                handler.PlatformView.Settings.UseWideViewPort = true;
                handler.PlatformView.Settings.MixedContentMode = Android.Webkit.MixedContentHandling.AlwaysAllow;
            });
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}