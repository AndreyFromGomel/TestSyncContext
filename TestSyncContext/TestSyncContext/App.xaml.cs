using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace TestSyncContext
{
    public partial class App : Application
    {
        private ThreadSyncContext _sC;
        [DllImport("__Internal", EntryPoint = "mono_get_runtime_build_info")]
        private extern static string GetMonoVersionInternal();

        public static string GetMonoVersion()
        {
            var type = Type.GetType("Mono.Runtime");
            if (type != null)
            {
                MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.Public | BindingFlags.Static);
                if (displayName != null)
                    return displayName.Invoke(null, null).ToString();
//                return GetMonoVersionInternal();
            }
//            else
            {
                return "Not Mono";
            }
        }

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
            _sC = new ThreadSyncContext();
            SynchronizationContext.SetSynchronizationContext(_sC);
            Device.StartTimer(new TimeSpan(0, 0, 1), () =>
            {
                _sC.Process();
                return true;
            });
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
