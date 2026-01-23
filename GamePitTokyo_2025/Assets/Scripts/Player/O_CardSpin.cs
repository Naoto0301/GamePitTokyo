using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.Rendering;
public class O_CardSpin : MonoBehaviour
{
    [SerializeField] int spinCount = 3;     // 何回回すか
    [SerializeField] float duration = 1.5f; // 全体の時間（秒）
    private void Start()
    {
        StartSpin();
    }
    public void StartSpin()
    {
        StartCoroutine(SpinCoroutine());
    }

    IEnumerator SpinCoroutine()
    {
        float totalAngle = 360f * spinCount;
        float currentAngle = 0f;

        float speed = totalAngle / duration;

        while (currentAngle < totalAngle)
        {
            float delta = speed * Time.deltaTime;
            transform.Rotate(0f, delta, 0f, Space.Self);
            currentAngle += delta;
            yield return null;
        }

        // ズレ防止（ピタッと止める）
        Vector3 euler = transform.eulerAngles;
        euler.y = Mathf.Round(euler.y / 360f) * 360f;
        transform.eulerAngles = euler;
    }
}
