using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YGOHandAnalysisFramework.Data;

namespace YGOHandAnalysisFramework.Projects.PotOfProsperity
{
    public interface IProsperityTargetCardGroup<TCardGroupName> : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        bool IsProsperityTarget { get; }
    }
}
