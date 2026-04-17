using EdgeDetection.Patches;
using Satchel.BetterMenus;
using Satchel.BetterMenus.Config;
using System.Globalization;
using UnityEngine.UI;
using static EdgeDetection.ModMenu.MenuUtils;
using SatchelInputField = Satchel.BetterMenus.InputField;
using UInputField = UnityEngine.UI.InputField;

namespace EdgeDetection.ModMenu;

/// <summary>
/// Menu element that accepts <see cref="Color"/> input in 6-character hex strings.
/// Includes a preview swatch beside the hex code.
/// The Name property is considered a language key for getting localized text.
/// </summary>
internal class HexColorInput : SatchelInputField {

	public HexColorInput(string name, Action<Color> storeVal, Func<Color> loadVal)
		: base(
			name: name,
			_storeValue: null, _loadValue: null,
			_characterLimit: 6,
			_config: InputFieldConfig.DefaultText with {
				characterValidation = UInputField.CharacterValidation.Alphanumeric,
				contentType = UInputField.ContentType.Custom,
				saveType = InputFieldSaveType.EditEnd
			}
		)
	{
		storeValue = str => {
			Color c = HexParser(str);
			storeVal(c);
			RefreshSwatch(c);
		};

		loadValue = () => {
			Color c = loadVal();
			RefreshSwatch(c);
			return HexUnparser(c);
		};

		void RefreshSwatch(Color c) {
			// I don't know why just changing the color on the
			// old material does nothing, and I am tired.
			var img = inputField.transform.Find("Swatch").GetComponent<Image>();
			Material old = img.material;
			old.color = c;
			img.material = new Material(old);
			UObject.Destroy(old);
		}
	}

	public override GameObjectRow Create(Modding.Menu.ContentArea c, Menu Instance, bool AddToList = true) {
		GameObjectRow ret = base.Create(c, Instance, AddToList);

		Localization.AutoLocalize(label.gameObject, Name);

		inputField.gameObject.SetAnchors(new Vector2(1, 0.5f));
		inputField.gameObject.SetSizeDelta(new Vector2(230, 60));
		inputField.transform.AsRect.anchoredPosition = new Vector2(-100, 0);
		inputField.textComponent.alignment = TextAnchor.MiddleRight;

		var swatch = UIGameObject("Swatch", inputField.gameObject);

		swatch.AddComponent<Image>().material = UIMaterial(Color.white);

		var outline = swatch.AddComponent<Outline>();
		outline.effectColor = new Color(0.4f, 0.4f, 0.4f, 1);
		outline.effectDistance = new(3, 3);

		swatch.RectTransform.sizeDelta = new Vector2(40, 40);
		swatch.RectTransform.SetAnchors(new Vector2(0, 0.5f));

		return ret;
	}

	public override void Update() {
		base.Update();
		underLine.gameObject.SetActive(false);
		inputField.characterValidation = UInputField.CharacterValidation.Alphanumeric;
		inputField.onValidateInput = HexValidation;
		inputField.characterLimit = 6;
	}


	/// <summary>
	/// <see cref="UInputField"/> validation for hex codes; only accepts chars a-fA-F0-7.
	/// </summary>
	static char HexValidation(string input, int index, char addedChar)
		=> Parse($"{addedChar}", out _) ? char.ToUpper(addedChar) : '0';

	/// <summary>
	/// Parses a 6-character hex string into a <see cref="Color"/> if it can,
	/// or returns <see cref="InvalidColor"/> if it can't.
	/// </summary>
	static Color HexParser(string x) {
		x = x.Replace("#", "").Trim();

		if (x.Length < 6)
			return InvalidColor;

		var (red, grn, blu) = (x.Substring(0, 2), x.Substring(2, 2), x.Substring(4, 2));

		if (Parse(red, out var r) && Parse(grn, out var g) && Parse(blu, out var b))
			return new Color32(r, g, b, 255);
		return InvalidColor;
	}

	/// <summary>
	/// Parses a <see cref="Color"/> into a 6-character hex string if it can,
	/// or returns "######" if it can't.
	/// </summary>
	static string HexUnparser(Color c) {
		if (In01(c.r) && In01(c.g) && In01(c.b)) {
			Color32 c32 = c;
			return $"{c32.r:X2}{c32.g:X2}{c32.b:X2}";
		}
		return "######";
		static bool In01(float f) => 0 <= f && f <= 1;
	}

	static bool Parse(string s, out byte b)
		=> byte.TryParse(s, NumberStyles.HexNumber, null, out b);

	static readonly Color InvalidColor = new(-1, -1, -1, -1);

}
