using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FaceEmojiClipSet", menuName = "Quang/Face Emoji Clip Set", order = 10)]
public class FaceEmojiClipSet : ScriptableObject
{
    [Tooltip("Danh sách clip dùng chung giữa nhiều prefab.")]
    public List<FaceEmoji.FaceClip> clips = new List<FaceEmoji.FaceClip>();

    // Tạo bản sao để không sửa trực tiếp asset khi chạy
    public List<FaceEmoji.FaceClip> CloneClips()
    {
        var list = new List<FaceEmoji.FaceClip>(clips.Count);
        foreach (var c in clips)
        {
            var nc = new FaceEmoji.FaceClip
            {
                key = c.key,
                startFrame = c.startFrame,
                endFrame = c.endFrame,
                fps = c.fps,
                playMode = c.playMode,
                loopDelay = c.loopDelay,
                loopDelayFrame = c.loopDelayFrame,
                loopDelayRandomMin = c.loopDelayRandomMin,
                loopDelayRandomMax = c.loopDelayRandomMax,
            };
            list.Add(nc);
        }
        return list;
    }
}
