using Language;
using Modding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EdgeDetection.Patches;

/// <summary>
/// Loads localized strings and fakes a language sheet for them in a way
/// similar to Silksong's i18n, which i miss dearly.
/// Also fixes a couple problems with menu titles and buttons that
/// don't auto-localize and should.
/// </summary>
internal static class Localization {

	internal static void Patch() {
		On.Language.Language.Get_string_string += LanguageGetHook;
		UIManager.EditMenus += FixMenuLocalizationHook;
	}

	#region API

	internal const string SHEET = "Mods.kaycodes13.EdgeDetection";

	/// <summary>
	/// Adds an <see cref="AutoLocalizeTextUI"/> component to the given UI element, using this mod's language sheet.
	/// </summary>
	internal static void AutoLocalize(GameObject go, string key)
		=> AutoLocalize(go, SHEET, key);

	/// <summary>
	/// Adds an <see cref="AutoLocalizeTextUI"/> component to the given UI element.
	/// </summary>
	internal static void AutoLocalize(GameObject go, string sheet, string key) {
		var x = go.AddComponentIfNotPresent<AutoLocalizeTextUI>();
		x.textField = go.GetComponent<UnityEngine.UI.Text>();
		x.sheetTitle = sheet;
		x.textKey = key;
	}

	#endregion
	#region Hooks

	/// <summary>
	/// Fakes this mod having its own language sheet.
	/// Doing this with an On hook instead of a ModHooks hook to prevent constant warnings in Player.log.
	/// </summary>
	private static string LanguageGetHook(On.Language.Language.orig_Get_string_string orig, string key, string sheetTitle) {
		if (sheetTitle == SHEET)
			return Localized(key);
		return orig(key, sheetTitle);
	}

	/// <summary>
	/// Fixes this mod's title and back button not being properly localized.
	/// Also fixes the Mods menu's back button.
	/// </summary>
	private static void FixMenuLocalizationHook() {
		UIManager.instance.StartCoroutine(Coro());

		IEnumerator Coro() {
			yield return null;
			var uiCanvas = UIManager.instance.UICanvas.transform;

			// The Mods menu's own Back button :|
			AutoLocalize(
				uiCanvas.Find("ModListMenu/Control/BackButton/Label").gameObject,
				"MainMenu", "NAV_BACK"
			);

			// This mod's open-menu button
			AutoLocalize(
				uiCanvas.Find($"ModListMenu/Content/ScrollMask/ScrollingPane/{Inst.Name}_Settings/Label").gameObject,
				"MOD_TITLE"
			);

			// This mod's title and back button
			AutoLocalize(
				uiCanvas.Find($"{Inst.Name}/Title").gameObject,
				"MOD_TITLE"
			);
			AutoLocalize(
				uiCanvas.Find($"{Inst.Name}/Control/BackButton/Label").gameObject,
				"MainMenu", "NAV_BACK"
			);
		}
	}

	#endregion
	#region Internals

	/// <summary>
	/// Returns this mod's localized string for the given key in the current language,
	/// with English as a fallback if that key doesn't exist.
	/// </summary>
	static string Localized(string key) {
		if (
			Langs.TryGetValue(Language.Language.CurrentLanguage(), out var current)
			&& current.Value.TryGetValue(key, out var currStr)
		) {
			return currStr;
		}
		else if (
			Langs.TryGetValue(LanguageCode.EN, out var en)
			&& en.Value.TryGetValue(key, out var enStr)
		) {
			return enStr;
		}
		return $"### {SHEET}/{key} ###";
	}

	/// <summary>
	/// Lazily loaded localized strings.
	/// </summary>
	static Dictionary<LanguageCode, Lazy<Dictionary<string, string>>> Langs {
		get {
			if (field != null)
				return field;

			field = [];
			var paths = Utils.asm.GetManifestResourceNames().Where(x => x.Contains("Languages"));

			foreach (string path in paths) {
				string codeStr = path.Split('.').Reverse().ElementAt(1);
				try {
					var code = (LanguageCode)Enum.Parse(typeof(LanguageCode), codeStr, ignoreCase: true);
					field[code] = new(() => Utils.ReadJsonAsset<Dictionary<string, string>>(path));
				} catch (ArgumentException ex) {
					Inst.LogError(ex);
				}
			}
			return field;
		}
	}

	#endregion
}
