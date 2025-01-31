using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageDataSO", menuName = "Create StageDataSO")]
public class StageDataSO : ScriptableObject {
    public List<StageData> stageDataList = new();

    private bool isSettingAddresses;

    private void OnValidate() {
        // インスペクターにプレハブをアサインした際に自動的にパスを設定させる方法
        if (!isSettingAddresses) {
            isSettingAddresses = true;
            // PrefabAddressSetter クラスが正しく呼び出せているか確認
            //PrefabAddressSetter.SetPrefabAddresses();
            isSettingAddresses = false;
        }
    }
}