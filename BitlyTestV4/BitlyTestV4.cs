using System;
using System.Linq;
using System.Threading.Tasks;
using BitlyAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitlyTestV4
{
    [TestClass]
    public class BitlyTestV4
    {
        private static readonly string _genericAccessToken = "97d61af824f19e03bb69ed3cc470cd1afec8602a";

        [TestMethod]
        public void GetGroups()
        {
            //TODO remove this access token from my account.
            var bitly = new Bitly(_genericAccessToken);
            Assert.IsNotNull(bitly);
            var groups = bitly.GetGroups().Result;
            Assert.IsTrue(groups.Any());
        }


        [TestMethod]
        public async Task PostShorten()
        {
            //TODO remove this access token from my account.
            var bitly = new Bitly(_genericAccessToken);
            Assert.IsNotNull(bitly);
            var linkResponse = await bitly.PostShorten("https://www.google.ca/");
            Assert.AreEqual("https://s.phansoft.ca/1WuTssO", linkResponse.Link);
            Assert.AreEqual("https://www.google.ca/",linkResponse.LongUrl);
        }

    }
}
