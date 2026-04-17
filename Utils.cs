using HutongGames.PlayMaker.Actions;
using Language;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EdgeDetection;

/// <summary>
/// Miscellaneous utility functions and extensions.
/// </summary>
internal static class Utils {
	#region Assembly

	/// <summary>
	/// Static reference to this assembly.
	/// </summary>
	internal static readonly Assembly asm = Assembly.GetExecutingAssembly();

	/// <summary>
	/// Static reference to this assembly's version.
	/// </summary>
	internal static readonly Version version = asm.GetName().Version;

	#endregion
	#region Assets

	/// <summary>
	/// Streams the embedded resource at <paramref name="path"/>,
	/// invokes <paramref name="action"/>, disposes of the stream.
	/// </summary>
	internal static void ReadAsset(string path, Action<Stream> action) {
		if (!path.StartsWith(nameof(EdgeDetection)))
			path = $"{(nameof(EdgeDetection))}.Assets.{path}";
		using Stream stream = asm.GetManifestResourceStream(path);
		action.Invoke(stream);
	}

	/// <summary>
	/// Deserializes the embedded json file at <paramref name="path"/>
	/// to data of type <typeparamref name="T"/>.
	/// </summary>
	internal static T ReadJsonAsset<T>(string path) {
		if (!path.StartsWith(nameof(EdgeDetection)))
			path = $"{(nameof(EdgeDetection))}.Assets.{path}";
		T value;
		using (StreamReader reader = new(asm.GetManifestResourceStream(path))) {
			value = JsonConvert.DeserializeObject<T>(reader.ReadToEnd())!;
		}
		return value;
	}

	#endregion
	#region Iterators

	/// <summary>
	/// Enumerates the Transforms of all GameObjects in <paramref name="roots"/>
	/// and all their descendants.
	/// </summary>
	internal static IEnumerable<Transform> WalkHierarchy(IEnumerable<GameObject> roots) {
		foreach (Transform t in roots.SelectMany(x => SelfAndWalkHierarchy(x)))
			yield return t;
	}

	/// <summary>
	/// Enumerates the Transforms of a GameObject and all its descendants.
	/// </summary>
	internal static IEnumerable<Transform> SelfAndWalkHierarchy(GameObject go) {
		yield return go.transform;
		foreach (Transform descendant in WalkHierarchy(go))
			yield return descendant;
	}

	/// <summary>
	/// Enumerates the Transforms of all the descendants of a GameObject.
	/// </summary>
	internal static IEnumerable<Transform> WalkHierarchy(GameObject go) {
		foreach (Transform t in go.transform) {
			yield return t;
			foreach (Transform descendant in WalkHierarchy(t.gameObject))
				yield return descendant;
		}
	}

	#endregion
	#region Extensions

	extension<K,V>(Dictionary<K,V> dict) {
		internal void TryAdd(K k, V v) {
			if (!dict.ContainsKey(k))
				dict[k] = v;
		}
	}


	extension(GameObject go) {
		internal T AddComponentIfNotPresent<T>() where T : Component {
			if (go.TryGetComponent<T>(out var c))
				return c;
			return go.AddComponent<T>();
		}
	}

	extension(Transform t) {
		internal void Reset() {
			t.localScale = Vector3.one;
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
		}
		internal void SetParentReset(Transform p) {
			t.SetParent(p);
			t.Reset();
		}
	}

	extension(Mesh mesh) {
		/// <summary>
		/// Rotates a <see cref="Mesh"/>'s vertices about <see cref="Vector3.zero"/>.
		/// </summary>
		internal void RotateVertices(Quaternion rotation)
			=> mesh.RotateVertices(rotation, Vector3.zero);

		/// <summary>
		/// Rotates a <see cref="Mesh"/>'s vertices about <paramref name="center"/>.
		/// </summary>
		internal void RotateVertices(Quaternion rotation, Vector3 center) {
			mesh.vertices = [..
				mesh.vertices.Select(v => rotation * (v - center) + center)
			];
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
		}
	}

	#endregion
}

