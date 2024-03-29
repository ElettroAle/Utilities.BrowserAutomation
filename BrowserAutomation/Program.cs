﻿using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

var siteDownloaderLink = "https://saveweb2zip.com/en";
var targetSite = "https://www.la7.it/registrazioni/registrazioni-propaganda";
var sessionId = Guid.NewGuid().ToString();
var iterations = 0;

EdgeDriver downloader;
EdgeDriver scraper;

PrepareDrivers();

try
{
    // ++++ Core Logic ++++
    while (true)
    {
        if (WaitingForOpening())
        {
            RefreshPage();
        }
        else
        {
            SaveSourceCode();
            DownloadPage();
            WaitAFewTime(1000);
            TakeScreenShoot();
            ReenableDownloader();

            if (TheSeatsHaveEnded())
            {
                WaitingForUserThatRealizesItsEnded();
                break;
            }

            WaitAFewTime(5000);
            RefreshPage();
        }
        KeepScreenOn();
        WaitAFewTime(100);
        iterations++;
    }
    // ++++ End Core Logic ++++
}
catch (Exception ex)
{
    LogException(ex);
}
finally
{
    CloseAllDrivers();
    SayTaskFinished();
}


void PrepareDrivers()
{
    var deviceDriver = EdgeDriverService.CreateDefaultService();
    deviceDriver.HideCommandPromptWindow = true;

    EdgeOptions options = new();
    options.AddArguments("--disable-infobars");

    downloader = new EdgeDriver(deviceDriver, options);
    downloader.Navigate().GoToUrl(siteDownloaderLink);
    downloader.FindElement(By.Name("websiteLink")).SendKeys(targetSite);

    scraper = new EdgeDriver(deviceDriver, options);
    scraper.Manage().Window.Maximize();

    scraper.Navigate().GoToUrl(targetSite);
    scraper.FindElement(By.Id("btn_donotaccept")).Click();
}


bool WaitingForOpening()
    => scraper.PageSource.Contains("saranno aperte", StringComparison.CurrentCultureIgnoreCase);

bool TheSeatsHaveEnded()
    => scraper.PageSource.Contains("(grazie)", StringComparison.CurrentCultureIgnoreCase);

void RefreshPage()
{
    scraper.Navigate().Refresh();
}
void TakeScreenShoot()
{
    var screenshot = ((ITakesScreenshot)scraper).GetScreenshot();

    string folder = $"s/{sessionId}";
    if (!Directory.Exists(folder))
        Directory.CreateDirectory(folder);

    screenshot.SaveAsFile($"{folder}/{iterations}.png");
}
void SaveSourceCode()
{
    string folder = $"c/{sessionId}";
    if (!Directory.Exists(folder))
        Directory.CreateDirectory(folder);

    File.WriteAllText($"{folder}/{iterations}.html", scraper.PageSource);
}
void DownloadPage()
{
    downloader.FindElement(By.ClassName("main-form__btn")).Click();
}
void ReenableDownloader()
{
    while (true)
    {
        try
        {
            var closeButton = downloader.FindElement(By.ClassName("popup-links__donat"));
            closeButton.Click();
            break;
        }
        catch
        {
            Thread.Sleep(100);
        }
    }
}
static void KeepScreenOn()
{
    // TODO: keep screen on
}
static void WaitAFewTime(int ms)
{
    Thread.Sleep(ms);
}
static void LogException(Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(ex);
    Console.ResetColor();
}
static void SayTaskFinished()
{
    Console.WriteLine("Done!");
    Console.WriteLine("Press any key to exit...");
}
void CloseAllDrivers()
{
    scraper?.Quit();
    downloader?.Quit();
}

static void WaitingForUserThatRealizesItsEnded()
{
    Console.WriteLine("The seats have ended!");
    Console.ReadKey();
}