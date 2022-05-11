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
	//// ���g������ÓI�t�B�[���h
	//private static Loader loader;

	// �ǂݍ��ރV�[�����X�g
	[SerializeField]
	private List<AssetReference> scene;

	// �O�̃V�[���̃C���X�^���X�B�V�[�����A�����[�h���邽�߂ɕێ�����
	private SceneInstance previousScene;

	// �n���h��
	private AsyncOperationHandle<long> downloadSizeHandle;     // �_�E�����[�h����A�Z�b�g�o���h���̐���ɗ��p����n���h��
	private AsyncOperationHandle downloadDependenciesHandle;   // �_�E�����[�h����A�Z�b�g�Ɉˑ�����A�Z�b�g�̃_�E�����[�h�Ɏg�p����n���h��

	private AssetReference sceneAssetReference;    // �V�[���ǂݍ��݂ɗ��p

	// �A�����[�h���邩�ǂ���
	private bool unload;

	// ���[�h�󋵂�`����UI�̃L�����o�X
	[SerializeField]
	private Canvas loaderCanvas;

	// �_�E�����[�h�󋵂Ɏg�p����X���C�_�[
	[SerializeField]
	private Slider downloadSlider;

	// �ǂݍ��ݗ�
	[SerializeField]
	private Text downloadPercentText;

	// Loader �V�[���̃J����
	private Camera loaderSceneCamera;

	private string loadSceneName;


	void Start() {
		loaderSceneCamera = Camera.main;
		if (SceneManager.GetActiveScene().name == "Loader") {
			LoadScene(SceneName.Title.ToString());
		}
	}

	//�@�V�[���ǂݍ��ݏ���
	public void LoadScene(string sceneName) {
		loadSceneName = sceneName;

		// �O�̃V�[��������ꍇ�̓A�����[�h����
		if (unload) {
			// previousScene ���A�����[�h�B�A�����[�h���I�����Ă���AOnSceneUnloaded �����s
			Addressables.UnloadSceneAsync(previousScene).Completed += OnSceneUnloaded;
		}
		StartCoroutine(Loading(sceneName));
	}

	//�@���ۂ̓ǂݍ��ݏ���
	private IEnumerator Loading(string sceneName) {

		// sceneAssetReference �̃Z�b�g
		if (sceneName == SceneName.Title.ToString()) {
			sceneAssetReference = scene[0];
		} else if (sceneName == SceneName.Stage1.ToString()) {
			sceneAssetReference = scene[1];
		} else if (sceneName == SceneName.Stage2.ToString()) {
			sceneAssetReference = scene[2];
		}

		// �ǂݍ��ރV�[���̃_�E�����[�h�T�C�Y���v�Z
		downloadSizeHandle = Addressables.GetDownloadSizeAsync(sceneAssetReference);

		// �n���h���̃��[�h���������Ă��Ȃ��ꍇ
		if (!downloadSizeHandle.IsDone) {
			yield return downloadSizeHandle;
		}

		// �_�E�����[�h����A�Z�b�g�Ɉˑ��֌W�̂���A�Z�b�g���_�E�����[�h���邽�߂̃n���h�����쐬
		downloadDependenciesHandle = Addressables.DownloadDependenciesAsync(sceneAssetReference);

		// �_�E�����[�h�̏󋵂� 95�� ��菬�������̂݃L�����o�X�\���B�L���b�V������Ă��鎞���̓��[�f�B���O�L�����o�X��\�����Ȃ�
		if (downloadDependenciesHandle.GetDownloadStatus().Percent < 0.95f) {
			loaderCanvas.enabled = true;
		}

		// �ˑ��֌W�̂���A�Z�b�g���_�E�����[�h���Ă���i���󋵂�\��(None �̏ꍇ�A�����ł����s�ł��Ȃ��A�i�s���Ă����Ԃ�\��)
		while (downloadDependenciesHandle.Status == AsyncOperationStatus.None) {
			downloadSlider.value = downloadDependenciesHandle.GetDownloadStatus().Percent;
			downloadPercentText.text = (downloadDependenciesHandle.GetDownloadStatus().Percent * 100).ToString("00.0") + "%";
			yield return null;
		}

		// �n���h���̃��[�h���������Ă��Ȃ��ꍇ
		if (!downloadDependenciesHandle.IsDone) {
			yield return downloadDependenciesHandle;
		}

		// ���݂̃V�[���ɒǉ�����`�Ŏ��̃V�[����񓯊��Ń��[�h�B���[�h���I��������AOnSceneLoaded ���\�b�h�����s
		sceneAssetReference.LoadSceneAsync(LoadSceneMode.Additive).Completed += OnSceneLoaded;
	}

	/// <summary>
	/// �V�[���̃��[�h
	/// </summary>
	/// <param name="obj"></param>
	private void OnSceneLoaded(AsyncOperationHandle<SceneInstance> obj) {

		// �V�[���̃��[�h�ɐ������Ă�����(�n���h���̃X�e�[�^�X�������ł���ꍇ)
		if (obj.Status == AsyncOperationStatus.Succeeded) {
			previousScene = obj.Result;

			// ���̃V�[����ǂݍ��ލۂɁA���݂̃V�[�����A�����[�h����悤�ɃZ�b�g����
			unload = true;

			// �L�����o�X����
			loaderCanvas.enabled = false;

			// Loader �V�[���̃J�����𖳌�(���̃V�[���ɉe�������邽��)
			loaderSceneCamera.enabled = false;

			// �n���h���̉��
			Addressables.Release(downloadSizeHandle);
			Addressables.Release(downloadDependenciesHandle);
		} else if (obj.Status == AsyncOperationStatus.Failed) {
			Debug.Log("���s");
		}

	}

	/// <summary>
	/// �V�[���̃A�����[�h
	/// </summary>
	/// <param name="obj"></param>
	private void OnSceneUnloaded(AsyncOperationHandle<SceneInstance> obj) {

		// �V�[���̃A�����[�h�ɐ������Ă�����(�n���h���̃X�e�[�^�X�������ł���ꍇ)
		if (obj.Status == AsyncOperationStatus.Succeeded) {
			unload = false;

			// ��̃C���X�^���X����
			previousScene = new SceneInstance();

			// ���̃V�[����ǂݍ���ł���i����������悤�ɂ��邽�߃J������L����
			loaderSceneCamera.enabled = true;
		}
	}
}
