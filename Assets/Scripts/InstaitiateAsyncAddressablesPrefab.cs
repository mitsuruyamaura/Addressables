using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class InstaitiateAsyncAddressablesPrefab : MonoBehaviour
{
    [SerializeField]
    private AssetReference cubePrefab;

    private List<GameObject> cubeInstanceList = new ();

    
    void Start()
    {
        // InstatiateAsync の場合、インスタンス化されると参照カウントが増えるので、個々のゲームオブジェクトに Addressables.Release を書いたスクリプトをアタッチして解放させる
        Addressables.InstantiateAsync(cubePrefab, new(0, 0, 0), Quaternion.identity).Completed += Loaded;
        Addressables.InstantiateAsync(cubePrefab, new(1, 0, 0), Quaternion.identity).Completed += Loaded;

        Invoke("Delete", 5.0f);
    }

    /// <summary>
    /// ロード後の処理
    /// </summary>
    /// <param name="obj"></param>
    public void Loaded(AsyncOperationHandle<GameObject> obj) {
        if (obj.Status == AsyncOperationStatus.Succeeded) {
            cubeInstanceList.Add(obj.Result);
        }
    }

    /// <summary>
    /// 削除
    /// </summary>
    public void Delete() {
        foreach (var item in cubeInstanceList) {
            Destroy(item);
        }
        cubeInstanceList.Clear();
    }
}
