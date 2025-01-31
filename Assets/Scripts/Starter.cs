using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Starter : MonoBehaviour
{
    public AssetReference soundDataSOPrefab;
    public AudioSource audioSource;

    private AsyncOperationHandle operationHandle;
    private SoundDataSO soundDataSO;

    
    void Start()
    {
        Addressables.LoadAssetAsync<SoundDataSO>(soundDataSOPrefab)
            .Completed += (obj) => {
                operationHandle = obj;
                soundDataSO = obj.Result;

                audioSource.clip = soundDataSO.soundDataList[0].audioClip;
                audioSource.Play();
        };
    }

}
