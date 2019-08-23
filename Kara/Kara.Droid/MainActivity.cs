using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Java.Lang.Reflect;
using Kara.Assets;
using Kara.CustomRenderer;
using Kara.Droid.Helpers;
using Kara.Helpers;
using System;
using System.Threading.Tasks;

namespace Kara.Droid
{
    [Activity(Label = "@string/ApplicationName", Icon = "@drawable/icon", Theme = "@style/MainTheme", NoHistory = false, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public DevicePolicyManager devicePolicyManager;
        public ComponentName myDeviceAdmin;
        public static Typeface IranSansFont;
        public static MainActivity MainActivityInstance;

        protected override void OnCreate(Bundle bundle)
        {
            MainActivityInstance = this;

            devicePolicyManager = (DevicePolicyManager)GetSystemService(Context.DevicePolicyService);
            myDeviceAdmin = new ComponentName(this, Java.Lang.Class.FromType(typeof(DeviceAdmin)));

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            //////////////////////////////////////////

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            Xamarin.Forms.Forms.Init(this, bundle);
            DisplayCrashReport();

            //////////////////////////////////////////

            IranSansFont = Typeface.CreateFromAsset(Assets, "IRANSansMobile.ttf");
            foreach (var font in new string[] { "DEFAULT", "MONOSPACE", "SERIF", "SANS_SERIF" })
                FontsOverride.SetDefaultFont(font, IranSansFont);

            Xamarin.FormsMaps.Init(this, bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            OxyPlot.Xamarin.Forms.Platform.Android.PlotViewRenderer.Init();

            App.imagesDirectory = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "StuffsGallery");

            InitializeSharedResources(this, ContentResolver);

            App.Downloader = new Downloader();
            App.QRScanner = new QRScan(this);
            App.Uploader = new Uploader();
            App.BluetoothPrinter = new BluetoothPrinter(this);
            PersianDatePicker.FragmentManager = FragmentManager;
            App.PersianDatePicker = new PersianDatePicker();

            App.DeviceSizeDensity = Resources.DisplayMetrics.Density;

            try
            {
                CreateMapIcons();
            }
            catch (Exception err)
            {
            }

            LoadApplication(new App());

            KaraNewServiceLauncher.StartAndScheduleAlarmManagerForkaraNewService(this);

            Xamarin.Essentials.Platform.Init(this, bundle); // add this line to your code, it may also be called: bundle
        }

        //#region Error handling
        private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
        {
            var newExc = new Exception("TaskSchedulerOnUnobservedTaskException", unobservedTaskExceptionEventArgs.Exception);
            LogUnhandledException(newExc);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var newExc = new Exception("CurrentDomainOnUnhandledException", unhandledExceptionEventArgs.ExceptionObject as Exception);
            LogUnhandledException(newExc);
        }

        internal static void LogUnhandledException(Exception exception)
        {
            try
            {
                const string errorFileName = "Fatal.log";
                var libraryPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // iOS: Environment.SpecialFolder.Resources
                var errorFilePath = System.IO.Path.Combine(libraryPath, errorFileName);
                var errorMessage = String.Format("Time: {0}\r\nUniversalLineInApp: {1}\r\nSpecialLog: {2}\r\nError: Unhandled Exception\r\n{3}",
                DateTime.Now, App.Last5UniversalLineInApp, App.SpecialLog, exception.ToString());
                if (!string.IsNullOrEmpty(Kara.OrderInsertForm.MultipleRecordsInAllStuffsData_Log))
                    errorMessage = "MultipleRecordsInAllStuffsData_Log: " + Kara.OrderInsertForm.MultipleRecordsInAllStuffsData_Log + ", errorMessage: " + errorMessage;
                System.IO.File.WriteAllText(errorFilePath, errorMessage);

                // Log to Android Device Logging.
                Android.Util.Log.Error("Crash Report", errorMessage);
            }
            catch
            {
                // just suppress any error logging exceptions
            }
        }

        /// <summary>
        // If there is an unhandled exception, the exception information is diplayed
        // on screen the next time the app is started (only in debug configuration)
        /// </summary>
        //[Conditional("DEBUG")]
        private void DisplayCrashReport()
        {
            const string errorFilename = "Fatal.log";
            var libraryPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var errorFilePath = System.IO.Path.Combine(libraryPath, errorFilename);

            if (!System.IO.File.Exists(errorFilePath))
            {
                return;
            }

            var errorText = System.IO.File.ReadAllText(errorFilePath);
            new AlertDialog.Builder(this)
                .SetPositiveButton("ارسال به سرور", async (sender, args) =>
                {
                    var SendResult = await Connectivity.SubmitExceptionsLog(errorText);
                    if (SendResult.Success)
                        System.IO.File.Delete(errorFilePath);
                })
                .SetNegativeButton("انصراف و حذف", (sender, args) =>
                {
                    System.IO.File.Delete(errorFilePath);
                })
                .SetMessage("در اجرای قبلی نرم افزار خطایی رخ داده است. لطفا برای کمک به ما در بهبود کیفیت برنامه این موارد را برایمان ارسال کنید.")
                .SetTitle("گزارش خطا")
                .Show();
        }

        //‪#endregion

        public static void InitializeSharedResources(Context Context, ContentResolver ContentResolver)
        {
            App.KaraVersion = new KaraVersion();
            App.TCPClient = new TCPClient();
            App.DBFileName = GetLocalFilePath("karadb.db3");
            App.DB = new DBRepository(App.DBFileName);
            App.ToastMessageHandler = new ToastMessageHandler(Context);
            App.File = new Helpers.File();
            App.PersianDateConverter = new PersianDateConverter();
            //App.MajorDeviceSetting = new MajorDeviceSetting() { ContentResolver = ContentResolver, Context = Context };
        }

        private void CreateMapIcons()
        {
            CustomMapRenderer.MapIcons = new System.Collections.Generic.Dictionary<MapIcon, BitmapDescriptor>();

            var BlueBitmap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.PinBlueMarker);
            var RedBitmap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.PinRedMarker);
            var GreenBitmap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.PinGreenMarker);

            var BlueBallon = BitmapDescriptorFactory.FromBitmap(BlueBitmap);
            var RedBallon = BitmapDescriptorFactory.FromBitmap(RedBitmap);
            var GreenBallon = BitmapDescriptorFactory.FromBitmap(GreenBitmap);

            CustomMapRenderer.MapIcons.Add(CustomRenderer.MapIcon.BlueBallon, BlueBallon);
            CustomMapRenderer.MapIcons.Add(CustomRenderer.MapIcon.RedBallon, RedBallon);
            CustomMapRenderer.MapIcons.Add(CustomRenderer.MapIcon.GreenBallon, GreenBallon);
        }

        private static string GetLocalFilePath(string filename)
        {
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            return System.IO.Path.Combine(path, filename);
        }

        protected override void OnStart()
        {
            base.OnStart();
            //var loc = App.CheckGps().GetAwaiter().GetResult();
            //if(loc!=null)
            //{
            //    MessagingCenter.Send<object, string>(this, "CheckGps", "true");
            //}
            //CheckMajorSystemSettingsToBeTruelySet(null);
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        public const int QRScanRequestCode = 0;//Max:65536

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == QRScanRequestCode)
            {
                ((QRScan)App.QRScanner).OnActivityResult(resultCode, data);
            }
            else if (requestCode == (int)SettingDialougeLauncherRequestCode.DateTime ||
                requestCode == (int)SettingDialougeLauncherRequestCode.DeviceAdminSetting ||
                requestCode == (int)SettingDialougeLauncherRequestCode.GPSSetting ||
                requestCode == (int)SettingDialougeLauncherRequestCode.InternetConnection)
            {
                //CheckMajorSystemSettingsToBeTruelySet(requestCode);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == (int)SettingDialougeLauncherRequestCode.GPSPermission)
            {
                //CheckMajorSystemSettingsToBeTruelySet(requestCode);
            }

            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    public class FontsOverride
    {
        public static void SetDefaultFont(string staticTypefaceFieldName, Typeface InsteadFont)
        {
            ReplaceFont(staticTypefaceFieldName, InsteadFont);
        }

        protected static void ReplaceFont(string staticTypefaceFieldName, Typeface newTypeface)
        {
            try
            {
                Field staticField = ((Java.Lang.Object)(newTypeface)).Class.GetDeclaredField(staticTypefaceFieldName);
                staticField.Accessible = true;
                staticField.Set(null, newTypeface);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}