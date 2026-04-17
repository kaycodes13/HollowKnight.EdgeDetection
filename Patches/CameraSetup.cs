using EdgeDetection.Components;

namespace EdgeDetection.Patches;

internal static class CameraSetup {

	internal static void Patch()
		=> On.GameCameras.Start += CameraStartHook;

	static void CameraStartHook(On.GameCameras.orig_Start orig, GameCameras self) {
		orig(self);

		foreach (var pass in PassDefs) {
			var detector = self.mainCamera.gameObject.AddComponent<EdgeDetectionPass>();
			(detector.Id,
			detector.LineColor,
			detector.LineWidth,
			detector.HalfResolution,
			detector.Layers,
			detector.AlphaThreshold,
			detector.ClipFar,
			detector.ClipNear,
			detector.ExcludePass) = pass;
		}
	}

}
