using Kara.Assets;
using Plugin.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Kara.CustomRenderer;
using Kara.Helpers;

namespace Kara
{
    public partial class LoginForm : GradientContentPage
    {
        Entry ServerAddress = new MyEntry() { HorizontalTextAlignment = TextAlignment.End, Placeholder = "آدرس سرور، مثلا: 192.168.1.2".ReplaceLatinDigits(), LeftRounded = true };
        Image ServerAddressIcon = new EntryCompanionIcon() { Source = "url.png" };
        Entry Username = new MyEntry() { HorizontalTextAlignment = TextAlignment.End, Placeholder = "نام کاربری", LeftRounded = true };
        Image UsernameIcon = new EntryCompanionIcon() { Source = "username.png" };
        Entry Password = new MyEntry() { HorizontalTextAlignment = TextAlignment.End, Placeholder = "کلمه عبور", IsPassword = true, LeftRounded = true };
        Image PasswordIcon = new EntryCompanionIcon() { Source = "password.png" };
        Button LoginButton = new RoundButton() { Text = "ورود", FontAttributes = FontAttributes.Bold };
        ActivityIndicator BusyIndicator = new ActivityIndicator() { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, HeightRequest = 30, Color = Color.FromHex("E6EBEF"), IsRunning = false };
        Label LoginErrorText = new Label() { TextColor = Color.FromHex("f33"), HorizontalTextAlignment = TextAlignment.Center };

        public LoginForm()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            
            LoginButton.Clicked += Login;

            ServerAddress.Text = App.ServerAddress;
            if(App.Username.Value != "")
                Username.Text = App.Username.Value;

            BusyIndicator.IsRunning = false;
        }

        Guid LastSizeAllocationId = Guid.NewGuid();
        protected override async void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            Guid ThisSizeAllocationId = Guid.NewGuid();
            LastSizeAllocationId = ThisSizeAllocationId;
            await Task.Delay(100);
            if (LastSizeAllocationId == ThisSizeAllocationId)
                sizeChanged(width, height);
        }

        double LastWidth, LastHeight;
        public void sizeChanged(double width, double height)
        {
            try
            {
                if (LastWidth != width || LastHeight != height)
                {
                    LastWidth = width;
                    LastHeight = height;

                    LoginLayoutGrid.RowDefinitions = new RowDefinitionCollection() {
                    new RowDefinition() { Height = new GridLength(10, GridUnitType.Star) },
                    new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition() { Height = new GridLength(0.5, GridUnitType.Auto) },
                    new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition() { Height = new GridLength(10, GridUnitType.Star) }
                };

                    var y = 0.8;
                    LoginLayoutGrid.ColumnDefinitions = new ColumnDefinitionCollection() {
                    new ColumnDefinition() { Width = new GridLength((1 - y) / 2, GridUnitType.Star) },
                    new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) },
                    new ColumnDefinition() { Width = new GridLength(y, GridUnitType.Star) },
                    new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) },
                    new ColumnDefinition() { Width = new GridLength((1 - y) / 2, GridUnitType.Star) }
                };

                    LoginLayoutGrid.Children.Clear();

                    LoginLayoutGrid.Children.Add(ServerAddress, 1, 1);
                    Grid.SetColumnSpan(ServerAddress, 2);
                    LoginLayoutGrid.Children.Add(ServerAddressIcon, 3, 1);
                    LoginLayoutGrid.Children.Add(Username, 1, 2);
                    Grid.SetColumnSpan(Username, 2);
                    LoginLayoutGrid.Children.Add(UsernameIcon, 3, 2);
                    LoginLayoutGrid.Children.Add(Password, 1, 3);
                    Grid.SetColumnSpan(Password, 2);
                    LoginLayoutGrid.Children.Add(PasswordIcon, 3, 3);
                    LoginLayoutGrid.Children.Add(LoginButton, 1, 5);
                    Grid.SetColumnSpan(LoginButton, 3);
                    LoginLayoutGrid.Children.Add(BusyIndicator, 1, 5);
                    LoginLayoutGrid.Children.Add(LoginErrorText, 1, 6);
                    Grid.SetColumnSpan(LoginErrorText, 3);
                }
            }
            catch (Exception)
            {
            }
        }

        public async void Login(object sender, EventArgs args)
        {
            LoginErrorText.IsVisible = false;
            var _ServerAddress = ServerAddress != null ? ServerAddress.Text != null ? ServerAddress.Text.ReplacePersianDigits() : "" : "";
            App.ServerAddress = _ServerAddress;
            
            var _Username = Username.Text;
            var _Password = Password.Text;
            var ResultTask = Kara.Assets.Connectivity.Login(_Username, _Password);

            BusyIndicator.IsRunning = true;
            var Result = await ResultTask;
            BusyIndicator.IsRunning = false;

            if (!Result.Success)
            {
                LoginErrorText.Text = Result.Message;
                LoginErrorText.IsVisible = true;
                return;
            }
            
            App.UserId.Value = Result.Data.UserId;
            App.Username.Value = _Username;
            App.Password.Value = _Password;
            App.UserPersonnelId.Value = Result.Data.PersonnelId;
            App.UserRealName.Value = Result.Data.RealName;
            
            await Navigation.PushAsync(new MainMenu()
            {
                StartColor = Color.FromHex("E6EBEF"),
                EndColor = Color.FromHex("A6CFED")
            });

            if(App.UserId.Value != App.LastLoginUserId.Value)
            {
                await App.DB.CleanDataBaseAsync();
                await Navigation.PushAsync(new UpdateDBForm()
                {
                    StartColor = Color.FromHex("E6EBEF"),
                    EndColor = Color.FromHex("A6CFED")
                });
                //App.MajorDeviceSetting.MajorDeviceSettingsChanged(ChangedMajorDeviceSetting.InitialStartup);
            }
            Navigation.RemovePage(this);
        }
    }
}
