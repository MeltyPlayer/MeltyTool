using System;
using System.Web;

using fin.io.bundles;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;


namespace fin.io.web;

public record MockGameAndLocalPath(string GameName, string LocalPath)
    : IGameAndLocalPath;

public class GitHubUtilTests {
  private void SomeMethod1_<T>(T value) => this.SomeMethod2_("Foobar");

  private void SomeMethod2_(string message)
    => throw new NotImplementedException(message);

  [Test]
  public void TestGetNewIssueUrlWithoutContext() {
    try {
      this.SomeMethod1_(0);
    } catch (Exception e) {
      var issueUrl = GitHubUtil.GetNewIssueUrl(e, null);
      var parsedQueryString
          = HttpUtility.ParseQueryString(issueUrl.Split('?')[1]);

      Assert.AreEqual("[Enhancement] (Enter a description)",
                      parsedQueryString["title"]);
      Assert.AreEqual(
          """
          **Stack trace**
          ```
          System.NotImplementedException: Foobar
              at fin.io.web.GitHubUtilTests.SomeMethod2_(System.String message) in //FinModelUtility/Fin/Fin Tests/io/web/GitHubUtilTests.cs:line 20
              at fin.io.web.GitHubUtilTests.SomeMethod1_<T>(T value) in //FinModelUtility/Fin/Fin Tests/io/web/GitHubUtilTests.cs:line 17
              at fin.io.web.GitHubUtilTests.TestGetNewIssueUrlWithoutContext() in //FinModelUtility/Fin/Fin Tests/io/web/GitHubUtilTests.cs:line 25
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

  [Test]
  public void TestGetNewIssueUrlWithContext() {
    try {
      this.SomeMethod1_(0);
    } catch (Exception e) {
      var issueUrl
          = GitHubUtil.GetNewIssueUrl(
              e,
              new LoadFileBundleExceptionContext(
                  new MockGameAndLocalPath("mario_game", @"foo\bar\file.txt")));
      var parsedQueryString
          = HttpUtility.ParseQueryString(issueUrl.Split('?')[1]);

      Assert.AreEqual(@"[Bug] Failed to load mario_game\foo\bar\file.txt",
                      parsedQueryString["title"]);
      Assert.AreEqual(
          """
          **Stack trace**
          ```
          System.NotImplementedException: Foobar
              at fin.io.web.GitHubUtilTests.SomeMethod2_(System.String message) in //FinModelUtility/Fin/Fin Tests/io/web/GitHubUtilTests.cs:line 20
              at fin.io.web.GitHubUtilTests.SomeMethod1_<T>(T value) in //FinModelUtility/Fin/Fin Tests/io/web/GitHubUtilTests.cs:line 17
              at fin.io.web.GitHubUtilTests.TestGetNewIssueUrlWithContext() in //FinModelUtility/Fin/Fin Tests/io/web/GitHubUtilTests.cs:line 64
          ```

          **To Reproduce**
          Steps to reproduce the behavior:

          1. Attempted to load mario_game\foo\bar\file.txt.

          **Additional context**
          Add any other context about the problem here.
          """,
          parsedQueryString["body"]);
      return;
    }

    Assert.Fail();
  }
}