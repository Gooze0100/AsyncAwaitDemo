using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AsyncAwaitDemoWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // this is for cancellation
        // controls weteher we cancel or not
        CancellationTokenSource cts = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        // first time it is slow because it needs to find proxy server and second or other time it is faster because it remembers until app closes
        private void executeSync_Click(object sender, RoutedEventArgs e)
        {
            // this is much more precise that date now
            var watch = System.Diagnostics.Stopwatch.StartNew();

            // var results = DemoMethods.RunDownloadSync();
            var results = DemoMethods.RunDownloadParallelSync();
            PrintResults(results);

            watch.Stop();
            // this shows elapsed time from previous to this one point
            // this is really good performace counter for general stuff
            var elapsedMs = watch.ElapsedMilliseconds;

            resultsWindow.Text += $"Total execution time: {elapsedMs}";
        }

        // you can return void if you return for event
        private async void executeAsync_Click(object sender, RoutedEventArgs e)
        {
            // built in in system namespace
            Progress<ProgressReportModel> progress = new();
            // event ProgressChanged that we can hook into
            progress.ProgressChanged += ReportProgress;

            var watch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // async stopps the timer before it is finishing its work
                // await RunDownloadAsync();
                // await RunDownloadParallelAsync();
                var results = await DemoMethods.RunDownloadAsync(progress, cts.Token);
                PrintResults(results);
            }
            catch (OperationCanceledException)
            {
                // you can do not use any exception
                resultsWindow.Text += $"The async download was cancelled {Environment.NewLine}";
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            resultsWindow.Text += $"Total execution time: {elapsedMs}";
        }

        // passing as event e
        private void ReportProgress(object? sender, ProgressReportModel e)
        {
            dashboardProgress.Value = e.PercentageComplete;
            PrintResults(e.SitesDownloaded);
        }

        private async void parallelExecuteAsync_Click(object sender, RoutedEventArgs e)
        {
            Progress<ProgressReportModel> progress = new();
            progress.ProgressChanged += ReportProgress;

            var watch = System.Diagnostics.Stopwatch.StartNew();

            //var results = await DemoMethods.RunDownloadParallelAsync();
            var results = await DemoMethods.RunDownloadParallelAsyncV2(progress);
            PrintResults(results);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            resultsWindow.Text += $"Total execution time: {elapsedMs}";
        }

        private void cancelOperation_Click(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
        }

        private void PrintResults(List<WebsiteDataModel> results)
        {
            resultsWindow.Text = "";
            foreach (var item in results)
            {
                resultsWindow.Text += $"{item.WebsiteUrl} downloaded: {item.WebsiteData.Length} characters long. {Environment.NewLine}";
            }
        }
    }
}

// sychronous programming is like line you do a,b,c,d work, but if you need a,b,c to work that D should be completed then you use Asynchronous programming.
// this gives us a lot, because you can wait for some thing to happen and show to user that is loading or something
// if system will have resources it will try to do all 4 tasks in same time

// when to use await it is when you need to get data at some point for program
// mostly it is wrapped in async await and Task


// so advanced stuff for async is just cancellation of token and getting progress

// on async you can use the app so it is better
