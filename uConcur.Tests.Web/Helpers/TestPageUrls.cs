using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uConcur.Tests.Web.Helpers {
    public static class TestPageUrls {
        // This should match data in the App_Data/Umbraco.sdf.sql file.
        // Normally I prefer to create test data in-test, however this is a very specific library
        // with narrow tests, so test framework does not need data creation capabilities.
        public const string PublishedTextPage = "/umbraco#/content/content/edit/1053";
    }
}
