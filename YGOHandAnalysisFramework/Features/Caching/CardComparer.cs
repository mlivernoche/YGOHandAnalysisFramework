namespace YGOHandAnalysisFramework.Features.Caching
{
    internal sealed class CardComparer : IEqualityComparer<CardGroupDTO>
    {
        public static IEqualityComparer<CardGroupDTO> Instance { get; } = new CardComparer();

        bool IEqualityComparer<CardGroupDTO>.Equals(CardGroupDTO? x, CardGroupDTO? y)
        {
            if (x is null)
            {
                return false;
            }

            if (y is null)
            {
                return false;
            }

            return
                x.CardName.Equals(y.CardName) &&
                x.Size == y.Size &&
                x.Minimum == y.Minimum &&
                x.Maximum == y.Maximum;
        }

        int IEqualityComparer<CardGroupDTO>.GetHashCode(CardGroupDTO obj)
        {
            return HashCode.Combine(obj.CardName, obj.Size, obj.Minimum, obj.Maximum);
        }
    }
}
