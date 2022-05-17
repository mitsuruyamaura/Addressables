using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

// ���߂��߂���
// https://gametukurikata.com/basic/addressables

public class InstantiateAddressablesPrefab : MonoBehaviour
{
    [SerializeField]
    private AssetReference cubePrefab;   // Addressables ���烍�[�h������@�ɂ��A�^��ς���

    [SerializeField]
    private AssetReferenceGameObject spherePrefab;

    private AsyncOperationHandle<GameObject> cubePrefabHandle;      // Addressables �̃A�Z�b�g�����[�h����ۂ̃n���h��
    private AsyncOperationHandle<GameObject> spherePrerfabHandle;   // �g�p���I�������������

    private GameObject loadCubePrefab;     // Addressables �̃A�Z�b�g�����[�h�������ʂ̃Q�[���I�u�W�F�N�g�����Ă����B������g���ăC���X�^���X������
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

        Addressables.LoadAssetAsync<GameObject>(cubePrefab)   // cubePrefab �����[�h���A�I����ACompleted �̒�����������
            .Completed += (obj) => {
                cubePrefabHandle = obj;        // ���[�h�����n���h�� obj �� Handle �ɕێ�����
                loadCubePrefab = obj.Result;�@ // ���[�h�����A�Z�b�g�� Prefab �ɕێ�����
                instancedCubeObjList.Add(Instantiate(loadCubePrefab, Vector3.zero, Quaternion.identity));
                instancedCubeObjList.Add(Instantiate(loadCubePrefab, new (1, 0 ,0), Quaternion.identity));
            };

        spherePrefab.LoadAssetAsync<GameObject>().Completed += Loaded;�@// �������Ⴄ�����ŁA������̕��@����Ɠ���(���\�b�h�����Ă��邩�A�ǂ���)

        await UniTask.Delay(10, false, PlayerLoopTiming.Update, token);
        Delete();

        instancedCubeObjList.Add(Instantiate(loadCubePrefab, Vector3.zero, Quaternion.identity));

        //Invoke("Delete", 10.0f);
    }

    /// <summary>
    /// �n���h���̃X�e�[�^�X���m�F��A�C���X�^���X����
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
    /// �폜�Ɖ��
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

        // �e�n���h���̉��
        Addressables.ReleaseInstance(cubePrefabHandle);
        Addressables.Release(spherePrerfabHandle);
    }
}
