using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class NpcManager : MonoBehaviour
{
    Queue<GameObject> npcQueue = new Queue<GameObject>();
    LinkedList<GameObject> npcList = new LinkedList<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        npcList.AddLast(new GameObject("NPC1"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
