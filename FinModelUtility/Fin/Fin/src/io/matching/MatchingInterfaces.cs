using System.Collections.Generic;

namespace fin.io.matching;
/*public interface IMultiMatcher {
  IEnumerable<IReadOnlyGenericFile> Each(IMatchPattern pattern);
  IEnumerable<IReadOnlyGenericFile> All(IMatchPattern pattern);
  IReadOnlyGenericFile One(IMatchPattern pattern);
}*/

// Exact types
public interface IExactMatcher {
  string Text { get; }
}

// Pattern types
public interface IPatternMatcher {
  IReadOnlyList<IMatchSegment> Text { get; }
}

public interface IMatchSegment;

// Directory matchers
public interface IDirectoryMatcher<TDirectory, TFile>
    where TDirectory : IDirectoryMatcher<TDirectory, TFile>
    where TFile : IFileMatcher<TDirectory, TFile> {
  TFile MatchFileExact(string text);
  IPatternDirectoryMatcher MatchFilePattern(string pattern);

  TDirectory MatchDirectoryExact(string text);
  IPatternDirectoryMatcher MatchDirectoryPattern(string pattern);
}

public interface IExactDirectoryMatcher
    : IDirectoryMatcher<IExactDirectoryMatcher, IExactFileMatcher>,
      IExactMatcher;

public interface IPatternDirectoryMatcher
    : IDirectoryMatcher<IPatternDirectoryMatcher, IPatternFileMatcher>;

// File matchers
public interface IFileMatcher<TDirectory, TFile>
    where TDirectory : IDirectoryMatcher<TDirectory, TFile>
    where TFile : IFileMatcher<TDirectory, TFile>;

public interface IExactFileMatcher
    : IFileMatcher<IExactDirectoryMatcher, IExactFileMatcher>,
      IExactMatcher;

public interface IPatternFileMatcher
    : IFileMatcher<IPatternDirectoryMatcher, IPatternFileMatcher>;