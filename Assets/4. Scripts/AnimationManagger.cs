using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManagger : MonoBehaviour
{
    public GameObject _menuCamera;
    public static GameObject menuCamera;

	private void Start()
	{
		menuCamera = _menuCamera;
	}

	public static void HideUI()
	{
		UIManager.controlState = UIManager.state.animation;
	}

	public static void ShowUI()
	{
		UIManager.BackToGame();
	}

	public static IEnumerator ViewZoomInNumerator()
	{
		StartZoomIn();

		yield return new WaitForSeconds(10f);

		EndZoomIn();
	}

	public static void StartZoomIn()
	{
		HideUI();
		
		GameManager.instance.localPlayer.gameObject.GetComponentInChildren<Camera>().enabled = false;
		GameManager.instance.localPlayer.gameObject.GetComponentInChildren<AudioListener>().enabled = false;
		menuCamera.SetActive(true);
		menuCamera.GetComponent<Animator>().SetTrigger("zoomIn");
		
	}

	public static void EndZoomIn()
	{
		ShowUI();

		GameManager.instance.localPlayer.gameObject.GetComponentInChildren<Camera>().enabled = true;
		GameManager.instance.localPlayer.gameObject.GetComponentInChildren<AudioListener>().enabled = true;
		menuCamera.SetActive(false);
		menuCamera.GetComponent<Animator>().SetTrigger("stopZoomIn");
	}
}
