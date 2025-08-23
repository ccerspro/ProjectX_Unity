using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AimState { Default, Focused }
public class AimManager : MonoBehaviour
{
    public static AimManager I { get; private set; }
    // Start is called before the first frame update
    [SerializeField] GameObject defaultAim;
    [SerializeField] GameObject focusedAim;

    [Header("Fade Settings")]
    [SerializeField] float fadeDuration = 0.08f;
    [SerializeField] CanvasGroup defaultGroup;
    [SerializeField] CanvasGroup focusedGroup;

    AimState _state = AimState.Default;
    Coroutine _fadeCo;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);

        if (!defaultAim) defaultAim = transform.Find("DefaultAim")?.gameObject;
        if (!focusedAim) focusedAim = transform.Find("FocusedAim")?.gameObject;

        if (defaultAim && !defaultGroup) defaultGroup = defaultAim.GetComponent<CanvasGroup>() ?? defaultAim.AddComponent<CanvasGroup>();
        if (focusedAim && !focusedGroup) focusedGroup = focusedAim.GetComponent<CanvasGroup>() ?? focusedAim.AddComponent<CanvasGroup>();

        if (defaultGroup) defaultGroup.alpha = 1f;
        if (focusedGroup) focusedGroup.alpha = 0f;

        AssertSetup();
    }

    void OnDestroy()
    {
        if (I == this) I = null;
    }

    void Start()
    {
        SetState(AimState.Default, instant: true);
    }

    public void SetDefaultAim() => SetState(AimState.Default);
    public void SetFocusedAim() => SetState(AimState.Focused);

    public void SetState(AimState state, bool instant = false)
    {
        _state = state;
        if (instant || !defaultGroup || !focusedGroup)
        {
            if (defaultAim) defaultAim.SetActive(state == AimState.Default);
            if (focusedAim) focusedAim.SetActive(state == AimState.Focused);
            return;
        }

        if (_fadeCo != null) StopCoroutine(_fadeCo);
        _fadeCo = StartCoroutine(FadeTo(state));
    }

    System.Collections.IEnumerator FadeTo(AimState state)
    {
        float t = 0f;
        float dur = Mathf.Max(0.001f, fadeDuration);
        float dFrom = defaultGroup.alpha, dTo = (state == AimState.Default) ? 1f : 0f;
        float fFrom = focusedGroup.alpha, fTo = (state == AimState.Focused) ? 1f : 0f;

        // Ensure both are active while fading
        if (defaultAim && !defaultAim.activeSelf) defaultAim.SetActive(true);
        if (focusedAim && !focusedAim.activeSelf) focusedAim.SetActive(true);

        while (t < dur)
        {
            t += Time.unscaledDeltaTime; // unaffected by timescale
            float k = t / dur;
            defaultGroup.alpha = Mathf.Lerp(dFrom, dTo, k);
            focusedGroup.alpha = Mathf.Lerp(fFrom, fTo, k);
            yield return null;
        }

        defaultGroup.alpha = dTo;
        focusedGroup.alpha = fTo;

        if (defaultAim) defaultAim.SetActive(dTo > 0.99f);
        if (focusedAim) focusedAim.SetActive(fTo > 0.99f);
        _fadeCo = null;
    }

    void AssertSetup()
    {
#if UNITY_EDITOR
        if (!defaultAim) Debug.LogWarning("[AimManager] DefaultAim not assigned/found.", this);
        if (!focusedAim) Debug.LogWarning("[AimManager] FocusedAim not assigned/found.", this);
        if ((!defaultGroup || !focusedGroup))
            Debug.LogWarning("[AimManager] Using fade but missing CanvasGroups; they will be added at runtime.", this);
#endif
    }

}
