using Android.App;
using Android.Runtime;

namespace HabitTracker.Mobile;

#if DEBUG
[Application(UsesCleartextTraffic = true)] // plain HTTP to sam local, Debug builds only
#else
[Application]
#endif
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
