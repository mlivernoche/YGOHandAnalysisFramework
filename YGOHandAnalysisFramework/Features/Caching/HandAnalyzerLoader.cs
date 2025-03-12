using SharpCompress.Common;
using System.Text.Json;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Data.Extensions.Json;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;

namespace YGOHandAnalysisFramework.Features.Caching
{
    public abstract class HandAnalyzerLoader<TCardGroup, TCardGroupName>
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        internal Result<HandAnalyzer<TCardGroup, TCardGroupName>, Exception> TryLoadHandAnalyzer(HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> buildArgs)
        {
            using var source = LoadFromCache(buildArgs);
            if (!TryLoadFromCache(source, buildArgs).GetResult(out var handAnalyzer, out var error))
            {
                return new(error);
            }

            return new(handAnalyzer);
        }

        internal void CreateCache(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer)
        {
            var cardGroups = new List<CardGroupDTO>();
            {
                foreach (var cardGroup in handAnalyzer.CardGroups.Values)
                {
                    cardGroups.Add(new CardGroupDTO
                    {
                        CardName = ConvertCardNameToString(cardGroup.Name),
                        Size = cardGroup.Size,
                        Minimum = cardGroup.Minimum,
                        Maximum = cardGroup.Maximum,
                    });
                }
            }

            var handCombinations = new List<HandCombinationDTO>();
            {
                foreach (var hand in handAnalyzer.Combinations)
                {
                    var cardNames = new List<HandElementDTO>();

                    foreach (var element in hand.GetAllHandElements())
                    {
                        cardNames.Add(new HandElementDTO
                        {
                            HandName = ConvertCardNameToString(element.HandName),
                            MinimumSize = element.MinimumSize,
                            MaximumSize = element.MaximumSize,
                        });
                    }

                    handCombinations.Add(new HandCombinationDTO
                    {
                        CardNames = [.. cardNames]
                    });
                }
            }

            var cache = new HandAnalyzerCacheDTO
            {
                HandSize = handAnalyzer.HandSize,
                CardGroups = [.. cardGroups],
                HandCombinations = [.. handCombinations],
            };

            var json = JsonSerializer.Serialize(cache);
            File.WriteAllText($"{handAnalyzer.AnalyzerName}.json", json);
        }

        internal void CreateCache(IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> handAnalyzers)
        {
            foreach (var handAnalyzer in handAnalyzers)
            {
                CreateCache(handAnalyzer);
            }
        }

        private Result<HandAnalyzer<TCardGroup, TCardGroupName>, Exception> TryLoadFromCache(Stream source, HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> buildArgs)
        {
            if (!JsonExtensions.TryParseValue<HandAnalyzerCacheDTO>(source).GetResult(out var handAnalyzerCache, out var jsonError))
            {
                return new(jsonError);
            }

            if(handAnalyzerCache.HandSize != buildArgs.HandSize)
            {
                return new(new Exception($"Hand sizes don't match: {handAnalyzerCache.HandSize} ({nameof(handAnalyzerCache)}) vs {buildArgs.HandSize} ({nameof(buildArgs)})."));
            }

            var cardGroups = new List<CardGroupDTO>();
            var cardList = CardList.Create<TCardGroup, TCardGroupName>(buildArgs.CardGroups);

            foreach (var card in cardList)
            {
                cardGroups.Add(new CardGroupDTO
                {
                    CardName = ConvertCardNameToString(card.Name) ?? throw new NullReferenceException($"Card has no name."),
                    Size = card.Size,
                    Minimum = card.Minimum,
                    Maximum = card.Maximum,
                });
            }

            var cacheSet = new HashSet<CardGroupDTO>(handAnalyzerCache.CardGroups, CardComparer.Instance);
            var cardGroupsSet = new HashSet<CardGroupDTO>(cardGroups, CardComparer.Instance);

            if (!cacheSet.SetEquals(cardGroupsSet))
            {
                return new(new Exception($"Cached ({nameof(handAnalyzerCache)}) CardGroups do not match with provided ({nameof(buildArgs)}) CardGroups."));
            }

            var handCombinations = new HashSet<HandCombination<TCardGroupName>>();

            foreach (var hand in handAnalyzerCache.HandCombinations)
            {
                var elements = new HashSet<HandElement<TCardGroupName>>();
                foreach (var cardName in hand.CardNames)
                {
                    elements.Add(new HandElement<TCardGroupName>()
                    {
                        HandName = ConvertCardNameFromString(cardName.HandName),
                        MinimumSize = cardName.MinimumSize,
                        MaximumSize = cardName.MaximumSize,
                    });
                }

                handCombinations.Add(new HandCombination<TCardGroupName>(elements));
            }

            return new(new HandAnalyzer<TCardGroup, TCardGroupName>(buildArgs, handCombinations));
        }

        protected virtual Stream LoadFromCache(HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> buildArgs)
        {
            return new FileStream($"cache/{buildArgs.AnalyzerName}.json", FileMode.Open, FileAccess.Read);
        }

        protected abstract TCardGroupName ConvertCardNameFromString(string cardName);
        protected abstract string ConvertCardNameToString(TCardGroupName cardName);
    }
}
