using EdgeDetection.Patches;
using Modding.Menu;
using Modding.Menu.Config;
using Satchel;
using Satchel.BetterMenus;
using UnityEngine.UI;

namespace EdgeDetection.ModMenu;

/// <summary>
/// A boolean toggle that uses the game's own localized On/Off text for the value.
/// The Name and Description properties are considered language keys for getting localized text.
/// </summary>
internal class BoolOption : Element {

	public string Description;

	public Action<bool> StoreValue;
	public Func<bool> LoadValue;

	static string[] GetOptions() => [
		Language.Language.Get("MOH_OFF", "MainMenu"),
		Language.Language.Get("MOH_ON", "MainMenu")
	];

	void Apply(MenuSetting self, int index)
		=> StoreValue(index == 1 ? true : false);
	void Refresh(MenuSetting self, bool alsoApplySetting)
		=> self.optionList.SetOptionTo(LoadValue() ? 1 : 0);

	public BoolOption(string name, string desc, Action<bool> storeVal, Func<bool> loadVal, string Id = "__UseName") : base(Id, name) {
		Name = name;
		Description = desc;
		StoreValue = storeVal;
		LoadValue = loadVal;
	}

	public override GameObjectRow Create(ContentArea c, Menu Instance, bool AddToList = true) {
		if (Name == null) throw ArgNullEx(nameof(Name));
		if (StoreValue == null) throw ArgNullEx(nameof(StoreValue));
		if (LoadValue == null) throw ArgNullEx(nameof(LoadValue));

		c.AddHorizontalOption(
			Name,
			new HorizontalOptionConfig {
				ApplySetting = Apply,
				RefreshSetting = Refresh,
				CancelAction = _ => Instance.CancelAction(),
				Description = new DescriptionInfo {
					Text = Description
				},
				Label = Name,
				Options = GetOptions(),
				Style = HorizontalOptionStyle.VanillaStyle
			},
			out var option
		);

		gameObject = option.gameObject;

		if (AddToList)
			Instance.MenuOrder.Add(new GameObjectRow(gameObject));

		OnBuilt += () => {
			if (option != null && option.menuSetting != null)
				option.menuSetting.RefreshValueFromGameSettings();
		};
		((IContainer)Parent).OnBuilt += (_, _) => OnBuiltInvoke();
		
		Localization.AutoLocalize(gameObject.Find("Label").gameObject, Name);
		Localization.AutoLocalize(gameObject.Find("Description").gameObject, Description);

		return new GameObjectRow(gameObject);
	}

	public override void Update() {
		var option = gameObject.GetComponent<MenuOptionHorizontal>();
		option.optionList = GetOptions();
		option.menuSetting.customApplySetting = Apply;
		option.menuSetting.customRefreshSetting = Refresh;
		option.menuSetting.RefreshValueFromGameSettings();
	}

	static ArgumentNullException ArgNullEx(string name)
		=> new ArgumentNullException(name, $"{name} cannot be null");

}
