using ActiveUp.Net.Common;
using NUnit.Framework;
using System.IO;
using System.Reflection;

namespace ActiveUp.Net.Tests.Common.RFC2184
{
    public class Rfc2184ContentParameterParserTests
    {
        private static string _baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        [Test]
        public void shouldDecodeSimpleParameters()
        {
            var parameterParser = new Rfc2184ContentParameterParser();
            parameterParser.Add("URL=\"ftp://cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar\"");
            parameterParser.Add("title*=us-ascii'en-us'This%20is%20%2A%2A%2Afun%2A%2A%2A");
            parameterParser.Parse();
            Assert.AreEqual(2, parameterParser.Parameters.Count);
            Assert.AreEqual("ftp://cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar", parameterParser.Parameters["url"]);
            Assert.AreEqual("This%20is%20%2A%2A%2Afun%2A%2A%2A", parameterParser.Parameters["title"]);
        }

        [Test]
        public void shouldDecodeMultiplelineParameters()
        {
            var parameterParser = new Rfc2184ContentParameterParser();
            parameterParser.Add("URL*0=\"ftp://\";");
            parameterParser.Add("URL*1=\"cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar\"");
            parameterParser.Add("title*=us-ascii'en-us'This%20is%20%2A%2A%2Afun%2A%2A%2A");
            parameterParser.Parse();
            Assert.AreEqual(2, parameterParser.Parameters.Count);
            Assert.AreEqual("ftp://cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar", parameterParser.Parameters["url"]);
            Assert.AreEqual("This%20is%20%2A%2A%2Afun%2A%2A%2A", parameterParser.Parameters["title"]);
        }

        [Test]
        public void shouldDecodeMultiplelineParametersWithCharset()
        {
            var parameterParser = new Rfc2184ContentParameterParser();
            parameterParser.Add("URL*0=\"ftp://\";");
            parameterParser.Add("URL*1=\"cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar\"");
            parameterParser.Add("title*1*=us-ascii'en'This%20is%20even%20more%20");
            parameterParser.Add("title*2*=%2A%2A%2Afun%2A%2A%2A%20");
            parameterParser.Add("title*3=\"isn't it!\"");
            parameterParser.Parse();
            Assert.AreEqual(2, parameterParser.Parameters.Count);
            Assert.AreEqual("ftp://cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar", parameterParser.Parameters["url"]);
            Assert.AreEqual("This%20is%20even%20more%20%2A%2A%2Afun%2A%2A%2A%20isn't it!", parameterParser.Parameters["title"]);
        }
    }
}
