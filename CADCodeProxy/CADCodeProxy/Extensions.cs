using CADCode;
using CADCodeProxy.Enums;
using CADCodeProxy.Results;

namespace CADCodeProxy.CADCodeProxy;

internal static class Extensions {

    internal static IEnumerable<string> AsEnumerable(this StringCollection stringCollection) {
        foreach (var val in stringCollection) {
            if (val is string str)
                yield return str;
        }
    }

    internal static UnplacedPart[] GetUnplacedParts(this CADCodePanelOptimizerClass optimizer) {
        return optimizer.GetPartsNotPlaced()
                 .AsEnumerable()
                 .GroupBy(name => name)
                 .Select(group => new UnplacedPart() {
                     PartName = group.Key,
                     Qty = group.Count()
                 })
                 .ToArray();
    }

    internal static OffsetTypes AsCCOffset(this Offset offset) => offset switch {
        Offset.None => OffsetTypes.CC_OFFSET_NONE,
        Offset.Center => OffsetTypes.CC_OFFSET_CENTERLINE,
        Offset.Left => OffsetTypes.CC_OFFSET_LEFT,
        Offset.Right => OffsetTypes.CC_OFFSET_RIGHT,
        Offset.Inside => OffsetTypes.CC_OFFSET_INSIDE,
        Offset.Outside => OffsetTypes.CC_OFFSET_OUTSIDE,
        _ => throw new ArgumentException(nameof(offset))
    };

    internal static ArcTypes AsCCArcType(this ArcDirection direction) => direction switch {
        ArcDirection.ClockWise => ArcTypes.CC_CLOCKWISE_ARC,
        ArcDirection.CounterClockWise => ArcTypes.CC_COUNTER_CLOCKWISE_ARC,
        _ => ArcTypes.CC_UNKNOWN_ARC
    };

}
