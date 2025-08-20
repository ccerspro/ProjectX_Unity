using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using System;
using System.ComponentModel;

[Serializable]
public class Sound
{
    public String name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume;
    [Range(0.3f, 3f)]
    public float pitch;
    [HideInInspector]
    public AudioSource source;

}
