using MSCLoader;
using System.Collections.Generic;
using UnityEngine;

namespace DisplayBoltPositions
{
    public class IndicatorPool
    {
        private int size = 1;
        private List<GameObject> _pool;

        public void Start()
        {
            _pool = new List<GameObject>();
            for (int i = 0; i < size; i++)
            {
                GameObject obj = Spawn_Indicator();
                _pool.Add(obj);
            }
        }

        private GameObject Spawn_Indicator()
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "DisplayBoltPositions Indicator";

            // Make it uninteractable by removing colliders
            UnityEngine.Object.DestroyImmediate(sphere.GetComponent<Collider>()); // Removes physics collider
            UnityEngine.Object.DestroyImmediate(sphere.GetComponent<SphereCollider>()); // Just to be sure

            Renderer sphereRenderer = sphere.GetComponent<Renderer>();
            sphereRenderer.material = IndicatorShader.Material;

            sphere.SetActive(false);

            return sphere;
        }

        public GameObject Get_Indicator()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                if (!_pool[i].activeInHierarchy)
                {
                    return _pool[i];
                }
            }

            // Expand the pool if needed
            GameObject obj = Spawn_Indicator();
            _pool.Add(obj);
            ModConsole.Print($"Expanded object pool to {_pool.Count + 1}");
            return obj;
        }

        public void Return_To_Pool(GameObject obj)
        {
            obj.SetActive(false);
        }

        public void Return_All_To_Pool()
        {
            //ModConsole.Print("Returning all " + _pool.Count);
            for (int i = 0; i < _pool.Count; i++)
            {
                _pool[i].SetActive(false);
            }
        }
    }
}
