using System;
using System.Linq;
using BitlyAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitlyTestV4
{
    [TestClass]
    public class BitlyTestV4
    {
        [TestMethod]
        public void GetGroups()
        {
            //TODO remove this access token from my account.
            var bitly = new Bitly("97d61af824f19e03bb69ed3cc470cd1afec8602a");
            Assert.IsNotNull(bitly);
            var groups = bitly.GetGroups().Result;
            Assert.IsTrue(groups.Any());
        }

    }
}
