using EdgeDetection.Patches;
using Modding.Menu;
using Satchel.BetterMenus;
using Satchel.BetterMenus.Config;
using UnityEngine.UI;

namespace EdgeDetection.ModMenu;

/// <summary>
/// A TextPanel but the text is bigger and italic.
/// The Name property is considered a language key for getting localized text.
/// </summary>
internal class HeaderPanel : TextPanel {
	public HeaderPanel(string name) : base(name, fontSize: 60) {
		Config = new AdditionalTextPanelConfig {
			fontStyle = FontStyle.Italic
		};
	}

	public override GameObjectRow Create(ContentArea c, Menu Instance, bool AddToList = true) {
		var res = base.Create(c, Instance, AddToList);

		Localization.AutoLocalize(gameObject, Name);

		return res;
	}
}
