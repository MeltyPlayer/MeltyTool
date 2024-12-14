using System;
using System.Web;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.io.web;

public class GitHubUtilTests {
  [Test]
  public void TestGetNewIssueUrl() {
    try {
      this.SomeMethod1_(0);
    } catch (Exception e) {
      var issueUrl = GitHubUtil.GetNewIssueUrl(e);
      var parsedQueryString
          = HttpUtility.ParseQueryString(issueUrl.Split('?')[1]);

      Assert.AreEqual(
          """
          **Stack trace**
          ```
          System.NotImplementedException: Foobar
              at fin.io.web.GitHubUtilTests.SomeMethod2_(System.String message) in //FinModelUtility/Fin/Fin Tests/io/web/GitHubUtilTests.cs:line 49
              at fin.io.web.GitHubUtilTests.SomeMethod1_<T>(T value) in //FinModelUtility/Fin/Fin Tests/io/web/GitHubUtilTests.cs:line 46
              at fin.io.web.GitHubUtilTests.TestGetNewIssueUrl() in //FinModelUtility/Fin/Fin Tests/io/web/GitHubUtilTests.cs:line 14
          ```
          
          **To Reproduce**
          Steps to reproduce the behavior:
          1. Go to '...'
          2. Click on '....'
          3. Scroll down to '....'
          4. See error

          **Additional context**
          Add any other context about the problem here.

          """,
          parsedQueryString["body"]);
      return;
    }

    Assert.Fail();
  }

  private void SomeMethod1_<T>(T value) => this.SomeMethod2_("Foobar");

  private void SomeMethod2_(string message)
    => throw new NotImplementedException(message);
}