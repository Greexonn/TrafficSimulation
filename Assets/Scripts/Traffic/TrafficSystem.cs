using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;

public class TrafficSystem : MonoBehaviour
{
    public static TrafficSystem instance;

    //
    public List<NativeMultiHashMap<Entity, Entity>> graphs;

    void Awake()
    {
        if (instance != this)
        {
            if (instance != null)
                Destroy(this);
            else
                instance = this;
        }

        graphs = new List<NativeMultiHashMap<Entity, Entity>>();
    }

    void OnDestroy()
    {
        for (int i = 0; i < graphs.Count; i++)
        {
            if (graphs[i].IsCreated)
                graphs[i].Dispose();
        }
    }
}
