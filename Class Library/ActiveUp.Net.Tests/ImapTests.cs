﻿using System;
using ActiveUp.Net.Mail;
using ActiveUp.Net.Security;
using NUnit.Framework;

namespace ActiveUp.Net.Tests
{
    public class ImapTests
    {
        private const string _imapLogin = "[login]";
        private const string _imapPassword = "[password]";
        private const int _imapPort = 993;
        private const string _imapServerAddress = "imap.gmail.com";

        [Test, Ignore("Manual tests")]
        public void GetGmailLabels()
        {
            var client = new Imap4Client();
            client.ConnectSsl(_imapServerAddress, _imapPort);
            client.Login(_imapLogin, _imapPassword);
            var inbox = client.SelectMailbox("ToImport");

            var message = inbox.Fetch.MessageObjectWithGMailExtensions(1);

            message.HeaderFields["X-GM-LABELS"].ShouldEqual(@"""\\Important""");
            message.HeaderFields["X-GM-MSGID"].ShouldEqual(@"13c48ecc5057ff3c");
            message.HeaderFields["X-MSG-UID"].ShouldEqual(@"151");
            var toto = message.ToMimeString();
        }

        [Test, Ignore("Manual tests")]
        public void PeekAtGmailLabels()
        {
            var client = new Imap4Client();
            client.ConnectSsl(_imapServerAddress, _imapPort);
            client.Login(_imapLogin, _imapPassword);
            var inbox = client.SelectMailbox("ToImport");

            var message = inbox.Fetch.MessageObjectPeekWithGMailExtensions(1);

            message.HeaderFields["X-GM-LABELS"].ShouldEqual(@"""\\Important""");
            message.HeaderFields["X-GM-MSGID"].ShouldEqual(@"13c48ecc5057ff3c");
            message.HeaderFields["X-MSG-UID"].ShouldEqual(@"151");
        } 
    }
}