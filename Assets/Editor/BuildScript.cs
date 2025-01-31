using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.Build.Reporting;

public static class BuildScript {

    [System.Obsolete]
    public static void Build() {

        // Addressables ビルド
        string addressablesPath = BuildAddressables();
        Debug.Log($"Addressables build output: {addressablesPath}");

        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, ""); // Android 用設定 

        // Jenkins 上の Editor command line arguments でバージョン指定
        const string BUILD_VERSION = "buildVersion";
        string buildVersion = CommandLineArgs.GetValue(BUILD_VERSION) ?? "0.1"; // デフォルト値: 0.1
        Debug.Log($"🔹 ビルドバージョン: {buildVersion}");

        PlayerSettings.bundleVersion = buildVersion; // アプリのバージョン設定

        const string PRODUCT_NAME = "productName";
        string productName = CommandLineArgs.GetValue(PRODUCT_NAME) ?? Application.productName;
        PlayerSettings.productName = productName;

        string path = $"../output/{productName}.apk"; // 出力先を APK ファイルに変更
        FolderCreate(Path.GetDirectoryName(path)); // フォルダ生成


        BuildPlayerOptions buildOption = new BuildPlayerOptions {
            options = BuildOptions.CompressWithLz4, // 圧縮オプションを追加
            scenes = GetAllScenePaths(),
            target = BuildTarget.Android, // Android 用ターゲット
            locationPathName = path,
        };
        UnityEditor.Build.Reporting.BuildReport reports = BuildPipeline.BuildPlayer(buildOption);

        bool isSuccess = (reports.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded);

        Debug.Log("isSuccess:" + isSuccess);
        //if (isToolBar) { return; } // ツールバーからの実行だったら閉じたくないので終了

        AssetDatabase.SaveAssets();
        //EditorApplication.Exit(isSuccess ? 0 : 1); // 上手く動かなかった時の検出に使う

        switch (reports.summary.result) {
            case BuildResult.Succeeded:
                Debug.Log("Build succeeded.");
                EditorApplication.Exit(0); // 正常終了
                break;

            case BuildResult.Failed:
                Debug.LogError("Build failed.");
                EditorApplication.Exit(1); // エラー終了
                break;

            case BuildResult.Cancelled:
                Debug.LogError("Build canceled.");
                EditorApplication.Exit(1); // キャンセルもエラー扱い
                break;

            default:
                Debug.LogError("Build result unknown.");
                EditorApplication.Exit(1); // 不明な場合もエラー終了
                break;
        }
    }

    /// <summary>
    /// 出力先フォルダ生成
    /// </summary>
    /// <param name="path"></param>
    public static void FolderCreate(string path) {
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
    }

    // GetAllScenePaths
    public static string[] GetAllScenePaths() {
        var levels = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        return levels;
    }

    public static string BuildAddressables() {
        Debug.Log("Building Addressables...");

        // Addressables の設定を取得
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) {
            Debug.LogError("AddressableAssetSettings is not found.");
            return string.Empty;
        }

        // Remote Profile を設定
        string remoteProfileName = "Remote Profile"; // Remote Profile の名前
        string profileId = settings.profileSettings.GetProfileId(remoteProfileName);

        if (string.IsNullOrEmpty(profileId)) {
            Debug.LogError($"Profile '{remoteProfileName}' not found.");
            return string.Empty;
        }

        settings.activeProfileId = profileId; // Remote Profile を適用
        Debug.Log($"Using Addressables Profile: {remoteProfileName}");

        // Addressables のビルドを実行
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);

        if (!string.IsNullOrEmpty(result.Error)) {
            Debug.LogError("Addressables build failed: " + result.Error);
            return string.Empty;
        }

        Debug.Log("Addressables build completed.");
        return result.OutputPath;
    }


    [MenuItem("Build/Build Develop App")]
    [System.Obsolete]
    public static void BuildApp() {
        Build();
    }
}