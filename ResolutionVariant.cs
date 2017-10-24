using System;
using System.Collections.Generic;

namespace HyperGames.AssetBundles {

    [Serializable]
    public struct ResolutionVariant {
        public string name;
        public float maxDP;
    }

    public class ResolutionVariantComparer : Comparer<ResolutionVariant> {
        public override int Compare(ResolutionVariant x, ResolutionVariant y) {
            return x.maxDP.CompareTo(y.maxDP);
        }
    }
}
