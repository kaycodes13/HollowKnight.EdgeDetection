using EdgeDetection.Components;
using EdgeDetection.ModMenu;
using EdgeDetection.Patches;
using EdgeDetection.Structs;
using Modding;
using Satchel.BetterMenus;
using System.Collections.Generic;
using System.Linq;

namespace EdgeDetection;

public class EdgeDetection : Mod, IGlobalSettings<Dictionary<string, PassSettings>>, ICustomMenuMod {
	#region Boilerplate

	/// <summary>
	/// Instance of the mod.
	/// </summary>
	internal static EdgeDetection Inst {
		get {
			if (field == null)
				throw new InvalidOperationException($"An instance of {nameof(EdgeDetection)} was never constructed");
			return field;
		}
		private set {
			if (field != null)
				throw new InvalidOperationException($"An instance of {nameof(EdgeDetection)} has already been constructed");
			field = value;
		}
	}

	public override string GetVersion() => $"{Utils.version}";

	#endregion

	/// <summary>
	/// Renders laplacian edge detection on black/white masks, then can be
	/// used to composite the edges only onto another texture.
	/// </summary>
	internal static Shader EdgeDetectionShader { get; private set; } = null!;

	/// <summary>
	/// Definitions for all edge detection passes which should be performed, in order.
	/// </summary>
	internal static readonly PassDef[] PassDefs = Utils.ReadJsonAsset<PassDef[]>($"pass_definitions.json");

	public EdgeDetection() : base("Edge Detection") {
		Inst = this;

		Log("Loading assets...");
		Utils.ReadAsset($"shader.bundle", stream => {
			AssetBundle bundle = AssetBundle.LoadFromStream(stream);
			EdgeDetectionShader = bundle.LoadAsset<Shader>("assets/edgedetection.shader");
			bundle.Unload(unloadAllLoadedObjects: false);
		});

		Log("Applying hooks...");
		Localization.Patch();
		ApplyObjectMods.Patch();
		CameraSetup.Patch();

		Log("Initialized!");
	}

	public override void Initialize() {}

	#region Settings

	public void OnLoadGlobal(Dictionary<string, PassSettings> settings) {
		if (settings == null || settings.Count == 0)
			return;

		Log("Applying settings...");

		if (EdgeDetectionPass.Passes.Count > 0) {
			foreach (var (id, pass) in EdgeDetectionPass.Passes) {
				if (settings.TryGetValue(id, out var s)) {
					pass.LineColor = s.Colour;
					pass.LineWidth = s.Width;
					pass.HalfResolution = s.HalfRes;
				}
			}
		} else {
			for(int i = 0; i < PassDefs.Length; i++) {
				if (settings.TryGetValue(PassDefs[i].Id, out var s)) {
					PassDefs[i] = PassDefs[i] with {
						Colour = s.Colour,
						Width = s.Width,
						HalfRes = s.HalfRes
					};
				}
			}
		}
	}

	public Dictionary<string, PassSettings> OnSaveGlobal() {
		Dictionary<string, PassSettings> settings = [];
		if (EdgeDetectionPass.Passes.Count > 0) {
			foreach(var (id, pass) in EdgeDetectionPass.Passes)
				settings[id] = new() {
					Colour = pass.LineColor,
					Width = pass.LineWidth,
					HalfRes = pass.HalfResolution
				};
		}
		else {
			foreach (var pass in PassDefs)
				settings[pass.Id] = new() {
					Colour = pass.Colour,
					Width = pass.Width,
					HalfRes = pass.HalfRes
				};
		}
		return settings;
	}

	#endregion

	#region Menu

	public bool ToggleButtonInsideMenu => false;

	public override string GetMenuButtonText() => Name;

	public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? _)
		=> SatchelMenu.GetCachedMenuScreen(modListMenu);

	Menu SatchelMenu {
		get {
			if (field == null) {
				Element[] elements = [..
					PassDefs.SelectMany(x => GenerateDetectorOptions(EdgeDetectionPass.Passes[x.Id]))
				];
				field = new(name: Inst.Name, elements);
				field.OnReflow += FirstUpdate;

				void FirstUpdate(object _, ReflowEventArgs a) {
					foreach (var elem in elements)
						elem.Update();
					field.OnReflow -= FirstUpdate;
				}
			}
			return field;
		}
	}

	/// <summary>
	/// Menu options for an <see cref="EdgeDetectionPass"/>.
	/// </summary>
	static IEnumerable<Element> GenerateDetectorOptions(EdgeDetectionPass pass) {
		HeaderPanel title = new($"{pass.Id}_NAME");

		HexColorInput colour = new(
			"LINE_COLOUR_LABEL",
			storeVal: value => pass.LineColor = value,
			loadVal: () => pass.LineColor
		);

		ByteSlider width = new(
			"LINE_WIDTH_LABEL",
			storeVal: value => pass.LineWidth = value,
			loadVal: () => pass.LineWidth,
			min: EdgeDetectionPass.WIDTH_MIN,
			max: EdgeDetectionPass.WIDTH_MAX
		);

		BoolOption halfRes = new(
			"HALF_RES_LABEL",
			"HALF_RES_DESC",
			storeVal: value => pass.HalfResolution = value,
			loadVal: () => pass.HalfResolution
		);

		return [title, colour, width, halfRes];
	}

	#endregion
}
