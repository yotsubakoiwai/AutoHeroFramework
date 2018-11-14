using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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

            for (int i = 1; i <= GetNumberOfPages(); i++)
            {

                GetAllYearsWithinPageAndCompareTo(2015);

                //get all the prices within a page and compare the current row price to the next
                IList<IWebElement> numPrice = driver.FindElements(By.XPath("//div[@data-qa-selector='price']"));
                int currentRowPrice;
                int nextRowPrice;

                for (int j = 0; j < numPrice.Count; j++)
                {
                    Int32.TryParse(numPrice[j].Text.Replace("€", "").Replace(".", "").Replace(",", "").Trim(), out currentRowPrice);
                    if (j < numPrice.Count - 1)
                    {
                        Int32.TryParse(numPrice[j + 1].Text.Replace("€", "").Replace(".", "").Replace(",", "").Trim(), out nextRowPrice);
                        if (currentRowPrice >= nextRowPrice)
                        {
                            Assert.GreaterOrEqual(currentRowPrice, nextRowPrice, "price: " + currentRowPrice + " is greater than or equal to price: " + nextRowPrice);
                            Console.WriteLine("TEST PASSED - " + currentRowPrice + " is greater than or equal to price: " + nextRowPrice);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Last row of page " + i + " is " + currentRowPrice);
                    }
                }

                if (i < GetNumberOfPages())
                {
                    //click on the next page number
                    int nextPage = i + 1;
                    driver.FindElement(By.XPath("//ul[@class='pagination']/li/a[text()='" + nextPage + "']")).Click();

                    //wait for next page to load
                    var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.UrlContains("?page=" + nextPage));
                }
                else
                {
                    Console.WriteLine("This is the last page, " + i);
                }

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


        void GetAllYearsWithinPageAndCompareTo(int yearToCompare)
        {
            //get all the years within the page and compare to 2015
            var year = driver.FindElements(By.XPath("//ul[@data-qa-selector='spec-list']/li[1]"));
                    for (int k = 0; k<year.Count; k++)
                    {

                        int rowYear;
                        Int32.TryParse(year[k].Text.Replace("•", "").Substring(4, 4), out rowYear);
                        if (rowYear >= yearToCompare)
                        {
                            Assert.GreaterOrEqual(rowYear, yearToCompare, rowYear + " is greater than or equal to 2015");
                            Console.WriteLine("TEST PASSED - " + rowYear + " is greater than or equal to 2015");
                        }
                    }
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
