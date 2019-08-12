using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Kara.Droid.Helpers;
using Android.Content;
using Kara.CustomRenderer;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Java.Lang.Reflect;
using CarouselView.FormsPlugin.Android;
using Android.Content.Res;
using Kara.Helpers;
using Android.App.Admin;
using Android.Locations;
using Kara.Assets;
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
            
            //App.KaraTimeProvider = new KaraTimeProvider();
            //App.InternetDate = new InternetDate();

            //KaraNewService.SetSettings
            //(
            //    App.ServerAddress,
            //    App.DailyTrackingBeginTime_Seconds.Value,
            //    App.DailyTrackingEndTime_Seconds.Value,
            //    App.MaxAcceptableAccuracy.Value,
            //    App.GetLocationsPerid.Value,
            //    App.ShouldTurnGPSAndNetworkAutomatically.Value,
            //    App.GPSTracking_GPSShouldBeTurnedOnToWorkWithApp.Value,
            //    App.GPSTracking_NetworkShouldBeTurnedOnToWorkWithApp.Value
            //);   
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
            var RedBitmap   = BitmapFactory.DecodeResource(Resources, Resource.Drawable.PinRedMarker);
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

        //private async void CheckMajorSystemSettingsToBeTruelySet(int? requestCode)
        //{
        //    if (requestCode.HasValue)
        //    {
        //        var SettingDialougeLaunched = false;
        //        if ((SettingDialougeLauncherRequestCode)requestCode == SettingDialougeLauncherRequestCode.DeviceAdminSetting)
        //            SettingDialougeLaunched = App.MajorDeviceSetting.CheckDeviceAdminSetting();
        //        else if ((SettingDialougeLauncherRequestCode)requestCode == SettingDialougeLauncherRequestCode.DateTime)
        //            SettingDialougeLaunched = App.MajorDeviceSetting.CheckDateTimeSetting();
        //        else if ((SettingDialougeLauncherRequestCode)requestCode == SettingDialougeLauncherRequestCode.GPSSetting)
        //            SettingDialougeLaunched = App.MajorDeviceSetting.CheckGPSSetting();
        //        else if ((SettingDialougeLauncherRequestCode)requestCode == SettingDialougeLauncherRequestCode.GPSPermission)
        //            SettingDialougeLaunched = App.MajorDeviceSetting.CheckGPSPermission();
        //        else if ((SettingDialougeLauncherRequestCode)requestCode == SettingDialougeLauncherRequestCode.InternetConnection)
        //            SettingDialougeLaunched = await App.MajorDeviceSetting.CheckInternetConnection(false);

        //        if (!SettingDialougeLaunched)
        //            CheckMajorSystemSettingsToBeTruelySet(null);
        //    }
        //    else
        //    {
        //        var OneSettingCheckLaunched = App.MajorDeviceSetting.CheckDeviceAdminSetting();
        //        if (!OneSettingCheckLaunched)
        //            OneSettingCheckLaunched = App.MajorDeviceSetting.CheckDateTimeSetting();
        //        if (!OneSettingCheckLaunched)
        //            OneSettingCheckLaunched = App.MajorDeviceSetting.CheckGPSSetting();
        //        if (!OneSettingCheckLaunched)
        //            OneSettingCheckLaunched = App.MajorDeviceSetting.CheckGPSPermission();
        //        if (!OneSettingCheckLaunched)
        //            OneSettingCheckLaunched = await App.MajorDeviceSetting.CheckInternetConnection(false);
        //    }
        //}

        protected override void OnStart()
        {
            base.OnStart();

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
        }

        //TODO
        //class AutoTimeChangedReceiver : BroadcastReceiver
        //{
        //    public ContentResolver ContentResolver { get; set; }
        //    public override void OnReceive(Context context, Android.Content.Intent intent)
        //    {
        //        if (Android.Provider.Settings.Global.GetInt(ContentResolver, Android.Provider.Settings.Global.AutoTime, 0) == 1)
        //            App.MajorDeviceSettingsChanged(App.ChangedMajorDeviceSetting.AutomaticTimeEnabled);
        //        if (Android.Provider.Settings.Global.GetInt(ContentResolver, Android.Provider.Settings.Global.AutoTime, 0) != 1)
        //            App.MajorDeviceSettingsChanged(App.ChangedMajorDeviceSetting.AutomaticTimeDisabled);
        //    }
        //}


        //class KaraNewServiceLocationChangedReceiver : BroadcastReceiver
        //{
        //    public override void OnReceive(Context context, Android.Content.Intent intent)
        //    {
        //        var NewLocationStr = intent.GetStringExtra("NewLocation");
        //        if(!string.IsNullOrEmpty(NewLocationStr))
        //        {
        //            var NewLocation = NewLocationModel.FromString(NewLocationStr);
        //            if(NewLocation.DeviceState == (int)Kara.Assets.DeviceState.GoodLocation || NewLocation.DeviceState == (int)Kara.Assets.DeviceState.LocationWithTooMuchError)
        //                App.LastLocation = NewLocation;
        //        }
        //    }
        //}

        //class KaraNewServiceUnsentLocationAvailableReceiver : BroadcastReceiver
        //{
        //    public override void OnReceive(Context context, Android.Content.Intent intent)
        //    {
        //        ((MainActivity)context).GetUnsentLocationsFromKaraNewService();
        //        InvokeAbortBroadcast();
        //    }
        //}

        //class KaraNewServiceKaraTimeChangedReceiver : BroadcastReceiver
        //{
        //    public override void OnReceive(Context context, Android.Content.Intent intent)
        //    {
        //        var KaraTimeStr = intent.GetStringExtra("KaraTime");
        //        if (!string.IsNullOrEmpty(KaraTimeStr))
        //            DateTime.Now = Convert.ToDateTime(KaraTimeStr);
        //    }
        //}

        //void GetUnsentLocationsFromKaraNewService()
        //{
        //    //if (isBound)
        //    //{
        //    //    RunOnUiThread(async () =>
        //    //    {
        //    //        var karaNewService = binder.GetKaraNewService();
        //    //        while (true)
        //    //        {
        //    //            var locations = karaNewService.GetLocations();

        //    //            if (locations != null)
        //    //            {
        //    //                var result = await App.DB.InsertAllRecordsAsync<NewLocationModel>(locations);
        //    //                if (result.Success)
        //    //                    karaNewService.LocationsReceived(locations);

        //    //                if (locations.Count < 100)
        //    //                    break;
        //    //            }
        //    //        }
        //    //    });
        //    //}
        //}

        //class KaraNewServiceConnection : Java.Lang.Object, IServiceConnection
        //{
        //    MainActivity activity;
        //
        //    public KaraNewServiceConnection(MainActivity activity)
        //    {
        //        this.activity = activity;
        //    }
        //
        //    public void OnServiceConnected(ComponentName name, IBinder service)
        //    {
        //        var karaNewServiceBinder = service as KaraNewServiceBinder;
        //        if (karaNewServiceBinder != null)
        //        {
        //            var binder = (KaraNewServiceBinder)service;
        //            activity.binder = binder;
        //            activity.isBound = true;
        //        }
        //    }
        //
        //    public void OnServiceDisconnected(ComponentName name)
        //    {
        //        activity.isBound = false;
        //    }
        //}
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

