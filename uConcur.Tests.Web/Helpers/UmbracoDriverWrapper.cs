using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using AshMind.Extensions;
using NUnit.Framework;
using OpenQA.Selenium;
using uConcur.Tests.Web.Helpers.Models;

namespace uConcur.Tests.Web.Helpers {
    public class UmbracoDriverWrapper : IDisposable {
        private readonly IWebDriver _webDriver;

        public UmbracoDriverWrapper(IWebDriver webDriver) {
            _webDriver = webDriver;
        }

        public UmbracoDriverWrapper Login(string username, string password = "testpassword") {
            WaitForElement(By.Name("username")).SendKeys(username);
            _webDriver.FindElement(By.Name("password")).SendKeys(password);
            _webDriver.FindElement(By.CssSelector("button[type=submit]")).Click();
            WaitUntil(() => !_webDriver.Url.Contains("/login"), "Login failed.");
            return this;
        }

        public UmbracoDriverWrapper ForProperty(string name, Action<IWebElement> action) {
            action(_webDriver.FindElement(By.CssSelector($".umb-editor[id='{name}']")));
            return this;
        }

        public UmbracoDriverWrapper Save() {
            // notification intersects with the menu (at least in 7.4.0)
            WaitUntil(() => !HasNotifications(), "'Save' might be overlayed by notification that didn't disappear yet.");

            _webDriver.FindElement(By.CssSelector(".umb-tab-buttons .btn-success.dropdown-toggle")).Click();
            _webDriver.FindElement(By.CssSelector(".umb-tab-buttons .dropdown-menu li:first-child a")).Click();
            return this;
        }

        public UmbracoDriverWrapper SaveAndPublish() {
            WaitUntil(
                () => _webDriver.FindElements(By.CssSelector(".umb-tab-buttons umb-button__overlay")).Count == 0,
                "'Save and publish' button is in overlay mode and cannot be clicked."
            );
            _webDriver.FindElement(By.CssSelector(".umb-tab-buttons [type=button].btn-success")).Click();
            return this;
        }

        public UmbracoDriverWrapper WaitForSuccessNotification(string expectedHeadline) {
            return WaitForNotification(BootstrapAlertType.Success, expectedHeadline);
        }

        public UmbracoDriverWrapper WaitForNotification(BootstrapAlertType expectedType, string expectedHeadline) {
            var notification = WaitForNotification();
            Assert.AreEqual(expectedType, notification.AlertType);
            Assert.AreEqual(expectedHeadline, notification.Headline);
            return this;
        }

        public UmbracoDriverWrapper DismissNotifications() {
            foreach (var close in _webDriver.FindElements(By.CssSelector(".umb-notifications .alert .close"))) {
                close.Click();
            }
            WaitUntil(() => !HasNotifications(), "Failed to dismiss one or more notifications.");
            return this;
        }

        public UmbracoNotification WaitForNotification() {
            WaitUntil(HasNotifications, "Failed to get a notification.");
            var element = _webDriver.FindElement(By.CssSelector(".umb-notifications .alert"));
            var alertTypeString = Regex.Match(element.GetAttribute("class"), "alert-(success|error)").Groups[1].Value;
            var headline = element.FindElement(By.CssSelector("strong"));
            var message = element.FindElement(By.CssSelector("span"));

            return new UmbracoNotification(
                (BootstrapAlertType)Enum.Parse(typeof(BootstrapAlertType), alertTypeString, true),
                headline.Text.Trim().RemoveEnd(":"),
                message.Text
            );
        }

        public UmbracoDriverWrapper Refresh() {
            _webDriver.Navigate().Refresh();
            return this;
        }

        private bool HasNotifications() {
            return _webDriver.FindElements(By.CssSelector(".umb-notifications .alert")).Count > 0;
        }

        private IWebElement WaitForElement(By by) {
            IWebElement element = null;
            WaitUntil(
                () => {
                    element = _webDriver.FindElements(by).FirstOrDefault();
                    return element != null;
                },
                $"Failed to find element {by}."
            );
            return element;
        }

        private void WaitUntil(Func<bool> condition, string message) {
            var waitTime = 100.Milliseconds();
            var maxTryCount = (int)(2.Minutes().TotalMilliseconds / waitTime.TotalMilliseconds);
            var tryCount = 0;

            while (!condition()) {
                tryCount += 1;
                if (tryCount >= maxTryCount)
                    throw new TimeoutException(message);

                Thread.Sleep(waitTime);
            }
        }

        public void Dispose() {
            _webDriver.Quit();
            _webDriver.Dispose();
        }
    }
}
