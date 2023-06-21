namespace CADCodeProxy.Enums;

public enum Offset {
    None,
    Center,
    Left,
    Right,
    Inside,
    Outside
}

public static class OffsetExtension {

    public static string ToCSVCode(this Offset offset) => offset switch {
        Offset.None => "",
        Offset.Center => "C",
        Offset.Left => "L",
        Offset.Right => "R",
        Offset.Inside => "I",
        Offset.Outside => "0",
        _ => ""
    };

}
