﻿using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Kara.CustomRenderer;
using Kara.Assets;
using Xamarin.Essentials;

namespace Kara
{
    public partial class MainMenu : GradientContentPage
    {
        ToolbarItem LogoutMenu, UserNameMenu;

        Image Image_InsertFailedVisit, Image_InsertOrder, Image_Customers, Image_Settings, Image_UpdateDB, Image_Visits, Image_Backups, Image_PartnerReport, Image_Report;
        Label Label_InsertFailedVisit, Label_InsertOrder, Label_Customers, Label_Settings, Label_UpdateDB, Label_Visits, Label_Backups, Label_PartnerReport, Label_Report;
        Image[] MenuImages;
        Label[] MenuLabels;

        private bool _firstTime = true;


        public MainMenu()
        {
            InitializeComponent();

            SetTodayDateAsTitle();

            UserNameMenu = new ToolbarItem() { Text = App.UserRealName.Value, Priority = 0, Order = ToolbarItemOrder.Primary };
            this.ToolbarItems.Add(UserNameMenu);
            LogoutMenu = new ToolbarItem() { Text = "خروج", Priority = 0, Order = ToolbarItemOrder.Primary, Icon = "Logout.png" };
            LogoutMenu.Clicked += LogoutMenu_Clicked;
            this.ToolbarItems.Add(LogoutMenu);

            Image_Customers = new Image() { Source = "MainMenu_Customers.png" };
            Image_InsertOrder = new Image() { Source = "MainMenu_AddInvoice.png" };
            Image_InsertFailedVisit = new Image() { Source = "MainMenu_AddFailedInvoice.png" };
            Image_Visits = new Image() { Source = "MainMenu_Visits.png" };
            Image_UpdateDB = new Image() { Source = "MainMenu_UpdateDB.png" };
            Image_Settings = new Image() { Source = "MainMenu_Settings.png" };
            Image_Backups = new Image() { Source = "MainMenu_Backups.png" };
            Image_PartnerReport = new Image() { Source = "MainMenu_PartnerReport.png" };
            Image_Report = new Image() { Source = "MainMenu_Reports.png" };

            Label_Customers = new Label() { Text = "لیست مشتریان", HorizontalTextAlignment = TextAlignment.Center, FontAttributes = FontAttributes.Bold, FontSize = 15 };
            Label_InsertOrder = new Label() { Text = "ثبت سفارش", HorizontalTextAlignment = TextAlignment.Center, FontAttributes = FontAttributes.Bold, FontSize = 15 };
            Label_InsertFailedVisit = new Label() { Text = "ثبت عدم سفارش", HorizontalTextAlignment = TextAlignment.Center, FontAttributes = FontAttributes.Bold, FontSize = 15 };
            Label_Visits = new Label() { Text = "اطلاعات ثبت شده", HorizontalTextAlignment = TextAlignment.Center, FontAttributes = FontAttributes.Bold, FontSize = 15 };
            Label_UpdateDB = new Label() { Text = "بروزرسانی اطلاعات", HorizontalTextAlignment = TextAlignment.Center, FontAttributes = FontAttributes.Bold, FontSize = 15 };
            Label_Settings = new Label() { Text = "تنظیمات", HorizontalTextAlignment = TextAlignment.Center, FontAttributes = FontAttributes.Bold, FontSize = 15 };
            Label_Backups = new Label() { Text = "پشتیبان اطلاعات", HorizontalTextAlignment = TextAlignment.Center, FontAttributes = FontAttributes.Bold, FontSize = 15 };
            Label_PartnerReport = new Label() { Text = "گردش حساب مشتری", HorizontalTextAlignment = TextAlignment.Center, FontAttributes = FontAttributes.Bold, FontSize = 15 };
            Label_Report = new Label() { Text = "گزارشات", HorizontalTextAlignment = TextAlignment.Center, FontAttributes = FontAttributes.Bold, FontSize = 15 };

            MenuImages = new Image[] { Image_Customers, Image_InsertOrder, Image_InsertFailedVisit, Image_Visits, Image_PartnerReport, Image_Report, Image_UpdateDB, Image_Settings, Image_Backups };
            MenuLabels = new Label[] { Label_Customers, Label_InsertOrder, Label_InsertFailedVisit, Label_Visits, Label_PartnerReport, Label_Report, Label_UpdateDB, Label_Settings, Label_Backups };



            Image_Customers.GestureRecognizers.Add(new TapGestureRecognizer(MainMenu_GoToPartnerListForm));
            Image_InsertOrder.GestureRecognizers.Add(new TapGestureRecognizer(MainMenu_GoToOrderInsertForm));
            Image_InsertFailedVisit.GestureRecognizers.Add(new TapGestureRecognizer(MainMenu_GoToFailedOrderInsertForm));
            Image_Visits.GestureRecognizers.Add(new TapGestureRecognizer(MainMenu_GoToInsertedInformationsForm));
            Image_UpdateDB.GestureRecognizers.Add(new TapGestureRecognizer(MainMenu_GoToUpdateDBForm));
            Image_Settings.GestureRecognizers.Add(new TapGestureRecognizer(MainMenu_GoToSettingForm));
            Image_Backups.GestureRecognizers.Add(new TapGestureRecognizer(MainMenu_GoToBackupForm));
            Image_PartnerReport.GestureRecognizers.Add(new TapGestureRecognizer(MainMenu_GoToPartnerReportForm));
            Image_Report.GestureRecognizers.Add(new TapGestureRecognizer(MainMenu_GoToReportsForm));


            MessagingCenter.Subscribe<object, string>(this, "CheckGps", (sender, msg) =>
            {
                Device.BeginInvokeOnMainThread(() => {
                    bool.TryParse(msg, out bool GpsEnabled);
                    App.GpsEnabled = GpsEnabled;
                    //App.ToastMessageHandler.ShowMessage( msg,Helpers.ToastMessageDuration.Long);
                });
            });
        }

        private async void SetTodayDateAsTitle()
        {
            this.Title = DateTime.Now.ToShortStringForDate().ReplaceLatinDigits();
            await Task.Delay(30000);
            SetTodayDateAsTitle();
        }

        private async void LogoutMenu_Clicked(object sender, EventArgs e)
        {
            var answer = await DisplayAlert("خروج از نرم افزار", "آیا مطمئنید؟", "بله", "خیر");
            if (answer)
            {
                App.LastLoginUserId.Value = App.UserId.Value;
                App.UserId.Value = Guid.Empty;
                var LoginForm = new LoginForm()
                {
                    StartColor = Color.FromHex("E6EBEF"),
                    EndColor = Color.FromHex("A6CFED")
                };
                await this.Navigation.PushAsync(LoginForm);
                this.Navigation.RemovePage(this);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if(_firstTime)
            {
                _firstTime = false;
                MessagingCenter.Send(this, "MainMenuOpened");
            }

            if (App.FirstGpsDetecting)
            {
                App.FirstGpsDetecting = false;
                App.GpsEnabled = true;
                var task = Task.Run(async
                    () =>
                    {
                        var loc = await App.CheckGps();
                        if (loc == null)
                        {
                            await Navigation.PushAsync(new MessageForm());
                        }
                    }
                );
            }

        }

        DateTime? LastBackButtonPressedTime = null;
        protected override bool OnBackButtonPressed()
        {
            if(LastBackButtonPressedTime.HasValue && LastBackButtonPressedTime > DateTime.Now.AddSeconds(-5))
                return base.OnBackButtonPressed();
            else
            {
                LastBackButtonPressedTime = DateTime.Now;
                return true;
            }
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

                    int RowCount, ColCount;
                    if (height > width)
                    {
                        RowCount = 4;
                        ColCount = 3;
                    }
                    else
                    {
                        RowCount = 3;
                        ColCount = 4;
                    }

                    MainMenuGrid.RowDefinitions = new RowDefinitionCollection();
                    MainMenuGrid.ColumnDefinitions = new ColumnDefinitionCollection();
                    MainMenuGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(10, GridUnitType.Absolute) });
                    for (int i = 0; i < RowCount; i++)
                    {
                        MainMenuGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(2, GridUnitType.Star) });
                        MainMenuGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                    }
                    var y1 = 0.4;
                    var y2 = 0.75 * Math.Max(width, height) / Math.Min(width, height) * y1;
                    var y = width > height ? y1 : y2;
                    for (int i = 0; i < ColCount; i++)
                    {
                        MainMenuGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength((1 - y) / 2, GridUnitType.Star) });
                        MainMenuGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(y, GridUnitType.Star) });
                        MainMenuGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength((1 - y) / 2, GridUnitType.Star) });
                    }

                    MainMenuGrid.Children.Clear();
                    for (int i = 0; i < RowCount; i++)
                    {
                        for (int j = 0; j < ColCount; j++)
                        {
                            var ItemNumber = i * ColCount + j;
                            if (ItemNumber < MenuImages.Length)
                            {
                                var img = MenuImages[ItemNumber];
                                var lbl = MenuLabels[ItemNumber];

                                MainMenuGrid.Children.Add(img, ((ColCount - 1) * 3) - (j * 3) + 1, i * 2 + 1);
                                MainMenuGrid.Children.Add(lbl, ((ColCount - 1) * 3) - (j * 3), i * 2 + 2);
                                Grid.SetColumnSpan(lbl, 3);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void MainMenu_GoToUpdateDBForm(View arg1, object arg2)
        {
            Navigation.PushAsync(new UpdateDBForm()
            {
                StartColor = Color.FromHex("E6EBEF"),
                EndColor = Color.FromHex("A6CFED")
            });
        }

        private bool CheckGpsConnection()
        {
            if(!App.GpsEnabled)
            {
                App.ToastMessageHandler.ShowMessage("لطفا مکان یاب را فعال نمایید", Helpers.ToastMessageDuration.Long);
                return false;
            }

            return true;
        }

        async void MainMenu_GoToPartnerListForm(View arg1, object arg2)
        {
            if(CheckGpsConnection())
              await Navigation.PushAsync(new PartnerListForm());
        }

        async void MainMenu_GoToOrderInsertForm(View arg1, object arg2)
        {
            if (CheckGpsConnection())
            {
                var OrderInsertForm = new OrderInsertForm(null, null, null, null, null)
                {
                    StartColor = Color.FromHex("E6EBEF"),
                    EndColor = Color.FromHex("A6CFED")
                };
                await Navigation.PushAsync(OrderInsertForm);
            }
        }

        async void MainMenu_GoToFailedOrderInsertForm(View arg1, object arg2)
        {
            if (CheckGpsConnection())
            {
                var FailedOrderInsertForm = new FailedOrderInsertForm(null, null, null, null)
                {
                    StartColor = Color.FromHex("E6EBEF"),
                    EndColor = Color.FromHex("A6CFED")
                };
                await Navigation.PushAsync(FailedOrderInsertForm);
            }
        }

        async void MainMenu_GoToInsertedInformationsForm(View arg1, object arg2)
        {
            if (CheckGpsConnection())
            {
                var InsertedInformations = new InsertedInformations();
                await Navigation.PushAsync(InsertedInformations);
            }
        }

        async void MainMenu_GoToSettingForm(View arg1, object arg2)
        {
            var SettingsForm = new SettingsForm()
            {
                StartColor = Color.FromHex("E6EBEF"),
                EndColor = Color.FromHex("A6CFED")
            };
            await Navigation.PushAsync(SettingsForm);
        }

        async void MainMenu_GoToBackupForm(View arg1, object arg2)
        {
            var BackupsForm = new BackupsForm()
            {
                StartColor = Color.FromHex("E6EBEF"),
                EndColor = Color.FromHex("A6CFED")
            };
            await Navigation.PushAsync(BackupsForm);
        }

        async void MainMenu_GoToPartnerReportForm(View arg1, object arg2)
        {
            var PartnerReportForm = new PartnerReportForm(null);
            await Navigation.PushAsync(PartnerReportForm, false);

            var PartnerListForm = new PartnerListForm();
            PartnerListForm.PartnerReportForm = PartnerReportForm;
            await Navigation.PushAsync(PartnerListForm);
        }

        async void MainMenu_GoToReportsForm(View arg1, object arg2)
        {
            var ReportForm = new ReportForm()
            {
                StartColor = Color.FromHex("E6EBEF"),
                EndColor = Color.FromHex("A6CFED")
            };
            await Navigation.PushAsync(ReportForm, false);
        }
    }
}
