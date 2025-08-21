using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZhouSoftware
{
    [CreateAssetMenu(menuName = "ZhouSoftware/Audio/Audio Bank")]
    public class AudioBank : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string id;       // e.g., "flashlight_toggle"
            public AudioClip clip;  // exactly one clip per id
        }

        [SerializeField] private Entry[] entries;
        private Dictionary<string, AudioClip> _map;

        public AudioClip Get(string id)
        {
            if (_map == null)
            {
                _map = new Dictionary<string, AudioClip>(StringComparer.Ordinal);
                foreach (var e in entries)
                    if (!string.IsNullOrEmpty(e.id) && e.clip) _map[e.id] = e.clip;
            }
            _map.TryGetValue(id, out var clip);
            return clip;
        }
    }
}
