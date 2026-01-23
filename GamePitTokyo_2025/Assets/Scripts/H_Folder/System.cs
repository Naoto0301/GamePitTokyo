using System;

[System.Flags]
public enum ExposedDir
{
    None = 0,
    Up = 1 << 0,
    Down = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3,
}
public enum CornerType
{
    None,

    // 外角（ブロックが角になっている）
    OuterUpLeft,
    OuterUpRight,
    OuterDownLeft,
    OuterDownRight,

    // 内角（へこんでいる）
    InnerUpLeft,
    InnerUpRight,
    InnerDownLeft,
    InnerDownRight,
}

[System.Serializable]
public struct SpawnRange
{
    public int left;
    public int right;
    public int top;
    public int bottom;
}