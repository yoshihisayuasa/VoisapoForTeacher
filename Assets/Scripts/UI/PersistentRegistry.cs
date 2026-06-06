using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public static class PersistentRegistry
    {
        private static readonly List<GameObject> _objects = new();

        public static void Register(GameObject obj)
        {
            _objects.Add(obj);
        }

        public static void DestroyAll()
        {
            foreach (var obj in _objects)
            {
                if (obj != null)
                {
                    Object.Destroy(obj);
                }
            }
            _objects.Clear();
        }
    }
}
