using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInfo : MonoBehaviour
{
    [System.Serializable]
    public struct SpriteInfo {
        public string name;
        public Sprite sprite;
    }

    public SpriteInfo[] sprites;
    public string characterName;
}
