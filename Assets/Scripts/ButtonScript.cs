using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneName {
	Title,
	Stage1,
	Stage2,
}

public class ButtonScript : MonoBehaviour
{
	// Loader �V�[���� SceneLoader �X�N���v�g
	private SceneLoader loader;
	// �{�^���P

	private Button button1;
	// �{�^���Q

	private Button button2;
	[SerializeField]

	private SceneName sceneName;

	void Start() {
		loader = FindObjectOfType<SceneLoader>();
		button1 = transform.Find("Button1").GetComponent<Button>();
		button2 = transform.Find("Button2").GetComponent<Button>();

		//�@�ݒ肵��sceneName�ɉ����ă{�^���������ꂽ��ǂݍ��ރV�[����ύX����
		if (sceneName == SceneName.Title) {
			button1.onClick.AddListener(() => loader.LoadScene(SceneName.Stage1.ToString()));
			button2.onClick.AddListener(() => loader.LoadScene(SceneName.Stage2.ToString()));
		} else if (sceneName == SceneName.Stage1) {
			button1.onClick.AddListener(() => loader.LoadScene(SceneName.Title.ToString()));
			button2.onClick.AddListener(() => loader.LoadScene(SceneName.Stage2.ToString()));
		} else if (sceneName == SceneName.Stage2) {
			button1.onClick.AddListener(() => loader.LoadScene(SceneName.Title.ToString()));
			button2.onClick.AddListener(() => loader.LoadScene(SceneName.Stage1.ToString()));
		}
	}
}
