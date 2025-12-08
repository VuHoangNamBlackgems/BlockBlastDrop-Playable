using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class FaceEmoji : MonoBehaviour
{
    public enum PlayMode { Once, Loop, PingPong, HoldLast }

    [Serializable]
    public class FaceClip
    {
        [Tooltip("Tên biểu cảm (Idle, Happy, Angry, Blink, ...)")]
        public string key = "Idle";

        [Header("Frames (dải liên tục)")]
        public int startFrame = 0;   // inclusive
        public int endFrame = 0;   // inclusive

        [Header("Playback")]
        [Min(1f)] public float fps = 12f;
        public PlayMode playMode = PlayMode.Loop;
        [Header("Loop Delay")]
        [Tooltip("Delay in seconds after a loop completes before continuing (0 = no delay)")]
        public float loopDelay = 0f;
        [Tooltip("Frame to display during the loop delay. -1 = hold last frame of the clip, otherwise an absolute frame index on the sheet.")]
        public int loopDelayFrame = -1;
        [Header("Loop Delay Random")]
        [Tooltip("When loopDelay > 0, this min value is added as a random extra delay (Random.Range(min, max)). When loopDelay < 0, a random delay is chosen between min and max.")]
        public float loopDelayRandomMin = 0f;
        [Tooltip("Max for random delay addition or random delay range.")]
        public float loopDelayRandomMax = 0f;


    }

    [Header("Target")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private int materialIndex = 0;

    [Header("Clips (cục bộ)")]
    [SerializeField]
    private List<FaceClip> clips = new List<FaceClip>()
    {
        new FaceClip{ key="Idle",  startFrame=0, endFrame=3,  fps=8f,  playMode=PlayMode.Loop },
        new FaceClip{ key="Blink", startFrame=4, endFrame=6,  fps=16f, playMode=PlayMode.Once },
        new FaceClip{ key="Happy", startFrame=8, endFrame=11, fps=12f, playMode=PlayMode.Loop },
    };

    [Header("ClipSet (dùng chung)")]
    [Tooltip("ScriptableObject chứa danh sách clip dùng chung.")]
    [SerializeField] private FaceEmojiClipSet clipSet;
    [Tooltip("Tự động copy clip từ ClipSet vào danh sách cục bộ khi Awake()")]
    [SerializeField] private bool loadClipsFromSetOnAwake = true;

    [Header("Startup")]
    [SerializeField] private string defaultClip = "Idle";
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Sheet Info")]
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows = 4;

    static readonly int ID_UseCustomIndex = Shader.PropertyToID("_UseCustomIndex");
    static readonly int ID_Index = Shader.PropertyToID("_Index");
    static readonly int ID_Columns = Shader.PropertyToID("_Columns");
    static readonly int ID_Rows = Shader.PropertyToID("_Rows");

    private MaterialPropertyBlock mpb;
    private FaceClip currentClip;
    private string currentKey = "";
    private float timer;
    private int step;
    private int dir = 1;
    private int playedLoops = 0;
    // Loop delay runtime state
    private bool inLoopDelay = false;
    private float loopDelayTimer = 0f;
    // when in delay and loopDelayFrame == -1 we hold last step; cache it
    private int delayHoldStep = 0;

    private int TotalFrames => Mathf.Max(1, columns * rows);

    public List<FaceClip> Clips => clips;
    public FaceEmojiClipSet ClipSet => clipSet;
    public string CurrentKey => currentKey;

    void Reset() { targetRenderer = GetComponent<Renderer>(); }

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponent<Renderer>();
        if (mpb == null) mpb = new MaterialPropertyBlock();

        // Nạp từ ClipSet nếu có
        if (clipSet && loadClipsFromSetOnAwake) CopyClipsFromSet(clipSet);

        TryReadSheetInfoFromMaterial();
        ApplyIndex(0);
    }

    void Start()
    {
        if (playOnStart && !string.IsNullOrEmpty(defaultClip)) Play(defaultClip);
    }

    void Update()
    {
        if (currentClip == null) return;
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (dt <= 0f) return;

        // If we're in a loop delay, count down the delay timer and exit early (holding the delay frame)
        if (inLoopDelay)
        {
            loopDelayTimer -= dt;
            if (loopDelayTimer <= 0f)
            {
                // end delay and resume playback; reset timers so the next frame timing starts fresh
                inLoopDelay = false;
                loopDelayTimer = 0f;
                timer = 0f;
                // if holding last step, make sure step is set to the next logical value based on playMode
                // for Loop mode we already advanced step when loop completed; for Once/PingPong/HoldLast no-op
            }
            ApplyIndex(EvaluateFrameAtCurrentStep());
            return;
        }

        timer += dt;
        float stepDuration = 1f / Mathf.Max(1f, currentClip.fps);
        while (timer >= stepDuration)
        {
            timer -= stepDuration;
            AdvanceOneStep();
            // If entering loop delay inside AdvanceOneStep, break out so we don't advance further frames while delayed
            if (inLoopDelay) break;
        }
        ApplyIndex(EvaluateFrameAtCurrentStep());
    }

    // ===== Public API =====
    public void Play(string key)
    {
        var clip = FindClip(key);
        if (clip == null)
        {
            Debug.LogWarning($"[FaceEmoji] Không tìm thấy clip '{key}'.");
            clip = FindClip(defaultClip);
        }

        currentClip = clip;
        currentKey = key;
        timer = 0f; step = 0; dir = 1; playedLoops = 0;
        inLoopDelay = false; loopDelayTimer = 0f; delayHoldStep = 0;
        ApplyIndex(EvaluateFrameAtCurrentStep());
    }

    public void Stop(bool holdLastFrame = false)
    {
        if (currentClip == null) return;
        if (!holdLastFrame) { step = 0; ApplyIndex(EvaluateFrameAtCurrentStep()); }
        currentClip = null; currentKey = "";
        inLoopDelay = false; loopDelayTimer = 0f;
    }

    public void SetFrame(int frameIndex)
    {
        currentClip = null; currentKey = "";
        ApplyIndex(frameIndex);
    }

    public void CopyClipsFromSet(FaceEmojiClipSet set)
    {
        if (!set) return;
        clips = new List<FaceClip>(set.CloneClips());
    }

    // ===== Internal =====
    private void AdvanceOneStep()
    {
        int len = ClipLength(currentClip);
        if (len <= 0) return;

        switch (currentClip.playMode)
        {
            case PlayMode.Loop:
                step = (step + 1) % len;
                if (step == 0)
                {
                    playedLoops++;
                    // trigger loop delay if configured
                    if (currentClip.loopDelay > 0f)
                    {
                        inLoopDelay = true;
                        // Compute actual delay timer using rules:
                        // - If loopDelay == 0 => no delay (we don't get here)
                        // - If loopDelay > 0 => fixed base delay + optional random extra between loopDelayRandomMin..loopDelayRandomMax
                        // - If loopDelay < 0 => choose random delay between abs(loopDelay) and max(loopDelayRandomMax, abs(loopDelay))
                        float baseDelay = currentClip.loopDelay;
                        float actualDelay = 0f;
                        if (baseDelay > 0f)
                        {
                            float addMin = Mathf.Min(currentClip.loopDelayRandomMin, currentClip.loopDelayRandomMax);
                            float addMax = Mathf.Max(currentClip.loopDelayRandomMin, currentClip.loopDelayRandomMax);
                            float extra = (addMax > addMin) ? UnityEngine.Random.Range(addMin, addMax) : addMin;
                            actualDelay = baseDelay + extra;
                        }
                        else // baseDelay < 0 => random delay
                        {
                            float absBase = Mathf.Abs(baseDelay);
                            float rMin = Mathf.Min(absBase, currentClip.loopDelayRandomMin, currentClip.loopDelayRandomMax);
                            float rMax = Mathf.Max(absBase, currentClip.loopDelayRandomMin, currentClip.loopDelayRandomMax);
                            actualDelay = UnityEngine.Random.Range(rMin, rMax);
                        }
                        loopDelayTimer = actualDelay;
                        // determine which frame to hold during delay
                        if (currentClip.loopDelayFrame == -1)
                        {
                            // hold last frame of clip (which is len - 1)
                            delayHoldStep = len - 1;
                        }
                        else
                        {
                            // treat loopDelayFrame as an absolute frame index on the sheet; clamp inside EvaluateFrameAtCurrentStep
                            delayHoldStep = currentClip.loopDelayFrame;
                        }
                    }
                    MaybeStopByRepeat();
                }
                break;

            case PlayMode.Once:
                if (step < len - 1) step++;
                else { playedLoops++; if (!MaybeStopByRepeat()) step = len - 1; }
                break;

            case PlayMode.PingPong:
                step += dir;
                if (step >= len) { dir = -1; step = Mathf.Clamp(len - 2, 0, len - 1); playedLoops++; MaybeStopByRepeat(); }
                else if (step < 0) { dir = 1; step = Mathf.Min(1, len - 1); playedLoops++; MaybeStopByRepeat(); }
                break;

            case PlayMode.HoldLast:
                if (step < len - 1) step++;
                break;
        }
    }

    private bool MaybeStopByRepeat()
    {
        if (currentClip == null) return true;
        // If we're in a loop delay, do not stop playback; let delay finish first
        if (inLoopDelay) return false;
        return false;
    }

    private int EvaluateFrameAtCurrentStep()
    {
        if (currentClip == null) return GetCurrentIndexFromMaterial();
        // if currently in a configured loop delay, return the chosen delay frame
        if (inLoopDelay)
        {
            // if delayHoldStep is within clip-relative range, map it to absolute frame; otherwise treat as absolute
            int s2, e2, len2; GetClampedRange(currentClip, out s2, out e2, out len2);
            if (currentClip.loopDelayFrame == -1)
            {
                // hold last frame of the clip range
                int raw2 = s2 + Mathf.Clamp(delayHoldStep, 0, Mathf.Max(0, len2 - 1));
                return Mathf.Clamp(raw2, 0, TotalFrames - 1);
            }
            else
            {
                // loopDelayFrame provided as an absolute frame index
                return Mathf.Clamp(delayHoldStep, 0, TotalFrames - 1);
            }
        }
        int start, end, len; GetClampedRange(currentClip, out start, out end, out len);
        if (len <= 0) return Mathf.Clamp(start, 0, TotalFrames - 1);
        int raw = start + Mathf.Clamp(step, 0, len - 1);
        return Mathf.Clamp(raw, 0, TotalFrames - 1);
    }

    private int ClipLength(FaceClip clip)
    {
        int s, e, l; GetClampedRange(clip, out s, out e, out l); return l;
    }

    private void GetClampedRange(FaceClip clip, out int start, out int end, out int length)
    {
        int max = TotalFrames - 1;
        start = Mathf.Clamp(clip.startFrame, 0, max);
        end = Mathf.Clamp(clip.endFrame, 0, max);
        if (end < start) (start, end) = (end, start);
        length = end - start + 1;
    }

    private void ApplyIndex(int frameIndex)
    {
        if (!targetRenderer) return;
        if (mpb == null) mpb = new MaterialPropertyBlock();

        targetRenderer.GetPropertyBlock(mpb, materialIndex);
        mpb.SetFloat(ID_UseCustomIndex, 1f);
        mpb.SetFloat(ID_Index, Mathf.Clamp(frameIndex, 0, TotalFrames - 1));
        if (columns > 0) mpb.SetFloat(ID_Columns, columns);
        if (rows > 0) mpb.SetFloat(ID_Rows, rows);
        targetRenderer.SetPropertyBlock(mpb, materialIndex);
    }

    private FaceClip FindClip(string key) => clips.Find(c => string.Equals(c.key, key, StringComparison.OrdinalIgnoreCase));

    private void TryReadSheetInfoFromMaterial()
    {
        var mat = GetMaterialAt(materialIndex);
        if (!mat) return;
        if (mat.HasProperty(ID_Columns)) { int c = Mathf.RoundToInt(mat.GetFloat(ID_Columns)); if (c > 0) columns = c; }
        if (mat.HasProperty(ID_Rows)) { int r = Mathf.RoundToInt(mat.GetFloat(ID_Rows)); if (r > 0) rows = r; }
    }

    private int GetCurrentIndexFromMaterial()
    {
        var mat = GetMaterialAt(materialIndex);
        if (mat && mat.HasProperty(ID_Index)) return Mathf.RoundToInt(mat.GetFloat(ID_Index));
        return 0;
    }

    private Material GetMaterialAt(int index)
    {
        if (!targetRenderer) return null;
        var mats = targetRenderer.sharedMaterials;
        if (index < 0 || index >= mats.Length) return null;
        return mats[index];
    }

    public void PlayRandomEmoji()
    {
        string key = null;
        while (key == null || key == "Pickup")
        {
            int randomIndex = UnityEngine.Random.Range(0, clips.Count);
            key = clips[randomIndex].key;
        }
        Play(key);
    }

#if UNITY_EDITOR
    [ContextMenu("Play Idle")] private void _PlayIdle() => Play("Idle");
    [ContextMenu("Play Blink")] private void _PlayBlink() => Play("Blink");
    [ContextMenu("Hold Frame 0")] private void _Hold0() => SetFrame(0);
#endif
}
