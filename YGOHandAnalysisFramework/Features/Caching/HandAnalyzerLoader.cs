using System.Text.Json;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;

namespace YGOHandAnalysisFramework.Features.Caching
{
    public abstract class HandAnalyzerLoader<TCardGroup, TCardGroupName>
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        internal (bool Success, HandAnalyzer<TCardGroup, TCardGroupName>? Result) TryLoadHandAnalyzer(HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> buildArgs)
        {

            if (TryLoadFromCache($"{buildArgs.AnalyzerName}.json", buildArgs) is (true, HandAnalyzer<TCardGroup, TCardGroupName> cached))
            {
                return (true, cached);
            }

            return (false, null);
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

        private (bool Success, HandAnalyzer<TCardGroup, TCardGroupName>? Result) TryLoadFromCache(string filePath, HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> buildArgs)
        {
            static (bool Success, HandAnalyzerCacheDTO? Result) Deserialize(string filePath)
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var result = JsonSerializer.Deserialize<HandAnalyzerCacheDTO>(stream);

                if (result != null)
                {
                    return (true, result);
                }

                return (false, null);
            }

            if (!File.Exists(filePath))
            {
                return (false, null);
            }

            if (Deserialize(filePath) is not (true, HandAnalyzerCacheDTO cache))
            {
                return (false, null);
            }

            if(cache.HandSize != buildArgs.HandSize)
            {
                return (false, null);
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

            var cacheSet = new HashSet<CardGroupDTO>(cache.CardGroups, CardComparer.Instance);
            var cardGroupsSet = new HashSet<CardGroupDTO>(cardGroups, CardComparer.Instance);

            if (!cacheSet.SetEquals(cardGroupsSet))
            {
                return (false, null);
            }

            var handCombinations = new HashSet<HandCombination<TCardGroupName>>();

            foreach (var hand in cache.HandCombinations)
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

            return (true, new HandAnalyzer<TCardGroup, TCardGroupName>(buildArgs, handCombinations));
        }

        protected abstract TCardGroupName ConvertCardNameFromString(string cardName);
        protected abstract string ConvertCardNameToString(TCardGroupName cardName);
    }
}
