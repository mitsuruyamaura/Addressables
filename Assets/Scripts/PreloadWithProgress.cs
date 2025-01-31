using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 未使用
/// 公式のアセットデータのダウンロード処理
/// 依存関係の構築とキャッシュクリア
/// </summary>
internal class PreloadWithProgress : MonoBehaviour {

    public string preloadLabel = "preload";
    public UnityEvent<float> ProgressEvent;
    public UnityEvent<bool> CompletionEvent;
    private AsyncOperationHandle downloadHandle;
    private CancellationTokenSource cts;


    private void Start() {
        cts = new CancellationTokenSource();
        PreloadAssetsAsync(cts.Token).Forget(); // Fire-and-forget でタスクを開始
    }

    private async UniTask PreloadAssetsAsync(CancellationToken token) {
        try {
            downloadHandle = Addressables.DownloadDependenciesAsync(preloadLabel, false);
            float progress = 0;

            // 進捗をポーリングして更新
            while (!downloadHandle.IsDone) {
                float percentageComplete = downloadHandle.GetDownloadStatus().Percent;

                if (percentageComplete > progress * 1.1f) // 少なくとも10%ごとに更新
                {
                    progress = percentageComplete;
                    ProgressEvent?.Invoke(progress);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token); // 毎フレーム進捗をチェック
            }

            // 完了状態を通知
            CompletionEvent?.Invoke(downloadHandle.Status == AsyncOperationStatus.Succeeded);
        } catch (OperationCanceledException) {
            Debug.LogWarning("Preloading cancelled.");
        } finally {
            // Handle をリリース
            if (downloadHandle.IsValid()) {
                Addressables.Release(downloadHandle);
            }
        }
    }

    private void OnDestroy() {
        cts?.Cancel(); // タスクをキャンセル
        cts?.Dispose();
    }
}