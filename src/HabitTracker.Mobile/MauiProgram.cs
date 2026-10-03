using HabitTracker.Mobile.Pages;
using HabitTracker.Mobile.Services;
using HabitTracker.Mobile.ViewModels;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;

namespace HabitTracker.Mobile;

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
            });

        // Auth
#if DEBUG
        builder.Services.AddSingleton<ICognitoAuthService, DevAuthService>();
#else
        builder.Services.AddSingleton(_ => CognitoSettings.Load());
        builder.Services.AddSingleton<ICognitoAuthService, CognitoAuthService>();
#endif
        builder.Services.AddSingleton<SecureStorageService>();

        // HTTP Client with auth header
#if DEBUG
    #if ANDROID
        var baseUrl = "http://10.0.2.2:3000"; // sam local start-api, seen from the Android emulator
    #else
        var baseUrl = "http://127.0.0.1:3000"; // sam local start-api
    #endif
#else
        var baseUrl = ApiSettings.Load().BaseUrl;
#endif
        builder.Services.AddTransient<AuthHeaderHandler>();
        builder.Services.AddHttpClient<IHabitService, HabitService>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>()
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.DisableForUnsafeHttpMethods(); // never retry POST (e.g. create habit)
            });

        // ViewModels
        builder.Services.AddTransient<HabitsViewModel>();
        builder.Services.AddTransient<AddHabitViewModel>();
        builder.Services.AddTransient<HabitDetailViewModel>();
        builder.Services.AddTransient<EditHabitViewModel>();
        builder.Services.AddTransient<StatsViewModel>();
        builder.Services.AddTransient<LoginViewModel>();

        // Pages
        builder.Services.AddTransient<HabitsPage>();
        builder.Services.AddTransient<AddHabitPage>();
        builder.Services.AddTransient<HabitDetailPage>();
        builder.Services.AddTransient<EditHabitPage>();
        builder.Services.AddTransient<StatsPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
