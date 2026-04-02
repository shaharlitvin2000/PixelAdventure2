using Cinemachine;
using System.Threading.Tasks;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CinemachineVirtualCamera vcam;

    private CinemachineFramingTransposer transposer;
    private Vector3 originalDamping;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        transposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();

        if (transposer != null)
        {
            originalDamping = new Vector3(
                transposer.m_XDamping,
                transposer.m_YDamping,
                transposer.m_ZDamping
            );
        }
    }

    async Task Fade(float targetAlpha, float duration)
    {
        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, targetAlpha, t / duration);
            await Task.Yield();
        }

        canvasGroup.alpha = targetAlpha;
    }

    public async Task FadeOut(float duration)
    {
        await Fade(1f, duration);
        SetDamping(Vector3.zero);
    }

    public async Task FadeIn(float duration)
    {
        await Fade(0f, duration);
        SetDamping(originalDamping);
    }

    void SetDamping(Vector3 d)
    {
        if (transposer == null) return;

        transposer.m_XDamping = d.x;
        transposer.m_YDamping = d.y;
        transposer.m_ZDamping = d.z;
    }
}