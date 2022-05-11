using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonScript : MonoBehaviour
{
	public enum SceneName {
		Title,
		Stage1,
		Stage2,
	}

	//　LoaderシーンのLoaderスクリプト
	private SceneLoader loader;
	//　ボタン１
	private Button button1;
	//　ボタン２
	private Button button2;
	[SerializeField]
	private SceneName sceneName;

	// Start is called before the first frame update
	void Start() {
		loader = FindObjectOfType<SceneLoader>();
		button1 = transform.Find("Button1").GetComponent<Button>();
		button2 = transform.Find("Button2").GetComponent<Button>();

		//　設定したsceneNameに応じてボタンが押されたら読み込むシーンを変更する
		if (sceneName == SceneName.Title) {
			button1.onClick.AddListener(() => loader.LoadScene("Stage1"));
			button2.onClick.AddListener(() => loader.LoadScene("Stage2"));
		} else if (sceneName == SceneName.Stage1) {
			button1.onClick.AddListener(() => loader.LoadScene("Title"));
			button2.onClick.AddListener(() => loader.LoadScene("Stage2"));
		} else if (sceneName == SceneName.Stage2) {
			button1.onClick.AddListener(() => loader.LoadScene("Title"));
			button2.onClick.AddListener(() => loader.LoadScene("Stage1"));
		}
	}
}
