using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AutoHeroFramework
{
    public class AutoHeroSearchTest
    {
        IWebDriver driver;

        [SetUp]
        public void SetUp()
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddUserProfilePreference("intl.accept_languages", "en");
            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
            chromeOptions.AddArguments("disable-infobars");
            chromeOptions.AddArguments("start-maximized");
            driver = new ChromeDriver(ReturnProjectExecutableFolder() + "/bin/Debug", chromeOptions);
        }

        [Test]
        public void SearchTest()
        {
            NavigateToUrl("https://www.autohero.com/de/search");

            SelectRegistrationDateYearFrom("2015");

            SelectSortBy("Höchster Preis");

            WaitForPageElementsToLoad();


            //get the number of records and divide by 24 (number of records displayed per page) then round up to get the number of pages
            var numOfRecords = driver.FindElement(By.XPath("//div[@data-qa-selector='results-amount']"));
            var numOfPages = Math.Ceiling(Decimal.Divide(Int32.Parse(numOfRecords.Text.Replace("Treffer", "").Trim()), 24));

            for (int i = 1; i < numOfPages; i++)
            {
                //get all the years within the page and compare to 2015
                var year = driver.FindElements(By.XPath("//ul[@data-qa-selector='spec-list']/li[1]"));
                for (int k = 0; k < year.Count; k++)
                {
                    int rowYear = Convert.ToInt32(year[k].Text.Replace("•", "").Substring(4, 4));
                    if (rowYear >= 2015)
                    {
                        Assert.GreaterOrEqual(rowYear, 2015, rowYear + " is greater than or equal to 2015");
                        Console.WriteLine("TEST PASSED - " + rowYear + " is greater than or equal to 2015");
                    }
                }

                //get all the prices within a page and compare the current row price to the next
                IList<IWebElement> price = driver.FindElements(By.XPath("//div[@data-qa-selector='price']"));
                for (int j = 0; j < price.Count; j++)
                {
                    double currentRowPrice = Convert.ToDouble(price[j].Text.Replace("€", "").Trim());

                    if(j < price.Count - 1)
                    {
                        double nextRowPrice = Convert.ToDouble(price[j + 1].Text.Replace("€", "").Trim());
                        if (currentRowPrice >= nextRowPrice)
                        {
                            Assert.GreaterOrEqual(currentRowPrice, nextRowPrice, "price: " + currentRowPrice + " is greater than or equal to price: " + nextRowPrice);
                            Console.WriteLine("TEST PASSED - " + currentRowPrice + " is greater than or equal to price: " + nextRowPrice);
                        }
                    }
                    else
                    {
                        Console.WriteLine("This is the last record. Price " + currentRowPrice + " have nothing to compare to.");
                    }
                }

                //click on the next page number
                int nextPage = i + 1;
                driver.FindElement(By.XPath("//ul[@class='pagination']/li/a[text()='" + nextPage + "']")).Click();

                //wait for next page to load
                var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.UrlContains("?page=" + nextPage));
            }
        }


        [TearDown]
        public void TearDown()
        {
            driver.Close();
        }


        void NavigateToUrl(string url)
        {
            driver.Url = url;
            //driver.Manage().Window.Maximize(); //doesn't work in Chrome OSX
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);
        }


        void SelectRegistrationDateYearFrom(string year)
        {
            //find registration by link and click
            driver.FindElement(By.XPath("//span[text()='Erstzulassung ab']")).Click();

            //create year list
            var selectYear = new SelectElement(driver.FindElement(By.Name("yearRange.min")));

            //select by value
            selectYear.SelectByText(year);
        }


        void SelectSortBy(string sort)
        {
            //create sort list 
            var selectSort = new SelectElement(driver.FindElement(By.Name("sort")));
            //select by value
            selectSort.SelectByText(sort);
        }


        void WaitForPageElementsToLoad()
        {
            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//li[@data-qa-selector='active-filter']")));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//button[@data-qa-selector='reset-active-filters-button']")));
        }


        int GetNumberOfPages()
        {
            //get the number of records and divide by 24 (number of records displayed per page) then round up to get the number of pages
            var numOfRecords = driver.FindElement(By.XPath("//div[@data-qa-selector='results-amount']"));
            var numOfPages = Math.Ceiling(Decimal.Divide(Int32.Parse(numOfRecords.Text.Replace("Treffer", "").Trim()), 24));
            return Convert.ToInt32(numOfPages);
        }


        string ReturnProjectExecutableFolder()
        {
            return Directory.GetParent(
                   Directory.GetParent(
                    Path.GetDirectoryName(
                        Assembly.GetExecutingAssembly().Location)).ToString()).ToString();

        }
    }
}
