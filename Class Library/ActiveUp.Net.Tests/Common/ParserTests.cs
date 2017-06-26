using System;
using System.IO;
using ActiveUp.Net.Mail;
using NUnit.Framework;
using System.Reflection;

namespace ActiveUp.Net.Tests.Common
{
    [TestFixture]
    public class ParserTests
    {
        private static string _baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        [Test]
        public void should_parse_simple_date()
        {
            var utcDate = Parser.ParseAsUniversalDateTime("Mon, 24 Jun 2013 10:37:36 +0100");

            utcDate.ShouldEqual(new DateTime(2013, 06, 24, 09, 37, 36));
        }

        [Test]
        public void should_clean_input()
        {
            var utcDate = Parser.ParseAsUniversalDateTime("(noise\\input)Mon, 24 Jun 2013 10:37:36 +0100");

            utcDate.ShouldEqual(new DateTime(2013, 06, 24, 09, 37, 36));
        }

        [Test]
        public void should_return_resulting_date_in_utc()
        {
            var utcDate = Parser.ParseAsUniversalDateTime("Mon, 24 Jun 2013 10:37:36 +0100");

            utcDate.Kind.ShouldEqual(DateTimeKind.Utc);
        }

        [Test]
        public void should_parse_date_with_no_day_of_week()
        {
            var utcDate = Parser.ParseAsUniversalDateTime("24 Jun 2013 10:37:36 +0100");

            utcDate.ShouldEqual(new DateTime(2013, 06, 24, 09, 37, 36));
        }

        [Test]
        public void should_parse_date_with_two_digits_year()
        {
            var utcDate = Parser.ParseAsUniversalDateTime("Mon, 24 Jun 13 10:37:36 +0100");

            utcDate.ShouldEqual(new DateTime(2013, 06, 24, 09, 37, 36));
        }

        [Test]
        public void should_parse_date_with_no_seconds()
        {
            var utcDate = Parser.ParseAsUniversalDateTime("Mon, 24 Jun 2013 10:37 +0100");

            utcDate.ShouldEqual(new DateTime(2013, 06, 24, 09, 37, 00));
        }

        [Test]
        public void should_parse_basic_address()
        {
            var address = Parser.ParseAddress("here@there.com");
            address.Email.ShouldEqual("here@there.com");
            address.Name.ShouldEqual(string.Empty);
        }

        [Test]
        public void should_parse_quoted_address()
        {
            var address = Parser.ParseAddress("<\"here@there.com\">");
            address.Email.ShouldEqual("here@there.com");
            address.Name.ShouldEqual(string.Empty);
        }

        [Test]
        public void should_parse_address_with_quoted_display_name()
        {
            var address = Parser.ParseAddress("\"Display Name\" <display@name.de>");
            address.Email.ShouldEqual("display@name.de");
            address.Name.ShouldEqual("Display Name");
        }

        [Test]
        public void should_parse_address_with_non_quoted_display_name()
        {
            var address = Parser.ParseAddress("DisplayName <display@name.de>");
            address.Email.ShouldEqual("display@name.de");
            address.Name.ShouldEqual("DisplayName");
        }

        [Test]
        public void should_parse_address_with_chevrons_in_display_name()
        {
            var address = Parser.ParseAddress("\"Display Name <with Chevrons>\" <Chevrons@displayname.de>");
            address.Email.ShouldEqual("Chevrons@displayname.de");
            address.Name.ShouldEqual("Display Name <with Chevrons>");
        }

        [Test]
        public void should_parse_address_with_no_closing_quote_after_display_name()
        {
            var address = Parser.ParseAddress("\"Display Name only one quote <Chevrons@displayname.de>");
            address.Email.ShouldEqual("Chevrons@displayname.de");
            address.Name.ShouldEqual("Display Name only one quote");
        }

        [Test]
        public void should_parse_address_with_invalid_empty_quote()
        {
            var address = Parser.ParseAddress("\"\" Invoice@dymak.nl\"");
            address.Email.ShouldEqual("Invoice@dymak.nl");
            address.Name.ShouldEqual("");
        }

        /// <summary>
        /// [discussion:641270] - Created discussion to validate if this test is rigth.
        /// </summary>
        [Test]
        public void should_append_text_parts_with_inline_disposition()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\text_multipart_email.eml");

            message.BodyText.Text.ShouldEqual("Good morning,\r\nThis is the body of the message.\r\n\r\nThis is the attached disclamer\r\n");
        }

        /// <summary>
        /// [discussion:641270] - Created discussion to validate if this test is rigth.
        /// </summary>
        [Test]
        public void should_append_html_parts_with_inline_disposition()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\html_multipart_email.eml");

            message.BodyHtml.Text.ShouldEqual("Good morning,\r\n<em>This is the body of the message.</em>\r\n\r\nThis is the <em>attached</em> disclamer\r\n");
        }

        [Test]
        public void should_decode_content_name()
        {
            var message = Parser.ParseMessage(File.ReadAllText(_baseDir + "\\resource\\japanese_email.eml"));

            message.Attachments[0].ContentName.ShouldEqual("大阪瓦斯9532.pdf");
        }

        /// <summary>
        ///  https://tools.ietf.org/html/rfc2387
        /// </summary>
        [Test(Description = "LineBreak \r or \n only fail.")]
        public void should_recognize_line_break_of_notepad_text_in_body()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\quoted-printable-notepad-linebreak.eml");
            message.BodyText.Text.ShouldEqual("Alatur,\r\rFoi criada uma nova solicitação para TESTE SOLICITANTE.\r\rCliente: TESTE HOTEL\rEmpresa: TESTE\rC. Custo: TESTE TESTE\r\r\r>>> PASSAGEM AÉREA\rDescrição.: (GRU) Cumbica / (LAS) Las Vegas 04/Jan Manhã (06:00 às 12:00) (Econômica)\rHorário...: considerando saída\rPagamento.: FATURADO\r\rDescrição.: (LAS) Las Vegas / (GRU) Cumbica 07/Jan Manhã (06:00 às 12:00) (Econômica)\rHorário...: considerando saída\rPagamento.: FATURADO\r\r\r>>> SOLICITANTE\rteste solicitante (fulfillment@alatur.com)\r\r\rDestinatários que estão recebendo esse email: \rtms@argoit.com.br (tms@argoit.com.br)\rteste solicitante (fulfillment@alatur.com)\rtesteodare@encontact.com.br (testeodare@encontact.com.br)\rodare@encontact.com.br (odare@encontact.com.br)\r\rPara acessá-la clique em: \r<https://arb.alatur.com/alatur/default.aspx?Id=5a03cdaf-1503-e611-9406-90b11c25f027&LinkId=FLXMfbCeRo72PRAkakfyOg%3d%3d> \r\rEMAIL AUTOMÁTICO, NÃO RESPONDA ESSA MENSAGEM\r\n");
            message.BodyHtml.Text.ShouldEqual("");
        }

        /// <summary>
        /// TODO: Change test to fail process correct action in attachment without filename. Actualy the system process.
        /// </summary>
        [Test(Description = "Attachment without filename")]
        public void ParseAttachmentWitoutFilename()
        {
            Message message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\attachment-witout-file-name.eml");
            message.Attachments.Count.ShouldEqual(2);
            for (int i = 0; i < message.Attachments.Count; i++)
                Assert.IsNotNull(message.Attachments[i].Filename);
        }
        /// <summary>
        /// Fields: Confirm-Reading-To, Return-Receipt-To, Disposition-Notification-To was indicated without e-mail address.
        /// RFC3798 has more information about details of parse https://tools.ietf.org/html/rfc3798
        /// NOTE: Without address the system will work with a null return on parse.
        /// </summary>
        [Test(Description = "ConfirmRead, DispositionNotificationTo and ReturnReceiptTo having exception.")]
        public void MustParseInvalidConfirmReadReturnReceipt()
        {
            Message message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\confirm_read_parse_problem.eml");
            Assert.IsNull(message.ConfirmRead);
            Assert.IsNull(message.ReturnReceipt);
            Assert.AreEqual(0, message.Recipients.Count);
        }

        [Test(Description = "")]
        public void MustParseEmlWithWrongImageAsPartOfEmailBody()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\image-as-body-part.eml");
            Assert.AreEqual("CAM3z=h1WB0qSZPU+PWL5VqxsL1k1gmh0pmJivD1G+LjNC5jTLA@mail.serverhost.com", message.MessageId);
            Assert.AreEqual("Boa tarde roger,\r\n\r\nAgradeço a atenção e atendimento. Pode fechar o pedido com 2 cápsulas no\r\nvalor passado de $123.312.313,04.\r\n\r\nMoro em Abracox, eu busco pessoalmente ou recebo no meu endereço? E qual o\r\nprazo de entrega e formas de pagamento?\r\n\r\nObrigado,\r\nJosé roger\r\n\r\n\r\nEm 23 de julho de 2016 09:00, roger Munes <\r\nroger@destinataryhost.com> escreveu:\r\n\r\n> Com 2 cáp deu $123.312.313,04 fico no seu aguardo para finalizar o pedido..\r\n>\r\n>\r\n> Atenciosamente, roger Mussa\r\n>  [image: Customer Supplier]*roger de Souza Nunes* /\r\n> Atendimento\r\n> roger@destinataryhost.com <%7bEmail%7d>\r\n>\r\n>\r\n> *Customer supplier*\r\n> 0800 116 7284 -  (99) 9376-8104\r\n> http://www.destinataryhost.com\r\n>\r\n>\r\n>\r\n> [image: Twitter] <https://www.twitter.com/customersupplier>  [image:\r\n> Facebook] <https://www.facebook.com/custsupplier>  [image: Instagram]\r\n> <https://www.instagram.com/custsupplier>\r\n> Antes de imprimir este e-mail veja se é necessário e pense em sua\r\n> responsabilidade com o *Meio Ambiente*.\r\n>\r\n>\r\n>\r\n> *De:* rogerneto@serverhost.com\r\n> *Enviada em:* 22/07/2016 19:13:51\r\n> *Para:* roger Munes\r\n> *Assunto:* Re: Re: Contact\r\n> Olá roger, esse valor é com 90 cápsulas, correto? Veja por gentileza com\r\n> 2 aproveito para comprar logo mais.\r\n>\r\n> Obrigado pela atenção.\r\n>\r\n> José roger\r\n>\r\n> Em 22 de julho de 2016 16:05, roger Munes <\r\n> roger@destinataryhost.com> escreveu:\r\n>\r\n> Boa tarde tudo bem ? orçamento 345788 consegui por $ 2.222,00\r\n> fico no seu aguardo.\r\n>\r\n>\r\n> Atenciosamente, roger Mussa\r\n>  [image: Customer Supplier]*roger Munes* /\r\n> Atendimento\r\n> roger@destinataryhost.com <%7bEmail%7d>\r\n>\r\n>\r\n> *Customer supplier*\r\n> 0800 116 7284 -  (99) 9376-8104\r\n> http://www.destinataryhost.com\r\n>\r\n>\r\n>\r\n> [image: Twitter] <https://www.twitter.com/customersupplier>  [image:\r\n> Facebook] <https://www.facebook.com/custsupplier>  [image: Instagram]\r\n> <https://www.instagram.com/custsupplier>\r\n> Antes de imprimir este e-mail veja se é necessário e pense em sua\r\n> responsabilidade com o *Meio Ambiente*.\r\n>\r\n>\r\n>\r\n> *De:* rogerneto@serverhost.com\r\n> *Enviada em:* 22/07/2016 14:55:08\r\n> *Para:* roger Munes\r\n> *Assunto:* Re: Contact\r\n> Boa tarde roger,\r\n>\r\n> Agradeço o contato. Ainda não comprei porém tenho o orçamento abaixo que\r\n> infelizmente está abaixo da Miligrama. Caso consiga cobrir, prefiro comprar\r\n> com vocês por já ser cliente e ter outras compras com sucesso no histórico.\r\n>\r\n>\r\n> Obrigado,\r\n> José roger\r\n>\r\n>\r\n>\r\n> Em 22 de julho de 2016 14:49, roger Munes <\r\n> roger@destinataryhost.com> escreveu:\r\n>\r\n> Boa tarde amigo, como vai ?\r\n>\r\n> Chegou a finalizar o pedido, comprou em outro lugar ? que achou do meu\r\n> orçamento vamos negociar cubro a oferta de qualquer concorrente.\r\n>\r\n>\r\n> Atenciosamente, roger Mussa\r\n>  [image: Customer Supplier]*roger de Souza Nunes* /\r\n> Atendimento\r\n> roger@destinataryhost.com <%7bEmail%7d>\r\n>\r\n>\r\n> *Customer supplier*\r\n> 0800 116 7284 -  (99) 9376-8104\r\n> http://www.destinataryhost.com\r\n>\r\n>\r\n>\r\n> [image: Twitter] <https://www.twitter.com/customersupplier>  [image:\r\n> Facebook] <https://www.facebook.com/custsupplier>  [image: Instagram]\r\n> <https://www.instagram.com/custsupplier>\r\n> Antes de imprimir este e-mail veja se é necessário e pense em sua\r\n> responsabilidade com o *Meio Ambiente*.\r\n>\r\n>\r\n>\r\n>\r\n".Replace("\r\n", ""), message.BodyText.Text.Replace("\r\n", ""));
            Assert.AreEqual("<div dir=\"ltr\">Boa tarde roger,<div><br></div><div>Agradeço a atenção e atendimento. Pode fechar o pedido com 2 cápsulas no valor passado de <span style=\"font-size:12.8px\">$ 2.222,00.</span></div><div><br></div><div>Moro em Cubivila, eu busco pessoalmente ou recebo no meu endereço? E qual o prazo de entrega e formas de pagamento?</div><div><br></div><div>Obrigado,</div><div>José roger</div><div><br></div></div><div class=\"gmail_extra\"><br><div class=\"gmail_quote\">Em 23 de julho de 2016 09:00, roger Munes <span dir=\"ltr\">&lt;<a href=\"mailto:roger@destinataryhost.com\" target=\"_blank\">roger@custsupplier.com..br</a&gt;</span> escreveu:<br><blockquote class=\"gmail_quote\" style=\"margin:0 0 0 .8ex;border-left:1px #ccc solid;padding-left:1ex\"><div>Com 2 cáp deu $ 2.222,00 fico no seu aguardo para finalizar o pedido.    \r\n<span class=\"\"><div>\r\n<p><br>\r\nAtenciosamente, roger Mussa<br>\r\n <img alt=\"Customer Supplier\" src=\"http://www.customerhost.com/assinatura/logo.png\"><strong>roger de Souza Nunes</strong> / Atendimento<br>\r\n<a href=\"mailto:%7bEmail%7d\" target=\"_blank\">roger@destinataryhost.com</a><br>\r\n<br>\r\n<br>\r\n<strong>Customer supplier</strong> <br>\r\n0800 116 7284 - <img alt=\"\" src=\"http://customerhost.com/assinatura/whatsappm.png\"> (99) 9376-8104<br>\r\n<a href=\"http://www.destinataryhost.com/\" target=\"_blank\">http://www.destinataryhost.com</a>  <br>\r\n<br>\r\n<br>\r\n<br>\r\n<a href=\"https://www.twitter.com/customersupplier\" target=\"_blank\"><img alt=\"Twitter\" src=\"http://www.customerhost.com/assinatura/twitterm.png\"></a>  <a href=\"https://www.facebook.com/custsupplier\" target=\"_blank\"><img alt=\"Facebook\" src=\"http://www.customerhost.com/assinatura/facebookm.png\"></a>  <a href=\"https://www.instagram.com/custsupplier\" target=\"_blank\"><img alt=\"Instagram\" src=\"http://www.customerhost.com/assinatura/instagramm.png\"></a><br>\r\nAntes de imprimir este e-mail veja se é necessário e pense em sua responsabilidade com o <strong>Meio Ambiente</strong>.</p>\r\n\r\n<p><img alt=\"\" src=\"cid:31391411\" style=\"border:0px solid black;min-height:200px;margin-bottom:0px;margin-left:0px;margin-right:0px;margin-top:0px;width:600px\"></p>\r\n\r\n<p> </p>\r\n</div>\r\n \r\n\r\n</span><div style=\"text-align:left\"><strong>De:</strong> <a href=\"mailto:rogerneto@serverhost.com\" target=\"_blank\">rogerneto@serverhost.com</a><br>\r\n<strong>Enviada em:</strong> 22/07/2016 19:13:51<span class=\"\"><br>\r\n<strong>Para:</strong> roger Munes<br>\r\n</span><strong>Assunto:</strong> Re: Re: Contact</div><div><div class=\"h5\">\r\n\r\n<div>Olá roger, esse valor é com 90 cápsulas, correto? Veja por gentileza com 2 aproveito para comprar logo mais.\r\n<div> </div>\r\n\r\n<div>Obrigado pela atenção.</div>\r\n\r\n<div> </div>\r\n\r\n<div>José roger</div>\r\n</div>\r\n\r\n<div> \r\n<div>Em 22 de julho de 2016 16:05, roger Munes &lt;<a href=\"mailto:roger@destinataryhost.com\" target=\"_blank\">roger@destinataryhost.com</a>&gt; escreveu:\r\n\r\n<blockquote>\r\n<div>Boa tarde tudo bem ? orçamento 345788 consegui por $ 2.222,00<br>\r\nfico no seu aguardo.\r\n<div>\r\n<p><br>\r\nAtenciosamente, roger Mussa<br>\r\n <img alt=\"Customer Supplier\" src=\"http://www.customerhost.com/assinatura/logo.png\"><strong>roger de Souza Nunes</strong> / Atendimento<br>\r\n<a href=\"mailto:%7bEmail%7d\" target=\"_blank\">roger@destinataryhost.com</a><br>\r\n<br>\r\n<br>\r\n<strong>Customer supplier</strong> <br>\r\n0800 116 7284 - <img alt=\"\" src=\"http://customerhost.com/assinatura/whatsappm.png\"> (99) 9376-8104<br>\r\n<a href=\"http://www.destinataryhost.com/\" target=\"_blank\">http://www.fmiligrama.com.br</a>  <br>\r\n<br>\r\n<br>\r\n<br>\r\n<a href=\"https://www.twitter.com/customersupplier\" target=\"_blank\"><img alt=\"Twitter\" src=\"http://www.customerhost.com/assinatura/twitterm.png\"></a>  <a href=\"https://www.facebook.com/custsupplier\" target=\"_blank\"><img alt=\"Facebook\" src=\"http://www.customerhost.com/assinatura/facebookm.png\"></a>  <a href=\"https://www.instagram.com/custsupplier\" target=\"_blank\"><img alt=\"Instagram\" src=\"http://www.customerhost.com/assinatura/instagramm.png\"></a><br>\r\nAntes de imprimir este e-mail veja se é necessário e pense em sua responsabilidade com o <strong>Meio Ambiente</strong>.</p>\r\n\r\n<p><img alt=\"\" src=\"cid:16636849\" style=\"border:0px solid black;margin-bottom:0px;margin-left:0px;margin-right:0px;margin-top:0px;min-height:200px;width:600px\"></p>\r\n\r\n<p> </p>\r\n</div>\r\n \r\n\r\n<div style=\"text-align:left\"><strong>De:</strong> <a href=\"mailto:ramalhoneto@serverhost.com\" target=\"_blank\">rogerneto@serverhost.com</a><br>\r\n<strong>Enviada em:</strong> 22/07/2016 14:55:08<br>\r\n<strong>Para:</strong> roger Munes<br>\r\n<strong>Assunto:</strong> Re: Contact</div>\r\n\r\n<div>\r\n<div>\r\n<div>Boa tarde roger,\r\n<div> </div>\r\n\r\n<div>Agradeço o contato. Ainda não comprei porém tenho o orçamento abaixo que infelizmente está abaixo da Miligrama. Caso consiga cobrir, prefiro comprar com vocês por já ser cliente e ter outras compras com sucesso no histórico.</div>\r\n\r\n<div> </div>\r\n\r\n<div> </div>\r\n\r\n<div>Obrigado,</div>\r\n\r\n<div>José roger</div>\r\n\r\n<div> </div>\r\n\r\n<div> </div>\r\n\r\n<div> </div>\r\n\r\n<div>\r\n<div>Em 22 de julho de 2016 14:49, roger Munes &lt;<a href=\"mailto:roger@destinataryhost.com\" target=\"_blank\">roger@destinataryhost.com</a>&gt; escreveu:\r\n\r\n<blockquote>\r\n<div>Boa tarde amigo, como vai ?<br>\r\n<br>\r\nChegou a finalizar o pedido, comprou em outro lugar ? que achou do meu orçamento vamos negociar cubro a oferta de qualquer concorrente.\r\n<div>\r\n<p><br>\r\nAtenciosamente, roger Mussa<br>\r\n <img alt=\"Customer Supplier\" src=\"http://www.customerhost.com/assinatura/logo.png\"><strong>roger de Souza Nunes</strong> / Atendimento<br>\r\n<a href=\"mailto:%7bEmail%7d\" target=\"_blank\">roger@destinataryhost.com</a><br>\r\n<br>\r\n<br>\r\n<strong>Customer supplier</strong> <br>\r\n0800 116 7284 - <img alt=\"\" src=\"http://customerhost.com/assinatura/whatsappm.png\"> (99) 9376-8104<br>\r\n<a href=\"http://www.destinataryhost.com/\" target=\"_blank\">http://www.fmiligrama.com.br</a>  <br>\r\n<br>\r\n<br>\r\n<br>\r\n<a href=\"https://www.twitter.com/customersupplier\" target=\"_blank\"><img alt=\"Twitter\" src=\"http://www.customerhost.com/assinatura/twitterm.png\"></a>  <a href=\"https://www.facebook.com/custsupplier\" target=\"_blank\"><img alt=\"Facebook\" src=\"http://www.customerhost.cor/assinatura/facebookm.png\"></a>  <a href=\"https://www.instagram.com/custsupplier\" target=\"_blank\"><img alt=\"Instagram\" src=\"http://www.customerhost.com/assinatura/instagramm.png\"></a><br>\r\nAntes de imprimir este e-mail veja se é necessário e pense em sua responsabilidade com o <strong>Meio Ambiente</strong>.</p>\r\n\r\n<p><img alt=\"\" src=\"cid:16636849\" style=\"border:0px solid black;margin-bottom:0px;margin-left:0px;margin-right:0px;margin-top:0px;min-height:200px;width:600px\"></p>\r\n\r\n<p> </p>\r\n</div>\r\n</div>\r\n</blockquote>\r\n</div>\r\n</div>\r\n</div>\r\n</div>\r\n</div>\r\n</div>\r\n</blockquote>\r\n</div>\r\n</div>\r\n</div></div></div></blockquote></div><br></div>\r\n".Replace("\r\n", ""), message.BodyHtml.Text.Replace("\r\n", ""));
            Assert.AreEqual("Re: Re: Re: Contact", message.Subject);
            Assert.AreEqual(1, message.To.Count);
            Assert.AreEqual(2, message.EmbeddedObjects.Count);
            Assert.AreEqual(0, message.Attachments.Count);

            // Manual validation: Save the attachment to validate if has valid image.
            //var i = 0;
            //foreach (MimePart item in message.EmbeddedObjects)
            //{
            //    var fileName = item.ContentName ?? "file" + i + ".jpg";
            //    var fileNameDecoded = item.ContentName ?? "file_decoded_" + i + ".jpg";
            //    i++;
            //    File.WriteAllBytes(fileName, item.BinaryContent);
            //    File.WriteAllBytes(fileNameDecoded, Convert.FromBase64String(item.TextContentTransferEncoded));
            //}
        }

        [Test(Description = "")]
        public void MustParseEmlWithoutContentTypeSubtypeWithLostTextBody()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\text_without_contenttype_subtype.eml");
            Assert.AreEqual("hash@sender.production.server.com", message.MessageId);
            Assert.IsFalse(string.IsNullOrWhiteSpace(message.BodyText.Text));
            Assert.IsTrue(string.IsNullOrWhiteSpace(message.BodyHtml.Text));
            Assert.AreEqual("plain", message.ContentType.SubType);
            Assert.AreEqual("text", message.ContentType.Type);
            Assert.AreEqual("text", message.ContentType.MimeType);
        }

        //[Test(Description = "")]
        //public void MustParseEmlWithContentTransferEncode8Bit()
        //{
        //    var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\content-transfer-encode-8bit.eml");
        //    Assert.AreEqual("58caaa74.6625ed0a.22a2d.5376@mx.google.com", message.MessageId);
        //    Assert.AreEqual("Special char test  çãõáéíóú", message.Subject);
        //    Assert.IsFalse(string.IsNullOrWhiteSpace(message.BodyText.Text));
        //    Assert.AreEqual("Body special char test  çãõáéíóú", message.BodyText.Text);
        //    Assert.IsTrue(string.IsNullOrWhiteSpace(message.BodyHtml.Text));
        //}

        //[Test(Description = "")]
        //public void MustParseEmlWithContentTransferEncode8BitUtf8FlowedHistory()
        //{
        //    var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\content-transfer-encode-8bit-utf8-flowed.eml");
        //    Assert.AreEqual("bd502b4d-c631-9ff4-791f-fc01c9efc0e5@EmpresaX.com.br", message.MessageId);
        //    Assert.AreEqual("Re: BLA BLÁ BLA XYZ/ XYZ / TROCA DE PACOTES origem ABC Destino XYZ x XYZ Nfs 666666 / 777777 Fornecedor Xamego INDUSTRIA", message.Subject);
        //    Assert.IsFalse(string.IsNullOrWhiteSpace(message.BodyText.Text));
        //    var result = message.BodyText.Text;
        //    Assert.AreEqual("This is a multi-part message in MIME format.Boa tarde,\r\n\r\n*Fulano*, conforme o conversado em nosso teste, os dados do erro ja \r\nforam coletados, estamos trabalhando com a máxima urgência afim de \r\nefetuarmos a identificação.\r\n\r\n_Em contato com o cliente estou tentando reproduzir o problema\r\nnesta mensagem._\r\n\r\n_*Ciclano*( Empresa B ), por gentileza, conforme ja conversado, peço, \r\npriorizar acompanhar a estrutura errada deste e-mail._\r\n\r\n\r\nCerta da atenção, agradeço\r\n\r\n\r\n*Reinaldo Coelho *\r\nNosso Grupo\r\n\r\n*\r\nEm 12/04/2017 09:10, Fulano escreveu:\r\n>\r\n> Ok, Agradeço a atenção Ciclano.\r\n>\r\n> *Reinaldo Coelho *\r\n> *Meu cargo atual*\r\n> EmpresaX\r\n> *Fone:(11) 2222-4444 / Ramal: 123 **\r\n> *Email:mary.anne@EmpresaX.com.br \r\n> <mailto:mary.anne@EmpresaX.com.br>*\r\n> *Acesse nosso site:www.EmpresaX.com.br <http://www.EmpresaX.com.br>*\r\n> Nosso Grupo\r\n>\r\n> *\r\n> Em 12/04/2017 09:01, Florencia Ramos Conceição escreveu:\r\n>>\r\n>>\r\n>>\r\n>> Sim volumes pertencido a XYZ já desembarcarão e segue hoje para \r\n>> araguaina-to\r\n>>\r\n>> Duvidas a disposição.\r\n>>\r\n>>\r\n>> -- \r\n>> *Florencia   Ramos Conceição*\r\n>> * Pendencia Fiscal*\r\n>>\r\n>> *Fone:(12)1111-6333*\r\n>> *Email:xyz.pendencias1@EmpresaX.com.br*\r\n>> *Acesse nosso site:www.EmpresaX.com.br <http://www.EmpresaX.com.br>*\r\n>>\r\n>> *----- Original Message ----*\r\n>> *From:* mary.anne@EmpresaX.com.br\r\n>> *To:* \"Florencia Ramos Conceição\" (xyz.pendencias1@EmpresaX.com.br)\r\n>> *Cc:* \"Carlão Steave\" (vendasnonorte@prego.com.br)\r\n>> *Date:* Tue, 11 Apr 2017 15:11:37 -0300\r\n>> *Subject:* Re: BLA BLÁ BLA XYZ/ XYZ / TROCA DE PACOTES origem ABC Destino \r\n>>\r\n>>     Boa tarde,\r\n>>\r\n>>     tentei contato telefônico, porem não foi possível; o cliente que\r\n>>     xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\r\n>>     yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy\r\n>>     informar se o volume do mesmo ja consta em XYZ???\r\n>>\r\n>>     Fico no aguardo para informarmos ao cliente.\r\n>>\r\n>>     Agradeço\r\n>>\r\n>>     att,\r\n>>\r\n>>     *Reinaldo Coelho *\r\n>>     *Meu cargo atual*\r\n>>\r\n>>     *Fone:(11) 2222-4444 / Ramal: 123 **\r\n>>     *Email:mary.anne@EmpresaX.com.br\r\n>>     <mailto:mary.anne@EmpresaX.com.br>*\r\n>>     *Acesse nosso site:www.EmpresaX.com.br\r\n>>     <http://www.EmpresaX.com.br>*\r\n>>\r\n>>\r\n>>     *\r\n>>     Em 05/04/2017 17:03, Florencia Ramos Conceição escreveu:\r\n>>\r\n>>\r\n>>         Esta seguindo os dois\r\n>>\r\n>>\r\n>>         -- \r\n>>         *Florencia   Ramos Conceição*\r\n>>         * Pendencia Fiscal*\r\n>>\r\n>>         *Fone:(12)1111-6333*\r\n>>         *Email:xyz.pendencias1@EmpresaX.com.br*\r\n>>         *Acesse nosso site:www.EmpresaX.com.br\r\n>>         <http://www.EmpresaX.com.br>*\r\n>>\r\n>>         *----- Original Message ----*\r\n>>         *From:* XYZ.pendencia04@EmpresaX.com.br\r\n>>         *To:* \"Florencia Ramos Conceição\"\r\n>>         (xyz.pendencias1@EmpresaX.com.br), \"Fulano\"\r\n>>         (mary.anne@EmpresaX.com.br)\r\n>>         *Date:* Wed, 5 Apr 2017 16:58:34 -0300\r\n>>         *Subject:* Re: BLA BLÁ BLA XYZ/ XYZ / TROCA DE PACOTES origem ABC Destino \r\n>>\r\n>>             Ok, lembrando que trata-se de dois volume de XYZ.\r\n>>\r\n>>             Obrigada.\r\n>>\r\n>>\r\n>>\r\n>>\r\n>>\r\n>>             Em 5/4/2017 16:53, Florencia Ramos Conceição escreveu:\r\n>>\r\n>>                 Volume  pertencido a filial XYZ  esta seguindo\r\n>>                 atraves de RRI-0031800000\r\n>>\r\n>>\r\n>>                 -- \r\n>>                 *Florencia   Ramos Conceição*\r\n>>                 * Pendencia Fiscal*\r\n>>\r\n>>                 *Fone:(12)1111-6333*\r\n>>                 *Email:xyz.pendencias1@EmpresaX.com.br*\r\n>>                 *Acesse nosso site:www.EmpresaX.com.br\r\n>>                 <http://www.EmpresaX.com.br>*\r\n>>\r\n>>                 *----- Original Message ----*\r\n>>                 *From:* XYZ.pendencia04@EmpresaX.com.br\r\n>>                 *To:* \"Fulano\" (mary.anne@EmpresaX.com.br),\r\n>>                 \"Florencia Ramos Conceição\"\r\n>>                 (xyz.pendencias1@EmpresaX.com.br)\r\n>>                 *Date:* Wed, 5 Apr 2017 09:15:54 -0300\r\n>>                 *Subject:* BLA BLÁ BLA XYZ/ XYZ / TROCA DE PACOTES origem ABC Destino \r\n>>\r\n>>                     Bom dia !\r\n>>\r\n>>                     Fulano,\r\n>>\r\n>>\r\n>>                     Essa destroca esta difícil de ser resolvida, pois\r\n>>                     os volumes que se encontravam em XYZ   , foi\r\n>>                     enviado , e até o momento não foi nos enviado os\r\n>>                     nossos corretos . O cliente XYZ nos cobra\r\n>>                     posicionamento , e se que tenho retorno da filial\r\n>>                     XYZ.\r\n>>\r\n>>                     Favor resolver esse caso , o quanto antes.\r\n>>\r\n>>\r\n>>\r\n>>                     Em 5/4/2017 08:48, Fulano escreveu:\r\n>>\r\n>>                         Bom dial,\r\n>>\r\n>>                         Pessoal informaçoes referente a\r\n>>                         destroca??...pXYZiso de um retorno*URGENTE,\r\n>>                         *pois o fornecedor( nossoMONITORADO) tem nos\r\n>>                         cobra regularmente este posicionamento.\r\n>>\r\n>>                         Fico no aguardo, para que possamos\r\n>>                         comunica-lo o mais breve possival\r\n>>\r\n>>                         att,\r\n>>\r\n>>                         *Reinaldo Coelho *\r\n>>                         *Meu cargo atual*\r\n>>\r\n>>                         *Fone:(11) 2222-4444 / Ramal: 123 **\r\n>>                         *Email:mary.anne@EmpresaX.com.br*\r\n>>                         *Acesse nosso site:www.EmpresaX.com.br*\r\n>>\r\n>>\r\n>>                         *\r\n>>                         Em 03/04/2017 09:48, Fulano escreveu:\r\n>>\r\n>>                             Bom dia,\r\n>>\r\n>>                             Ciclano assim que possível posicionar,\r\n>>                             peço também que verifique a XYZusa do\r\n>>                             cliente sobre 3 volumes, pois o erro era\r\n>>                             apenas em 2 volumes, sendo estes para a\r\n>>                             filial de XYZ...\r\n>>\r\n>>                             *Reinaldo Coelho *\r\n>>                             *Meu cargo atual*\r\n>>\r\n>>                             *Fone:(11) 2222-4444 / Ramal: 123 **\r\n>>                             *Email:mary.anne@EmpresaX.com.br*\r\n>>                             *Acesse nosso site:www.EmpresaX.com.br*\r\n>>\r\n>>\r\n>>                             *\r\n>>                             Em 03/04/2017 09:44, Gabriela Xavier escreveu:\r\n>>\r\n>>                                 Bom dia !\r\n>>\r\n>>                                 Temos algum posicionamento ?\r\n>>\r\n>>                                 Nosso cliente nos cobra RETORNO COM\r\n>>                                 URGÊNCIA...\r\n>>\r\n>>\r\n>>                                 Em 31/3/2017 09:35, Fulano escreveu:\r\n>>\r\n>>                                     Bom dia,\r\n>>\r\n>>                                     Ok, agradeço a atenção.\r\n>>\r\n>>                                     *Reinaldo Coelho *\r\n>>                                     *Meu cargo atual*\r\n>>\r\n>>                                     *Fone:(11) 2222-4444 / Ramal: 123 **\r\n>>                                     *Email:mary.anne@EmpresaX.com.br*\r\n>>\r\n>>                                     *Acesse nosso\r\n>>                                     site:www.EmpresaX.com.br*\r\n>>\r\n>>\r\n>>                                     *\r\n>>                                     Em 31/03/2017 09:25, Ciclano\r\n>>                                     Ramos Conceição escreveu:\r\n>>\r\n>>                                         Valéria Bom Dia\r\n>>\r\n>>                                         Trata-se de rota do interior,\r\n>>                                         no qual já foi XYZusado os 03\r\n>>                                         volumes pelo cliente, e\r\n>>                                         parceiro já esta retornando\r\n>>                                         com mercadoria para\r\n>>                                         transportadora para estarmos\r\n>>                                         verificando, assim que tiver\r\n>>                                         ok, informo ID de envio para\r\n>>                                         acompanhamento,\r\n>>\r\n>>                                         Duvidas a disposição.\r\n>>\r\n>>\r\n>>                                         -- \r\n>>                                         *Florencia   Ramos Conceição*\r\n>>                                         * Pendencia Fiscal*\r\n>>\r\n>>                                         *Fone:(12)1111-6333*\r\n>>                                         *Email:xyz.pendencias1@EmpresaX.com.br*\r\n>>\r\n>>                                         *Acesse nosso\r\n>>                                         site:www.EmpresaX.com.br*\r\n>>                                         & amp; lt; /p>\r\n>>\r\n>>                                         *----- Original Message ----*\r\n>>                                         *From:*\r\n>>                                         XYZ.pendencia04@EmpresaX.com.br\r\n>>                                         *To:* \"Fulano\"\r\n>>                                         (mary.anne@EmpresaX.com.br),\r\n>>                                         \"Roque Neto\"\r\n>>                                         (xyz.pendencias1@EmpresaX.com.br)\r\n>>                                         *Date:* Fri, 31 Mar 2017\r\n>>                                         08:56:52 -0300\r\n>>                                         *Subject:* BLA BLÁ BLA XYZ/ \r\n>>\t\t\t\t\t\t\t\t\t\t\tXYZ / TROCA DE PACOTES origem \r\n>>\t\t\t\t\t\t\t\t\t\t\tABC Destino \r\n>>\r\n>>                                             Bom dia !\r\n>>\r\n>>                                             Ciclano,\r\n>>\r\n>>\r\n>>                                             Favor nos posicionar\r\n>>                                             referente ao volume de\r\n>>                                             XYZ ,  pois o mesmo nos\r\n>>                                             cobra retorno COM URGÊNCIA.\r\n>>\r\n>>\r\n>>\r\n>>                                             Em 30/3/2017 08:24, Fulano\r\n>>                                             escreveu:\r\n>>\r\n>>                                                 Bom dia,\r\n>>\r\n>>                                                 Gabriela, agradeço o\r\n>>                                                 retorno.\r\n>>\r\n>>                                                 Ciclano, assim que\r\n>>                                                 possível nos\r\n>>                                                 posicionar frente ao\r\n>>                                                 envio do volume de XYZ.\r\n>>\r\n>>                                                 Obrigada.\r\n>>\r\n>>                                                 att,\r\n>>\r\n>>                                                 *Reinaldo Coelho *\r\n>>                                                 *Meu cargo atual*\r\n>>\r\n>>                                                 *Fone:(18) 2103-4777\r\n>>                                                 / Ramal: 725 **\r\n>>                                                 *Email:mary.anne@EmpresaX.com.br*\r\n>>\r\n>>                                                 *Acesse nosso\r\n>>                                                 site:www.EmpresaX.com.br*\r\n>>\r\n>>\r\n>>                                                 *\r\n>>                                                 Em 29/03/2017 10:35,\r\n>>                                                 Gabriela Xavier escreveu:\r\n>>\r\n>>                                                     Bom dia !\r\n>>\r\n>>                                                     Troca confirmada\r\n>>                                                     , os volumes de\r\n>>                                                     XYZ serão\r\n>>                                                     enviados hoje\r\n>>                                                     através dos RRIs\r\n>>                                                     35468888 /\r\n>>                                                     31755555 , devido\r\n>>                                                     a fiscalização.\r\n>>\r\n>>                                                     Gentileza\r\n>>                                                     acompanhar\r\n>>                                                     desembarque dos\r\n>>                                                     mesmos,  e nos\r\n>>                                                     enviar os nossos\r\n>>                                                     com urgência...\r\n>>\r\n>>\r\n>>\r\n>>\r\n>>                                                     Bueno ,\r\n>>\r\n>>                                                     Favor associar a\r\n>>                                                     devida viagem em\r\n>>                                                     sistema do SSAAS\r\n>>                                                     abaixo , hoje.\r\n>>\r\n>>\r\n>>\r\n>>\r\n>>\r\n>>\r\n>>\r\n>>\r\n>>\r\n>>                                                     -- \r\n>>\r\n>>                                                     Em 29/3/2017\r\n>>                                                     08:21, Fulano\r\n>>                                                     escreveu:\r\n>>\r\n>>                                                         Bom dia,\r\n>>\r\n>>                                                         Ok, por\r\n>>                                                         gentileza,\r\n>>                                                         assim que\r\n>>                                                         tiver\r\n>>                                                         informaçoes\r\n>>                                                         referente a\r\n>>                                                         este volume\r\n>>                                                         comunique por\r\n>>                                                         favor, para\r\n>>                                                         que a\r\n>>                                                         destroca seja\r\n>>                                                         efetuada o\r\n>>                                                         mais breve\r\n>>                                                         possivel.\r\n>>\r\n>>                                                         *Reinaldo Coelho *\r\n>>                                                         *Auxiliar\r\n>>                                                         Manutenção*\r\n>>\r\n>>                                                         *Fone:(18)\r\n>>                                                         2103-4777 /\r\n>>                                                         Ramal: 725 **\r\n>>                                                         *Email:mary.anne@EmpresaX.com.br*\r\n>>\r\n>>                                                         *Acesse nosso\r\n>>                                                         site:www.EmpresaX.com.br*\r\n>>\r\n>>\r\n>>                                                         *\r\n>>                                                         Em 28/03/2017\r\n>>                                                         17:50,\r\n>>                                                         Ciclano Ramos\r\n>>                                                         Conceição\r\n>>                                                         escreveu:\r\n>>\r\n>>                                                             ok\r\n>>\r\n>>                                                             já\r\n>>                                                             estamos\r\n>>                                                             verificando.\r\n>>\r\n>>\r\n>>                                                             -- \r\n>>                                                             *Ciclano\r\n>>                                                             Ramos\r\n>>                                                              Conceição*\r\n>>                                                             * Pendencia\r\n>>                                                             Fiscal*\r\n>>\r\n>>                                                             *Fone:(12)1111-6333*\r\n>>\r\n>>                                                             *Email:xyz.pendencias1@EmpresaX.com.br*\r\n>>\r\n>>                                                             *Acesse\r\n>>                                                             nosso\r\n>>                                                             site:www.EmpresaX.com.br*\r\n>>                                                             & amp;\r\n>>                                                             amp; lt; /p>\r\n>>\r\n>>                                                             *-----\r\n>>                                                             Original\r\n>>                                                             Message ----*\r\n>>                                                             *From:*\r\n>>                                                             mary.anne@EmpresaX.com.br\r\n>>                                                             *To:*\r\n>>                                                             \"Roque\r\n>>                                                             Neto\"\r\n>>                                                             (xyz.pendencias1@EmpresaX.com.br),\r\n>>                                                             \"Gabriela\r\n>>                                                             Xavier\"\r\n>>                                                             (XYZ.pendencia04@EmpresaX.com.br)\r\n>>                                                             *Cc:*\r\n>>                                                             \"Logística\r\n>>                                                             | Grupo\r\n>>                                                             Xamego\"\r\n>>                                                             (logistica@Xamego.com.br)\r\n>>                                                             *Date:*\r\n>>                                                             Tue, 28\r\n>>                                                             Mar 2017\r\n>>                                                             17:36:24\r\n>>                                                             -0300\r\n>>                                                             *Subject:*\r\n>>\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tBLA BLÁ BLA XYZ/ XYZ / \r\n>>\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tTROCA DE PACOTES origem ABC Destino \r\n>>\r\n>>                                                                 Boa\r\n>>                                                                 tarde\r\n>>                                                                 colegas,\r\n>>\r\n>>                                                                 Os\r\n>>                                                                 conhecimentos\r\n>>                                                                 citados\r\n>>                                                                 constam\r\n>>                                                                 em\r\n>>                                                                 entrega,\r\n>>                                                                 porem\r\n>>                                                                 houve\r\n>>                                                                 troca\r\n>>                                                                 de\r\n>>                                                                 etiquetagem\r\n>>                                                                 em\r\n>>                                                                 XYZ,\r\n>>                                                                 desta\r\n>>                                                                 forma,\r\n>>                                                                 peço\r\n>>                                                                 a\r\n>>                                                                 atenção\r\n>>                                                                 para\r\n>>                                                                 que\r\n>>                                                                 possamos\r\n>>                                                                 destrocar\r\n>>                                                                 o\r\n>>                                                                 mais\r\n>>                                                                 breve\r\n>>                                                                 possível\r\n>>\r\n>>                                                                 XYZ\r\n>>                                                                 322433,\r\n>>                                                                 CLIENTE\r\n>>                                                                 ME DE\r\n>>                                                                 S\r\n>>                                                                 CASTRO\r\n>>                                                                 (XYZ), \r\n>>                                                                 trocado\r\n>>                                                                 com o\r\n>>                                                                 AWB\r\n>>                                                                 383555,\r\n>>                                                                 CLIENTE\r\n>>                                                                 XE\r\n>>                                                                 MENTES(XYZ)\r\n>>\r\n>>                                                                 (O\r\n>>                                                                 cliente\r\n>>                                                                 XE\r\n>>                                                                 MENTES,\r\n>>                                                                 conforme\r\n>>                                                                 informaçoes,\r\n>>                                                                 devolveu\r\n>>                                                                 a\r\n>>                                                                 mercadoria\r\n>>                                                                 no\r\n>>                                                                 ato\r\n>>                                                                 da\r\n>>                                                                 entrega)\r\n>>\r\n>>                                                                 *Gabriela*\r\n>>                                                                 por\r\n>>                                                                 gentileza,\r\n>>                                                                 verificar\r\n>>                                                                 informação\r\n>>                                                                 de\r\n>>                                                                 devolução\r\n>>                                                                 para\r\n>>                                                                 que\r\n>>                                                                 se\r\n>>                                                                 possa\r\n>>                                                                 enviar\r\n>>                                                                 o\r\n>>                                                                 volume\r\n>>                                                                 para\r\n>>                                                                 XYZ,\r\n>>                                                                 por\r\n>>                                                                 gentileza.\r\n>>\r\n>>                                                                 *Anislei,*\r\n>>                                                                 não\r\n>>                                                                 temos\r\n>>                                                                 informaçoes\r\n>>                                                                 de\r\n>>                                                                 que o\r\n>>                                                                 cliente\r\n>>                                                                 de\r\n>>                                                                 XYZ,\r\n>>                                                                 ja\r\n>>                                                                 verificou\r\n>>                                                                 o\r\n>>                                                                 erro,\r\n>>                                                                 desta\r\n>>                                                                 forma,\r\n>>                                                                 peço,\r\n>>                                                                 entrar\r\n>>                                                                 em\r\n>>                                                                 contato\r\n>>                                                                 com a\r\n>>                                                                 tripulação\r\n>>                                                                 para\r\n>>                                                                 que a\r\n>>                                                                 entrega\r\n>>                                                                 não\r\n>>                                                                 seja\r\n>>                                                                 finalizada,\r\n>>                                                                 e\r\n>>                                                                 encaminhar\r\n>>                                                                 o\r\n>>                                                                 volume\r\n>>                                                                 para XYZ\r\n>>\r\n>>                                                                 Fico\r\n>>                                                                 no\r\n>>                                                                 aguardo,\r\n>>                                                                 pois\r\n>>                                                                 trata-se\r\n>>                                                                 de um\r\n>>                                                                 cliente\r\n>>                                                                 monitorado\r\n>>\r\n>>                                                                 *Valéria\r\n>>                                                                 Coelho *\r\n>>                                                                 *Auxiliar\r\n>>                                                                 Manutenção*\r\n>>\r\n>>\r\n>>                                                                 *Fone:(11)\r\n>>                                                                 2100-4777\r\n>>                                                                 /\r\n>\r\n\r\n", result);
        //    Assert.IsTrue(string.IsNullOrWhiteSpace(message.BodyHtml.Text));
        //}

        [Test(Description = "")]
        public void MustParseEml8BitCharsetWindows1252()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\content-transfer-encode-8bit-charset-windows-1252.eml");
            Assert.AreEqual("e309e334-44b3-9e10-058d-c4cdbacae79c@examplehotel.com.br", message.MessageId);
            Assert.AreEqual("Re: Solicitação de Reserva - Josh Ronald Moreira Jr.", message.Subject);
            Assert.IsFalse(string.IsNullOrWhiteSpace(message.BodyText.Text));
            var result = message.BodyText.Text;
            Assert.IsFalse(string.IsNullOrWhiteSpace(message.BodyHtml.Text));
        }

        [Test(Description = "")]
        public void MustParseEml8BitCharsetUtf8NewLined()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\content-transfer-encode-8bit-charset-utf8-new-lined.eml");
            Assert.AreEqual("a33787b3-fdf1-47cc-8985-b4d34b0a3485@xtgap4s7mta1215.xt.local", message.MessageId);
            Assert.AreEqual("ATENÇÃO: Ofertas que vão te surpreender. Troque seus pontos!", message.Subject);
            Assert.IsFalse(string.IsNullOrWhiteSpace(message.BodyText.Text));
            var result = message.BodyText.Text;
            Assert.IsFalse(string.IsNullOrWhiteSpace(message.BodyHtml.Text));
        }

        [Test(Description = "")]
        public void MustParseEmlFromIphone()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\html_multipart_email_with_more_than_one_subpart.eml");
            Assert.AreEqual("B82DBC10-2267-41A9-B3CC-A019752FE6E1@gmail.com", message.MessageId);
            Assert.AreEqual("RES: Email multipart related multilevel", message.Subject);
            Assert.IsFalse(string.IsNullOrWhiteSpace(message.BodyText.Text));
            var textMessageExpected = @"Hi,

I?m the first step of message to test.

Sender
Helper
E-mail: sender@sender.com
Web: www.sender.com<http://www.sender.com/>

";
            var resultText = message.BodyText.Text;
            Assert.AreEqual(textMessageExpected, resultText);
            Assert.IsFalse(string.IsNullOrWhiteSpace(message.BodyHtml.Text));
            var htmlMessageExpected = @"<html><body><div><h1>Part 1</h1></div></body></html>

<html><body dir=""auto""><head><meta http-equiv=""content-type"" content=""text/html; charset=us-ascii""></head><blockquote type=""cite""><div></div></blockquote><blockquote type=""cite""><div><meta http-equiv=""content-type"" content=""text/html; charset=us-ascii""><blockquote type=""cite""><div></div></blockquote><blockquote type=""cite""><div></div></blockquote></div></blockquote><blockquote type=""cite""><div></div></blockquote></body></html>
<html><body dir=""auto""><head><meta http-equiv=""content-type"" content=""text/html; charset=us-ascii""></head><blockquote type=""cite""><div></div></blockquote><blockquote type=""cite""><div><meta http-equiv=""content-type"" content=""text/html; charset=us-ascii""><blockquote type=""cite""><div></div></blockquote><blockquote type=""cite""><div></div></blockquote></div></blockquote><blockquote type=""cite""><div></div></blockquote></body></html>
<html><head><meta http-equiv=""content-type"" content=""text/html; charset=us-ascii""></head><body dir=""auto""><blockquote type=""cite""><div></div></blockquote><blockquote type=""cite""><div><meta http-equiv=""content-type"" content=""text/html; charset=us-ascii""><blockquote type=""cite""><div></div></blockquote></div></blockquote></body></html>";
            var resultHtml = message.BodyHtml.Text;
            Assert.AreEqual(htmlMessageExpected, resultHtml);
        }

    }
}
