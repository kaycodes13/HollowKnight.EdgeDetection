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
		#if DEBUG
		Log("--- This is a development build ---");
		#endif

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

		foreach(var (id, passSettings) in settings) {
			int i = Array.FindIndex(PassDefs, x => x.Id == id);
			if (i >= 0) {
				PassDefs[i] = PassDefs[i] with {
					Colour = passSettings.Colour,
					Width = passSettings.Width,
					HalfRes = passSettings.HalfRes
				};
			}
			else
				LogError($"Failed to apply settings to {id} pass");

			if (EdgeDetectionPass.Passes.TryGetValue(id, out var component)) {
				component.LineColor = passSettings.Colour;
				component.LineWidth = passSettings.Width;
				component.HalfResolution = passSettings.HalfRes;
			}
		}

	}

	public Dictionary<string, PassSettings> OnSaveGlobal() {
		Dictionary<string, PassSettings> settings = [];
		foreach (var pass in PassDefs)
			settings[pass.Id] = new() {
				Colour = pass.Colour,
				Width = pass.Width,
				HalfRes = pass.HalfRes
			};
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
			field ??= new(name: Inst.Name, elements: [..
				PassDefs.SelectMany(x => GenerateDetectorOptions(EdgeDetectionPass.Passes[x.Id]))
			]);
			return field;
		}
	}

	/// <summary>
	/// Menu options for an <see cref="EdgeDetectionPass"/>.
	/// </summary>
	static IEnumerable<Element> GenerateDetectorOptions(EdgeDetectionPass pass) {
		int i = Array.FindIndex(PassDefs, x => x.Id == pass.Id);

		HeaderPanel title = new($"{pass.Id}_NAME");

		HexColorInput colour = new(
			"LINE_COLOUR_LABEL",
			storeVal: value => {
				pass.LineColor = value;
				PassDefs[i] = PassDefs[i] with { Colour = value };
			},
			loadVal: () => pass.LineColor
		);

		ByteSlider width = new(
			"LINE_WIDTH_LABEL",
			storeVal: value => {
				pass.LineWidth = value;
				PassDefs[i] = PassDefs[i] with { Width = value };
			},
			loadVal: () => pass.LineWidth,
			min: EdgeDetectionPass.WIDTH_MIN,
			max: EdgeDetectionPass.WIDTH_MAX
		);

		BoolOption halfRes = new(
			"HALF_RES_LABEL",
			"HALF_RES_DESC",
			storeVal: value => {
				pass.HalfResolution = value;
				PassDefs[i] = PassDefs[i] with { HalfRes = value };
			},
			loadVal: () => pass.HalfResolution
		);

		return [title, colour, width, halfRes];
	}

	#endregion
}
