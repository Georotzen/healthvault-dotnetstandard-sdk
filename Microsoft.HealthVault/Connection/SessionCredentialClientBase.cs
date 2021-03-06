﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Microsoft.HealthVault.Configuration;
using Microsoft.HealthVault.Helpers;
using Microsoft.HealthVault.Transport;

namespace Microsoft.HealthVault.Connection
{
    internal abstract class SessionCredentialClientBase : ISessionCredentialClient
    {
        public IConnectionInternal Connection { get; set; }

        public async Task<SessionCredential> GetSessionCredentialAsync(CancellationToken token)
        {
            if (Connection == null)
            {
                throw new NotSupportedException($"{nameof(Connection)} is required");
            }

            HealthServiceResponseData responseData = await Connection
                .ExecuteAsync(HealthVaultMethods.CreateAuthenticatedSessionToken, 2, ConstructCreateTokenInfoXml())
                .ConfigureAwait(false);

            return GetSessionCredential(responseData);
        }

        public virtual string ConstructCreateTokenInfoXml()
        {
            StringBuilder infoXml = new StringBuilder(128);
            XmlWriterSettings settings = SDKHelper.XmlUnicodeWriterSettings;

            using (XmlWriter writer = XmlWriter.Create(infoXml, settings))
            {
                // Add the PersonInfo elements
                writer.WriteStartElement("auth-info");

                ConstructCreateTokenInfoXmlAppIdPart(writer);

                writer.WriteStartElement("credential");
                WriteInfoXml(writer);
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.Flush();
            }

            return infoXml.ToString();
        }

        private SessionCredential GetSessionCredential(HealthServiceResponseData responseData)
        {
            if (responseData == null)
            {
                throw new ArgumentNullException($"{nameof(responseData)}");
            }

            SessionCredential sessionCredential = GetAuthenticationToken(responseData.InfoNavigator);

            // TODO: Update with returned expiry when #55406 is completed. 4h is the default HealthVault token expiry time
            sessionCredential.ExpirationUtc = DateTimeOffset.UtcNow.AddHours(4);

            return sessionCredential;
        }

        /// <summary>
        /// Extracts the authentication token from the response XML.
        /// </summary>
        ///
        ///
        /// <param name="nav">
        /// The path to the token.
        /// </param>
        ///
        private SessionCredential GetAuthenticationToken(
            XPathNavigator nav)
        {
            SessionCredential sessionCredential = new SessionCredential();

            XPathExpression authTokenPath = GetAuthTokenXPath(nav);
            XPathNodeIterator navTokenIterator = nav.Select(authTokenPath);

            GetTokenByParseResponse(navTokenIterator, sessionCredential);

            XPathNavigator sharedSecret = nav.SelectSingleNode("shared-secret");
            sessionCredential.SharedSecret = sharedSecret.Value;

            return sessionCredential;
        }

        private XPathExpression GetAuthTokenXPath(XPathNavigator infoNav)
        {
            return GetTokenXPathExpression(infoNav, "/wc:info/token");
        }

        private XPathExpression GetTokenXPathExpression(
            XPathNavigator infoNav,
            string xPath)
        {
            XPathExpression infoXPathExp = XPathExpression.Compile(xPath);

            XmlNamespaceManager infoXmlNamespaceManager =
                new XmlNamespaceManager(infoNav.NameTable);

            // TODO: Learn more about these XPath expressions
            // This constant used to be a less obvious constant (get-only property), and I'm not sure
            // when there was ever a chance for it to be anything other than the string it's testing for.
            string nsName = HealthVaultMethods.CreateAuthenticatedSessionToken.ToString();
            if (nsName == "CreateAuthenticatedSessionToken")
            {
                nsName = "CreateAuthenticatedSessionToken2";
            }

            infoXmlNamespaceManager.AddNamespace(
                "wc",
                "urn:com.microsoft.wc.methods.response." + nsName);

            infoXPathExp.SetContext(infoXmlNamespaceManager);

            return infoXPathExp;
        }

        private static void GetTokenByParseResponse(
            XPathNodeIterator navTokenIterator,
            SessionCredential sessionCredential)
        {
            foreach (XPathNavigator tokenNav in navTokenIterator)
            {
                sessionCredential.Token = tokenNav.Value;
            }
        }

        public virtual void ConstructCreateTokenInfoXmlAppIdPart(XmlWriter writer)
        {
            Validator.ThrowIfArgumentNull(writer, nameof(writer), Resources.WriteXmlNullWriter);
            writer.WriteStartElement("app-id");

            var healthApplicationConfiguration = Ioc.Get<HealthVaultConfiguration>();

            if (healthApplicationConfiguration.IsMultiRecordApp)
            {
                writer.WriteAttributeString("is-multi-record-app", "true");
            }

            writer.WriteValue(Connection.ApplicationId.ToString());
            writer.WriteEndElement();
        }

        public abstract void WriteInfoXml(XmlWriter writer);
    }
}
