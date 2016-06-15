using System;
using NUnit.Framework;
using OpenQA.Selenium;
using uConcur.Tests.Web.Helpers;
using uConcur.Tests.Web.Helpers.Models;

namespace uConcur.Tests.Web {
    public class ConcurrentEditingTests : WebTestBase {
        [Test]
        public void Save_Fails_WhenSomeoneElseHasSavedContentSinceItWasOpened() {
            var driver1 = UmbracoDriverAt(TestPageUrls.PublishedTextPage).Login("editor.1");
            var driver2 = UmbracoDriverAt(TestPageUrls.PublishedTextPage).Login("editor.2");

            driver1.ForProperty("text", ReplaceTextWithTimestamped("Edited by 1"));
            driver2.ForProperty("text", ReplaceTextWithTimestamped("Edited by 2"));

            driver1.Save().WaitForSuccessNotification("Content saved");
            var notification = driver2.Save().WaitForNotification();

            Assert.AreEqual(BootstrapAlertType.Error, notification.AlertType);
            StringAssert.Contains("changed by user Test Editor 1", notification.Message);
            StringAssert.Contains("refresh the page", notification.Message);
        }

        [Test]
        public void SaveAndPublish_Fails_WhenSomeoneElseHasSavedContentSinceItWasOpened() {
            var driver1 = UmbracoDriverAt(TestPageUrls.PublishedTextPage).Login("editor.1");
            var driver2 = UmbracoDriverAt(TestPageUrls.PublishedTextPage).Login("editor.2");

            driver1.ForProperty("text", ReplaceTextWithTimestamped("Edited by 1"));
            driver2.ForProperty("text", ReplaceTextWithTimestamped("Edited by 2"));

            driver1.Save().WaitForSuccessNotification("Content saved");
            var notification = driver2.SaveAndPublish().WaitForNotification();

            Assert.AreEqual(BootstrapAlertType.Error, notification.AlertType);
            StringAssert.Contains("changed by user Test Editor 1", notification.Message);
            StringAssert.Contains("refresh the page", notification.Message);
        }

        [Test]
        public void Save_Succeeds_WhenPageIsRefreshedAfterConflict() {
            var driver1 = UmbracoDriverAt(TestPageUrls.PublishedTextPage).Login("editor.1");
            var driver2 = UmbracoDriverAt(TestPageUrls.PublishedTextPage).Login("editor.2");

            driver1.ForProperty("text", ReplaceTextWithTimestamped("Edited by 1"));
            driver2.ForProperty("text", ReplaceTextWithTimestamped("Edited by 2"));

            driver1.Save().WaitForSuccessNotification("Content saved");
            driver2.Save().WaitForNotification(BootstrapAlertType.Error, "Validation");

            var notification = driver2.Refresh().Save().WaitForNotification();

            Assert.AreEqual(BootstrapAlertType.Success, notification.AlertType);
            Assert.AreEqual("Content saved", notification.Headline);
        }

        [Test]
        public void SaveAndPublish_Succeeds_WhenPageIsRefreshedAfterConflict() {
            var driver1 = UmbracoDriverAt(TestPageUrls.PublishedTextPage).Login("editor.1");
            var driver2 = UmbracoDriverAt(TestPageUrls.PublishedTextPage).Login("editor.2");

            driver1.ForProperty("text", ReplaceTextWithTimestamped("Edited by 1"));
            driver2.ForProperty("text", ReplaceTextWithTimestamped("Edited by 2"));

            driver1.Save().WaitForSuccessNotification("Content saved");
            driver2.SaveAndPublish().WaitForNotification(BootstrapAlertType.Error, "Validation");

            var notification = driver2.Refresh().SaveAndPublish().WaitForNotification();

            Assert.AreEqual(BootstrapAlertType.Success, notification.AlertType);
            Assert.AreEqual("Content published", notification.Headline);
        }

        [Test]
        public void Save_Succeeds_WhenDoneTwiceBySameUser() {
            var driver = UmbracoDriverAt(TestPageUrls.PublishedTextPage).Login("editor.1");
            driver.ForProperty("text", ReplaceTextWithTimestamped("Edited 1"));
            driver.Save().WaitForSuccessNotification("Content saved");

            var notification = driver.Save().WaitForNotification();

            Assert.AreEqual(BootstrapAlertType.Success, notification.AlertType);
            Assert.AreEqual("Content saved", notification.Headline);
        }

        [Test]
        public void SaveAndPublish_Succeeds_WhenDoneTwiceByTheSameUser() {
            var driver = UmbracoDriverAt(TestPageUrls.PublishedTextPage).Login("editor.1");
            driver.ForProperty("text", ReplaceTextWithTimestamped("Edited 1"));
            driver.SaveAndPublish().WaitForSuccessNotification("Content published");

            driver.ForProperty("text", ReplaceTextWithTimestamped("Edited 2"));
            var notification = driver.SaveAndPublish().WaitForNotification();

            Assert.AreEqual(BootstrapAlertType.Success, notification.AlertType);
            Assert.AreEqual("Content published", notification.Headline);
        }

        private Action<IWebElement> ReplaceTextWithTimestamped(string value) {
            return textarea => {
                textarea.Clear();
                textarea.SendKeys($"{value} ({DateTime.Now:HH:mm:ss.fff})");
            };
        }
    }
}
