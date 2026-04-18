using GlobalEnums;

namespace EdgeDetection.ModMenu;

internal static class MenuUtils {

	/// <summary>
	/// Creates a GameObject on the UGUI layer and sets its parent.
	/// </summary>
	internal static GameObject UIGameObject(string name, GameObject? parent) {
		GameObject go = new(name) { layer = (int)PhysLayers.UGUI };
		if (parent)
			go.transform.SetParentReset(parent!.transform);
		return go;
	}

	/// <summary>
	/// Creates a new material using the default UI shader.
	/// </summary>
	internal static Material UIMaterial(Color? color = null) {
		if (!uiShader)
			uiShader = Shader.Find("UI/Default");
		return new Material(uiShader) { color = color ?? Color.white };
	}
	static Shader? uiShader;

	extension (GameObject go) {
		internal RectTransform RectTransform => (RectTransform)go.transform;

		internal void SetAnchors(Vector2 anchor)
			=> go.RectTransform.anchorMax = go.RectTransform.anchorMin = anchor;

		internal void SetSizeDelta(Vector2 size) => go.RectTransform.sizeDelta = size;
	}

	extension (Transform t) {
		internal RectTransform AsRect => (RectTransform)t;
		internal void SetAnchors(Vector2 anchor)
			=> t.AsRect.anchorMax = t.AsRect.anchorMin = anchor;
	}
}
