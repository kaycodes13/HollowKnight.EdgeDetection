using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class CreateAssetBundle {
	
	static readonly BuildTarget[] targets = new BuildTarget[] {
		BuildTarget.StandaloneWindows64,
		BuildTarget.StandaloneOSX,
		BuildTarget.StandaloneLinux64,
	};
	
	static readonly string directory = $"{Application.dataPath}/../AssetBundles";
	
	[MenuItem("Assets/Create Shader Bundle")]
    static void BuildShaderBundle() {
		try {
			Directory.CreateDirectory(directory);
			
			var ab = new AssetBundleBuild();
			ab.assetBundleName = "shader";
			ab.assetNames = Directory.EnumerateFiles("Assets/", "*.shader", SearchOption.AllDirectories).ToArray();
			
			var builds = new AssetBundleBuild[] {ab};
			
			foreach (var target in targets) {
				string path = Path.Combine(directory, target.ToString());
				Directory.CreateDirectory(path);
				try {
					BuildPipeline.BuildAssetBundles(
						path,
						builds,
						BuildAssetBundleOptions.AssetBundleStripUnityVersion,
						target
					);
				}
				catch (Exception e) { Debug.LogError(e); }
			}
		}
		catch (Exception e) { Debug.LogError(e); }
	}
	
}
