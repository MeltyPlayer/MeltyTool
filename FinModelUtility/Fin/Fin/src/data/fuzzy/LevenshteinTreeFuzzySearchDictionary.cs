using System;
using System.Collections.Generic;
using System.Linq;

using fin.data.dictionaries;

namespace fin.data.fuzzy;

public class
    LevenshteinTreeFuzzySearchDictionary<T> : IFuzzySearchDictionary<T> {
  private readonly ListDictionary<T, string> dataToKeywords_ = new();

  public void Add(string keyword, T associatedData)
    => this.dataToKeywords_.Add(associatedData, keyword.ToLower());

  public IEnumerable<IFuzzySearchResult<T>> Search(
      string filterText,
      float minMatchPercentage) {
    var filterTokens = filterText.ToLower().Split(' ');

    var dataChangeDistancesAndSimilarities = this.dataToKeywords_.GetPairs().Select(
        pair => {
          var (data, keywords) = pair;
          var tokenChangeDistancesAndSimilarities = filterTokens.Select(
                  token => {
                    var changeDistanceAndSimilarities =
                        keywords.Select(keyword => {
                                  var changeDistance =
                                      token.Length -
                                      this.FindLongestLengthOfSubstring_(
                                          keyword, token);
                                  var addOrRemoveDistance =
                                      Math.Abs(
                                          keyword.Length - token.Length) -
                                      changeDistance;

                                  var editDistance = changeDistance +
                                      addOrRemoveDistance;
                                  var difference = 1f *
                                                   editDistance /
                                                   Math.Max(keyword.Length, token.Length);
                                  var similarity = 1 - difference;

                                  return new ChangeDistanceAndSimilarity(
                                      changeDistance, similarity);
                                })
                                .ToList();

                    changeDistanceAndSimilarities.Sort((lhs, rhs) => {
                      var (lhsChangeDistance, lhsSimilarity) = lhs;
                      var (rhsChangeDistance, rhsSimilarity) = rhs;

                      if (lhsChangeDistance == rhsChangeDistance) {
                        return -lhsSimilarity.CompareTo(rhsSimilarity);
                      }

                      return lhsChangeDistance.CompareTo(rhsChangeDistance);
                    });

                    return changeDistanceAndSimilarities.First();
                  })
              .ToList();

          var merged = tokenChangeDistancesAndSimilarities
              .Aggregate(
                  (total, current) => new ChangeDistanceAndSimilarity(
                      total.ChangeDistance + current.ChangeDistance,
                      total.Similarity * current.Similarity));
                
          return new SimilarityInfo(data, merged.ChangeDistance, merged.Similarity);
        }).ToList();

    return dataChangeDistancesAndSimilarities
        .Select(similarityInfo => new FuzzySearchResult(similarityInfo.Data, similarityInfo.ChangeDistance, similarityInfo.Similarity));
  }

  private record ChangeDistanceAndSimilarity(int ChangeDistance,
                                             float Similarity);

  private record SimilarityInfo(T Data,
                                int ChangeDistance,
                                float Similarity) :
      ChangeDistanceAndSimilarity(ChangeDistance, Similarity);

  private record FuzzySearchResult
  (T Data,
   int ChangeDistance,
   float Similarity) : IFuzzySearchResult<T>;

  private int
      FindLongestLengthOfSubstring_(string container, string substring) {
    var substringLength = substring.Length;
    var matchLengths = new int[substringLength];

    var bestMatchLength = 0;
    for (var containerIndex = 0;
         containerIndex < container.Length;
         ++containerIndex) {
      var containerChar = container[containerIndex];

      for (var substringIndex = 0;
           substringIndex < substringLength;
           ++substringIndex) {
        var i = (containerIndex + substringIndex) % substringLength;
        var substringChar = substring[i];

        var matchLength = i == 0 ? 0 : matchLengths[substringIndex];

        matchLength += substringChar == containerChar ? 1 : 0;

        matchLengths[substringIndex] = matchLength;

        bestMatchLength = Math.Max(bestMatchLength, matchLength);
        if (bestMatchLength == substringLength) {
          return bestMatchLength;
        }
      }
    }

    return bestMatchLength;
  }
}