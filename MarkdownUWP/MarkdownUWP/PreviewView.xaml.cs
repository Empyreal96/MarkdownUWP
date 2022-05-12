using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MarkdownUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PreviewView : Page
    {
        public static WebView webView { get; set; }
        public PreviewView()
        {
            this.InitializeComponent();

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
            //Preview.NavigateToString(await MainPage.ConvertAsync(MainPage.currentText));
            String data = "<html><head>"
                + "<style type=\"text/css\">body{color: #fff; background-color: #FF333337;}"
                + "</style></head>"
                + "<body>"
                + "<span style=\"font-family: 'Segoe UI'\">"+ await MainPage.ConvertAsync(MainPage.currentText) + "</span>"
                + "</body></html>";
            Preview.NavigateToString(data);
        }
    }
}
