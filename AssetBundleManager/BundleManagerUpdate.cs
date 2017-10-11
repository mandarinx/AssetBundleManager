using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HyperGames.AssetBundles {

    public class BundleManagerUpdate : MonoBehaviour {
        private AssetBundleManager manager;
        private int active;

        private void Awake() {
            active = 0;
        }

        public void Init(AssetBundleManager p_manager) {
            manager = p_manager;
        }
        
        public void Activate() {
            Debug.Log("[Updater] Activate");
            active = 1;
        }
        
        public void Deactivate() {
            Debug.Log("[Updater] Deactivate");
            active = 0;
        }

        private void Update() {
            for (int i = 0; i < active; ++i) {
                manager.Update();
            }
        }
    }
}
