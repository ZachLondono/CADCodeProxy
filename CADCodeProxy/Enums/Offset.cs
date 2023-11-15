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

    public static Offset FromCSVCode(string code) => code switch {
        "L" => Offset.Left,
        "R" => Offset.Right,
        "I" => Offset.Inside,
        "O" => Offset.Outside,
        "C" => Offset.Center,
        "" => Offset.None,
        _ => throw new InvalidOperationException("")
    };

}
