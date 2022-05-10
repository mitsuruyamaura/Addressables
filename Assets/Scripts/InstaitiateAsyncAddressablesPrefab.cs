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
        // InstatiateAsync �̏ꍇ�A�C���X�^���X�������ƎQ�ƃJ�E���g��������̂ŁA�X�̃Q�[���I�u�W�F�N�g�� Addressables.Release ���������X�N���v�g���A�^�b�`���ĉ��������
        Addressables.InstantiateAsync(cubePrefab, new(0, 0, 0), Quaternion.identity).Completed += Loaded;
        Addressables.InstantiateAsync(cubePrefab, new(1, 0, 0), Quaternion.identity).Completed += Loaded;

        Invoke("Delete", 5.0f);
    }

    /// <summary>
    /// ���[�h��̏���
    /// </summary>
    /// <param name="obj"></param>
    public void Loaded(AsyncOperationHandle<GameObject> obj) {
        if (obj.Status == AsyncOperationStatus.Succeeded) {
            cubeInstanceList.Add(obj.Result);
        }
    }

    /// <summary>
    /// �폜
    /// </summary>
    public void Delete() {
        foreach (var item in cubeInstanceList) {
            Destroy(item);
        }
        cubeInstanceList.Clear();
    }
}
