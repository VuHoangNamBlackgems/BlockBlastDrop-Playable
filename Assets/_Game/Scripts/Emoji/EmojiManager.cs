using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmojiManager : MonoBehaviour
{
    public List<Emoji> m_List = new List<Emoji>();

    float detectRange = 5F;

    public static EmojiManager Ins;
    private void Awake()
    {
        Ins = this;
    }

    public void AddEmoji(Emoji emoji)
    {
        m_List.Add(emoji);
    }
    public void RemoveEmoji(Emoji emoji)
    {
        m_List.Remove(emoji);
    }


    public void MoveDistance(Vector3 Pos)
    {
        foreach (var emoji in m_List)
        {
            float dist = Vector3.Distance(Pos, emoji.transform.position);
            if (dist < detectRange && !emoji.IsMove)
            {
                emoji.SetDirector(Pos);
            }
            else
            {
                emoji.SetResetDirec();

            }
        }

    }

    public void ResetPos()
    {
        foreach (var emoji in m_List)
        {
            emoji.ResetPos();
        }

    }
}
