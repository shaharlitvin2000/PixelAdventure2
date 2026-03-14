using System.Collections.Generic;
using UnityEngine;

public class SoundEffectLibrary : MonoBehaviour
{
    [System.Serializable]
    public struct SoundEffectGroup
    {
        public string name;
        public List<AudioClip> audioClips; // רשימה של צלילים כמו בתמונה
    }

    public List<SoundEffectGroup> soundEffectGroups;

    public AudioClip GetRandomClipByName(string name)
    {
        foreach (var group in soundEffectGroups)
        {
            // הוספנו .Trim() ו-OrdinalIgnoreCase כדי שהקוד יהיה "חסין לטעויות"
            if (group.name.Trim().Equals(name.Trim(), System.StringComparison.OrdinalIgnoreCase)
                && group.audioClips.Count > 0)
            {
                int randomIndex = Random.Range(0, group.audioClips.Count);
                return group.audioClips[randomIndex];
            }
        }
        return null;
    }
}