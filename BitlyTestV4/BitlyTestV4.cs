using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BitlyAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitlyTestV4
{
    [TestClass]
    public class BitlyTestV4
    {
        //TODO remove this access token from my account.
        private static readonly string _genericAccessToken = "";

        [TestMethod]
        public void GetGroups()
        {
            var bitly = new Bitly(_genericAccessToken);
            Assert.IsNotNull(bitly);
            var groups = bitly.GetGroups().Result;
            Assert.IsTrue(groups.Any());
        }


        [TestMethod]
        public async Task PostShorten()
        {
            var bitly = new Bitly(_genericAccessToken);
            Assert.IsNotNull(bitly);
            var linkResponse = await bitly.PostShorten("https://www.google.ca/");
            Assert.AreEqual("https://s.phansoft.ca/1WuTssO", linkResponse.Link);
            Assert.AreEqual("https://www.google.ca/",linkResponse.LongUrl);
        }

        [TestMethod]
        public async Task GetBitlinksByGroup()
        {
            var bitly = new Bitly(_genericAccessToken);
            Assert.IsNotNull(bitly);
            var firstPage = await bitly.GetBitlinksByGroup();
            Assert.IsTrue(firstPage.Links.Any());
        }

        [TestMethod]
        public async Task GetBitlinksByGroupRecent()
        {
            var bitly = new Bitly(_genericAccessToken);
            Assert.IsNotNull(bitly);
            var now = DateTime.Now;
            var testUrl = "https://www.google.ca/?q=" + now.ToLongDateString();
            var linkResponse = await bitly.PostShorten(testUrl);
            Thread.Sleep(new TimeSpan(0,0,1,0));
            var newest = await bitly.GetBitlinksByGroup(createdAfter: now.AddMinutes(-10));
            Assert.IsTrue(newest.Links.Any(l => l.LongUrl == testUrl));
        }

    }
}
