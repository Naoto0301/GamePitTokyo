using UnityEngine;

public class TileBaseEX : MonoBehaviour
{
    public bool isHit = true;

    void Start()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = isHit;
    }

    void Update()
    {
        // エディット中にも反映したければ Updateで
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = isHit;
    }
}
