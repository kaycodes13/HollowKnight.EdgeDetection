using EdgeDetection.Components;
using EdgeDetection.Structs;
using Modding;
using System.Collections;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace EdgeDetection.Patches;

internal static class ApplyObjectMods {

	internal static void Patch() {
		On.GameCameras.Start += SceneParticlesHook;
		On.HeroController.Awake += HeroAwakeHook;
		ModHooks.ObjectPoolSpawnHook += ObjectSpawnHook;
		USceneManager.activeSceneChanged += SceneChangedHook;
		USceneManager.sceneLoaded += SceneLoadedHook;
	}

	#region Hooks

	private static void SceneParticlesHook(On.GameCameras.orig_Start orig, GameCameras self) {
		orig(self);

		foreach (Transform t in Utils.WalkHierarchy(self.sceneParticles.gameObject))
			t.gameObject.layer = ObjectMods.HIDE_LAYER_INT;
	}

	static void HeroAwakeHook(On.HeroController.orig_Awake orig, HeroController self) {
		orig(self);

		ObjectMods ghostMods = Utils.ReadJsonAsset<ObjectMods>("ghost_modifications.json");

		foreach (Transform t in Utils.WalkHierarchy(self.gameObject)) {
			HideParticles(t);
			ghostMods.Apply(t);
		}
	}

	// TODO: not catching "Death Puff Boss" on the no particles pass :/
	static GameObject ObjectSpawnHook(GameObject result) {
		foreach (Transform t in Utils.SelfAndWalkHierarchy(result)) {
			if (ObjectVisualizer.IsVisualizer(t))
				continue;
			HideParticles(t);
			genericMods.Apply(t);
		}
		return result;
	}

	static void SceneChangedHook(Scene _, Scene to) => SceneHandler(to);
	static void SceneLoadedHook(Scene to, LoadSceneMode mode) => SceneHandler(to);
	static void SceneHandler(Scene scene) {
		if (!GameManager.instance)
			return;

		GameManager.instance.StartCoroutine(Coro(scene));

		static IEnumerator Coro(Scene scn) {
			for (int i = 0; i < 2; i++) yield return null;
			if (!scn.isLoaded) yield break;

			foreach (Transform t in Utils.WalkHierarchy(scn.GetRootGameObjects())) {
				if (ObjectVisualizer.IsVisualizer(t))
					continue;
				HideParticles(t);
				genericMods.Apply(t);
			}
		}
	}


	#endregion
	#region Internals

	static readonly ObjectMods
		genericMods = Utils.ReadJsonAsset<ObjectMods>("generic_modifications.json");

	static void HideParticles(Transform t) {
		GameObject go = t.gameObject;
		if (go.GetComponent<ParticleSystemRenderer>() && !go.GetComponent<Collider2D>())
			go.layer = ObjectMods.HIDE_LAYER_INT;
	}

	#endregion
}
