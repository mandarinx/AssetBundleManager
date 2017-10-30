using UnityEngine;

namespace HyperGames.AssetBundles {

    public class AssetBundleLoadStatus : CustomYieldInstruction {

        private readonly BundleLoadOperation op;

        public override bool keepWaiting {
            get { return op.progress < 1f; }
        }

        public float progress {
            get { return op.progress; }
        }

        public bool error {
            get { return op.error; }
        }

        public string errorMessage {
            get { return op.errorMsg; }
        }

        public AssetBundleLoadStatus(BundleLoadOperation loadOp) {
            op = loadOp;
        }
    }
}
