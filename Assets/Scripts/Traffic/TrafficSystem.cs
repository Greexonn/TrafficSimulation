using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Traffic
{
    public class TrafficSystem : MonoBehaviour
    {
        public static TrafficSystem Instance;

        //
        public List<NativeParallelMultiHashMap<Entity, Entity>> Graphs;

        private void Awake()
        {
            if (Instance != this)
            {
                if (Instance != null)
                    Destroy(this);
                else
                    Instance = this;
            }

            Graphs = new List<NativeParallelMultiHashMap<Entity, Entity>>();
        }

        private void OnDestroy()
        {
            for (var i = 0; i < Graphs.Count; i++)
            {
                if (Graphs[i].IsCreated)
                    Graphs[i].Dispose();
            }
        }
    }
}
