using EdgeDetection.Patches;
using Modding.Menu;
using Satchel.BetterMenus;
using System.Collections;

namespace EdgeDetection.ModMenu;

/// <summary>
/// A slider for byte values.
/// Resized to match other menu elements better.
/// The Name property is considered a language key for getting localized text.
/// </summary>
internal class ByteSlider : CustomSlider {

	public ByteSlider(
		string name, Action<byte> storeVal, Func<byte> loadVal,
		byte min = byte.MinValue, byte max = byte.MaxValue
	)
	: base(name, null, null, min, max)
	{
		EnsureValidRange();
		StoreValue = f => storeVal((byte)Mathf.Clamp((int)f, minValue, maxValue));
		LoadValue = () => Mathf.Clamp(loadVal(), minValue, maxValue);
	}

	public override GameObjectRow Create(ContentArea c, Menu Instance, bool AddToList = true) {
		var res = base.Create(c, Instance, AddToList);

		Localization.AutoLocalize(label.gameObject, Name);
		slider.onValueChanged.AddListener(_ =>
			label.GetComponent<AutoLocalizeTextUI>().RefreshTextFromLocalization()
		);

		// fixing the width and position so it matches the other controls

		slider.gameObject.SetSizeDelta(new Vector2(260, 22));

		label.gameObject.RectTransform.anchoredPosition = new Vector2(-328, 0);

		valueLabel.gameObject.SetAnchors(new Vector2(1, 0.5f));
		valueLabel.transform.AsRect.anchoredPosition = new Vector2(30, 0);

		var cursors = slider.transform.Find("CursorHotspot").AsRect;
		cursors.sizeDelta = new Vector2(1000, 114);
		cursors.anchoredPosition = new Vector2(-300, 0);

		return res;
	}

	public override void Update() {
		EnsureValidRange();
		base.Update();
		slider.transform.parent.AsRect.pivot = new Vector2(0.5f, 0.8f);
		label.GetComponent<AutoLocalizeTextUI>().RefreshTextFromLocalization();
	}

	void EnsureValidRange() {
		wholeNumbers = true;
		if (maxValue < minValue)
			(minValue, maxValue) = (maxValue, minValue);
		minValue = Math.Max(minValue, byte.MinValue);
		maxValue = Math.Min(maxValue, byte.MaxValue);
	}

}
