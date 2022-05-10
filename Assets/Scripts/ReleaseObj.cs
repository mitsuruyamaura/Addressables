using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ReleaseObj : MonoBehaviour
{
    private void OnDestroy() {
        Addressables.Release(gameObject);
        Debug.Log("ƒŠƒ\[ƒX‰ğ•ú");
    }
}
