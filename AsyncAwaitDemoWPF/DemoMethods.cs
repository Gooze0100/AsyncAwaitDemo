using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AsyncAwaitDemoWPF;

public static class DemoMethods
{
    public static List<string> PrepData()
    {
        List<string> output = new List<string>();

        //resultsWindow.Text = "";

        output.Add("https://www.yahoo.com");
        output.Add("https://www.google.com");
        output.Add("https://www.microsoft.com");
        output.Add("https://www.cnn.com");
        output.Add("https://www.stackoverflow.com");

        return output;
    }

    //public static void RunDownloadSync()
    public static List<WebsiteDataModel> RunDownloadSync()
    {
        List<string> websites = PrepData();
        List<WebsiteDataModel> output = new();

        foreach (string site in websites)
        {
            WebsiteDataModel results = DownloadWebsite(site);
            //ReportWebsiteInfo(results);
            output.Add(results);
        }

        return output;
    }

    public static List<WebsiteDataModel> RunDownloadParallelSync()
    {
        List<string> websites = PrepData();
        List<WebsiteDataModel> output = new();

        // we say run this code inside
        // you need to say what you are passing in e.g. string
        // for each item we are dooing Action
        // Action is a method called in line that do not return value, it just making a call or just do these two things
        // it is the same as dowload sync, but it does dowload those site in parallel to each other
        // is does all tasks in same time synchronously and it lock everything up until it is done
        Parallel.ForEach<string>(websites, (site) =>
        {
            WebsiteDataModel results = DownloadWebsite(site);
            output.Add(results);
        });

        return output;
    }

    // when you are returning async you need to always return Task or Task<T>
    public static async Task<List<WebsiteDataModel>> RunDownloadParallelAsyncV2(IProgress<ProgressReportModel> progress)
    {
        List<string> websites = PrepData();
        List<WebsiteDataModel> output = new();
        ProgressReportModel report = new();

        await Task.Run(() => {
            Parallel.ForEach<string>(websites, (site) =>
            {
                WebsiteDataModel results = DownloadWebsite(site);
                output.Add(results);

                report.SitesDownloaded = output;
                report.PercentageComplete = (output.Count * 100) / websites.Count;
                progress.Report(report);
            });
        });

        return output;
    }

    // never return void in async methods,
    // you can return void if you return for event
    // convention is that you need to write Async in any method that are gonna use it
    // so for everybody let know that you use this as async 
    //private async Task RunDownloadAsync()
    // IProgress it is generic. everytime when we do progress we can call this and it will bubble up an event to the caller
    // Recommended to create a model for report with the data you need in it
    public static async Task<List<WebsiteDataModel>> RunDownloadAsync(IProgress<ProgressReportModel> progress, CancellationToken cancellationToken)
    {
        List<string> websites = PrepData();
        List<WebsiteDataModel> output = new();
        ProgressReportModel report = new();

        foreach (string site in websites)
        {
            // Task.Run let us run it asynchronously, run this code aas our task
            // we wrapped this code in async bubble
            // because of this it execute in paralel or first doing first website and saying then to wait for it and blocks it
            // we can do it in paralel
            // here we do do this task and wait for it and other do it and wait for it
            // Task.Run(() wraps and makes the call async when it is not async
            //WebsiteDataModel results = await Task.Run(() => DownloadWebsite(site));
            WebsiteDataModel results = await DownloadWebsiteAsync(site);
            //ReportWebsiteInfo(results);
            output.Add(results);

            // if CancellationToken is activated we cancel this task and throw exception - operation cancelled exception
            // it let us e.g. close the connections if we need, not just shut down everything
            cancellationToken.ThrowIfCancellationRequested();

            report.SitesDownloaded = output;
            // take total of websites
            report.PercentageComplete = (output.Count * 100) / websites.Count;

            // this is how you can get progress in Async calls
            // it gives info for us back
            // this send back what type you requested
            progress.Report(report);
        }

        return output;
    }

    public static async Task<List<WebsiteDataModel>> RunDownloadParallelAsync()
    {
        List<string> websites = PrepData();
        List<Task<WebsiteDataModel>> tasks = new();

        foreach (string site in websites)
        {
            // so this just put then in list all those bubbles and it will run at the same time
            // tasks.Add(Task.Run(() => DownloadWebsite(site)));
            tasks.Add(DownloadWebsiteAsync(site));
        }

        // whenAll does all the tasks we pass in the whole set of them, and you just wait untill all of them is done
        // this is more advanced, because you do all the list all dowloading and then working with them
        // so we putting those bubbles in work 
        // we just get all results and then just loop through them and putting the results
        // so it will execute much faster because it will first get results
        // so we just wait for largest to download
        // waited to all go get and then worked with them
        // this is not the best mechanism because when it do something it cannot report something back, because it waits for all to be finished
        // it is good for quite a lot of things but for reporting not
        var results = await Task.WhenAll(tasks);

        //foreach (var item in results)
        //{
        //    ReportWebsiteInfo(item);
        //}

        return new List<WebsiteDataModel>(results);
    }

    // Task is to show that we could get the whole bubble
    // when we return async method but we say keep going we need to have task to get whole bubble and then you can do it later with something
    private static async Task<WebsiteDataModel> DownloadWebsiteAsync(string websiteURL)
    {
        WebsiteDataModel output = new();
        WebClient client = new();

        output.WebsiteUrl = websiteURL;
        output.WebsiteData = await client.DownloadStringTaskAsync(websiteURL);

        return output;
    }

    private static WebsiteDataModel DownloadWebsite(string websiteURL)
    {
        WebsiteDataModel output = new();
        WebClient client = new();

        output.WebsiteUrl = websiteURL;
        output.WebsiteData = client.DownloadString(websiteURL);

        return output;
    }

    //private void ReportWebsiteInfo(WebsiteDataModel data)
    //{
    //    resultsWindow.Text += $"{data.WebsiteUrl} downloaded: {data.WebsiteData.Length} characters long. {Environment.NewLine}";
    //}
}
