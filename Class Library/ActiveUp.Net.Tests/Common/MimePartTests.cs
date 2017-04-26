using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ActiveUp.Net.Mail;
using NUnit.Framework;
using System.Reflection;

namespace ActiveUp.Net.Tests.Common
{
    [TestFixture]
    public class MimePartTests
    {
        private static string _baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string _textfilePath;
        private static string _imagefilePath;
        private const string _textContentFileName = ".txt";
        private const string _binaryContentFileName = ".gif";

        public MimePartTests()
        {
            _textfilePath = _baseDir + "\\resource\\test_textfile.txt";
            _imagefilePath = _baseDir + "\\resource\\test_picture.gif";
        }

        [Test]
        public void should_initialize_from_byte_array_for_text_content()
        {
            var content = "My MIME part content";
            var binaryContent = Encoding.UTF8.GetBytes(content);

            var mimePart = new MimePart(binaryContent, _textContentFileName);

            mimePart.BinaryContent.ShouldEqual(binaryContent);
            mimePart.ContentType.MimeType.ShouldEqual("text/plain");
            mimePart.ContentDisposition.FileName.ShouldEqual("\".txt\"");
            mimePart.ContentName.ShouldEqual(".txt");
            mimePart.ContentTransferEncoding.ShouldEqual(ContentTransferEncoding.QuotedPrintable);
            mimePart.TextContent.ShouldEqual(content);
            mimePart.Charset.ShouldBeNull();
        }

        [Test]
        public void should_initialize_from_byte_array_for_binary_content()
        {
            var binaryContent = new byte[] { 01, 02, 03, 04, 05 };

            var mimePart = new MimePart(binaryContent, _binaryContentFileName);

            mimePart.BinaryContent.ShouldEqual(binaryContent);
            mimePart.ContentType.MimeType.ShouldEqual("image/gif");
            mimePart.ContentDisposition.FileName.ShouldEqual("\".gif\"");
            mimePart.ContentName.ShouldEqual(".gif");
            mimePart.ContentTransferEncoding.ShouldEqual(ContentTransferEncoding.Base64);
            mimePart.TextContent.ShouldEqual("AQIDBAU=");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void should_initialize_from_file(bool generateContentId)
        {
            var fileBytes = File.ReadAllBytes(_imagefilePath);

            var mimePart = new MimePart(_imagefilePath, generateContentId);

            mimePart.BinaryContent.ShouldEqual(fileBytes);
            mimePart.ContentType.MimeType.ShouldEqual("image/gif");
            mimePart.ContentDisposition.FileName.ShouldEqual("\"test_picture.gif\"");
            mimePart.ContentName.ShouldEqual("test_picture.gif");
            if (generateContentId)
                mimePart.ContentId.ShouldNotBeNull();
            else
                mimePart.ContentId.ShouldBeNull();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void should_initialize_from_file_with_charset(bool generateContentId)
        {
            var fileBytes = File.ReadAllBytes(_textfilePath);

            var mimePart = new MimePart(_textfilePath, generateContentId, "ASCII");

            mimePart.BinaryContent.ShouldEqual(fileBytes);
            mimePart.ContentType.MimeType.ShouldEqual("text/plain");
            mimePart.ContentDisposition.FileName.ShouldEqual("\"test_textfile.txt\"");
            mimePart.ContentName.ShouldEqual("test_textfile.txt");
            mimePart.Charset.ShouldEqual("ASCII");
            mimePart.ContentTransferEncoding.ShouldEqual(ContentTransferEncoding.QuotedPrintable);
            mimePart.TextContent.ShouldEqual("Hello, World !");
            if (generateContentId)
                mimePart.ContentId.ShouldNotBeNull();
            else
                mimePart.ContentId.ShouldBeNull();
        }

        [Test]
        public void should_generate_unique_ContentId()
        {
            var firstContentId = new MimePart(_textfilePath, true).ContentId;
            var secondContentId = new MimePart(_textfilePath, true).ContentId;

            firstContentId.ShouldMatch("^<AMLv2[0-9]*@[^>]*>$");
            secondContentId.ShouldMatch("^<AMLv2[0-9]*@[^>]*>$");
            firstContentId.ShouldNotEqual(secondContentId);
        }

        [Test]
        public void should_initialize_from_file_with_content_id()
        {
            var fileBytes = File.ReadAllBytes(_imagefilePath);
            const string contentId = "ContentId";

            var mimePart = new MimePart(_imagefilePath, contentId);

            mimePart.BinaryContent.ShouldEqual(fileBytes);
            mimePart.ContentType.MimeType.ShouldEqual("image/gif");
            mimePart.ContentDisposition.FileName.ShouldEqual("\"test_picture.gif\"");
            mimePart.ContentName.ShouldEqual("test_picture.gif");
            mimePart.Charset.ShouldBeNull();
            mimePart.ContentId.ShouldEqual("<" + contentId + ">");
            mimePart.ContentTransferEncoding.ShouldEqual(ContentTransferEncoding.Base64);
        }

        [Test]
        public void should_initialize_from_file_with_content_id_and_charset()
        {
            var fileBytes = File.ReadAllBytes(_textfilePath);
            const string contentId = "ContentId";

            var mimePart = new MimePart(_textfilePath, contentId, "ASCII");

            mimePart.BinaryContent.ShouldEqual(fileBytes);
            mimePart.ContentType.MimeType.ShouldEqual("text/plain");
            mimePart.ContentDisposition.FileName.ShouldEqual("\"test_textfile.txt\"");
            mimePart.ContentName.ShouldEqual("test_textfile.txt");
            mimePart.Charset.ShouldEqual("ASCII");
            mimePart.ContentId.ShouldEqual("<" + contentId + ">");
            mimePart.ContentTransferEncoding.ShouldEqual(ContentTransferEncoding.QuotedPrintable);
        }

        [Test]
        public void should_save_to_file()
        {
            var binaryContent = Encoding.UTF8.GetBytes("My MIME part content");
            var mimePart = new MimePart(binaryContent, _textContentFileName);
            var tempFilePath = Path.GetTempFileName();

            var returnValue = mimePart.StoreToFile(tempFilePath);

            returnValue.ShouldEqual(returnValue);
            File.ReadAllBytes(tempFilePath).ShouldEqual(binaryContent);
        }

        [Test]
        public void should_erase_the_file_before_writing_in_it_to_remove_eventual_leftovers_when_overwriting()
        {
            var binaryContent = Encoding.UTF8.GetBytes("My MIME part content");
            var shortBinaryContent = Encoding.UTF8.GetBytes("My MIME part");
            var tempFilePath = Path.GetTempFileName();

            new MimePart(binaryContent, _textContentFileName).StoreToFile(tempFilePath);
            new MimePart(shortBinaryContent, _textContentFileName).StoreToFile(tempFilePath);

            File.ReadAllBytes(tempFilePath).ShouldEqual(shortBinaryContent);
        }

        [Test]
        public void should_generate_basic_header_string()
        {
            var fileBytes = File.ReadAllBytes(_textfilePath);
            var mimePart = new MimePart(fileBytes, _textContentFileName);

            var headerString = mimePart.GetHeaderString();

            headerString.ShouldMatch("Content-Type: text/plain;\r\n");
            headerString.ShouldMatch("Content-Name: .txt");
            headerString.ShouldMatch("Content-Transfer-Encoding: quoted-printable");
            headerString.ShouldMatch("\r\n$");
        }

        [Test]
        public void should_generate_header_string_with_content_disposition()
        {
            var fileBytes = File.ReadAllBytes(_textfilePath);
            var mimePart = new MimePart(fileBytes, _textContentFileName) { ContentDisposition = { Disposition = "foo" } };

            var headerString = mimePart.GetHeaderString();

            headerString.ShouldMatch("Content-Disposition: foo;\r\n");
        }

        [Test]
        public void should_not_repeat_content_disposition_nor_content_type_if_they_were_added_to_the_header_fields()
        {
            var fileBytes = File.ReadAllBytes(_textfilePath);
            var mimePart = new MimePart(fileBytes, _textContentFileName) { ContentDisposition = { Disposition = "foo" } };
            mimePart.HeaderFields.Add("content-disposition", "foo");
            mimePart.HeaderFields.Add("content-type", "foo");

            var headerString = mimePart.GetHeaderString();

            Regex.Matches(headerString, "Content-Disposition").Count.ShouldEqual(1);
            Regex.Matches(headerString, "Content-Type").Count.ShouldEqual(1);
        }

        [Test]
        public void should_generage_header_string_with_contentId()
        {
            var mimePart = new MimePart(_textfilePath, true);

            var headerString = mimePart.GetHeaderString();

            headerString.ShouldMatch("Content-ID: <AMLv2[0-9]*@[^>]*>\r\n");
        }

        [Test]
        public void should_force_base64_encoding_if_specified()
        {
            var fileBytes = File.ReadAllBytes(_textfilePath);
            var mimePart = new MimePart(fileBytes, _textContentFileName);

            var headerString = mimePart.GetHeaderString(forceBase64Encoding: true);

            headerString.ShouldMatch("Content-Transfer-Encoding: base64");
        }

        [Test]
        public void should_handle_parts_with_no_charset_and_empty_binary_content_when_forcing_base64()
        {
            var mimePart = new MimePart(new byte[0], _textContentFileName) { TextContent = null };

            Assert.DoesNotThrow(() => mimePart.ToMimeString(true));
        }

        [Test]
        public void should_serialize_single_part_message_to_mime_string()
        {
            var textFileContent = File.ReadAllText(_textfilePath);
            var mimePart = new MimePart(_textfilePath, false);

            var mimeString = mimePart.ToMimeString();

            mimeString.ShouldMatch("Content-Type: text/plain;\r\n");
            mimeString.ShouldMatch(@"\r\n" + textFileContent + "$");
        }

        [Test]
        public void should_use_existing_boundary_if_provided_when_generating_mime_string()
        {
            var mimePart = CreateMultipartMimePart("MyAmazingBoundary");

            var mimeString = mimePart.ToMimeString();

            mimeString.ShouldMatch(@"	boundary=""MyAmazingBoundary""\r\n");
            Regex.Matches(mimeString, "MyAmazingBoundary").Count.ShouldEqual(4);
        }

        private static MimePart CreateMultipartMimePart(string boundaryOverride = null)
        {
            return new MimePart
            {
                ContentType =
                        {
                            MimeType = "multipart/mixed",
                            Parameters = { { "boundary", boundaryOverride } }
                        },
                SubParts = new MimePartCollection
                        {
                            new MimePart(_textfilePath, true),
                            new MimePart(_imagefilePath, false)
                        }
            };
        }

        [Test]
        public void should_serialize_multipart_message_to_mime_string()
        {
            var mimePart = CreateMultipartMimePart();

            var subParts = mimePart.ToMimeString().SplitMimeParts();

            subParts.Length.ShouldEqual(3);
            var messagePart = subParts[0];
            messagePart.ShouldMatch("^Content-Type: multipart/mixed;\r\n");
            messagePart.ShouldMatch(@"	boundary=""---AU_MimePart_[0-9]*""\r\n");
            var textPart = subParts[1];
            textPart.ShouldMatch("^Content-Type: text/plain;\r\n");
            textPart.ShouldMatch("Hello, World !\r\n\r\n$");
            var imagePart = subParts[2];
            imagePart.ShouldMatch("^Content-Type: image/gif");
            imagePart.ShouldMatch("-----AU_MimePart_[0-9]*--\r\n$");
        }

        [Test]
        public void should_serialize_to_mime_string_mutipart_message_with_nested_parts()
        {
            var mimePart = new MimePart
            {
                ContentType =
                        {
                            MimeType = "multipart/mixed",
                            Parameters = { { "boundary", "OuterBoundary" } }
                        },
                SubParts = new MimePartCollection
                        {
                            new MimePart
                                {
                                    ContentType =
                                        {
                                            MimeType = "multipart/mixed",
                                            Parameters = { { "boundary", "InnerBoundary" } }

                                        },
                                    SubParts = new MimePartCollection
                                        {
                                            new MimePart(_textfilePath, true),
                                            new MimePart(_imagefilePath, false)
                                        }
                                },
                            new MimePart(_imagefilePath, false)
                        }
            };

            var outerSubParts = mimePart.ToMimeString().SplitMimeParts("--OuterBoundary\r\n");
            outerSubParts.Length.ShouldEqual(3);
            var mixedPart = outerSubParts[1];
            mixedPart.ShouldMatch("^Content-Type: multipart/mixed;");
            mixedPart.ShouldMatch(@"	boundary=""InnerBoundary""\r\n");
            var imagePart = outerSubParts[2];
            imagePart.ShouldMatch("^Content-Type: image/gif");
            imagePart.ShouldMatch("--OuterBoundary--\r\n$");

            var innerSubParts = mixedPart.SplitMimeParts("--InnerBoundary\r\n");
            innerSubParts.Length.ShouldEqual(3);
            var innerTextPart = innerSubParts[1];
            innerTextPart.ShouldMatch("^Content-Type: text/plain;\r\n");
            innerTextPart.ShouldMatch("Hello, World !\r\n\r\n$");
            var innerImagePart = innerSubParts[2];
            innerImagePart.ShouldMatch("^Content-Type: image/gif");
            innerImagePart.ShouldMatch("--InnerBoundary--\r\n\r\n\r\n$");
        }

        [Test]
        public void should_force_base64_encoding_when_serializing_to_mime_string()
        {
            var mimePart = CreateMultipartMimePart();

            var subParts = mimePart.ToMimeString(forceBase64Encoding: true).SplitMimeParts();

            subParts.Length.ShouldEqual(3);
            var messagePart = subParts[0];
            messagePart.ShouldMatch("^Content-Type: multipart/mixed;\r\n");
            messagePart.ShouldMatch("Content-Transfer-Encoding: base64");
            var textPart = subParts[1];
            textPart.ShouldMatch("Content-Transfer-Encoding: base64");
            textPart.ShouldMatch("SGVsbG8sIFdvcmxkICE=\r\n\r\n$");
            var imagePart = subParts[2];
            imagePart.ShouldMatch("Content-Transfer-Encoding: base64");
        }

        [Test]
        public void should_return_proper_content_encoding()
        {
            var mimePart = new MimePart();

            mimePart.HeaderFields["content-transfer-encoding"] = "quoted-printable";
            mimePart.ContentTransferEncoding.ShouldEqual(ContentTransferEncoding.QuotedPrintable);
            mimePart.HeaderFields["content-transfer-encoding"] = "base64";
            mimePart.ContentTransferEncoding.ShouldEqual(ContentTransferEncoding.Base64);
            mimePart.HeaderFields["content-transfer-encoding"] = "8bit";
            mimePart.ContentTransferEncoding.ShouldEqual(ContentTransferEncoding.EightBits);
            mimePart.HeaderFields["content-transfer-encoding"] = "7bit";
            mimePart.ContentTransferEncoding.ShouldEqual(ContentTransferEncoding.SevenBits);
            mimePart.HeaderFields["content-transfer-encoding"] = "binary";
            mimePart.ContentTransferEncoding.ShouldEqual(ContentTransferEncoding.Binary);
            mimePart.HeaderFields["content-transfer-encoding"] = "bla";
            mimePart.ContentTransferEncoding.ShouldEqual(ContentTransferEncoding.Unknown);
        }

        [Test]
        public void should_set_proper_content_encoding()
        {
            var mimePart = new MimePart();

            mimePart.ContentTransferEncoding = ContentTransferEncoding.Binary;
            mimePart.HeaderFields["content-transfer-encoding"].ShouldEqual("binary");
            mimePart.ContentTransferEncoding = ContentTransferEncoding.QuotedPrintable;
            mimePart.HeaderFields["content-transfer-encoding"].ShouldEqual("quoted-printable");
            mimePart.ContentTransferEncoding = ContentTransferEncoding.SevenBits;
            mimePart.HeaderFields["content-transfer-encoding"].ShouldEqual("7bit");
            mimePart.ContentTransferEncoding = ContentTransferEncoding.EightBits;
            mimePart.HeaderFields["content-transfer-encoding"].ShouldEqual("8bit");
            mimePart.ContentTransferEncoding = ContentTransferEncoding.None;
            mimePart.HeaderFields["content-transfer-encoding"].ShouldEqual("base64");
        }

        [Test]
        public void should_content_transfer_encode_text()
        {
            var longTextMessage = "This is a long text message that is big enough to be wrapped and contains characters like \r\n or < that will be encoded";
            var mimePart = new MimePart(Encoding.UTF8.GetBytes(longTextMessage), _textContentFileName);

            mimePart.ContentTransferEncoding = ContentTransferEncoding.SevenBits;
            mimePart.TextContentTransferEncoded.ShouldEqual(longTextMessage);
            mimePart.ContentTransferEncoding = ContentTransferEncoding.EightBits;
            mimePart.TextContentTransferEncoded.ShouldEqual("This is a long text message that is big enough to be wrapped and contains=\r\n characters like =0D=0A or < that will be encoded");
            mimePart.ContentType.MimeType = "message/thing";
            mimePart.TextContentTransferEncoded.ShouldEqual("This is a long text message that is big enough to be wrapped and contains characters like \r\n or < that will be encoded");
            mimePart.ContentType.MimeType = "unknown";
            mimePart.TextContentTransferEncoded.ShouldEqual("VGhpcyBpcyBhIGxvbmcgdGV4dCBtZXNzYWdlIHRoYXQgaXMgYmlnIGVub3VnaCB0byBiZSB3cm\r\nFwcGVkIGFuZCBjb250YWlucyBjaGFyYWN0ZXJzIGxpa2UgDQogb3IgPCB0aGF0IHdpbGwgYmUg\r\nZW5jb2RlZA==");
        }

        /// <summary>
        ///  https://tools.ietf.org/html/rfc2387
        /// </summary>
        [Test(Description = "")]
        public void should_append_html_parts_with_multipart_related_content_type()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\html_multipart_related_email_with_subparts.eml");
            message.BodyText.Text.ShouldEqual("Hi,\r\n\r\nI?m the first step of message to test.\r\n\r\nSender\r\nHelper\r\n[Descrição: Descrição: Descrição: Descrição: Imagem4]\r\nE-mail: sender@sender.com\r\nWeb: www.sender.com<http://www.sender.com/>\r\n\r\nCurta nossa página no Facebook<https://www.sender.com>\r\n\r\n\r\n\r\n\r\nDe: Destinatary - Client\r\nEnviada em: quarta-feira, 23 de março de 2016 18:39\r\nPara: Odare; suport@enterprise.com; suporte\r\nCc: contasapagar; Customer2 - Client; Customer3 - Client; Customer4 - Client\r\nAssunto: Email multipart related multilevel\r\n\r\nHi there,\r\n\r\nThis is the multipart original message.\r\n\r\n[cid:image003.jpg@01D185BB.A9907410]\r\n\r\n\r\nFollow the image of problem:\r\n\r\n[cid:image004.jpg@01D185BB.A9907410]\r\n\r\nAtt,\r\n\r\n\r\n\r\n\r\n\r\n");
            message.BodyHtml.Text.ShouldEqual("<html>\r\n<head>\r\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=iso-8859-1\">\r\n<meta name=\"Generator\" content=\"Microsoft Word 14 (filtered medium)\">\r\n<!--[if !mso]><style>v\\:* {behavior:url(#default#VML);}\r\no\\:* {behavior:url(#default#VML);}\r\nw\\:* {behavior:url(#default#VML);}\r\n..sape {behavior:url(#default#VML);}\r\n</style><![endif]--><style><!--\r\n/* Font Definitions */\r\n@font-face\r\n	{font-family:Calibri;\r\n	panose-1:2 15 5 2 2 2 4 3 2 4;}\r\n@font-face\r\n	{font-family:Tahoma;\r\n	panose-1:2 11 6 4 3 5 4 4 2 4;}\r\n@font-face\r\n	{font-family:Verdana;\r\n	panose-1:2 11 6 4 3 5 4 4 2 4;}\r\n@font-face\r\n	{font-family:\"Century Gothic\";\r\n	panose-1:2 11 5 2 2 2 2 2 2 4;}\r\n/* Style Definitions */\r\np.MsoNormal, li.MsoNormal, div.MsoNormal\r\n	{margin:0cm;\r\n	margin-bottom:.0001pt;\r\n	font-size:11.0pt;\r\n	font-family:\"Calibri\",\"sans-serif\";\r\n	mso-fareast-language:EN-US;}\r\na:link, span.MsoHyperlink\r\n	{mso-style-priority:99;\r\n	color:blue;\r\n	text-decoration:underline;}\r\na:visited, span.MsoHyperlinkFollowed\r\n	{mso-style-priority:99;\r\n	color:purple;\r\n	text-decoration:underline;}\r\np.MsoAcetate, li.MsoAcetate, div.MsoAcetate\r\n	{mso-style-priority:99;\r\n	mso-style-link:\"Texto de balão Char\";\r\n	margin:0cm;\r\n	margin-bottom:.0001pt;\r\n	font-size:8.0pt;\r\n	font-family:\"Tahoma\",\"sans-serif\";\r\n	mso-fareast-language:EN-US;}\r\nspan.TextodebaloChar\r\n	{mso-style-name:\"Texto de balão Char\";\r\n	mso-style-priority:99;\r\n	mso-style-link:\"Texto de balão\";\r\n	font-family:\"Tahoma\",\"sans-serif\";}\r\nspan.EstiloDeEmail19\r\n	{mso-style-type:personal;\r\n	font-family:\"Calibri\",\"sans-serif\";\r\n	color:windowtext;}\r\nspan.EstiloDeEmail20\r\n	{mso-style-type:personal-reply;\r\n	font-family:\"Calibri\",\"sans-serif\";\r\n	color:#365F91;}\r\n..MoChpDefault\r\n	{mso-style-type:export-only;\r\n	font-size:10.0pt;}\r\n@page WordSection1\r\n	{size:612.0pt 792.0pt;\r\n	margin:70.85pt 3.0cm 70.85pt 3.0cm;}\r\ndiv.WordSection1\r\n	{page:WordSection1;}\r\n--></style><!--[if gte mso 9]><xml>\r\n<o:shapedefaults v:ext=\"edit\" spidmax=\"1026\" />\r\n</xml><![endif]--><!--[if gte mso 9]><xml>\r\n<o:shapelayout v:ext=\"edit\">\r\n<o:idmap v:ext=\"edit\" data=\"1\" />\r\n</o:shapelayout></xml><![endif]-->\r\n</head>\r\n<body lang=\"PT-BR\" link=\"blue\" vlink=\"purple\">\r\n<div class=\"WordSection1\">\r\n<p class=\"MsoNormal\"><span style=\"color:#365F91\">Hi,<o:p></o:p></span></p>\r\n<p class=\"MsoNormal\"><span style=\"color:#365F91\"><o:p>&nbsp;</o:p></span></p>\r\n<p class=\"MsoNormal\"><span style=\"color:#365F91\">This is the multipart original message.<o:p></o:p></span></p>\r\n<p class=\"MsoNormal\"><b><span style=\"font-family:&quot;Century Gothic&quot;,&quot;sans-serif&quot;;color:#002060;mso-fareast-language:PT-BR\"><o:p>&nbsp;</o:p></span></b></p>\r\n<p class=\"MsoNormal\"><b><span style=\"font-family:&quot;Century Gothic&quot;,&quot;sans-serif&quot;;color:#002060;mso-fareast-language:PT-BR\">Sender<o:p></o:p></span></b></p>\r\n<table class=\"MsoNormalTable\" border=\"0\" cellpadding=\"0\">\r\n<tbody>\r\n<tr>\r\n<td style=\"padding:.75pt .75pt .75pt .75pt\">\r\n<p class=\"MsoNormal\"><i><span style=\"font-size:10.0pt;font-family:&quot;Century Gothic&quot;,&quot;sans-serif&quot;;color:#002060;mso-fareast-language:PT-BR\">Helper                 </span></i><i><span style=\"font-size:10.0pt;font-family:&quot;Century Gothic&quot;,&quot;sans-serif&quot;;color:#002060;mso-fareast-language:PT-BR\"><o:p></o:p></span></i></p>\r\n<p class=\"MsoNormal\"><span style=\"color:#1F497D;mso-fareast-language:PT-BR\"><img width=\"175\" height=\"104\" id=\"Imagem_x0020_1\" src=\"cid:image001.jpg@01D185BB.A9907410\" alt=\"Descrição: Descrição: Descrição: Descrição: Imagem4\"></span><span style=\"font-size:12.0pt;color:#1F497D;mso-fareast-language:PT-BR\"><o:p></o:p></span></p>\r\n</td>");
        }

        [Test(Description = "Processing multipart body containing Text and HTML with same Part Id.")]
        public void should_parse_multipart_body_text_and_html()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\multipart_body_text_and_html.eml");
            message.Subject.ShouldEqual("Reminder: Test - Please tell us about your stay");
            message.BodyText.Text.ShouldEqual("Prezado(a) Customer,\r\n \r\n Recentemente enviamos a você um convite para participar de nossa Pesquisa de Satisfação sobre a qualidade da experiência de sua hospedagem.\r\n \r\n Notamos que você não teve tempo para completar a pesquisa. Reiteramos nosso interesse em conhecer sua opinião sobre nossos produtos e serviços.\r\n \r\n Para preencher a pesquisa, por gentileza clique no endereço abaixo ou copie e cole o endereço no seu navegador.\r\n \r\n Obrigado por escolher a rede e esperamos reve-lo(a) em breve.\r\n \r\n Atenciosamente,\r\n \r\n Partner\r\n General Manager\r\n \r\n Por gentileza não responda diretamente á este convite.\r\n");
            message.BodyHtml.Text.ShouldEqual("<html>\r\n   <head>\r\n      <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">\r\n   </head>\r\n   <body>\r\n   <body>\r\n      <div>\r\n         <div align=\"center\">\r\n            <table align=\"center\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"700\" style=\"width:525.0pt;background:white\">\r\n               <tbody>\r\n                  <tr style=\"height:229.5pt\">\r\n                     <td width=\"700\" style=\"width:525.0pt;padding:0cm 0cm 0cm 0cm;height:229.5pt\">\r\n                        <div dir=\"ltr\" style=\"opacity: 1; left: 816.5px; top: 899.328125px;\">\r\n                           <div id=\":127\" role=\"button\" tabindex=\"0\" aria-label=\"Anhang &quot;&quot; herunterladen\" data-tooltip-class=\"a1V\" data-tooltip=\"Herunterladen\">\r\n                              <div></div>\r\n                           </div>\r\n                        </div>\r\n                        <u></u><u></u>  \r\n                     </td>\r\n                  </tr>\r\n                  <tr>\r\n                     <td style=\"color:#5c3339; font-family:Arial, sans-serif; font-size:10.5pt; line-height:150%; vertical-align:top\" valign=\"top\">\r\n                        <div style=\"background-color:#f5f5f5; padding:20.25pt 42.75pt 51.75pt 42.75pt\" bgcolor=\"#f5f5f5\">Prezado(a) <span style=\"color:#5f255f; font-family:&quot;Arial&quot;, &quot;sans-serif&quot;; font-size:10.5pt; font-weight:bold; line-height:150%\">Customer</span>,<br><br> Recentemente enviamos a você um convite para participar de nossa Pesquisa de Satisfação sobre a qualidade da experiência de sua hospedagem.<br><br> Notamos que você não teve tempo para completar a pesquisa. Reiteramos nosso interesse em conhecer sua opinião sobre nossos produtos e serviços.<br><br><br><br> Obrigado por escolher a rede e esperamos reve-lo(a) em breve.<br><br> Atenciosamente,<br><br><span style=\"color:#5f255f; font-family:&quot;Arial&quot;, &quot;sans-serif&quot;; font-size:10.5pt; font-weight:bold; line-height:150%\">Partner</span><br><span style=\"color:#5f255f; font-family:&quot;Arial&quot;, &quot;sans-serif&quot;; font-size:10.5pt; line-height:150%\">General Manager</span><br></div>\r\n                     </td>\r\n                  </tr>\r\n                  <tr style=\"height:99.0pt\">\r\n                     <td style=\"color:#5f255f; font-family:Arial, sans-serif; font-size:8.5pt; height:99pt\" height=\"99pt\">\r\n                     </td>\r\n                  </tr>\r\n                  <tr style=\"height:32.25pt\">\r\n                     <td style=\"background:#e3e4e4;padding:0cm 0cm 0cm 0cm;height:32.25pt\">\r\n                        <p align=\"center\" style=\"text-align:center\"><span style=\"font-family:&quot;Arial&quot;,&quot;sans-serif&quot;;color:white\"><u></u><u></u></span></p>\r\n                     </td>\r\n                  </tr>\r\n               </tbody>\r\n            </table>\r\n         </div>\r\n      </div>\r\n   </body>\r\n</html>");
            message.Attachments.Count.ShouldEqual(0);
            message.To.Count.ShouldEqual(1);
        }

        [Test(Description = "Process multipart body containing Types with UpperCase char.")]
        public void should_parse_multipart_body_with_mime_type_using_CamelCase()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\multipart_with_type_using_camel_case.eml");
            message.Subject.ShouldEqual("Res: Solicitação de reserva");
            Assert.IsFalse(string.IsNullOrWhiteSpace(message.BodyText.Text));
            Assert.IsFalse(string.IsNullOrWhiteSpace(message.BodyHtml.Text));
            message.BodyText.Text.ShouldEqual("BOA TARDE!\r\r\n\r\r\nRESERVA EFETUADA COM SUCESSO. \r\r\n \r\r\n \r\r\n \r\r\n \r\r\n﻿ ﻿ ﻿ ﻿ ﻿ ﻿ \r\r\n\r\r\n-------Mensagem original-------\r\r\n \r\r\nDe: user@domain-test.com\r\r\nData: 01/06/2016 17:08:59\r\r\nPara: client@clientdomain.com.br\r\r\nPara: client@clientdomain.com.br\r\r\nAssunto: Solicitação de reserva\r\r\n \r\r\nBoa tarde,\r\r\n\r\r\nFavor reservar conforme abaixo.\r\r\n  enterpriseSolicitação de Reserva \r\r\n\r\r\nDADOS DE GARANTIA\r\r\nTIPO / AGENCIATITULARBANDEIRANÚMEROEXPIRAÇÃOCÓD. SEG.\r\r\nNome: ENTERPRISE XPTO     \r\r\n\r\r\nINFORMAÇÕES ADICIONAIS\r\r\nAGÊNCIATELEFONEEMAILRESPONSÁVELORDEM DE SERVIÇOPROCESSO COMERCIAL\r\r\nNYCOMED(11)5555-4444plantao@domain-test.comNAME LASTNAME668029820.1.19\r\n2016/5935\r\r\n\r\r\n  \r\r\nAtenciosamente , \r\r\nNAME LASTNAME\r\r\nuser@domain-test.com \r\r\nAtendimento Posto Virtual Enterprise\r\r\nEnterprise XPTO\r\r\ntelefone +1132170000\r\r\ndireto +1132170000\r\r\ncelular +55 \r\r\nemergencial +55 11 3217-0000 \r\r\n\r\r\n \r\r\n\r\r\n\r\r\n \r\r\n ");
            message.BodyHtml.Text.ShouldEqual("<html><head>\r\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">\r\n<meta name=\"GENERATOR\" content=\"IncrediMail 1.0\">\r\n<style>v\\:* {\r\n\tBEHAVIOR: url (#default#vml)\r\n}\r\n</style>\r\n\r\n<!--IncrdiXMLRemarkStart>\r\n<IncrdiX-Info>\r\n<X-FID>B433CDFE-B71C-42C2-A5C1-D34C076A9851</X-FID>\r\n<X-FVER></X-FVER>\r\n<X-FIT></X-FIT>\r\n<X-FILE></X-FILE>\r\n<X-FCOL></X-FCOL>\r\n<X-FCAT></X-FCAT>\r\n<X-FDIS></X-FDIS>\r\n<X-Extensions></X-Extensions>\r\n<X-BG></X-BG>\r\n<X-BGT>repeat</X-BGT>\r\n<X-BGC>#ffffff</X-BGC>\r\n<X-BGPX></X-BGPX>\r\n<X-BGPY></X-BGPY>\r\n<X-ASN></X-ASN>\r\n<X-ASNF></X-ASNF>\r\n<X-ASH></X-ASH>\r\n<X-ASHF></X-ASHF>\r\n<X-AN></X-AN>\r\n<X-ANF></X-ANF>\r\n<X-AP></X-AP>\r\n<X-APF></X-APF>\r\n<X-AD></X-AD>\r\n<X-ADF></X-ADF>\r\n<X-AUTO>X-ASN,X-ASH,X-AN,X-AP,X-AD</X-AUTO>\r\n<X-CNT>;</X-CNT>\r\n</IncrdiX-Info>\r\n<IncrdiXMLRemarkEnd-->\r\n</head>\r\n<body bottomMargin=\"5\" leftmargin=\"5\" rightMargin=\"5\" scroll=\"yes\" topmargin=\"5\" bgcolor=\"#ffffff\" marginheight=\"0\" marginwidth=\"0\" webfixed=\"1\">\r\n<table id=\"INCREDIMAINTABLE\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\" background=\"\" bgcolor=\"#ffffff\" height=\"100%\">\r\n<tbody>\r\n<tr>\r\n<td style=\"POSITION: relative; PADDING-BOTTOM: 20px; PADDING-LEFT: 20px; PADDING-RIGHT: 20px; FONT-FAMILY: Verdana; DIRECTION: ltr; COLOR: #1c3966; FONT-SIZE: 12pt; PADDING-TOP: 20px\" id=\"INCREDITEXTREGION\" valign=\"top\">\r\n<div style=\"PADDING-LEFT: 2px\" id=\"INCREDI_TEXT_AREA\">\r\n<div>\r\n<div>BOA TARDE!</div>\r\n<div>&nbsp;</div>\r\n<div>RESERVA EFETUADA COM SUCESSO.&nbsp;</div>\r\n<div>&nbsp;</div>\r\n<div>&nbsp;</div></div></div>\r\n<div>\r\n<div>&nbsp;</div>\r\n<div>&nbsp;</div>\r\n<div style=\"FONT-FAMILY: Verdana; FONT-SIZE: 12pt\" dir=\"ltr\" id=\"IncrediOriginalFontSize\">\r\n<div id=\"INCREDISIGNATUREID\" ondrag=\"return false;\">﻿ ﻿ ﻿ ﻿ ﻿ ﻿ \r\n<div style=\"FONT-FAMILY: Times New Roman\"><img style=\"WIDTH: 397px; HEIGHT: 132px\" border=\"0\" src=\"cid:2F66EF6C-A1E0-446C-A57F-F466A334A38D\" width=\"100\" height=\"40\"></div></div>\r\n<div style=\"FONT-SIZE: 11pt\" dir=\"ltr\" id=\"IncrediOriginalMessage\"><i>-------Mensagem original-------</i></div>\r\n<div>&nbsp;</div>\r\n<div id=\"receivestrings\">\r\n<div style=\"FONT-SIZE: 11pt\" dir=\"ltr\"><i><b>De:</b></i> <a href=\"mailto:user@domain-test.com\">user@domain-test.com</a></div>\r\n<div style=\"FONT-SIZE: 11pt\" dir=\"ltr\"><i><b>Data:</b></i> 01/06/2016 17:08:59</div>\r\n<div style=\"FONT-SIZE: 11pt\" dir=\"ltr\"><i><b>Para:</b></i> <a href=\"mailto:client@clientdomain.com.br\">client@clientdomain.com.br</a></div>\r\n<div style=\"FONT-SIZE: 11pt\" dir=\"ltr\"><i><b>Assunto:</b></i> Solicitação de reserva</div></div>\r\n<div>&nbsp;</div>Boa tarde,<br><br>Favor reservar conforme abaixo.<br>&nbsp; \r\n<table style=\"LINE-HEIGHT: normal; WIDTH: 100%; FONT-FAMILY: wf_segoe-ui_normal,segoe ui,segoe wp,tahoma,arial,sans-serif; COLOR: rgb(33,33,33); FONT-SIZE: 15px\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n<tbody>\r\n<tr>\r\n<td>\r\n<table style=\"WIDTH: 100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n<tbody>\r\n<tr>\r\n<td>\r\n<h1>ENTERPRISE</h1></td>\r\n<td><strong>Solicitação de Reserva</strong></td>\r\n<td>&nbsp;</td></tr></tbody></table></td></tr>\r\n<tr>\r\n<td><strong>DADOS DA RESERVA</strong></td></tr>\r\n<tr>\r\n<td>\r\n<table style=\"WIDTH: 100%; BORDER-RIGHT: rgb(168,145,82) 1px solid\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n<tbody>\r\n<tr>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(168,145,82); BORDER-TOP-COLOR: rgb(168,145,82); WIDTH: 54px; BORDER-RIGHT-COLOR: rgb(168,145,82); BORDER-LEFT-COLOR: rgb(168,145,82)\"><strong>TIPO</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(168,145,82); BORDER-TOP-COLOR: rgb(168,145,82); BORDER-RIGHT-COLOR: rgb(168,145,82); BORDER-LEFT-COLOR: rgb(168,145,82)\"><strong>NOME</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(168,145,82); BORDER-TOP-COLOR: rgb(168,145,82); BORDER-RIGHT-COLOR: rgb(168,145,82); BORDER-LEFT-COLOR: rgb(168,145,82)\"><strong>DATA DA RESERVA</strong></td></tr>\r\n<tr>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(168,145,82); BORDER-TOP-COLOR: rgb(168,145,82); BORDER-RIGHT-COLOR: rgb(168,145,82); BORDER-LEFT-COLOR: rgb(168,145,82)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(168,145,82)\">Adulto</span></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(168,145,82); BORDER-TOP-COLOR: rgb(168,145,82); BORDER-RIGHT-COLOR: rgb(168,145,82); BORDER-LEFT-COLOR: rgb(168,145,82)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(168,145,82)\">CUSTOMER NAME</span></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(168,145,82); BORDER-TOP-COLOR: rgb(168,145,82); BORDER-RIGHT-COLOR: rgb(168,145,82); BORDER-LEFT-COLOR: rgb(168,145,82)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(168,145,82)\">01/06/2016</span></td></tr></tbody></table></td></tr>\r\n<tr>\r\n<td><strong>HOTEL</strong></td></tr>\r\n<tr>\r\n<td>\r\n<table style=\"BORDER-BOTTOM: rgb(244,165,51) 1px solid; BORDER-LEFT: rgb(244,165,51) 1px solid; WIDTH: 100%; BORDER-TOP: rgb(244,165,51) 1px solid; BORDER-RIGHT: rgb(244,165,51) 1px solid\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n<tbody>\r\n<tr>\r\n<td>\r\n<table style=\"WIDTH: 100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n<tbody>\r\n<tr>\r\n<td><strong>CLIENT HOTEL</strong></td></tr>\r\n<tr>\r\n<td><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(244,165,51)\">Address number 0, Iguatu, Brasil</span></td></tr>\r\n<tr>\r\n<td><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(244,165,51)\">Telefone: 88-8888-8888 / Fax: 88-8888-8888</span></td></tr></tbody></table></td>\r\n<td>&nbsp;</td></tr></tbody></table></td></tr>\r\n<tr>\r\n<td>\r\n<table style=\"WIDTH: 100%; BORDER-RIGHT: rgb(244,165,51) 1px solid\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n<tbody>\r\n<tr>\r\n<td colspan=\"8\">\r\n<table style=\"WIDTH: 100%; BORDER-RIGHT: rgb(244,165,51) 1px solid\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n<tbody>\r\n<tr>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><strong>CHECK IN</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><strong>CHECK OUT</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><strong>TIPO DE TARIFA</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); WIDTH: 114px; BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><strong>Nº DE HÓSPEDES</strong></td></tr>\r\n<tr>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(244,165,51)\">13/06/2016</span></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(244,165,51)\">14/06/2016</span></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(244,165,51)\">Tarifa Cliente</span></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(244,165,51)\">1</span></td></tr></tbody></table></td></tr>\r\n<tr>\r\n<td colspan=\"8\">&nbsp;</td></tr>\r\n<tr>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><strong>Nº DE NOITES</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><strong>DIÁRIA MÉDIA</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><strong>TAXA DE SERVIÇO</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\" colspan=\"2\"><strong>ISS</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><strong>TAXA DE TURISMO</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><strong>TOTAL ESTIMADO</strong></td></tr>\r\n<tr>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(244,165,51)\">1</span></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(244,165,51)\">R$ 150,80</span></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(244,165,51)\">-</span></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\" colspan=\"2\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(244,165,51)\">-</span></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(244,165,51)\">R$ 0,00</span></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(244,165,51); BORDER-TOP-COLOR: rgb(244,165,51); BORDER-RIGHT-COLOR: rgb(244,165,51); BORDER-LEFT-COLOR: rgb(244,165,51)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(244,165,51)\">R$ 150,80</span></td></tr></tbody></table></td></tr>\r\n<tr>\r\n<td><strong>INFORMAÇÕES DE PAGAMENTO</strong></td></tr>\r\n<tr>\r\n<td>\r\n<table style=\"WIDTH: 100%; BORDER-RIGHT: rgb(91,143,57) 1px solid\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n<tbody>\r\n<tr>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(91,143,57); BORDER-TOP-COLOR: rgb(91,143,57); BORDER-RIGHT-COLOR: rgb(91,143,57); BORDER-LEFT-COLOR: rgb(91,143,57)\"><strong>FORMA DE PAGAMENTO</strong></td></tr>\r\n<tr>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(91,143,57); BORDER-TOP-COLOR: rgb(91,143,57); BORDER-RIGHT-COLOR: rgb(91,143,57); BORDER-LEFT-COLOR: rgb(91,143,57)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(91,143,57)\">Pagamento Direto</span></td></tr></tbody></table></td></tr>\r\n<tr>\r\n<td><strong>DADOS DE GARANTIA</strong></td></tr>\r\n<tr>\r\n<td>\r\n<table style=\"WIDTH: 100%; BORDER-RIGHT: rgb(93,137,181) 1px solid\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n<tbody>\r\n<tr>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(93,137,181); BORDER-TOP-COLOR: rgb(93,137,181); BORDER-RIGHT-COLOR: rgb(93,137,181); BORDER-LEFT-COLOR: rgb(93,137,181)\"><strong>TIPO / AGENCIA</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(93,137,181); BORDER-TOP-COLOR: rgb(93,137,181); BORDER-RIGHT-COLOR: rgb(93,137,181); BORDER-LEFT-COLOR: rgb(93,137,181)\"><strong>TITULAR</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(93,137,181); BORDER-TOP-COLOR: rgb(93,137,181); BORDER-RIGHT-COLOR: rgb(93,137,181); BORDER-LEFT-COLOR: rgb(93,137,181)\"><strong>BANDEIRA</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(93,137,181); BORDER-TOP-COLOR: rgb(93,137,181); BORDER-RIGHT-COLOR: rgb(93,137,181); BORDER-LEFT-COLOR: rgb(93,137,181)\"><strong>NÚMERO</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(93,137,181); BORDER-TOP-COLOR: rgb(93,137,181); BORDER-RIGHT-COLOR: rgb(93,137,181); BORDER-LEFT-COLOR: rgb(93,137,181)\"><strong>EXPIRAÇÃO</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(93,137,181); BORDER-TOP-COLOR: rgb(93,137,181); BORDER-RIGHT-COLOR: rgb(93,137,181); BORDER-LEFT-COLOR: rgb(93,137,181)\"><strong>CÓD. SEG.</strong></td></tr>\r\n<tr>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(93,137,181); BORDER-TOP-COLOR: rgb(93,137,181); BORDER-RIGHT-COLOR: rgb(93,137,181); BORDER-LEFT-COLOR: rgb(93,137,181)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(93,137,181)\">Nome: ENTERPRISE JTB</span></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(93,137,181); BORDER-TOP-COLOR: rgb(93,137,181); BORDER-RIGHT-COLOR: rgb(93,137,181); BORDER-LEFT-COLOR: rgb(93,137,181)\">&nbsp;</td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(93,137,181); BORDER-TOP-COLOR: rgb(93,137,181); BORDER-RIGHT-COLOR: rgb(93,137,181); BORDER-LEFT-COLOR: rgb(93,137,181)\">&nbsp;</td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(93,137,181); BORDER-TOP-COLOR: rgb(93,137,181); BORDER-RIGHT-COLOR: rgb(93,137,181); BORDER-LEFT-COLOR: rgb(93,137,181)\">&nbsp;</td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(93,137,181); BORDER-TOP-COLOR: rgb(93,137,181); BORDER-RIGHT-COLOR: rgb(93,137,181); BORDER-LEFT-COLOR: rgb(93,137,181)\">&nbsp;</td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(93,137,181); BORDER-TOP-COLOR: rgb(93,137,181); BORDER-RIGHT-COLOR: rgb(93,137,181); BORDER-LEFT-COLOR: rgb(93,137,181)\">&nbsp;</td></tr></tbody></table></td></tr>\r\n<tr>\r\n<td><strong>INFORMAÇÕES ADICIONAIS</strong></td></tr>\r\n<tr>\r\n<td>\r\n<table style=\"WIDTH: 100%; BORDER-RIGHT: rgb(136,100,145) 1px solid\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n<tbody>\r\n<tr>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(136,100,145); BORDER-TOP-COLOR: rgb(136,100,145); BORDER-RIGHT-COLOR: rgb(136,100,145); BORDER-LEFT-COLOR: rgb(136,100,145)\"><strong>AGÊNCIA</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(136,100,145); BORDER-TOP-COLOR: rgb(136,100,145); BORDER-RIGHT-COLOR: rgb(136,100,145); BORDER-LEFT-COLOR: rgb(136,100,145)\"><strong>TELEFONE</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(136,100,145); BORDER-TOP-COLOR: rgb(136,100,145); BORDER-RIGHT-COLOR: rgb(136,100,145); BORDER-LEFT-COLOR: rgb(136,100,145)\"><strong>EMAIL</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(136,100,145); BORDER-TOP-COLOR: rgb(136,100,145); BORDER-RIGHT-COLOR: rgb(136,100,145); BORDER-LEFT-COLOR: rgb(136,100,145)\"><strong>RESPONSÁVEL</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(136,100,145); BORDER-TOP-COLOR: rgb(136,100,145); BORDER-RIGHT-COLOR: rgb(136,100,145); BORDER-LEFT-COLOR: rgb(136,100,145)\"><strong>ORDEM DE SERVIÇO</strong></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(136,100,145); BORDER-TOP-COLOR: rgb(136,100,145); BORDER-RIGHT-COLOR: rgb(136,100,145); BORDER-LEFT-COLOR: rgb(136,100,145)\"><strong>PROCESSO COMERCIAL</strong></td></tr>\r\n<tr>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(136,100,145); BORDER-TOP-COLOR: rgb(136,100,145); BORDER-RIGHT-COLOR: rgb(136,100,145); BORDER-LEFT-COLOR: rgb(136,100,145)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(136,100,145)\">NYCOMED</span></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(136,100,145); BORDER-TOP-COLOR: rgb(136,100,145); BORDER-RIGHT-COLOR: rgb(136,100,145); BORDER-LEFT-COLOR: rgb(136,100,145)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(136,100,145)\">(11)5555-4444</span></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(136,100,145); BORDER-TOP-COLOR: rgb(136,100,145); BORDER-RIGHT-COLOR: rgb(136,100,145); BORDER-LEFT-COLOR: rgb(136,100,145)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(136,100,145)\">plantao@domain-test.com</span></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(136,100,145); BORDER-TOP-COLOR: rgb(136,100,145); BORDER-RIGHT-COLOR: rgb(136,100,145); BORDER-LEFT-COLOR: rgb(136,100,145)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(136,100,145)\">NAME LASTNAME</span></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(136,100,145); BORDER-TOP-COLOR: rgb(136,100,145); BORDER-RIGHT-COLOR: rgb(136,100,145); BORDER-LEFT-COLOR: rgb(136,100,145)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(136,100,145)\">6680298</span></td>\r\n<td style=\"BORDER-BOTTOM-COLOR: rgb(136,100,145); BORDER-TOP-COLOR: rgb(136,100,145); BORDER-RIGHT-COLOR: rgb(136,100,145); BORDER-LEFT-COLOR: rgb(136,100,145)\"><span style=\"FONT-FAMILY: arial,helvetica,sans-serif; COLOR: rgb(136,100,145)\">20.1.19.2016/5935</span></td></tr></tbody></table></td></tr></tbody></table>&nbsp; \r\n<div>\r\n<p>Atenciosamente&nbsp;,&nbsp;</p>\r\n<p>NAME LASTNAME</p>\r\n<table style=\"WIDTH: 500px\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n<tbody>\r\n<tr>\r\n<td>\r\n<table style=\"WIDTH: 500px\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n<tbody>\r\n<tr>\r\n<td><a href=\"mailto:user@domain-test.com\" target=\"_blank\"><span style=\"COLOR: #000000\">user@domain-test.com</span></a> \r\n<p>Atendimento Posto Virtual Enterprise<br>Enterprise JTB<br>telefone &#43;6666999942<br>direto &#43;1166669999<br>celular &#43;55&nbsp;<br>emergencial&nbsp;<a href=\"tel:%2B55%2011%6666-9999\" target=\"_blank\"><span style=\"COLOR: #000000\">&#43;11 6666-9999</span></a></p></td>\r\n<td>&nbsp;</td>\r\n<td><img style=\"WIDTH: 2px; HEIGHT: 100px\" alt=\"\" src=\"http://domain-test.encontact.com.br/clientes/domain-test/images/redbar.jpg\"></td>\r\n<td>&nbsp;</td>\r\n<td><a href=\"https://intranet.domain-test.com/click/redirecionar/5/9f9591607c82fbc6901d580e10a3167a\" target=\"_blank\"><img style=\"WIDTH: 200px\" src=\"http://domain-test.encontact.com.br/clientes/domain-test/images/enterpriseLogo.jpg\"></a></td></tr></tbody></table></td></tr>\r\n<tr>\r\n<td>&nbsp;</td></tr>\r\n<tr>\r\n<td><a href=\"https://intranet.domain-test.com/click/redirecionar/32/9f9591607c82fbc6901d580e10a3167a\" target=\"_blank\"><img style=\"WIDTH: 390px\" src=\"https://ci3.googleusercontent.com/proxy/O79QaW19OqDhctptBgNx8FyAwi5Sau47BcSpmXG4RQ9g8I7fNQPW7u-5C5eGTuza6A34iHVpYTB5hhaCsz_QR3zoRlazbxtGZ5SNARg1VnS0PI9ozTa9joXGUI9iwCeaXtRNoIDoGzlT6gHDTw=s0-d-e1-ft#https://intranet.alatur.com/assets/custom/img/assinatura/2016/banners/enterprise_melhor.jpg\"></a></td></tr></tbody></table>\r\n<p>&nbsp;</p></div>\r\n<div>&nbsp;</div></div></div></td>\r\n<td valign=\"bottom\" width=\"1\" align=\"right\"><img src=\"cid:C3FF5858-6F4F-4BA3-8DE3-57D014B5B6E9\"></td></tr></tbody></table><span id=\"IncrediStamp\">\r\n<div id=\"fallback\">\r\n<table title=\"Animações GRATUITAS para seu e-mail\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"580\" align=\"center\">\r\n<tbody>\r\n<tr>\r\n<td style=\"PADDING-BOTTOM: 0px; PADDING-LEFT: 0px; PADDING-RIGHT: 0px; PADDING-TOP: 0px\" colspan=\"2\">\r\n<table style=\"WIDTH: 530px\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"530\">\r\n<tbody>\r\n<tr>\r\n<td style=\"PADDING-BOTTOM: 0px; PADDING-LEFT: 0px; PADDING-RIGHT: 0px; PADDING-TOP: 0px\" title=\"Animações GRATUITAS para seu e-mail\" align=\"right\"><a style=\"TEXT-DECORATION: none\" id=\"link\" href=\"http://www.incredimail.com/?id=621165&amp;did=10501&amp;ppd=2844,201206281825,22,1,2029939434204102655&amp;rui=157514647&amp;app_test_id=0&amp;sd=20160602\"><span border=\"0\" name=\"imgCache\"><img style=\"BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; BORDER-TOP: medium none; BORDER-RIGHT: medium none\" border=\"0\" src=\"cid:0F7F3E55-F56F-49A8-9585-C0A292E4ACA8\"> </span></a></td></tr></tbody></table></td></tr>\r\n<tr>\r\n<td style=\"PADDING-BOTTOM: 7px; PADDING-LEFT: 0px; PADDING-RIGHT: 0px; PADDING-TOP: 0px\" align=\"left\">\r\n<table style=\"WIDTH: 530px\" id=\"link\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"530\" href=\"http://www.incredimail.com?id=621165&amp;did=10501&amp;ppd=2844,201206281825,22,1,2029939434204102655\" valign=\"middle\"><!-- second line: content -->\r\n<tbody>\r\n<tr>\r\n<td style=\"WIDTH: 380px\" width=\"380\"><!-- text --><a style=\"TEXT-DECORATION: none\" id=\"link\" title=\"Animações GRATUITAS para seu e-mail\" href=\"http://www.incredimail.com/?id=621165&amp;did=10501&amp;ppd=2844,201206281825,22,1,2029939434204102655&amp;rui=157514647&amp;app_test_id=0&amp;sd=20160602\"><span style=\"FONT-FAMILY: Tahoma,arial; COLOR: #2259a6; FONT-SIZE: 19px; FONT-WEIGHT: bold\">Animações GRATUITAS para seu e-mail</span></a> </td>\r\n<td style=\"WIDTH: 130px\" width=\"130\"><!-- BUTTON -->\r\n<table style=\"WIDTH: 110px; HEIGHT: 35px\" title=\"Animações GRATUITAS para seu e-mail\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"110\" height=\"35\">\r\n<tbody><!-- TOP BORDER -->\r\n<tr>\r\n<td style=\"LINE-HEIGHT: 0; WIDTH: 1px; HEIGHT: 1px; FONT-SIZE: 1px\" bgcolor=\"#e2fbb2\" height=\"1\"></td><!-- 1X1 border -->\r\n<td style=\"LINE-HEIGHT: 0; HEIGHT: 1px; FONT-SIZE: 1px\" bgcolor=\"#bcd5a7\" height=\"1\" colspan=\"3\"><a style=\"WIDTH: 100%; DISPLAY: block; HEIGHT: 100%\" id=\"link\" href=\"http://www.incredimail.com/?id=621165&amp;did=10501&amp;ppd=2844,201206281825,22,1,2029939434204102655&amp;rui=157514647&amp;app_test_id=0&amp;sd=20160602\"><span></span></a></td>\r\n<td style=\"LINE-HEIGHT: 0; WIDTH: 1px; HEIGHT: 1px; FONT-SIZE: 1px\" bgcolor=\"#e2fbb2\" height=\"1\"></td></tr>\r\n<tr>\r\n<td style=\"LINE-HEIGHT: 0; WIDTH: 1px; HEIGHT: 1px; FONT-SIZE: 1px\" bgcolor=\"#91a184\" height=\"1\"></td><!-- 1X1 border -->\r\n<td style=\"LINE-HEIGHT: 0; HEIGHT: 1px; FONT-SIZE: 1px\" bgcolor=\"#e2fbb2\" height=\"1\" colspan=\"3\"><a style=\"WIDTH: 100%; DISPLAY: block; HEIGHT: 100%\" id=\"link\" href=\"http://www.incredimail.com/?id=621165&amp;did=10501&amp;ppd=2844,201206281825,22,1,2029939434204102655&amp;rui=157514647&amp;app_test_id=0&amp;sd=20160602\"><span></span></a></td>\r\n<td style=\"LINE-HEIGHT: 0; WIDTH: 1px; HEIGHT: 1px; FONT-SIZE: 1px\" bgcolor=\"#bcd5a7\" height=\"1\"></td></tr><!-- TEXT AREA -->\r\n<tr>\r\n<td style=\"LINE-HEIGHT: 0; WIDTH: 1px; HEIGHT: 32px; FONT-SIZE: 1px\" bgcolor=\"#91a184\" height=\"32\"><a style=\"WIDTH: 100%; DISPLAY: block; HEIGHT: 100%\" id=\"link\" href=\"http://www.incredimail.com/?id=621165&amp;did=10501&amp;ppd=2844,201206281825,22,1,2029939434204102655&amp;rui=157514647&amp;app_test_id=0&amp;sd=20160602\"><span></span></a></td>\r\n<td style=\"LINE-HEIGHT: 0; WIDTH: 1px; HEIGHT: 32px; FONT-SIZE: 1px\" bgcolor=\"#e2fbb2\" height=\"32\" border=\"0\"><a style=\"WIDTH: 100%; DISPLAY: block; HEIGHT: 100%\" id=\"link\" href=\"http://www.incredimail.com/?id=621165&amp;did=10501&amp;ppd=2844,201206281825,22,1,2029939434204102655&amp;rui=157514647&amp;app_test_id=0&amp;sd=20160602\"><span></span></a></td><!-- 1X1 border -->\r\n<td style=\"HEIGHT: 32px\" bgcolor=\"#90c03a\" height=\"32\" align=\"middle\"><a style=\"HEIGHT: 32px; TEXT-DECORATION: none\" id=\"link\" title=\"Animações GRATUITAS para seu e-mail\" href=\"http://www.incredimail..cm/?id=621165&amp;did=10501&amp;ppd=2844,201206281825,22,1,2029939434204102655&amp;rui=157514647&amp;app_test_id=0&amp;sd=20160602\" border=\"0\"><span style=\"TEXT-ALIGN: center; LINE-HEIGHT: 32px; DISPLAY: block; FONT-FAMILY: Tahoma; COLOR: black; FONT-SIZE: 15px; VERTICAL-ALIGN: middle; FONT-WEIGHT: bold; TEXT-DECORATION: none\" align=\"center\">Clique aqui!</span> </a></td>\r\n<td style=\"LINE-HEIGHT: 0; WIDTH: 1px; HEIGHT: 32px; FONT-SIZE: 1px\" bgcolor=\"#e2fbb2\" height=\"32\"><a style=\"WIDTH: 100%; DISPLAY: block; HEIGHT: 100%\" id=\"link\" href=\"http://www.incredimail.com/?id=621165&amp;did=10501&amp;ppd=2844,201206281825,22,1,2029939434204102655&amp;rui=157514647&amp;app_test_id=0&amp;sd=20160602\"><span></span></a></td>\r\n<td style=\"LINE-HEIGHT: 0; WIDTH: 1px; HEIGHT: 32px; FONT-SIZE: 1px\" bgcolor=\"#bcd5a7\" height=\"32\"><a style=\"WIDTH: 100%; DISPLAY: block; HEIGHT: 100%\" id=\"link\" href=\"http://www.incredimail.com/?id=621165&amp;did=10501&amp;ppd=2844,201206281825,22,1,2029939434204102655&amp;rui=157514647&amp;app_test_id=0&amp;sd=20160602\"><span></span></a></td></tr><!-- BOTTOM BORDER -->\r\n<tr>\r\n<td style=\"LINE-HEIGHT: 0; WIDTH: 1px; HEIGHT: 1px; FONT-SIZE: 1px\" bgcolor=\"#91a184\" height=\"1\"></td><!-- 1X1 border -->\r\n<td style=\"LINE-HEIGHT: 0; HEIGHT: 1px; FONT-SIZE: 1px\" bgcolor=\"#e2fbb2\" height=\"1\" colspan=\"3\"><a style=\"WIDTH: 100%; DISPLAY: block; HEIGHT: 100%\" id=\"link\" href=\"http://www.incredimail.com/?id=621165&amp;did=10501&amp;ppd=2844,201206281825,22,1,2029939434204102655&amp;rui=157514647&amp;app_test_id=0&amp;sd=20160602\"><span></span></a></td>\r\n<td style=\"LINE-HEIGHT: 0; WIDTH: 1px; HEIGHT: 1px; FONT-SIZE: 1px\" bgcolor=\"#bcd5a7\" height=\"1\"></td></tr>\r\n<tr>\r\n<td style=\"LINE-HEIGHT: 0; WIDTH: 1px; HEIGHT: 1px; FONT-SIZE: 1px\" bgcolor=\"#e2fbb2\" height=\"1\"></td><!-- 1X1 border -->\r\n<td style=\"LINE-HEIGHT: 0; HEIGHT: 1px; FONT-SIZE: 1px\" bgcolor=\"#91a184\" height=\"1\" colspan=\"3\"><a style=\"WIDTH: 100%; DISPLAY: block; HEIGHT: 100%\" id=\"link\" href=\"http://www.incredimail.com/?id=621165&amp;did=10501&amp;ppd=2844,201206281825,22,1,2029939434204102655&amp;rui=157514647&amp;app_test_id=0&amp;sd=20160602\"><span></span></a></td>\r\n<td style=\"LINE-HEIGHT: 0; WIDTH: 1px; HEIGHT: 1px; FONT-SIZE: 1px\" bgcolor=\"#e2fbb2\" height=\"1\"></td></tr></tbody></table><!-- BUTTON END --></td></tr></tbody></table></td></tr>\r\n<tr>\r\n<td style=\"PADDING-BOTTOM: 0px; PADDING-LEFT: 0px; PADDING-RIGHT: 0px; PADDING-TOP: 0px\" colspan=\"2\">\r\n<table style=\"WIDTH: 550px\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"550\">\r\n<tbody>\r\n<tr>\r\n<td style=\"PADDING-BOTTOM: 0px; PADDING-LEFT: 0px; PADDING-RIGHT: 0px; PADDING-TOP: 0px\" title=\"Animações GRATUITAS para seu e-mail\" align=\"right\"><a style=\"TEXT-DECORATION: none\" id=\"link\" href=\"http://www.incredimail.com/?id=621165&amp;did=10501&amp;ppd=2844,201206281825,22,1,2029939434204102655&amp;rui=157514647&amp;app_test_id=0&amp;sd=20160602\"><span border=\"0\" name=\"imgCache\"><img style=\"BORDER-BOTTOM: medium none; BORDER-LEFT: medium none; BORDER-TOP: medium none; BORDER-RIGHT: medium none\" border=\"0\" src=\"cid:0BFABC2D-B6DB-4ECC-B33F-784276AAC42F\"> </span></a></td></tr></tbody></table></td></tr></tbody></table>\r\n<table border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\" height=\"10\">\r\n<tbody>\r\n<tr></tr>\r\n<tr>\r\n<td style=\"FONT-SIZE: 10px\"></td></tr></tbody></table></div>\r\n<div style=\"LINE-HEIGHT: 0; FONT-SIZE: 0px\" id=\"pixel\"><img border=\"0\" src=\"http://www2l.incredimail.com/gcontent/stamps/new2011/pixel.gif?upn=2029939434204102655\" width=\"1\" height=\"1\"> </div></span></body></html>");
            message.Attachments.Count.ShouldEqual(0);
            message.EmbeddedObjects.Count.ShouldEqual(0);
            message.To.Count.ShouldEqual(1);
        }

        [Test(Description = "Process text only email received by iphone.")]
        public void should_parse_iphone_email()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\received-by-iphone-textonly.eml");
            message.Subject.ShouldEqual("Subject sent from iphone in test mail.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(message.BodyText.Text));
            Assert.IsTrue(string.IsNullOrWhiteSpace(message.BodyHtml.Text));
            message.BodyText.Text.ShouldEqual("Good afternoon Client,\r\nI need you to take care of this request, which has many details.\r\nSee the total value of the items and call them at the registration numbers.\r\nBe brief, try by email yourself, just tell the total and date of delivery.\r\n\r\nThank you!\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\nEnviado via IPhone ");
            message.Attachments.Count.ShouldEqual(0);
            message.EmbeddedObjects.Count.ShouldEqual(4);
            message.To.Count.ShouldEqual(1);
        }
    }

    public static class ExtendString
    {
        public static string[] SplitMimeParts(this string mimeString, string boundary = "-----AU_MimePart_[0-9]*\r\n")
        {
            return Regex.Split(mimeString, boundary);
        }
    }
}