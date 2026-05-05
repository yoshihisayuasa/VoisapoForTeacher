using UnityEngine;

namespace Assets.Scripts.UI
{
    public sealed class PersistentRoot : MonoBehaviour
    {
        private static PersistentRoot _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            if (TryGetComponent<Canvas>(out var canvas))
            {
                canvas.sortingOrder = 0;
            }
        }
    }
}
