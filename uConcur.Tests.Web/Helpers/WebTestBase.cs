using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using AshMind.Extensions;
using Centaur;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;

namespace uConcur.Tests.Web.Helpers {
    public class WebTestBase {
        private const int Port = 57394;
        private static readonly bool KeepWebDriversIfFailed = bool.Parse(ConfigurationManager.AppSettings["tests:KeepWebDriversIfFailed"]);
        private static bool _keepWebDriversAndIgnoreNewTests;

        private IISExpressHost _iis;
        private TestDatabase _database;
        private IList<UmbracoDriverWrapper> _driverWrappers;

        protected UmbracoDriverWrapper UmbracoDriverAt(string relativeUrl) {
            var options = new ChromeOptions();
            options.AddArguments("-incognito");

            var driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl($"http://localhost:{Port}{relativeUrl}");

            var wrapper = new UmbracoDriverWrapper(driver);
            _driverWrappers.Add(wrapper);
            return wrapper;
        }

        [TestFixtureSetUp]
        public virtual void BeforeAllTests() {
            var path = Path.GetFullPath(Path.Combine(
                // ReSharper disable once AssignNullToNotNullAttribute
                Assembly.GetExecutingAssembly().GetAssemblyFileFromCodeBase().DirectoryName,
                "../../../uConcur.Tests.Web.Umbraco-7.4.0"
            ));
            if (!Directory.Exists(path))
                throw new FileNotFoundException($"Path '{path}' was not found.", path);

            _database = new TestDatabase(Path.Combine(path, "App_Data/Umbraco.sdf"));
            _database.Recreate(Path.Combine(path, "App_Data/Umbraco.sdf.sql"));

            _iis = new IISExpressHost(path, Port)/* { LogOutput = true }*/;
            _iis.Start();
        }

        [SetUp]
        public virtual void BeforeEachTest() {
            if (_keepWebDriversAndIgnoreNewTests)
                Assert.Inconclusive();

            _driverWrappers = new List<UmbracoDriverWrapper>();
        }

        [TearDown]
        public virtual void AfterEachTest() {
            if (ShouldKeepDriversAndServer) {
                _keepWebDriversAndIgnoreNewTests = true;
                return;
            }

            Parallel.ForEach(_driverWrappers, d => d.Dispose());
        }

        [TestFixtureTearDown]
        public virtual void AfterAllTests() {
            if (ShouldKeepDriversAndServer)
                return;

            _iis.Dispose();
        }

        private bool ShouldKeepDriversAndServer => _keepWebDriversAndIgnoreNewTests
                                                || (KeepWebDriversIfFailed && TestContext.CurrentContext.Result.Status == TestStatus.Failed);
    }
}
