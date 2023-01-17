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
using ResponsiveButtonPanel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Networking.BackgroundTransfer;
using System.Threading;
using System.Net.NetworkInformation;
using SharpCompress.Archives;
using SharpCompress.Readers;
using SharpCompress.Common;
using Octokit;
using Windows.UI.Core;
using Windows.ApplicationModel.Activation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MarkdownUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page
    {
        /// <summary>
        /// MUST CHANGE THESE BEFORE EACH PUBLIC GITHUB RELEASE
        /// 
        /// MUST CHANGE THESE BEFORE EACH PUBLIC GITHUB RELEASE
        /// 
        /// MUST CHANGE THESE BEFORE EACH PUBLIC GITHUB RELEASE
        /// </summary>
        public static string CurrentBuildVersion = "1.0.7";
        public static string PreviousBuildVersion = "1.0.6";
        public static string NextBuildVersion = "1.0.8";
        public static string UploadedFileName = "Universal Markup Editor_1.0.8.0_Test.zip";
        public static string AppxUpdateName = "Universal Markup Editor_1.0.8.0_x86_x64_arm.appxbundle";

        public StorageFolder folder { get; set; }
        public StorageFile file { get; set; }
        public bool isLatestBuild { get; set; }
        public static string UpdateURL { get; set; }
        DownloadOperation downloadOperation;
        CancellationTokenSource cancellationToken;

        BackgroundDownloader backgroundDownloader = new BackgroundDownloader();
        ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
        string[] fontList = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();
        double fontSize { get; set; }
        ResourceDictionary EditorRes { get; set; }
        SolidColorBrush colorNew = new SolidColorBrush(new Windows.UI.Color() { A = 255, R = 45, B = 45, G = 44 });
        Color uiColor = new Windows.UI.Color() { A = 255, R = 45, B = 45, G = 44 };
        public static WebView Preview { get; set; }
        public static bool FileNameAccepted { get; set; }
        public static string SavedFileName = "Untitled.md";
        public static RelativePanel ToolbarControl { get; set; }
        //public MarkdownInit ViewModel => DataContext as MarkdownInit;
        public static string currentText { get; set; }
        ApplicationTheme currentTheme = Windows.UI.Xaml.Application.Current.RequestedTheme;
        public static StorageFile saveFile { get; set; }
        public static bool IsFileLoaded { get; set; }
        public static bool IsFileModified { get; set; }
        public static bool IsSessionSaved { get; set; }
        public static string SaveFileExtension { get; set; }
        public static StorageFile loadedFile { get; set; }


        public MainPage()
        {
            this.InitializeComponent();
            EditorRes = MarkEditor.Resources;

            // Detech System theme and apply the UI Colour changes
            if (currentTheme == ApplicationTheme.Dark)
            {
                MainPanel.Background = colorNew;
                Progress.Background = colorNew;
                EditorBackground.Background = colorNew;
                PreviewPane.Background = colorNew;
                AboutPane.Background = colorNew;
                SyntaxPane.Background = colorNew;
                SettingsPane.Background = colorNew;
                MarkEditor.RequestedTheme = ElementTheme.Dark;
                (EditorRes["TextControlForegroundFocused"] as SolidColorBrush).Color = Colors.White;
                EditorBackCombo.PlaceholderText = "Dark";
            }
            else
            {
                EditorBackground.Background = new SolidColorBrush(Colors.White);
                EditorBackCombo.PlaceholderText = "Light";

            }
            // first run check/ app settings load
            IsFirstRunComplete();

            // Configure visibility of the pop up windows, add fonts to lists and initial bool settings
            fontSize = MarkEditor.FontSize;
            FontSizeCombo.PlaceholderText = fontSize.ToString();
            FontComboBox.PlaceholderText = MarkEditor.FontFamily.Source;
            PreviewPane.Visibility = Visibility.Collapsed;
            AboutPane.Visibility = Visibility.Collapsed;
            SyntaxPane.Visibility = Visibility.Collapsed;
            InstallUpdateBtn.Visibility = Visibility.Collapsed;
            SettingsPane.Visibility = Visibility.Collapsed;
            saveFile = null;
            IsFileModified = true;
            IsSessionSaved = false;
            SaveFileExtension = ".md";
            foreach (var item in fontList)
            {
                FontComboBox.Items.Add(item);
            }

            // About text for popup window
            AboutText.Text = "This app makes use of these libraries:\n" +
                "- Markdown.Core Nuget by tobiasschulz\n" +
                "- Microsoft.UI.Xaml by Microsoft\n" +
                "- pdehne's ResponsiveButtonPanel Template\n" +
                "- Octokit Nuget by Github\n" +
                "- SharpCompressUWP fork by BAstifan\n\n" +
                "(2022 github.com/empyreal96)";

            // Markown Syntax help page text
            #region SyntaxHelp
            HeadersInfo.Text =
               "Headers are the titles for Pages and sections, 1 is largest 6 is smallest\n" +
               "Insert a hash (#) at the start of a line, followed by a space\n\n";
            HeadersSyntax.Text =
                "# Heading 1\n" +
                "## Heading 2\n" +
                "### Heading 3\n" +
                "#### Heading 4\n" +
                "##### Heading 5\n" +
                "###### Heading 6\n\n";

            ItalicsInfo.Text =
                "Italics need the whole word or phrase to be encased in either '* *' or '_ _'\n\n";
            ItalicsSyntax.Text =
                "*This will be Italic*\n" +
                "_This will also be Italic_\n\n";

            BoldInfo.Text =
                "Bold words or phrases need to be encased in '** **' or '__ __'\n\n";
            BoldSyntax.Text =
                "**This will be Bold**\n" +
                "__This will also be Bold__\n\n";

            StrikeInfo.Text =
                "Striked text need to be encased in '~~ ~~'\n\n";
            StrikeSyntax.Text =
                "~~This will have a line~~\n\n";

            ListsInfo.Text =
                "Lists use '*' or '-' as a prefix to display a list view, and can be stacked\n\n";
            ListsSyntax.Text =
                "* Item 1\n" +
                "* Item 2\n" +
                "  * Item 2a\n" +
                "  * Item 2a\n" +
                "     * Item 2b\n" +
                "     * Item 2b\n\n";

            LinksInfo.Text =
                "Links can display different text to the actual link, Text is encased in '[ ]' and link is '( )'\n\n";
            LinksSyntax.Text =
                "[Click Here](https://github.com/empyreal96)\n\n";

            ImageLinksInfo.Text =
                "Images can be inserted from links, using the same concept as Links but prefixing '!'\n\n";
            ImageLinksSyntax.Text =
                "![Image Desription](https://somesite.com/image.jpg)\n\n";

            ImageResizingInfo.Text =
                "Resizing requires a slightly different formatting, \n\n";
            ImageResizingSyntax.Text =
            "<img src=\"https://somesite.com/image.jpg \" width=\"385px\" align=\"center\">";

            TablesInfo.Text =
                "Tables take some setting up but can be achieved by 'creating' a table from characters as shown beside\n\n";
            TablesSyntax.Text =
                "|Header1|Header2|Header3|\n" +
                "| --- | --- | --- |\n" +
                "| Col1 | Col2 | Col3 |\n" +
                "| Row2 | Row2 | Row2 |\n" +
                "| This | Is a | Row |\n\n";

            HighlightInfo.Text =
                "Text and phrases can me highlighted to show emphasis, by encasing the words with ` `\n\n";
            HighlightSyntax.Text =
                "Here we have `emphasised` test to make it `stand out`\n\n";

            CodeInfo.Text =
                "Code snippets can be added to markdown to help display code samples, use ``` and write the code below\n\n";
            CodeSyntax.Text =
                "```\n" +
                "using System;\n" +
                "\n" +
                "namespace SampleCode\n" +
                "{\n" +
                "   public sealed partial class Sample : Page\n" +
                "   {\n" +
                "       public SampleCode\n" +
                "       {\n" +
                "           this.InitializeComponent();\n" +
                "       }\n" +
                "   }\n" +
                "}\n\n";
            #endregion


            // Check if Internet is connected and check for update
            bool isNetworkConnected = NetworkInterface.GetIsNetworkAvailable();
            if (isNetworkConnected == true)
            {
                CheckForUpdate();

            }
            else
            {

                ProgressBarDownload.Visibility = Visibility.Collapsed;
                DLUpdate.Visibility = Visibility.Collapsed;
            }

            // Check for existing file name, if none set as Untitled
            if (SavedFileName != null)
            {

                FileHeader.Text = SavedFileName;
                Debug.WriteLine($"Filename: {SavedFileName}");
            }
            else
            {
                FileHeader.Text = "Untitled.md";
            }

            // Enable SpellCheckToggle.Toggled here to avoid the event triggering when being see in Loading application settings
            SpellCheckTog.Toggled += SpellCheckTog_Toggled;
            
            // If a file was used to activate the app, check if a file exists from that activation and load that file
            if (OpenFileHelper.activatedFile != null)
            {
                OpenFile();
            }

        }

        public async void IsFirstRunComplete()
        {
            try
            {
                IPropertySet roamingProperties = ApplicationData.Current.RoamingSettings.Values;

              if (!roamingProperties.ContainsKey("FirstRunDone"))
                {
                    //defaults
                    if (currentTheme == ApplicationTheme.Light)
                    {
                        LocalSettings.Values["DefaultTheme"] = "Light";

                    }
                    else
                    {
                        LocalSettings.Values["DefaultTheme"] = "Dark";
                    }
                    LocalSettings.Values["DefaultFont"] = "Segoe UI";
                    LocalSettings.Values["DefaultFontSize"] = 15;

                    // settings

                    if (currentTheme == ApplicationTheme.Light)
                    {
                        LocalSettings.Values["UserTheme"] = "Light";

                    }
                    else
                    {
                        LocalSettings.Values["UserTheme"] = "Dark";
                    }
                    LocalSettings.Values["UserFont"] = "Segoe UI";
                    LocalSettings.Values["UserFontSize"] = (double)15;
                    LocalSettings.Values["UserSpellCheck"] = "false";
                    roamingProperties["FirstRunDone"] = bool.TrueString;

                    var WelcomePopup = new MessageDialog(
                        "Welcome to Universal Markup Editor!\n\n" +
                        "This App lets you Edit, Create, Save and Preview Markdown files in addition to a variety of general text based files");
                    WelcomePopup.Commands.Add(new UICommand("Close"));
                    await WelcomePopup.ShowAsync();
                }
                else
                {
                    AppSettingsLoad();
                    
                }
            }
            catch (Exception ex)
            {
                ThrowError(ex);
            }
        }

        private async void MainNav_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            try
            {
                // Keep current editor text when switching to a window
                currentText = MarkEditor.Text;

                // Filename set
                if (SavedFileName != null)
                {
                    FileHeader.Text = SavedFileName;
                }

                // Navigation Menu items
                switch (args.InvokedItemContainer.Tag.ToString())
                {
                    case "SettingsTag":
                        MainNav.IsPaneOpen = false;
                        SettingsPane.Visibility = Visibility.Visible;
                        break;

                    case "SaveFileTag":
                        SaveFile();
                        break;
                    case "SaveAsFileTag":
                        SaveAsFile();
                        break;
                    case "OpenFileTag":
                        OpenFile();
                        break;
                    case "PreviewView":
                        PreviewPane.Visibility = Visibility.Visible;
                        ApplicationTheme currentTheme = Windows.UI.Xaml.Application.Current.RequestedTheme;
                        // Markdown text preview requires some html to be passed with the data when loading the WebView element
                        // Change colours depending on System theme
                        if (currentTheme == ApplicationTheme.Dark)
                        {
                            String data = "<html><head>"
                           + "<style type=\"text/css\">body{color: #fff; background-color: #000;}"
                           + "</style></head>"
                           + "<body>"
                           + "<span style=\"font-family: 'Segoe UI'\">" + await ConvertAsync(currentText) + "</span>"
                           + "</body></html>";
                            PreviewWebview.NavigateToString(data);
                        }
                        else
                        {

                            String data = "<html><head>"
                           + "<style type=\"text/css\">body{color: #000; background-color: #fff;}"
                           + "</style></head>"
                           + "<body>"
                           + "<span style=\"font-family: 'Segoe UI'\">" + await ConvertAsync(currentText) + "</span>"
                           + "</body></html>";
                            PreviewWebview.NavigateToString(data);
                        }
                        break;
                    case "SyntaxHelp":
                        SyntaxPane.Visibility = Visibility.Visible;
                        break;
                    case "About":
                        AboutPane.Visibility = Visibility.Visible;
                        break;


                }

            }
            catch (System.Exception ex)
            {
                var ThrownException = new MessageDialog($"{ex.Message}\n\n{ex.Source}\n\n{ex.Data}\n\n{ex.StackTrace}\n\n{ex.InnerException}");
                ThrownException.Commands.Add(new UICommand("Close"));
                await ThrownException.ShowAsync();
            }
        }






        public async void ThrowError(Exception ex)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
async () =>
{
    var ThrownException = new MessageDialog($"{ex.Message}\n\n{ex.Source}\n\n{ex.Data}\n\n{ex.StackTrace}\n\n{ex.InnerException}");
    ThrownException.Commands.Add(new UICommand("Close"));
    await ThrownException.ShowAsync();
});

        }



       



        private async void OpenFile()
        {
            FileOpenPicker loadFile = new FileOpenPicker();
            loadFile.FileTypeFilter.Add(".md");
            loadFile.FileTypeFilter.Add(".markdown");
            loadFile.FileTypeFilter.Add(".cs");
            loadFile.FileTypeFilter.Add(".c");
            loadFile.FileTypeFilter.Add(".h");
            loadFile.FileTypeFilter.Add(".txt");
            loadFile.FileTypeFilter.Add(".cpp");
            loadFile.FileTypeFilter.Add(".xml");
            loadFile.FileTypeFilter.Add(".xaml");
            loadFile.FileTypeFilter.Add(".csv");
            loadFile.FileTypeFilter.Add(".ini");
            loadFile.FileTypeFilter.Add(".bat");
            loadFile.FileTypeFilter.Add(".inf");
            loadFile.FileTypeFilter.Add(".pl");
            loadFile.FileTypeFilter.Add(".csproj");
            loadFile.FileTypeFilter.Add(".patch");
            loadFile.FileTypeFilter.Add(".diff");
            loadFile.FileTypeFilter.Add(".asm");
            loadFile.FileTypeFilter.Add(".html");
            loadFile.FileTypeFilter.Add(".cmake");
            loadFile.FileTypeFilter.Add(".shtml");
            loadFile.FileTypeFilter.Add(".htm");
            loadFile.FileTypeFilter.Add(".ps1");
            loadFile.FileTypeFilter.Add(".nfo");
            loadFile.FileTypeFilter.Add(".lua");
            loadFile.FileTypeFilter.Add(".json");
            loadFile.FileTypeFilter.Add(".js");
            loadFile.FileTypeFilter.Add(".reg");
            loadFile.FileTypeFilter.Add(".cfg");
            loadFile.FileTypeFilter.Add(".log");
            loadFile.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            // Check if File was used to lauch the app, if so set that file as the Selected file
            if (OpenFileHelper.activatedFile == null)
            {
                loadedFile = await loadFile.PickSingleFileAsync();

            } else
            {
                loadedFile = OpenFileHelper.activatedFile;
            }


            if (loadedFile == null)
            {
                IsFileModified = false;
                IsFileLoaded = false;
                IsSessionSaved = false;
                return;
            }
            // set file name
            FileHeader.Text = loadedFile.Name;
            SavedFileName = loadedFile.Name;

            // read text
            string parsedText = await FileIO.ReadTextAsync(loadedFile);
            if (parsedText != null)
            {
                MarkEditor.Text = parsedText;
                currentText = parsedText;
                SaveFileExtension = Path.GetExtension(SavedFileName);
                // Check if file is a Markdown file, if not then hide Markdown specific toolbar with the basic toolbar
                if (SaveFileExtension != ".md" && SaveFileExtension != ".markdown")
                {
                    ToolbarDockMD.Visibility = Visibility.Collapsed;
                    ToolbarDockMD.IsEnabled = false;
                    ToolbarDock.Visibility = Visibility.Visible;
                    ToolbarDock.IsEnabled = true;
                }
                else
                {
                    ToolbarDock.Visibility = Visibility.Collapsed;
                    ToolbarDock.IsEnabled = false;
                    ToolbarDockMD.Visibility = Visibility.Visible;
                    ToolbarDockMD.IsEnabled = true;
                }
                Debug.WriteLine(SaveFileExtension);
                IsFileModified = false;
                IsFileLoaded = true;
                IsSessionSaved = false;
            }
        }





        ComboBox fileTypeBox { get; set; }
        TextBox fileNameUser { get; set; }
        public bool ContentDialogCancel { get; set; }
        private async void SaveFile()
        {
            Progress.IsEnabled = true;
            Progress.IsIndeterminate = true;
            try
            {
                if (loadedFile != null)
                {

                    // use a stream to save the currently loaded file
                    Debug.WriteLine($"Filename: {SavedFileName}");
                    var streamclearer = await loadedFile.OpenStreamForWriteAsync().ConfigureAwait(false);
                    streamclearer.SetLength(0);
                    using (var streamWriter = new StreamWriter(streamclearer))
                    {
                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                                    {
                                        streamWriter.Write(MarkEditor.Text);
                                    });
                        streamWriter.Flush();
                        streamWriter.Dispose();
                    }

                    SavedFileName = loadedFile.Name;
                    SaveFileExtension = Path.GetExtension(SavedFileName);
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            async () =>
                            {
                                // Update file name if needed, and adjust the loaded toolbar
                                FileHeader.Text = loadedFile.Name;

                                var ThrownSuccess = new MessageDialog($"File saved to: {loadedFile.Path}");
                                ThrownSuccess.Commands.Add(new UICommand("Close"));
                                await ThrownSuccess.ShowAsync();
                                if (SaveFileExtension != ".md" && SaveFileExtension != ".markdown")
                                {
                                    ToolbarDockMD.Visibility = Visibility.Collapsed;
                                    ToolbarDockMD.IsEnabled = false;
                                    ToolbarDock.Visibility = Visibility.Visible;
                                    ToolbarDock.IsEnabled = true;
                                }
                                else
                                {
                                    ToolbarDock.Visibility = Visibility.Collapsed;
                                    ToolbarDock.IsEnabled = false;
                                    ToolbarDockMD.Visibility = Visibility.Visible;
                                    ToolbarDockMD.IsEnabled = true;
                                }
                                IsFileModified = false;
                                IsFileLoaded = true;
                                IsSessionSaved = true;
                                Progress.IsEnabled = false;
                                Progress.IsIndeterminate = false;
                            });
                    return;
                }

                if (IsSessionSaved == true && IsFileModified == true)
                {

                    saveFile = await folder.CreateFileAsync($"{SavedFileName}", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(saveFile, MarkEditor.Text);
                    IsFileModified = false;
                    IsFileLoaded = true;
                    IsSessionSaved = true;
                    SavedFileName = saveFile.Name;
                    SaveFileExtension = Path.GetExtension(SavedFileName);
                    var ThrownSuccess = new MessageDialog($"File saved to: {saveFile.Path}");
                    ThrownSuccess.Commands.Add(new UICommand("Close"));
                    await ThrownSuccess.ShowAsync();
                    if (SaveFileExtension != ".md" && SaveFileExtension != ".markdown")
                    {
                        ToolbarDockMD.Visibility = Visibility.Collapsed;
                        ToolbarDockMD.IsEnabled = false;
                        ToolbarDock.Visibility = Visibility.Visible;
                        ToolbarDock.IsEnabled = true;
                    }
                    else
                    {
                        ToolbarDock.Visibility = Visibility.Collapsed;
                        ToolbarDock.IsEnabled = false;
                        ToolbarDockMD.Visibility = Visibility.Visible;
                        ToolbarDockMD.IsEnabled = true;
                    }
                    return;
                }
                else
                {
                    fileTypeBox = new ComboBox();
                    fileTypeBox.Items.Add(".md");
                    fileTypeBox.Items.Add(".markdown");
                    fileTypeBox.Items.Add(".cs");
                    fileTypeBox.Items.Add(".c");
                    fileTypeBox.Items.Add(".h");
                    fileTypeBox.Items.Add(".txt");
                    fileTypeBox.Items.Add(".cpp");
                    fileTypeBox.Items.Add(".xml");
                    fileTypeBox.Items.Add(".xaml");
                    fileTypeBox.Items.Add(".csv");
                    fileTypeBox.Items.Add(".ini");
                    fileTypeBox.Items.Add(".bat");
                    fileTypeBox.Items.Add(".inf");
                    fileTypeBox.Items.Add(".pl");
                    fileTypeBox.Items.Add(".csproj");
                    fileTypeBox.Items.Add(".patch");
                    fileTypeBox.Items.Add(".diff");
                    fileTypeBox.Items.Add(".asm");
                    fileTypeBox.Items.Add(".html");
                    fileTypeBox.Items.Add(".cmake");
                    fileTypeBox.Items.Add(".shtml");
                    fileTypeBox.Items.Add(".htm");
                    fileTypeBox.Items.Add(".ps1");
                    fileTypeBox.Items.Add(".nfo");
                    fileTypeBox.Items.Add(".lua");
                    fileTypeBox.Items.Add(".json");
                    fileTypeBox.Items.Add(".js");
                    fileTypeBox.Items.Add(".reg");
                    fileTypeBox.Items.Add(".log");
                    fileTypeBox.Items.Add(".cfg");
                    fileTypeBox.Items.Add(".*");
                    fileTypeBox.SelectedIndex = 0;
                    fileTypeBox.Height = 40;
                    fileTypeBox.Width = 250;
                    fileTypeBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                    fileTypeBox.SelectionChanged += FileTypeBox_SelectionChanged;

                    fileNameUser = new TextBox();
                    fileNameUser.PlaceholderText = "Enter a File Name";
                    fileNameUser.Text = SavedFileName;
                    fileNameUser.Height = 40;
                    StackPanel stackPanel = new StackPanel();
                    stackPanel.Children.Add(fileNameUser);
                    stackPanel.Children.Add(fileTypeBox);
                    ContentDialog dialog = new ContentDialog();
                    dialog.Content = stackPanel;

                    dialog.IsSecondaryButtonEnabled = true;
                    dialog.PrimaryButtonText = "Confirm";
                    dialog.SecondaryButtonText = "Cancel";
                    dialog.SecondaryButtonClick += Dialog_SecondaryButtonClick;
                    // Save as a new file

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
                            SaveFileExtension = fileTypeBox.SelectedItem.ToString();
                            Debug.WriteLine($"Extension: {SaveFileExtension}");
                        }
                    }
                    if (ContentDialogCancel == true)
                    {
                        return;
                    }
                    else
                    {
                        if (SavedFileName == null)
                        {
                            return;
                        }

                        if (loadedFile == null)
                        {
                            FolderPicker saveFolder = new FolderPicker();
                            saveFolder.FileTypeFilter.Add(".md");
                            saveFolder.FileTypeFilter.Add(".markdown");
                            saveFolder.FileTypeFilter.Add(".cs");
                            saveFolder.FileTypeFilter.Add(".c");
                            saveFolder.FileTypeFilter.Add(".h");
                            saveFolder.FileTypeFilter.Add(".txt");
                            saveFolder.FileTypeFilter.Add(".cpp");
                            saveFolder.FileTypeFilter.Add(".xml");
                            saveFolder.FileTypeFilter.Add(".xaml");
                            saveFolder.FileTypeFilter.Add(".csv");
                            saveFolder.FileTypeFilter.Add(".ini");
                            saveFolder.FileTypeFilter.Add(".bat");
                            saveFolder.FileTypeFilter.Add(".inf");
                            saveFolder.FileTypeFilter.Add(".pl");
                            saveFolder.FileTypeFilter.Add(".csproj");
                            saveFolder.FileTypeFilter.Add(".patch");
                            saveFolder.FileTypeFilter.Add(".diff");
                            saveFolder.FileTypeFilter.Add(".asm");
                            saveFolder.FileTypeFilter.Add(".html");
                            saveFolder.FileTypeFilter.Add(".cmake");
                            saveFolder.FileTypeFilter.Add(".shtml");
                            saveFolder.FileTypeFilter.Add(".htm");
                            saveFolder.FileTypeFilter.Add(".ps1");
                            saveFolder.FileTypeFilter.Add(".nfo");
                            saveFolder.FileTypeFilter.Add(".lua");
                            saveFolder.FileTypeFilter.Add(".json");
                            saveFolder.FileTypeFilter.Add(".js");
                            saveFolder.FileTypeFilter.Add(".reg");
                            saveFolder.FileTypeFilter.Add(".cfg");
                            saveFolder.FileTypeFilter.Add(".log");
                            folder = await saveFolder.PickSingleFolderAsync();
                            if (folder == null)
                            {
                                IsFileModified = true;
                                IsFileLoaded = true;
                                IsSessionSaved = false;
                                return;

                            }
                            Debug.WriteLine($"Filename: {SavedFileName}");
                            saveFile = await folder.CreateFileAsync($"{SavedFileName}", CreationCollisionOption.ReplaceExisting);
                            SavedFileName = saveFile.Name;
                            SaveFileExtension = Path.GetExtension(SavedFileName);
                            await FileIO.WriteTextAsync(saveFile, MarkEditor.Text);
                            FileHeader.Text = saveFile.Name;
                            var ThrownSuccess = new MessageDialog($"File saved to: {saveFile.Path}");
                            ThrownSuccess.Commands.Add(new UICommand("Close"));
                            await ThrownSuccess.ShowAsync();
                            if (SaveFileExtension != ".md" && SaveFileExtension != ".markdown")
                            {
                                ToolbarDockMD.Visibility = Visibility.Collapsed;
                                ToolbarDockMD.IsEnabled = false;
                                ToolbarDock.Visibility = Visibility.Visible;
                                ToolbarDock.IsEnabled = true;
                            }
                            else
                            {
                                ToolbarDock.Visibility = Visibility.Collapsed;
                                ToolbarDock.IsEnabled = false;
                                ToolbarDockMD.Visibility = Visibility.Visible;
                                ToolbarDockMD.IsEnabled = true;
                            }
                            IsFileModified = false;
                            IsFileLoaded = true;
                            IsSessionSaved = true;
                            Progress.IsEnabled = false;
                            Progress.IsIndeterminate = false;
                        }

                    }
                }

            }
            catch (Exception ex)
            {

                ThrowError(ex);
                IsFileModified = true;
                IsFileLoaded = true;
                IsSessionSaved = false;
                Progress.IsEnabled = false;
                Progress.IsIndeterminate = false;
            }
        }

        public async void SaveAsFile()
        {
            fileTypeBox = new ComboBox();
            fileTypeBox.Items.Add(".md");
            fileTypeBox.Items.Add(".markdown");
            fileTypeBox.Items.Add(".cs");
            fileTypeBox.Items.Add(".c");
            fileTypeBox.Items.Add(".h");
            fileTypeBox.Items.Add(".txt");
            fileTypeBox.Items.Add(".cpp");
            fileTypeBox.Items.Add(".xml");
            fileTypeBox.Items.Add(".xaml"); //
            fileTypeBox.Items.Add(".csv");
            fileTypeBox.Items.Add(".ini");
            fileTypeBox.Items.Add(".bat"); // 
            fileTypeBox.Items.Add(".inf");
            fileTypeBox.Items.Add(".pl");
            fileTypeBox.Items.Add(".csproj");
            fileTypeBox.Items.Add(".patch");
            fileTypeBox.Items.Add(".diff");
            fileTypeBox.Items.Add(".asm");
            fileTypeBox.Items.Add(".html");
            fileTypeBox.Items.Add(".cmake");
            fileTypeBox.Items.Add(".shtml");
            fileTypeBox.Items.Add(".htm");
            fileTypeBox.Items.Add(".ps1");
            fileTypeBox.Items.Add(".nfo");
            fileTypeBox.Items.Add(".lua");
            fileTypeBox.Items.Add(".json");
            fileTypeBox.Items.Add(".js");
            fileTypeBox.Items.Add(".reg");
            fileTypeBox.Items.Add(".log");
            fileTypeBox.Items.Add(".cfg");
            fileTypeBox.Items.Add(".*");
            fileTypeBox.SelectedIndex = 0;
            fileTypeBox.Height = 40;
            fileTypeBox.Width = 250;

            fileTypeBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            fileTypeBox.SelectionChanged += FileTypeBox_SelectionChanged;

            fileNameUser = new TextBox();
            fileNameUser.PlaceholderText = "Enter a File Name";
            fileNameUser.Text = SavedFileName;
            fileNameUser.Height = 40;
            fileNameUser.RequestedTheme = ElementTheme.Light;
            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(fileNameUser);
            stackPanel.Children.Add(fileTypeBox);
            ContentDialog dialog = new ContentDialog();
            dialog.Content = stackPanel;

            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Confirm";
            dialog.SecondaryButtonText = "Cancel";
            dialog.SecondaryButtonClick += Dialog_SecondaryButtonClick;

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
                    SaveFileExtension = fileTypeBox.SelectedItem.ToString();
                    Debug.WriteLine($"Extension: {SaveFileExtension}");
                }
            }
            if (ContentDialogCancel == true)
            {
                return;
            }
            else
            {
                FolderPicker saveFolder = new FolderPicker();
                saveFolder.FileTypeFilter.Add(".md");
                saveFolder.FileTypeFilter.Add(".markdown");
                saveFolder.FileTypeFilter.Add(".cs");
                saveFolder.FileTypeFilter.Add(".c");
                saveFolder.FileTypeFilter.Add(".h");
                saveFolder.FileTypeFilter.Add(".txt");
                saveFolder.FileTypeFilter.Add(".cpp");
                saveFolder.FileTypeFilter.Add(".xml");
                saveFolder.FileTypeFilter.Add(".xaml");
                saveFolder.FileTypeFilter.Add(".csv");
                saveFolder.FileTypeFilter.Add(".ini");
                saveFolder.FileTypeFilter.Add(".bat");
                saveFolder.FileTypeFilter.Add(".inf");
                saveFolder.FileTypeFilter.Add(".pl");
                saveFolder.FileTypeFilter.Add(".csproj");
                saveFolder.FileTypeFilter.Add(".patch");
                saveFolder.FileTypeFilter.Add(".diff");
                saveFolder.FileTypeFilter.Add(".asm"); //
                saveFolder.FileTypeFilter.Add(".html");
                saveFolder.FileTypeFilter.Add(".cmake");
                saveFolder.FileTypeFilter.Add(".shtml");
                saveFolder.FileTypeFilter.Add(".htm");
                saveFolder.FileTypeFilter.Add(".ps1");
                saveFolder.FileTypeFilter.Add(".nfo");
                saveFolder.FileTypeFilter.Add(".lua");
                saveFolder.FileTypeFilter.Add(".json");
                saveFolder.FileTypeFilter.Add(".js");
                saveFolder.FileTypeFilter.Add(".reg");
                saveFolder.FileTypeFilter.Add(".cfg");
                saveFolder.FileTypeFilter.Add(".log");
                folder = await saveFolder.PickSingleFolderAsync();
                if (folder == null)
                {
                    IsFileModified = true;
                    IsFileLoaded = true;
                    IsSessionSaved = false;
                    return;

                }
                Debug.WriteLine($"Filename: {SavedFileName}");
                saveFile = await folder.CreateFileAsync($"{SavedFileName}", CreationCollisionOption.ReplaceExisting);
                SavedFileName = saveFile.Name;
                SaveFileExtension = Path.GetExtension(SavedFileName);
                await FileIO.WriteTextAsync(saveFile, MarkEditor.Text);
                FileHeader.Text = saveFile.Name;
                var ThrownSuccess = new MessageDialog($"File saved to: {saveFile.Path}");
                ThrownSuccess.Commands.Add(new UICommand("Close"));
                await ThrownSuccess.ShowAsync();
                IsFileModified = false;
                IsFileLoaded = true;
                IsSessionSaved = true;
                Progress.IsEnabled = false;
                Progress.IsIndeterminate = false;
            }
        }



        private void FileTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string nameNoExt = Path.GetFileNameWithoutExtension(fileNameUser.Text);
            string newext = fileTypeBox.SelectedItem.ToString();
            fileNameUser.Text = nameNoExt + newext;
        }

        private void Dialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

            ContentDialogCancel = true;
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



        }
        //https://www.blakepell.com/blog/getting-the-cursor-row-and-column-position-in-a-uwp-textbox
        /// <summary>
        /// Returns the current column position on the current line the cursor is on.
        /// </summary>
        public static CursorPosition FindCursorPosition(TextBox tb)
        {
            int endMarker = tb.SelectionStart;

            if (endMarker == 0)
            {
                return new CursorPosition(1, 1);
            }

            int i = 0;
            int col = 1;
            int row = 1;

            foreach (char c in tb.Text)
            {
                i++;
                col++;

                if (c == '\r')
                {
                    row++;
                    col = 1;
                }

                if (i == endMarker)
                {
                    return new CursorPosition(row, col);
                }
            }

            return new CursorPosition(row, col);
        }

        private void MarkEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var line = FindCursorPosition(MarkEditor).Row;
            var column = FindCursorPosition(MarkEditor).Column;
            CursorLocationBox.Text = $"X: {line}, Y: {column}";
        }


        // Undo and Redo text for Windows Mobile compatibility
        // https://social.msdn.microsoft.com/Forums/ie/en-US/a8dde66b-9a38-4b6d-adc9-e135e34cf076/uwphow-to-enable-undo-of-textbox-on-a-windows-phone?forum=wpdevelop
        int count = 0;
        List<string> history = new List<string>();
        private bool _fromButton;

        private void MarkEditor_TextChanged(object sender, TextChangedEventArgs e)
        {

            IsFileModified = true;
            if (!FileHeader.Text.Contains(" *"))
            {
                FileHeader.Text += " *";
            }
            IsFileLoaded = true;
            if (count > 0 && count < history.Count - 1 && _fromButton == false)
            {
                history.RemoveRange(count, history.Count - count);
                count -= 1;
            }
            if (_fromButton == false)
            {
                history.Add(MarkEditor.Text);
                count += 1;
            }
        }


        /// <summary>
        /// Copy selected string/text to the Windows Clipboard
        /// </summary>
        /// <param name="text"></param>
        public void Clipboard_CopyText(string text)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(text);

            Clipboard.SetContent(dataPackage);
        }


        /// <summary>
        /// Copy the selected string/text to Windows Clipboard then remove the text from Editor
        /// </summary>
        /// <param name="text"></param>
        public void Clipboard_CutText(string text)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(text);

            Clipboard.SetContent(dataPackage);
            MarkEditor.SelectedText = "";
        }


        /// <summary>
        /// Paste data from Windows Clipboard to the Editor
        /// </summary>
        public async void Clipboard_PasteText()
        {
            var dataPackageView = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                var text = await dataPackageView.GetTextAsync();
                MarkEditor.SelectedText = text;
            }
        }


        private async void ResponsiveButtonPanelButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ResponsiveButtonPanelButton;
            // Manage button presses from the toolbar
            switch (button.Tag)
            {
                case "CopyTag":
                    Clipboard_CopyText(MarkEditor.SelectedText);
                    Debug.WriteLine("Copied data to clipboard: " + MarkEditor.SelectedText);
                    break;

                case "CutTag":
                    Clipboard_CutText(MarkEditor.SelectedText);
                    Debug.WriteLine("Moved data to clipboard: " + MarkEditor.SelectedText);
                    break;

                case "PasteTag":
                    Clipboard_PasteText();
                    Debug.WriteLine("Pasted Clipboard data");
                    break;
                case "CopyTag1":
                    Clipboard_CopyText(MarkEditor.SelectedText);
                    Debug.WriteLine("Copied data to clipboard: " + MarkEditor.SelectedText);
                    break;

                case "CutTag1":
                    Clipboard_CutText(MarkEditor.SelectedText);
                    Debug.WriteLine("Moved data to clipboard: " + MarkEditor.SelectedText);
                    break;

                case "PasteTag1":
                    Clipboard_PasteText();
                    Debug.WriteLine("Pasted Clipboard data");
                    break;
                case "BoldTag":
                    if (MarkEditor.SelectedText == "")
                    {
                        MarkEditor.SelectedText = "**Bold**";
                    }
                    else
                    {
                        string newdata = "**" + MarkEditor.SelectedText + "**";
                        MarkEditor.SelectedText = newdata;
                    }
                    break;
                case "ItalicTag":
                    if (MarkEditor.SelectedText == "")
                    {
                        MarkEditor.SelectedText = "*Italic*";
                    }
                    else
                    {
                        string newdata2 = "*" + MarkEditor.SelectedText + "*";
                        MarkEditor.SelectedText = newdata2;
                    }
                    break;
                case "StrikeTag":
                    if (MarkEditor.SelectedText == "")
                    {
                        MarkEditor.SelectedText = "~~~Strike~~~";
                    }
                    else
                    {
                        string newdata2 = "~~~" + MarkEditor.SelectedText + "~~~";
                        MarkEditor.SelectedText = newdata2;
                    }
                    break;
                case "Header1Tag":
                    if (MarkEditor.SelectedText == "")
                    {
                        MarkEditor.SelectedText = "# ";
                    }
                    else
                    {
                        string newdata2 = "# " + MarkEditor.SelectedText;
                        MarkEditor.SelectedText = newdata2;
                    }
                    break;

                case "UndoTag":
                    //MarkEditor.Undo();
                    _fromButton = true;
                    if (count > 0)
                    {
                        count -= 1;
                        MarkEditor.Text = history.ElementAt(count);
                    }
                    await Task.Delay(350);
                    _fromButton = false;
                    break;

                case "RedoTag":
                    //MarkEditor.Redo();
                    _fromButton = true;
                    if (count >= 0 && count < history.Count - 1)
                    {
                        count += 1;
                        MarkEditor.Text = history.ElementAt(count);
                    }
                    await Task.Delay(350);
                    _fromButton = false;
                    break;
                case "UndoTag1":
                    //MarkEditor.Undo();
                    _fromButton = true;
                    if (count > 0)
                    {
                        count -= 1;
                        MarkEditor.Text = history.ElementAt(count);
                    }
                    await Task.Delay(350);
                    _fromButton = false;
                    break;

                case "RedoTag1":
                    //MarkEditor.Redo();
                    _fromButton = true;
                    if (count >= 0 && count < history.Count - 1)
                    {
                        count += 1;
                        MarkEditor.Text = history.ElementAt(count);
                    }
                    await Task.Delay(350);
                    _fromButton = false;
                    break;

                case "ToolHelpTag1":
                    var ToolbarHelpPopup = new MessageDialog(
                        "Clicking:\n" +
                        "Bold = Makes selection Bold\n" +
                        "Italic = Makes selection Italic\n\n" +
                        "Some Issues may be present still",
                        "Toolbar Help"
                        );
                    ToolbarHelpPopup.Commands.Add(new UICommand("Close"));
                    await ToolbarHelpPopup.ShowAsync();
                    break;
                case "ToolHelpTag":
                    var ToolbarHelpPopup1 = new MessageDialog(
                        "Clicking:\n" +
                        "Bold = Makes selection Bold\n" +
                        "Italic = Makes selection Italic\n\n" +
                        "Some Issues may be present still",
                        "Toolbar Help"
                        );
                    ToolbarHelpPopup1.Commands.Add(new UICommand("Close"));
                    await ToolbarHelpPopup1.ShowAsync();
                    break;

            }

        }

        private void PreviewClose_Click(object sender, RoutedEventArgs e)
        {
            PreviewPane.Visibility = Visibility.Collapsed;
        }








        #region About page

        /////////////
        /// About Pane Code
        /////////////
        /// <summary>
        /// Check Github for latest Release tag, and download if available
        /// </summary>
        private async void CheckForUpdate()
        {
            await Windows.Storage.ApplicationData.Current.ClearAsync(ApplicationDataLocality.LocalCache);
            GitHubClient client = new GitHubClient(new ProductHeaderValue("MarkdownUWP"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("Empyreal96", "MarkdownUWP");
            var latestRelease = releases[0];

            if (latestRelease.Assets != null && latestRelease.Assets.Count > 0)
            {
                if (latestRelease.TagName == CurrentBuildVersion)
                {
                    isLatestBuild = true;

                    ProgressBarDownload.Visibility = Visibility.Collapsed;
                    DLUpdate.Visibility = Visibility.Collapsed;
                    DlGrid.Visibility = Visibility.Collapsed;
                }
                //Test Function
                if (latestRelease.TagName == PreviousBuildVersion)
                {
                    UpdateOut.Text = "You are on an unreleased build";
                    ProgressBarDownload.Visibility = Visibility.Collapsed;
                    DLUpdate.Visibility = Visibility.Collapsed;
                    DlGrid.Visibility = Visibility.Collapsed;
                }

                if (latestRelease.TagName == NextBuildVersion)
                {
                    var updateURL = latestRelease.Assets[0].BrowserDownloadUrl;
                    UpdateURL = $"https://github.com/Empyreal96/MarkdownUWP/releases/download/{latestRelease.TagName}/{UploadedFileName}";
                    Debug.WriteLine(updateURL);
                    UpdateOut.Visibility = Visibility.Visible;
                    ProgressBarDownload.Visibility = Visibility.Visible;
                    DLUpdate.Visibility = Visibility.Visible;
                    UpdateOut.Text = $"Latest Build: {latestRelease.TagName}\n";
                    UpdateOut.Text += $"Current Build: {CurrentBuildVersion}\n";
                    UpdateOut.Text += $"Date Update Published: {latestRelease.PublishedAt}\n\n";

                    UpdateOut.Text += $"Download URL: {UpdateURL}\n";
                }
            }
        }
        private async void DLUpdate_Click(object sender, RoutedEventArgs e)
        {

            try
            {


                FolderPicker folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                folderPicker.ViewMode = PickerViewMode.Thumbnail;
                folderPicker.FileTypeFilter.Add("*");
                folder = await folderPicker.PickSingleFolderAsync();
                if (folder == null)
                {
                    return;
                }
                file = await folder.CreateFileAsync($"{UploadedFileName}", CreationCollisionOption.GenerateUniqueName);

                downloadOperation = backgroundDownloader.CreateDownload(new Uri(UpdateURL), file);

                Progress<DownloadOperation> progress = new Progress<DownloadOperation>(progressChanged);
                cancellationToken = new CancellationTokenSource();
                await downloadOperation.StartAsync().AsTask(cancellationToken.Token, progress);
                InstallUpdateBtn.Visibility = Visibility.Visible;
                DLUpdate.Visibility = Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                var ThrownException = new MessageDialog($"{ex.Message}\n\n{ex.Source}\n\n{ex.Data}\n\n{ex.StackTrace}\n\n{ex.InnerException}");
                ThrownException.Commands.Add(new UICommand("Close"));
                await ThrownException.ShowAsync();
            }


        }

        private void InstallUpdateBtn_Click(object sender, RoutedEventArgs e)
        {


            InstallUpdate();
        }

        private async void InstallUpdate()
        {
            UpdateOut.Text = "Extracting Update to App Cache";
            try
            {
                Stream zipStream = await file.OpenStreamForReadAsync();
                using (var zipArchive = ArchiveFactory.Open(zipStream))
                {
                    //It should support 7z, zip, rar, gz, tar
                    var reader = zipArchive.ExtractAllEntries();

                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            await reader.WriteEntryToDirectory(Windows.Storage.ApplicationData.Current.LocalCacheFolder, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                        }
                    }
                }
                var options = new Windows.System.LauncherOptions();
                options.PreferredApplicationPackageFamilyName = "Microsoft.DesktopAppInstaller_8wekyb3d8bbwe";
                options.PreferredApplicationDisplayName = "App Installer";
                UpdateOut.Text = "Attempting to Install Update Package, Please Wait";
                await Windows.System.Launcher.LaunchFileAsync(await Windows.Storage.ApplicationData.Current.LocalCacheFolder.GetFileAsync(AppxUpdateName), options);

            }
            catch (Exception ex)
            {
                UpdateOut.Text = $"An error occured while trying to install: \n{ex.Message}";
            }
        }

        /// <summary>
        /// Progress for Download
        /// </summary>
        /// <param name="downloadOperation"></param>
        private void progressChanged(DownloadOperation downloadOperation)
        {
            int progress = (int)(100 * ((double)downloadOperation.Progress.BytesReceived / (double)downloadOperation.Progress.TotalBytesToReceive));
            //TextBlockProgress.Text = String.Format("{0} of {1} kb. downloaded - {2}% complete.", downloadOperation.Progress.BytesReceived / 1024, downloadOperation.Progress.TotalBytesToReceive / 1024, progress);
            ProgressBarDownload.Value = progress;
            switch (downloadOperation.Progress.Status)
            {
                case BackgroundTransferStatus.Running:
                    {
                        UpdateOut.Text = $"Downloading from {UpdateURL}";
                        //ButtonPauseResume.Content = "Pause";
                        break;
                    }
                case BackgroundTransferStatus.PausedByApplication:
                    {
                        UpdateOut.Text = "Download paused.";
                        //ButtonPauseResume.Content = "Resume";
                        break;
                    }
                case BackgroundTransferStatus.PausedCostedNetwork:
                    {
                        UpdateOut.Text = "Download paused because of metered connection.";
                        //ButtonPauseResume.Content = "Resume";
                        break;
                    }
                case BackgroundTransferStatus.PausedNoNetwork:
                    {
                        UpdateOut.Text = "No network detected. Please check your internet connection.";
                        break;
                    }
                case BackgroundTransferStatus.Error:
                    {
                        UpdateOut.Text = "An error occured while downloading.";
                        break;
                    }
            }
            if (progress >= 100)
            {
                UpdateOut.Text = $"Download complete. Update downloaded to {folder.Path}\\{UploadedFileName}";
                // ButtonCancel.IsEnabled = false;
                //ButtonPauseResume.IsEnabled = false;
                //ButtonDownload.IsEnabled = true;
                downloadOperation = null;
            }
        }

        private void AboutClose_Click(object sender, RoutedEventArgs e)
        {
            AboutPane.Visibility = Visibility.Collapsed;
        }
        #endregion


        private void SyntaxClose_Click(object sender, RoutedEventArgs e)
        {
            SyntaxPane.Visibility = Visibility.Collapsed;
        }

        private void Context_Click(object sender, RoutedEventArgs e)
        {
            var clickedItem = (sender as MenuFlyoutItem).Name.ToString();
            Debug.WriteLine(clickedItem);

            switch (clickedItem)
            {
                case "BoldContext":
                    if (MarkEditor.SelectedText == "")
                    {
                        MarkEditor.SelectedText = "**Bold**";
                    }
                    else
                    {
                        string newdata = "**" + MarkEditor.SelectedText + "**";
                        MarkEditor.SelectedText = newdata;
                    }
                    break;

                case "ItalicContext":
                    if (MarkEditor.SelectedText == "")
                    {
                        MarkEditor.SelectedText = "*Italic*";
                    }
                    else
                    {
                        string newdata2 = "*" + MarkEditor.SelectedText + "*";
                        MarkEditor.SelectedText = newdata2;
                    }
                    break;
            }
        }

        public class CursorPosition
        {
            public CursorPosition()
            {

            }

            public CursorPosition(int row, int column)
            {
                Row = row;
                Column = column;
            }

            public int Row { get; set; } = 1;

            public int Column { get; set; } = 1;

        }

        // Settings button
        private void NavigationViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SettingsPane.Visibility = Visibility.Visible;
            MainNav.IsPaneOpen = false;


        }

        private void SettingsClose_Click(object sender, RoutedEventArgs e)
        {
            SettingsPane.Visibility = Visibility.Collapsed;

        }





        private void DefaultSettings()
        {

        }


        /// <summary>
        /// Check the saved values and apply them
        /// </summary>
        private void AppSettingsLoad()
        {
            var UserTheme = LocalSettings.Values["UserTheme"].ToString();
            if (UserTheme == "Dark")
            {
                EditorBackCombo.PlaceholderText = "Dark";
                SwitchToDarkEditor();
            }
            else
            {
                EditorBackCombo.PlaceholderText = "Light";
                SwitchToLightEditor();
            }

            var UserFont = LocalSettings.Values["UserFont"].ToString();
            FontComboBox.PlaceholderText = UserFont;
            MarkEditor.FontFamily = new FontFamily(UserFont);

            double UserFontSize = (double)LocalSettings.Values["UserFontSize"];
            MarkEditor.FontSize = UserFontSize;
            FontSizeCombo.PlaceholderText = UserFontSize.ToString();

            var UserSpellCheck = LocalSettings.Values["UserSpellCheck"].ToString();
            if (UserSpellCheck == "true")
            {
                MarkEditor.IsSpellCheckEnabled = true;
                SpellCheckTog.IsOn = true;
            }
            else
            {
                MarkEditor.IsSpellCheckEnabled = false;
                SpellCheckTog.IsOn = false;
            }
        }

        private void SpellCheckTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (SpellCheckTog.IsOn == true)
            {
                LocalSettings.Values["UserSpellCheck"] = "true";
                MarkEditor.IsSpellCheckEnabled = true;

            }
            else
            {
                LocalSettings.Values["UserSpellCheck"] = "false";
                MarkEditor.IsSpellCheckEnabled = false;
            }
        }

        private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedFont = FontComboBox.SelectedItem.ToString();
            var testIndex = FontComboBox.SelectedIndex;
            Debug.WriteLine(selectedFont);
            LocalSettings.Values["UserFont"] = selectedFont;
            MarkEditor.FontFamily = new FontFamily(selectedFont);

        }

        private void FontSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var size = ((ComboBoxItem)FontSizeCombo.SelectedItem).Content.ToString();
            Debug.WriteLine(size);
            switch (size)
            {
                case "12":
                    LocalSettings.Values["UserFontSize"] = (double)12;
                    MarkEditor.FontSize = 12;
                    break;
                case "13":
                    LocalSettings.Values["UserFontSize"] = (double)13;
                    MarkEditor.FontSize = 13;
                    break;
                case "14":
                    LocalSettings.Values["UserFontSize"] = (double)14;
                    MarkEditor.FontSize = 14;
                    break;
                case "15":
                    LocalSettings.Values["UserFontSize"] = (double)15;
                    MarkEditor.FontSize = 15;
                    break;
                case "16":
                    LocalSettings.Values["UserFontSize"] = (double)16;
                    MarkEditor.FontSize = 16;
                    break;
                case "17":
                    LocalSettings.Values["UserFontSize"] = (double)17;
                    MarkEditor.FontSize = 17;
                    break;
                case "18":
                    LocalSettings.Values["UserFontSize"] = (double)18;
                    MarkEditor.FontSize = 18;
                    break;
                case "19":
                    LocalSettings.Values["UserFontSize"] = (double)19;
                    MarkEditor.FontSize = 19;
                    break;
                case "20":
                    LocalSettings.Values["UserFontSize"] = (double)20;
                    MarkEditor.FontSize = 20;
                    break;
            }
        }

        private void EditorBackCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Color transparentColor = new Windows.UI.Color() { A = 0, R = 45, B = 45, G = 44 };
            var theme = ((ComboBoxItem)EditorBackCombo.SelectedItem).Content.ToString();
            Debug.WriteLine(theme);
            EditorRes = MarkEditor.Resources;
            var NavigationRes = MainNav.Resources;
            switch (theme)
            {

                case "Light":
                    LocalSettings.Values["UserTheme"] = "Light";
                    SwitchToLightEditor();
                    break;
                case "Dark":
                    LocalSettings.Values["UserTheme"] = "Dark";
                    SwitchToDarkEditor();
                    break;
            }
        }

        private void SwitchToLightEditor()
        {
            MarkEditor.RequestedTheme = ElementTheme.Light;
            MarkEditor.Foreground = new SolidColorBrush(Colors.Black);
            EditorBackground.Background = new SolidColorBrush(Colors.White);
            (EditorRes["TextControlForegroundFocused"] as SolidColorBrush).Color = Colors.Black;
        }

        private void SwitchToDarkEditor()
        {

            if (currentTheme == ApplicationTheme.Light)
            {
                MarkEditor.RequestedTheme = ElementTheme.Light;
                EditorBackground.Background = new SolidColorBrush(uiColor);
                MarkEditor.Foreground = new SolidColorBrush(Colors.White);
                (EditorRes["TextControlForegroundFocused"] as SolidColorBrush).Color = Colors.White;
               
            }
            else
            {
                MarkEditor.RequestedTheme = ElementTheme.Dark;
                EditorBackground.Background = new SolidColorBrush(uiColor);
                MarkEditor.Foreground = new SolidColorBrush(Colors.White);
                (EditorRes["TextControlForegroundFocused"] as SolidColorBrush).Color = Colors.White;

            }
        }


        private void MarkEditor_LostFocus(object sender, RoutedEventArgs e)
        {


        }
    }


}
