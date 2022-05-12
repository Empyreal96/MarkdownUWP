using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Core.Markdown;
using System.Threading.Tasks;
using Windows.UI;
using System.Globalization;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MarkdownUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static WebView Preview { get; set; }
       public static bool FileNameAccepted { get; set; }
        public static string SavedFileName { get; set; }

        //public MarkdownInit ViewModel => DataContext as MarkdownInit;
        public static string currentText { get; set; }
        public MainPage()
        {
            this.InitializeComponent();
            if (SavedFileName != null)
            {
                
                FileHeader.Text = SavedFileName;
                Debug.WriteLine($"Filename: {SavedFileName}");
            } else
            {
                FileHeader.Text = "Untitled.md";
            }
            //FileHeader.Text = "Untitled.md";
           
           // var HomePage = $"MarkdownUWP.MainPage";
            //var HomePageType = Type.GetType(HomePage);
            //ContentFrame.Navigate(HomePageType);
        }

        private async void MainNav_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            try
            {
                currentText = MarkEditor.Text;
                if (SavedFileName != null)
                {
                    FileHeader.Text = SavedFileName;
                }
             
                NavigateToPage(args.InvokedItemContainer.Tag);
            }
            catch (System.Exception ex)
            {
                var ThrownException = new MessageDialog($"{ex.Message}\n\n{ex.Source}\n\n{ex.Data}\n\n{ex.StackTrace}\n\n{ex.InnerException}");
                ThrownException.Commands.Add(new UICommand("Close"));
                await ThrownException.ShowAsync();
            }
        }

        public async void NavigateToPage(object pageTag)
        {
            try
            {
               
                NavigationCacheMode = NavigationCacheMode.Enabled;
                var pageName = $"MarkdownUWP.{pageTag}";
                var pageType = Type.GetType(pageName);
                if (pageType == null)
                {

                }
                else
                {
                    ContentFrame.Navigate(pageType);
                }
                } catch (Exception ex)
            {
                ThrowError(ex);
            }
        }




        public async void ThrowError (Exception ex)
        {
            var ThrownException = new MessageDialog($"{ex.Message}\n\n{ex.Source}\n\n{ex.Data}\n\n{ex.StackTrace}\n\n{ex.InnerException}");
            ThrownException.Commands.Add(new UICommand("Close"));
            await ThrownException.ShowAsync();
        }






        private async void Openbtn_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker loadFile = new FileOpenPicker();
            loadFile.FileTypeFilter.Add(".md");
            loadFile.FileTypeFilter.Add(".markdown");
            loadFile.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            StorageFile file = await loadFile.PickSingleFileAsync();
            if (file == null)
            {
                return;
            }
            FileHeader.Text = file.Path;
            string parsedText = await FileIO.ReadTextAsync(file);
            if (parsedText != null)
            {
                MarkEditor.Text = parsedText;
                currentText = parsedText;
            }
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            Progress.IsEnabled = true;
            Progress.IsIndeterminate = true;
            try
            {
                TextBox fileNameUser = new TextBox();
                fileNameUser.PlaceholderText = "Enter a File Name";
                fileNameUser.Text = "Untitled.md";
                fileNameUser.Height = 45;
                ContentDialog dialog = new ContentDialog();
                dialog.Content = fileNameUser;
                dialog.IsSecondaryButtonEnabled = true;
                dialog.PrimaryButtonText = "Confirm";
                dialog.SecondaryButtonText = "Cancel";

                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    if (fileNameUser.Text == "")
                    {
                        fileNameUser.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                    }
                    else
                    {
                        fileNameUser.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Green);
                        SavedFileName = fileNameUser.Text;
                    }
                }

                if (SavedFileName == null)
                {
                    return;
                }

                if (!SavedFileName.Contains(".md"))
                {
                    string newname = $"{SavedFileName}.md";
                    SavedFileName = newname;
                }


                FolderPicker saveFolder = new FolderPicker();
                saveFolder.FileTypeFilter.Add(".md");
                saveFolder.FileTypeFilter.Add(".markdown");
                StorageFolder folder = await saveFolder.PickSingleFolderAsync();
                if (folder == null)
                {
                    return;
                }
                Debug.WriteLine($"Filename: {SavedFileName}");
                StorageFile saveFile = await folder.CreateFileAsync($"{SavedFileName}");
                SavedFileName = saveFile.Path;
                await FileIO.WriteTextAsync(saveFile, MarkEditor.Text);
                FileHeader.Text = saveFile.Path;
                var ThrownSuccess = new MessageDialog($"File saved to: {saveFile.Path}");
                ThrownSuccess.Commands.Add(new UICommand("Close"));
                await ThrownSuccess.ShowAsync();
                
                Progress.IsEnabled = false;
                Progress.IsIndeterminate = false;

            } catch (Exception ex)
            {
                ThrowError(ex);
                Progress.IsEnabled = false;
                Progress.IsIndeterminate = false;
            }
        }


        public static async Task<string> ConvertAsync(string MarkdownText)
        {
            return await Markdown.MarkdownToHTML(MarkdownText);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (currentText != null)
            {
                MarkEditor.Text = currentText;
            }
            }

        private void MarkEditor_GotFocus(object sender, RoutedEventArgs e)
        {
            
            //MarkEditor.Background = new SolidColorBrush(Colors.DarkSlateGray);
            
        }
    }

   
}
