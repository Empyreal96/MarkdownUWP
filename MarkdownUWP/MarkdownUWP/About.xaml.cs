using Octokit;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
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
    public sealed partial class About : Windows.UI.Xaml.Controls.Page
    {
        /// <summary>
        /// MUST CHANGE THESE BEFORE EACH PUBLIC GITHUB RELEASE
        /// 
        /// MUST CHANGE THESE BEFORE EACH PUBLIC GITHUB RELEASE
        /// 
        /// MUST CHANGE THESE BEFORE EACH PUBLIC GITHUB RELEASE
        /// </summary>
        public static string CurrentBuildVersion = "1.0.1";
        public static string PreviousBuildVersion = "1.0.0";
        public static string NextBuildVersion = "1.0.2";
        public static string UploadedFileName = "MarkdownUWP_1.0.1.0_Debug_Test.zip";
        public static string AppxUpdateName = "MarkdownUWP_1.0.1.0_x86_x64_arm_Debug.appxbundle";

        public StorageFolder folder { get; set; }
        public StorageFile file { get; set; }
        public bool isLatestBuild { get; set; }
        public static string UpdateURL { get; set; }
        DownloadOperation downloadOperation;
        CancellationTokenSource cancellationToken;

        BackgroundDownloader backgroundDownloader = new BackgroundDownloader();

        public About()
        {
            this.InitializeComponent();
            AboutText.Text = "This app makes use of these libraries:\n" +
                "- Markdown.Core Nuget by tobiasschulz\n" +
                "- Microsoft.UI.Xaml by Microsoft\n\n" +
                "(2022 github.com/empyreal96)";

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
        }

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
                }
                //Test Function
                 if(latestRelease.TagName == PreviousBuildVersion)
                 {
                     UpdateOut.Text = "You are on an unreleased build";
                 }

                else
                {
                    var updateURL = latestRelease.Assets[0].BrowserDownloadUrl;
                    UpdateURL = $"https://github.com/Empyreal96/Easy-Fetch-UWP/releases/download/{latestRelease.TagName}/{UploadedFileName}";
                    // string updateURL = $"https://github.com/Empyreal96/Easy-Fetch-UWP/releases/download/1.13.16-prerelease/Easy-Fetch_1.13.16.0_Debug_Test.zip";
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
    }


}
