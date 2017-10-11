using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using HyperGames.AssetBundles;

public class TestAssetBundleManager {

	[Test]
	public void Load_Bundle_Wo_Variant_Wo_VariantResolver() {
		GameObject abmanOwner = new GameObject("AssetBundleManager");
		AssetBundleConfig cfg = AssetDatabase.LoadAssetAtPath<AssetBundleConfig>("Assets/Scripts/AssetBundles/Tests/AssetBundleConfigTest.asset");
		AssetBundleManager abman = new AssetBundleManager(cfg, abmanOwner);
	}

	[Test]
	public void Load_Bundle_W_Variant_Wo_VariantResolver() {
	}

	[Test]
	public void Load_Bundle_Wo_Variant_W_VariantResolver() {
	}

	[Test]
	public void Load_Bundle_W_Variant_W_VariantResolver() {
	}

	[Test]
	public void Load_Bundle_W_Variant_W_DepVariants_W_VariantResolver() {
	}

	[Test]
	public void Load_Bundle_Wo_Variant_W_DepVariants_W_VariantResolver() {
	}

	[Test]
	public void Load_Bundle_W_Variant_Wo_DepVariants_W_VariantResolver() {
	}

	[Test]
	public void Load_Bundle_Wo_Variant_Wo_DepVariants_W_VariantResolver() {
	}

	[Test]
	public void Load_Bundle_W_Variant_W_DepVariants_Wo_VariantResolver() {
	}

	[Test]
	public void Load_Bundle_Wo_Variant_W_DepVariants_Wo_VariantResolver() {
	}

	[Test]
	public void Load_Bundle_W_Variant_Wo_DepVariants_Wo_VariantResolver() {
	}

	[Test]
	public void Load_Bundle_Wo_Variant_Wo_DepVariants_Wo_VariantResolver() {
	}
	
	// dpi tests
	// custom resolver

	// A UnityTest behaves like a coroutine in PlayMode
	// and allows you to yield null to skip a frame in EditMode
//	[UnityTest]
//	public IEnumerator NewEditModeTestWithEnumeratorPasses() {
//		// Use the Assert class to test conditions.
//		// yield to skip a frame
//		yield return null;
//	}
}
