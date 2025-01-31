using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

// かめくめさん
// https://gametukurikata.com/basic/addressables

public class InstantiateAddressablesPrefab : MonoBehaviour
{
    [SerializeField]
    private AssetReference cubePrefab;   // Addressables からロードする方法により、型を変える

    [SerializeField]
    private AssetReferenceGameObject spherePrefab;

    private AsyncOperationHandle<GameObject> cubePrefabHandle;      // Addressables のアセットをロードする際のハンドル
    private AsyncOperationHandle<GameObject> spherePrerfabHandle;   // 使用が終わったら解放する

    private GameObject loadCubePrefab;     // Addressables のアセットをロードした結果のゲームオブジェクトを入れておく。これを使ってインスタンス化する
    private GameObject loadShperePrefab; 

    [SerializeField]
    private List<GameObject> instancedCubeObjList = new ();

    [SerializeField]
    private List<GameObject> instancedSphereObjList = new ();


    async void Start()
    {
        var token = this.GetCancellationTokenOnDestroy();

        cubePrefabHandle = cubePrefab.InstantiateAsync(Vector3.one, Quaternion.identity);
        await cubePrefabHandle.Task;

        Addressables.LoadAssetAsync<GameObject>(cubePrefab)   // cubePrefab をロードし、終了後、Completed の中を処理する
            .Completed += (obj) => {
                cubePrefabHandle = obj;        // ロードしたハンドル obj を Handle に保持する
                loadCubePrefab = obj.Result;   // ロードしたアセットを Prefab に保持する
                instancedCubeObjList.Add(Instantiate(loadCubePrefab, Vector3.zero, Quaternion.identity));
                instancedCubeObjList.Add(Instantiate(loadCubePrefab, new (1, 0 ,0), Quaternion.identity));
            };

        spherePrefab.LoadAssetAsync<GameObject>().Completed += Loaded;  // やり方が違うだけで、こちらの方法も上と同じ(メソッド化しているか、どうか)

        await UniTask.Delay(10, false, PlayerLoopTiming.Update, token);
        Delete();

        instancedCubeObjList.Add(Instantiate(loadCubePrefab, Vector3.zero, Quaternion.identity));

        //Invoke("Delete", 10.0f);
    }

    /// <summary>
    /// ハンドルのステータスを確認後、インスタンスする
    /// </summary>
    /// <param name="obj"></param>
    public void Loaded(AsyncOperationHandle<GameObject> obj) {
        if (obj.Status == AsyncOperationStatus.Succeeded) {
            spherePrerfabHandle = obj;
            loadShperePrefab = obj.Result;
            instancedSphereObjList.Add(Instantiate(loadShperePrefab, new(0, 1, 0), Quaternion.identity));
            instancedSphereObjList.Add(Instantiate(loadShperePrefab, new(1, 1, 0), Quaternion.identity));
        }
    }

    /// <summary>
    /// 削除と解放
    /// </summary>
    public void Delete() {
        foreach (var item in instancedCubeObjList) {
            Destroy(item);
        }
        foreach (var item in instancedSphereObjList) {
            Destroy(item);
        }

        instancedCubeObjList.Clear();
        instancedSphereObjList.Clear();

        // 各ハンドルの解放
        Addressables.ReleaseInstance(cubePrefabHandle);
        Addressables.Release(spherePrerfabHandle);
    }
}
