using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;

/// <summary>
/// スクリプタブルオブジェクトにアサインしている Stage プレハブの Addressables のパスを自動登録する
/// </summary>
public static class PrefabAddressSetter {
    [MenuItem("Tools/Set Addressables Prefab Addresses")]

    public static void SetPrefabAddresses() {
        // StageDataSO 型の全てのスクリプタブルオブジェクトを検索し、アセットの GUID（一意の識別子）の配列が返る
        string[] guids = AssetDatabase.FindAssets("t:StageDataSO");

        foreach (string guid in guids) {
            // アセットの GUID から、そのアセットが保存されているパス(Assets/Path/To/Asset の形式)を取得
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // 指定したパスのアセットを読み込む。インスタンスが存在しない場合には null が返る
            StageDataSO stageDataSO = AssetDatabase.LoadAssetAtPath<StageDataSO>(path);

            if (stageDataSO == null) continue;

            // stageDataList 内の各 StageData に対して処理
            foreach (StageData stageData in stageDataSO.stageDataList) {

                // プレハブがアサインされている場合
                if (stageData.stage != null) {

                    // プレハブのパスを取得(指定されたアセットが保存されているファイルパスを取得)
                    string prefabPath = AssetDatabase.GetAssetPath(stageData.stage.gameObject);

                    // パスが取得できた場合
                    if (!string.IsNullOrEmpty(prefabPath)) {

                        // Addressables からアドレスを取得(Addressables 設定ファイルへの参照を取得するプロパティ)
                        // Settings プロパティからは Addressables に関する設定（アドレスやグループの管理など）にアクセス可能
                        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

                        // 指定した GUID に対応する AddressableAssetEntry（アセットのエントリ情報）を取得
                        // 引数には Addressables に登録されたアセットの GUID を指定。これにより、特定の Addressable アセットの設定にアクセス可能
                        // 今回の場合、Stage クラスがアタッチされているプレハブのゲームオブジェクトに関連する AddressableAssetEntry を取得
                        AddressableAssetEntry entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(prefabPath));

                        if (entry != null) {
                            // スクリプタブルオブジェクトのデータに各プレハブに設定されている Addressables のアドレスを自動設定
                            stageData.prefabAddress = entry.address;

                            // 指定したオブジェクトの変更を Unity エディタに知らせるメソッド
                            // 対象オブジェクトが変更されたとマークされ、次回のセーブで保存対象となる
                            // ScriptableObject やプレハブなどのアセットに対して、コードで変更を加えた場合に使用することでスクリプタブルオブジェクトを更新
                            EditorUtility.SetDirty(stageDataSO);
                            Debug.Log($"アドレスを設定: {stageData.prefabAddress}");
                        } else {
                            Debug.Log($"Addressables エントリが見つかりませんでした: {prefabPath}");
                        }
                    }
                }
            }
        }
        // アセットの変更内容を保存する。スクリプトやエディタ拡張でアセットに変更を加えた場合、このメソッドで保存を確実に行う
        AssetDatabase.SaveAssets();

        // アセットデータベースの更新を行い、プロジェクトウィンドウ(インスペクター)に最新の状態を反映
        AssetDatabase.Refresh();
    }
}