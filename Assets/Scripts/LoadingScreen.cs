using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup _fadePanel;

    [SerializeField] private Image _loadingBar;
    [SerializeField] private TMP_Text _loadingText;

    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private float _minimumLoadingTime = 0.5f;
    [SerializeField] private string _loadingTextFormat = "Loading... {0}%";

    private static LoadingScreen _instance;
    private bool _isLoading;

    public static LoadingScreen Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LoadingScreen>();
                if (_instance == null)
                {
                    Debug.LogError("No LoadingScreen found in the scene!");
                }
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeScreen();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeScreen()
    {
        _fadePanel.alpha = 0;
        _fadePanel.gameObject.SetActive(false);
        if (_loadingBar != null) _loadingBar.fillAmount = 0;
        if (_loadingText != null) _loadingText.text = string.Format(_loadingTextFormat, 0);
    }

    public void LoadScene(string sceneName)
    {
        if (!_isLoading)
        {
            StartCoroutine(LoadSceneRoutine(sceneName));
        }
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        _isLoading = true;
        float startTime = Time.time;

        _fadePanel.gameObject.SetActive(true);
        yield return _fadePanel.DOFade(1, _fadeDuration).WaitForCompletion();

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            if (_loadingBar != null)
            {
                _loadingBar.DOFillAmount(progress, 0.1f);
            }

            if (_loadingText != null)
            {
                _loadingText.text = string.Format(_loadingTextFormat, Mathf.Round(progress * 100));
            }

            if (asyncOperation.progress >= 0.9f && Time.time - startTime >= _minimumLoadingTime)
            {
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        yield return _fadePanel.DOFade(0, _fadeDuration).WaitForCompletion();
        _fadePanel.gameObject.SetActive(false);

        _isLoading = false;
    }

    private void OnDestroy()
    {
        DOTween.Kill(transform);
    }
}

public static class SceneLoader
{
    public static void LoadScene(string sceneName)
    {
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("LoadingScreen instance not found!");
            SceneManager.LoadScene(sceneName);
        }
    }
}