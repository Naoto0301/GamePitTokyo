using UnityEngine;

public class TileBaseEX : MonoBehaviour
{
    public BoxCollider2D col;

    [Header("=== State ===")]
    public ExposedDir exposed;
    public CornerType corner;
    public bool isHit;

    [Header("=== Sprites ===")]
    public Sprite solid;        // Š®‘S‚É–„‚Ü‚Á‚Ä‚¢‚é
    public Sprite top;          // ã‚ª‹ó‹Ci’n•\E°j
    public Sprite bottom;       // ‰º‚ª‹ó‹Ci“´ŒA“Vˆäj
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
        col = transform.GetComponent<BoxCollider2D>();
    }

    // =====================================
    public void ApplyVisual()
    {
        // ‡@ Špi“àŠpEŠOŠp‚ÍÅ—Dæj
        if (ApplyCorner())
            return;

        // ‡A ã‚ª‹ó‹C ¨ ’n•\ or “´ŒA°
        if (exposed.HasFlag(ExposedDir.Up))
        {
            sr.sprite = top;
            return;
        }

        // ‡B ‰º‚ª‹ó‹C ¨ “´ŒA“Vˆä
        if (exposed.HasFlag(ExposedDir.Down))
        {
            sr.sprite = bottom;
            return;
        }

        // ‡C ¶‰E‚Ì•Ç
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

        // ‡D Š®‘S‚É–„‚Ü‚Á‚Ä‚¢‚é
        sr.sprite = solid;
    }

    // =====================================
    bool ApplyCorner()
    {
        switch (corner)
        {
            // ---- ŠOŠp ----
            case CornerType.OuterUpLeft:
                sr.sprite = outerUpLeft; return true;
            case CornerType.OuterUpRight:
                sr.sprite = outerUpRight; return true;
            case CornerType.OuterDownLeft:
                sr.sprite = outerDownLeft; return true;
            case CornerType.OuterDownRight:
                sr.sprite = outerDownRight; return true;

            // ---- “àŠp ----
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