using UnityEngine;

public class TileBaseEX : MonoBehaviour
{
    [Header("=== State ===")]
    public ExposedDir exposed;
    public CornerType corner;
    public bool isHit;

    [Header("=== Sprites ===")]
    public Sprite solid;        // 完全に埋まっている
    public Sprite top;          // 上が空気（地表・床）
    public Sprite bottom;       // 下が空気（洞窟天井）
    public Sprite left;
    public Sprite right;

    [Header("=== Outer Corners ===")]
    public Sprite outerUpLeft;
    public Sprite outerUpRight;
    public Sprite outerDownLeft;
    public Sprite outerDownRight;

    [Header("=== Inner Corners ===")]
    public Sprite innerUpLeft;
    public Sprite innerUpRight;
    public Sprite innerDownLeft;
    public Sprite innerDownRight;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // =====================================
    public void ApplyVisual()
    {
        // ① 角（内角・外角は最優先）
        if (ApplyCorner())
            return;

        // ② 上が空気 → 地表 or 洞窟床
        if (exposed.HasFlag(ExposedDir.Up))
        {
            sr.sprite = top;
            return;
        }

        // ③ 下が空気 → 洞窟天井
        if (exposed.HasFlag(ExposedDir.Down))
        {
            sr.sprite = bottom;
            return;
        }

        // ④ 左右の壁
        if (exposed.HasFlag(ExposedDir.Left))
        {
            sr.sprite = left;
            return;
        }

        if (exposed.HasFlag(ExposedDir.Right))
        {
            sr.sprite = right;
            return;
        }

        // ⑤ 完全に埋まっている
        sr.sprite = solid;
    }

    // =====================================
    bool ApplyCorner()
    {
        switch (corner)
        {
            // ---- 外角 ----
            case CornerType.OuterUpLeft:
                sr.sprite = outerUpLeft; return true;
            case CornerType.OuterUpRight:
                sr.sprite = outerUpRight; return true;
            case CornerType.OuterDownLeft:
                sr.sprite = outerDownLeft; return true;
            case CornerType.OuterDownRight:
                sr.sprite = outerDownRight; return true;

            // ---- 内角 ----
            case CornerType.InnerUpLeft:
                sr.sprite = innerUpLeft; return true;
            case CornerType.InnerUpRight:
                sr.sprite = innerUpRight; return true;
            case CornerType.InnerDownLeft:
                sr.sprite = innerDownLeft; return true;
            case CornerType.InnerDownRight:
                sr.sprite = innerDownRight; return true;
        }
        return false;
    }
}