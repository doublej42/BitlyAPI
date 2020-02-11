using Microsoft.VisualStudio.TestTools.UnitTesting;
using BitlyAPI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitlyAPI.Tests
{
    [TestClass()]
    public class BitlyTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }
        

        [TestMethod()]
        public void BitlyTest()
        {

            var bitly = new Bitly("1dcc1d6fa0d41309f410f3943c244d87b9995fdb");
            Assert.IsNotNull(bitly);
        }

        [TestMethod()]
        public void ExpandTest()
        {
            var bitly = new Bitly("1dcc1d6fa0d41309f410f3943c244d87b9995fdb");
            var result = bitly.Expand("http://s.phansoft.ca/googletestphan");
            Assert.IsTrue(result.data.expand.Any());//found atleast one url
            Assert.AreEqual("https://www.google.ca/",result.data.expand.First().long_url);//it was the expected url
        }

        //[TestMethod()]
        //public void InfoTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void LinkLookupTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void UserLinkLookupTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void ShortenTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void LinkEditTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void UserLinkSaveTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void UserSaveCustomDomainKeywordTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void UserInfoTest()
        //{
        //    Assert.Fail();
        //}

        [TestMethod()]
        public void UserLinkHistoryTest()
        {
            var bitly = new Bitly("1dcc1d6fa0d41309f410f3943c244d87b9995fdb");
            var result = bitly.UserLinkHistory();
            Assert.IsTrue(result.data.link_history.Any());
            var resultSample = result.data.link_history.First(lh => lh.long_url == "https://www.google.ca/");
            
            Assert.AreEqual("http://s.phansoft.ca/googletestphan", resultSample.keyword_link);
        }
    }
}