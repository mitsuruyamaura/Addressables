using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
	////　自身を入れる静的フィールド
	//private static Loader loader;
	//　読み込むシーンリスト
	[SerializeField]
	private List<AssetReference> scene;
	//　前のシーンのインスタンス
	private SceneInstance previousScene;
	//　ハンドル
	private AsyncOperationHandle<long> downloadSizeHandle;
	private AsyncOperationHandle downloadDependenciesHandle;
	private AssetReference sceneAssetReference;
	//　アンロードするかどうか
	private bool unload;
	//　ロード状況を伝えるUIのキャンバス
	[SerializeField]
	private Canvas loaderCanvas;
	//　ダウンロード状況に使用するスライダー
	[SerializeField]
	private Slider downloadSlider;
	//　読み込み率
	[SerializeField]
	private Text downloadPercentText;
	// Loaderシーンのカメラ
	private Camera loaderSceneCamera;

	// Start is called before the first frame update
	void Start() {
		loaderSceneCamera = Camera.main;
		if (SceneManager.GetActiveScene().name == "Loader") {
			LoadScene("Title");
		}
	}

	//　シーン読み込み処理
	public void LoadScene(string sceneName) {
		//　前のシーンがある場合はアンロード処理
		if (unload) {
			Addressables.UnloadSceneAsync(previousScene).Completed += OnSceneUnloaded;
		}
		StartCoroutine(Loading(sceneName));
	}

	//　実際の読み込み処理
	private IEnumerator Loading(string sceneName) {

		if (sceneName == "Title") {
			sceneAssetReference = scene[0];
		} else if (sceneName == "Stage1") {
			sceneAssetReference = scene[1];
		} else if (sceneName == "Stage2") {
			sceneAssetReference = scene[2];
		}
		//　ダウンロードサイズを計算
		downloadSizeHandle = Addressables.GetDownloadSizeAsync(sceneAssetReference);

		if (!downloadSizeHandle.IsDone) {
			yield return downloadSizeHandle;
		}

		//　ダウンロードするアセットに依存関係のあるアセットをダウンロードするためのハンドルを作成
		downloadDependenciesHandle = Addressables.DownloadDependenciesAsync(sceneAssetReference);

		//　キャッシュされている時等はローディングキャンバスを表示しない
		if (downloadDependenciesHandle.GetDownloadStatus().Percent < 0.95f) {
			loaderCanvas.enabled = true;
		}

		//　進捗状況を表示(None の場合、成功でも失敗でもなく、進行している状態を表す)
		while (downloadDependenciesHandle.Status == AsyncOperationStatus.None) {
			downloadSlider.value = downloadDependenciesHandle.GetDownloadStatus().Percent;
			downloadPercentText.text = (downloadDependenciesHandle.GetDownloadStatus().Percent * 100).ToString("00.0") + "%";
			yield return null;
		}

		if (!downloadDependenciesHandle.IsDone) {
			yield return downloadDependenciesHandle;
		}

		// 現在のシーンに追加する形で次のシーンを非同期でロード。ロードが終了したら、OnSceneLoaded メソッドを実行
		sceneAssetReference.LoadSceneAsync(LoadSceneMode.Additive).Completed += OnSceneLoaded;

	}

	private void OnSceneLoaded(AsyncOperationHandle<SceneInstance> obj) {

		// ハンドルのステータスが成功である場合
		if (obj.Status == AsyncOperationStatus.Succeeded) {
			previousScene = obj.Result;
			unload = true;
			loaderCanvas.enabled = false;

			// Loader シーンのカメラを無効にする
			loaderSceneCamera.enabled = false;

			// ハンドルの解放
			Addressables.Release(downloadSizeHandle);
			Addressables.Release(downloadDependenciesHandle);
		} else if (obj.Status == AsyncOperationStatus.Failed) {
			Debug.Log("失敗");
		}

	}

	private void OnSceneUnloaded(AsyncOperationHandle<SceneInstance> obj) {
		if (obj.Status == AsyncOperationStatus.Succeeded) {
			unload = false;
			previousScene = new SceneInstance();
			loaderSceneCamera.enabled = true;
		}
	}
}
