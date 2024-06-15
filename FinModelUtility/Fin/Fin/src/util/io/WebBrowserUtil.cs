using System;
using System.Diagnostics;

using fin.util.asserts;

namespace fin.util.io {
  public static class WebBrowserUtil {
    public static void OpenUrl(string urlString) {
      Asserts.True(Uri.IsWellFormedUriString(urlString, UriKind.Absolute));
      Asserts.True(Uri.TryCreate(urlString, UriKind.Absolute, out var url));
      OpenUrl(url!);
    }

    public static void OpenUrl(Uri url) {
      Asserts.True(url.IsWellFormedOriginalString());
      Asserts.True(url.Scheme == Uri.UriSchemeHttp ||
                   url.Scheme == Uri.UriSchemeHttps);

      Process.Start(url.ToString());
    }
  }
}