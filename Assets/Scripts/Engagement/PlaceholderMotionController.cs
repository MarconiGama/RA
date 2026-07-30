using System.Collections;
using UnityEngine;

public sealed class PlaceholderMotionController : MonoBehaviour
{
    [SerializeField] private Transform motionRoot;
    [SerializeField] private float motionScale = 1f;

    private Coroutine routine;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    public string LastAnimation { get; private set; }
    public bool IsAnimating { get { return routine != null; } }

    private void Awake()
    {
        if (motionRoot == null) motionRoot = transform;
        CaptureInitialTransform();
    }

    public void Play(string animationName, bool reducedMotion)
    {
        StopMotion();
        LastAnimation = reducedMotion && animationName == "ShortFlight"
            ? "ReducedMotionResponse"
            : animationName;
        routine = StartCoroutine(Animate(LastAnimation));
    }

    public void StopMotion()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        if (motionRoot != null)
        {
            motionRoot.localPosition = initialPosition;
            motionRoot.localRotation = initialRotation;
            motionRoot.localScale = initialScale;
        }
    }

    private IEnumerator Animate(string animationName)
    {
        var duration = animationName == "ShortFlight" ? 2.2f : 0.8f;
        if (animationName == "ReducedMotionResponse") duration = 0.6f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            if (animationName == "ShortFlight")
            {
                motionRoot.localPosition = initialPosition + new Vector3(
                    Mathf.Sin(t * Mathf.PI) * 0.45f,
                    Mathf.Sin(t * Mathf.PI) * 0.25f,
                    0f) * motionScale;
                motionRoot.localRotation = initialRotation * Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 4f) * 8f);
            }
            else if (animationName == "ReducedMotionResponse")
            {
                motionRoot.localScale = initialScale * (1f + Mathf.Sin(t * Mathf.PI) * 0.05f);
            }
            else
            {
                motionRoot.localPosition = initialPosition + Vector3.up * Mathf.Sin(t * Mathf.PI) * 0.08f * motionScale;
            }
            yield return null;
        }
        motionRoot.localPosition = initialPosition;
        motionRoot.localRotation = initialRotation;
        motionRoot.localScale = initialScale;
        routine = null;
    }

    private void CaptureInitialTransform()
    {
        initialPosition = motionRoot.localPosition;
        initialRotation = motionRoot.localRotation;
        initialScale = motionRoot.localScale;
    }

    private void OnDisable()
    {
        StopMotion();
    }
}
