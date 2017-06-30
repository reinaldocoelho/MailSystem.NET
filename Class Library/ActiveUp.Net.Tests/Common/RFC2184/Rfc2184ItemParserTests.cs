using System.IO;
using NUnit.Framework;
using System.Reflection;
using ActiveUp.Net.Common;

namespace ActiveUp.Net.Tests.Common.RFC2184
{
    public partial class Rfc2184ItemParserTests
    {
        private static string _baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        [Test]
        public void shouldDecodeSimpleLine()
        {
            string normalLine = "URL=\"ftp://cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar\"";
            var normalItem = new Rfc2184ItemParser(normalLine);
            Assert.IsNull(normalItem.CharSet);
            Assert.IsFalse(normalItem.TextContainsSpecialChar);
            Assert.AreEqual(0, normalItem.Index);
            Assert.IsTrue(normalItem.IsSingleLine);
            Assert.IsNull(normalItem.Language);
            Assert.AreEqual("url", normalItem.Name);
            Assert.AreEqual("ftp://cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar", normalItem.Value);
        }

        [Test]
        public void shouldDecodeSimpleLineWithoutQuotes()
        {
            string normalLine = "URL=ftp://cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar";
            var normalItem = new Rfc2184ItemParser(normalLine);
            Assert.IsNull(normalItem.CharSet);
            Assert.IsFalse(normalItem.TextContainsSpecialChar);
            Assert.AreEqual(0, normalItem.Index);
            Assert.IsTrue(normalItem.IsSingleLine);
            Assert.IsNull(normalItem.Language);
            Assert.AreEqual("url", normalItem.Name);
            Assert.AreEqual("ftp://cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar", normalItem.Value);
        }

        [Test]
        public void shouldDecodeSimpleLineWithLanguage()
        {
            string normalLine = "title*=us-ascii'en-us'This%20is%20%2A%2A%2Afun%2A%2A%2A";
            var normalItem = new Rfc2184ItemParser(normalLine);
            Assert.AreEqual("us-ascii", normalItem.CharSet);
            Assert.IsTrue(normalItem.TextContainsSpecialChar);
            Assert.AreEqual(0, normalItem.Index);
            Assert.IsTrue(normalItem.IsSingleLine);
            Assert.AreEqual("en-us", normalItem.Language);
            Assert.AreEqual("title", normalItem.Name);
            Assert.AreEqual("This%20is%20%2A%2A%2Afun%2A%2A%2A", normalItem.Value);
        }

        [Test]
        public void shouldDecodeMultiLineSimple()
        {
            string line1 = "URL*0=\"ftp://\";";
            var line1Item = new Rfc2184ItemParser(line1);
            Assert.IsNull(line1Item.CharSet);
            Assert.IsFalse(line1Item.TextContainsSpecialChar);
            Assert.AreEqual(0, line1Item.Index);
            Assert.IsFalse(line1Item.IsSingleLine);
            Assert.IsNull(line1Item.Language);
            Assert.AreEqual("url", line1Item.Name);
            Assert.AreEqual("ftp://", line1Item.Value);

            string line2 = "URL*1=\"cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar\"";
            var line2Item = new Rfc2184ItemParser(line2);
            Assert.IsNull(line2Item.CharSet);
            Assert.IsFalse(line2Item.TextContainsSpecialChar);
            Assert.AreEqual(1, line2Item.Index);
            Assert.IsFalse(line2Item.IsSingleLine);
            Assert.IsNull(line2Item.Language);
            Assert.AreEqual("url", line2Item.Name);
            Assert.AreEqual("cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar", line2Item.Value);
        }

        [Test]
        public void shouldDecodeMultiLineLanguageAndCharset()
        {
            string line1 = "title*1*=us-ascii'en'This%20is%20even%20more%20";
            var line1Item = new Rfc2184ItemParser(line1);
            Assert.AreEqual("us-ascii", line1Item.CharSet);
            Assert.IsTrue(line1Item.TextContainsSpecialChar);
            Assert.AreEqual(1, line1Item.Index);
            Assert.IsFalse(line1Item.IsSingleLine);
            Assert.AreEqual("en", line1Item.Language);
            Assert.AreEqual("title", line1Item.Name);
            Assert.AreEqual("This%20is%20even%20more%20", line1Item.Value);

            string line2 = "title*2*=%2A%2A%2Afun%2A%2A%2A%20";
            var line2Item = new Rfc2184ItemParser(line2);
            Assert.IsNull(line2Item.CharSet);
            Assert.IsTrue(line2Item.TextContainsSpecialChar);
            Assert.AreEqual(2, line2Item.Index);
            Assert.IsFalse(line2Item.IsSingleLine);
            Assert.IsNull(line2Item.Language);
            Assert.AreEqual("title", line2Item.Name);
            Assert.AreEqual("%2A%2A%2Afun%2A%2A%2A%20", line2Item.Value);

            string line3 = "title*3=\"isn't it!\"";
            var line3Item = new Rfc2184ItemParser(line3);
            Assert.IsNull(line3Item.CharSet);
            Assert.IsFalse(line3Item.TextContainsSpecialChar);
            Assert.AreEqual(3, line3Item.Index);
            Assert.IsFalse(line3Item.IsSingleLine);
            Assert.IsNull(line3Item.Language);
            Assert.AreEqual("title", line3Item.Name);
            Assert.AreEqual("isn't it!", line3Item.Value);
        }
    }
}