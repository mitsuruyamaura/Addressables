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
	//// 自身を入れる静的フィールド
	//private static Loader loader;

	// 読み込むシーンリスト
	[SerializeField]
	private List<AssetReference> scene;

	// 前のシーンのインスタンス。シーンをアンロードするために保持する
	private SceneInstance previousScene;

	// ハンドル
	private AsyncOperationHandle<long> downloadSizeHandle;     // ダウンロードするアセットバンドルの制御に利用するハンドル
	private AsyncOperationHandle downloadDependenciesHandle;   // ダウンロードするアセットに依存するアセットのダウンロードに使用するハンドル

	private AssetReference sceneAssetReference;    // シーン読み込みに利用

	// アンロードするかどうか
	private bool unload;

	// ロード状況を伝えるUIのキャンバス
	[SerializeField]
	private Canvas loaderCanvas;

	// ダウンロード状況に使用するスライダー
	[SerializeField]
	private Slider downloadSlider;

	// 読み込み率
	[SerializeField]
	private Text downloadPercentText;

	// Loader シーンのカメラ
	private Camera loaderSceneCamera;

	private string loadSceneName;


	void Start() {
		loaderSceneCamera = Camera.main;
		if (SceneManager.GetActiveScene().name == "Loader") {
			LoadScene(SceneName.Title.ToString());
		}
	}

	//　シーン読み込み処理
	public void LoadScene(string sceneName) {
		loadSceneName = sceneName;

		// 前のシーンがある場合はアンロード処理
		if (unload) {
			// previousScene をアンロード。アンロードが終了してから、OnSceneUnloaded を実行
			Addressables.UnloadSceneAsync(previousScene).Completed += OnSceneUnloaded;
		}
		StartCoroutine(Loading(sceneName));
	}

	//　実際の読み込み処理
	private IEnumerator Loading(string sceneName) {

		// sceneAssetReference のセット
		if (sceneName == SceneName.Title.ToString()) {
			sceneAssetReference = scene[0];
		} else if (sceneName == SceneName.Stage1.ToString()) {
			sceneAssetReference = scene[1];
		} else if (sceneName == SceneName.Stage2.ToString()) {
			sceneAssetReference = scene[2];
		}

		// 読み込むシーンのダウンロードサイズを計算
		downloadSizeHandle = Addressables.GetDownloadSizeAsync(sceneAssetReference);

		// ハンドルのロードが完了していない場合
		if (!downloadSizeHandle.IsDone) {
			yield return downloadSizeHandle;
		}

		// ダウンロードするアセットに依存関係のあるアセットをダウンロードするためのハンドルを作成
		downloadDependenciesHandle = Addressables.DownloadDependenciesAsync(sceneAssetReference);

		// ダウンロードの状況が 95％ より小さい時のみキャンバス表示。キャッシュされている時等はローディングキャンバスを表示しない
		if (downloadDependenciesHandle.GetDownloadStatus().Percent < 0.95f) {
			loaderCanvas.enabled = true;
		}

		// 依存関係のあるアセットをダウンロードしている進捗状況を表示(None の場合、成功でも失敗でもなく、進行している状態を表す)
		while (downloadDependenciesHandle.Status == AsyncOperationStatus.None) {
			downloadSlider.value = downloadDependenciesHandle.GetDownloadStatus().Percent;
			downloadPercentText.text = (downloadDependenciesHandle.GetDownloadStatus().Percent * 100).ToString("00.0") + "%";
			yield return null;
		}

		// ハンドルのロードが完了していない場合
		if (!downloadDependenciesHandle.IsDone) {
			yield return downloadDependenciesHandle;
		}

		// 現在のシーンに追加する形で次のシーンを非同期でロード。ロードが終了したら、OnSceneLoaded メソッドを実行
		sceneAssetReference.LoadSceneAsync(LoadSceneMode.Additive).Completed += OnSceneLoaded;
	}

	/// <summary>
	/// シーンのロード
	/// </summary>
	/// <param name="obj"></param>
	private void OnSceneLoaded(AsyncOperationHandle<SceneInstance> obj) {

		// シーンのロードに成功していたら(ハンドルのステータスが成功である場合)
		if (obj.Status == AsyncOperationStatus.Succeeded) {
			previousScene = obj.Result;

			// 次のシーンを読み込む際に、現在のシーンをアンロードするようにセットする
			unload = true;

			// キャンバス無効
			loaderCanvas.enabled = false;

			// Loader シーンのカメラを無効(他のシーンに影響があるため)
			loaderSceneCamera.enabled = false;

			// ハンドルの解放
			Addressables.Release(downloadSizeHandle);
			Addressables.Release(downloadDependenciesHandle);
		} else if (obj.Status == AsyncOperationStatus.Failed) {
			Debug.Log("失敗");
		}

	}

	/// <summary>
	/// シーンのアンロード
	/// </summary>
	/// <param name="obj"></param>
	private void OnSceneUnloaded(AsyncOperationHandle<SceneInstance> obj) {

		// シーンのアンロードに成功していたら(ハンドルのステータスが成功である場合)
		if (obj.Status == AsyncOperationStatus.Succeeded) {
			unload = false;

			// 空のインスタンスを代入
			previousScene = new SceneInstance();

			// 次のシーンを読み込んでいる進捗を見えるようにするためカメラを有効化
			loaderSceneCamera.enabled = true;
		}
	}
}
