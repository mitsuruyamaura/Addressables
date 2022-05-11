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
	////�@���g������ÓI�t�B�[���h
	//private static Loader loader;
	//�@�ǂݍ��ރV�[�����X�g
	[SerializeField]
	private List<AssetReference> scene;
	//�@�O�̃V�[���̃C���X�^���X
	private SceneInstance previousScene;
	//�@�n���h��
	private AsyncOperationHandle<long> downloadSizeHandle;
	private AsyncOperationHandle downloadDependenciesHandle;
	private AssetReference sceneAssetReference;
	//�@�A�����[�h���邩�ǂ���
	private bool unload;
	//�@���[�h�󋵂�`����UI�̃L�����o�X
	[SerializeField]
	private Canvas loaderCanvas;
	//�@�_�E�����[�h�󋵂Ɏg�p����X���C�_�[
	[SerializeField]
	private Slider downloadSlider;
	//�@�ǂݍ��ݗ�
	[SerializeField]
	private Text downloadPercentText;
	// Loader�V�[���̃J����
	private Camera loaderSceneCamera;

	// Start is called before the first frame update
	void Start() {
		loaderSceneCamera = Camera.main;
		if (SceneManager.GetActiveScene().name == "Loader") {
			LoadScene("Title");
		}
	}

	//�@�V�[���ǂݍ��ݏ���
	public void LoadScene(string sceneName) {
		//�@�O�̃V�[��������ꍇ�̓A�����[�h����
		if (unload) {
			Addressables.UnloadSceneAsync(previousScene).Completed += OnSceneUnloaded;
		}
		StartCoroutine(Loading(sceneName));
	}

	//�@���ۂ̓ǂݍ��ݏ���
	private IEnumerator Loading(string sceneName) {

		if (sceneName == "Title") {
			sceneAssetReference = scene[0];
		} else if (sceneName == "Stage1") {
			sceneAssetReference = scene[1];
		} else if (sceneName == "Stage2") {
			sceneAssetReference = scene[2];
		}
		//�@�_�E�����[�h�T�C�Y���v�Z
		downloadSizeHandle = Addressables.GetDownloadSizeAsync(sceneAssetReference);

		if (!downloadSizeHandle.IsDone) {
			yield return downloadSizeHandle;
		}

		//�@�_�E�����[�h����A�Z�b�g�Ɉˑ��֌W�̂���A�Z�b�g���_�E�����[�h���邽�߂̃n���h�����쐬
		downloadDependenciesHandle = Addressables.DownloadDependenciesAsync(sceneAssetReference);

		//�@�L���b�V������Ă��鎞���̓��[�f�B���O�L�����o�X��\�����Ȃ�
		if (downloadDependenciesHandle.GetDownloadStatus().Percent < 0.95f) {
			loaderCanvas.enabled = true;
		}

		//�@�i���󋵂�\��(None �̏ꍇ�A�����ł����s�ł��Ȃ��A�i�s���Ă����Ԃ�\��)
		while (downloadDependenciesHandle.Status == AsyncOperationStatus.None) {
			downloadSlider.value = downloadDependenciesHandle.GetDownloadStatus().Percent;
			downloadPercentText.text = (downloadDependenciesHandle.GetDownloadStatus().Percent * 100).ToString("00.0") + "%";
			yield return null;
		}

		if (!downloadDependenciesHandle.IsDone) {
			yield return downloadDependenciesHandle;
		}

		// ���݂̃V�[���ɒǉ�����`�Ŏ��̃V�[����񓯊��Ń��[�h�B���[�h���I��������AOnSceneLoaded ���\�b�h�����s
		sceneAssetReference.LoadSceneAsync(LoadSceneMode.Additive).Completed += OnSceneLoaded;

	}

	private void OnSceneLoaded(AsyncOperationHandle<SceneInstance> obj) {

		// �n���h���̃X�e�[�^�X�������ł���ꍇ
		if (obj.Status == AsyncOperationStatus.Succeeded) {
			previousScene = obj.Result;
			unload = true;
			loaderCanvas.enabled = false;

			// Loader �V�[���̃J�����𖳌��ɂ���
			loaderSceneCamera.enabled = false;

			// �n���h���̉��
			Addressables.Release(downloadSizeHandle);
			Addressables.Release(downloadDependenciesHandle);
		} else if (obj.Status == AsyncOperationStatus.Failed) {
			Debug.Log("���s");
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
