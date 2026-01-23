using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("移動制限の設定")]
    [SerializeField] private bool limitX = true;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;

    [SerializeField] private bool limitY = true;
    [SerializeField] private float minY = -5f;
    [SerializeField] private float maxY = 5f;

    [SerializeField] O_Player player;
    [SerializeField] float player_y;

    // LateUpdateは全てのUpdate処理が終わった後に呼ばれるため、カメラ追従に適しています
    void LateUpdate()
    {
        if (!player) return;

        Vector3 targetPos = new Vector3(
            player.transform.position.x,
            player.transform.position.y + player_y,
            transform.position.z   // ← カメラのZは固定
        );

        if (limitX)
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);

        if (limitY)
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        transform.position = targetPos;
    }
}