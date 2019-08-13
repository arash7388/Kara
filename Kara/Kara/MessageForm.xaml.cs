using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kara
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessageForm : ContentPage
    {
        public MessageForm()
        {
            InitializeComponent();
        }

        private async void RetryButton_Clicked(object sender, EventArgs e)
        {
            var loc = await App.CheckGps();
            if (loc != null)
            {
                //await Navigation.PushAsync(new MainMenu()
                //{
                //    StartColor = Color.FromHex("E6EBEF"),
                //    EndColor = Color.FromHex("A6CFED")
                //});

                //Navigation.RemovePage(this);
                await Navigation.PopAsync();
            }
               
            
        }
    }
}