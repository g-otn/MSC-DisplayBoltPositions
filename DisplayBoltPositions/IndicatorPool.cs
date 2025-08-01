using MSCLoader;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DisplayBoltPositions
{
    public class IndicatorPool
    {
        private int _size = 10;
        private List<GameObject> _pool;
        private int _counter = 0;

        public void Start()
        {
            _pool = new List<GameObject>();
            for (int i = 0; i < _size; i++)
            {
                GameObject obj = Spawn_Indicator();
                _pool.Add(obj);
            }
        }

        private GameObject Spawn_Indicator()
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = $"DisplayBoltPositions Indicator {(++_counter).ToString("D2")}";

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
            return obj;
        }

        public List<GameObject> Get_Active_Indicators()
        {
            return _pool.Where(g => g.activeInHierarchy).ToList();
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
