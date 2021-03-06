// Copyright 2011, Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Author: api.anash@gmail.com (Anash P. Oommen)

using Google.Api.Ads.Common.Lib;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Google.Api.Ads.Dfa.Lib {
  /// <summary>
  /// The factory class for all DFA API services.
  /// </summary>
  public class DfaServiceFactory : ServiceFactory {
    /// <summary>
    /// Default public constructor.
    /// </summary>
    public DfaServiceFactory() {
    }

    /// <summary>
    /// Create a service object.
    /// </summary>
    /// <param name="signature">Signature of the service being created.</param>
    /// <param name="user">The user for which the service is being created.
    /// <param name="serverUrl">The server to which the API calls should be
    /// made.</param>
    /// </param>
    /// <returns>An object of the desired service type.</returns>
    public override AdsClient CreateService(ServiceSignature signature, AdsUser user,
        Uri serverUrl) {
      DfaAppConfig config = (DfaAppConfig) base.Config;

      if (serverUrl == null) {
        serverUrl = new Uri(config.DfaApiServer);
      }

      if (user == null) {
        throw new ArgumentNullException("user");
      }

      CheckServicePreconditions(signature);

      DfaServiceSignature dfaapiSignature = signature as DfaServiceSignature;

      AdsClient service = (AdsClient) Activator.CreateInstance(dfaapiSignature.ServiceType);

      if (config.Proxy != null) {
        service.Proxy = config.Proxy;
      }

      service.Timeout = config.Timeout;
      service.Url = string.Format("{0}{1}/api/dfa-api/{2}",
          serverUrl, dfaapiSignature.Version, dfaapiSignature.ServiceEndpoint);
      service.UserAgent = config.GetUserAgent();

      service.User = user;
      service.Signature = signature;

      service.GetType().GetProperty("RequestHeader").SetValue(service,
          GetRequestHeader(), null);
      SetRequestHeaderNameSpace(signature as DfaServiceSignature, service);

      return service;
    }

    /// <summary>
    /// Gets the request header.
    /// </summary>
    /// <returns>The request header.</returns>
    private RequestHeader GetRequestHeader() {
      DfaAppConfig config = (DfaAppConfig) base.Config;
      RequestHeader reqHeader = new RequestHeader();
      reqHeader.ApplicationName = config.GetUserAgent();
      return reqHeader;
    }

    /// <summary>
    /// Sets the request header namespace in outgoing Soap Requests.
    /// </summary>
    /// <param name="signature">The service creation parameters.</param>
    /// <param name="service">The service object for which RequestHeader
    /// needs to be patched.</param>
    private static void SetRequestHeaderNameSpace(DfaServiceSignature signature,
        AdsClient service) {
      // Set the header namespace prefix. For all /cm services, the members
      // shouldn't have xmlns. For all other services, the members should have
      // /cm as xmlns.
      object[] attributes = service.GetType().GetCustomAttributes(false);
      foreach (object attribute in attributes) {
        if (attribute is WebServiceBindingAttribute) {
          WebServiceBindingAttribute binding = (WebServiceBindingAttribute) attribute;

          string xmlns = binding.Namespace;
          RequestHeader svcRequestHeader = null;
          PropertyInfo propInfo = service.GetType().GetProperty("RequestHeader");
          if (propInfo != null) {
            svcRequestHeader = (RequestHeader) propInfo.GetValue(service, null);

            if (svcRequestHeader != null) {
              PropertyInfo wsPropInfo = svcRequestHeader.GetType().GetProperty("TargetNamespace");
              if (wsPropInfo != null) {
                wsPropInfo.SetValue(svcRequestHeader, xmlns, null);
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Reads the headers from App.config.
    /// </summary>
    /// <param name="config">The configuration class.</param>
    protected override void ReadHeadersFromConfig(AppConfig config) {
      // nothing to do here.
    }

    /// <summary>
    /// Checks preconditions of the service signature and throws and exception if the service
    /// cannot be generated.
    /// </summary>
    /// <param name="signature">the service signature for generating the service</param>
    protected override void CheckServicePreconditions(ServiceSignature signature) {
      if (signature == null) {
        throw new ArgumentNullException("signature");
      }

      if (!(signature is DfaServiceSignature)) {
        throw new InvalidCastException(string.Format(CultureInfo.InvariantCulture,
            DfaErrorMessages.SignatureIsOfWrongType, typeof(DfaServiceSignature)));
      }
    }
  }
}
