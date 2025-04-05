using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShaderPrewarm : MonoBehaviour
{
    [SerializeField] ShaderVariantCollection collection;
    [SerializeField] private TMPro.TMP_Text stage;
    [SerializeField] Image fill;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
#if UNITY_SERVER
        yield return StartCoroutine(ServerPreload());  
#else
        yield return StartCoroutine(ClientPreload());
#endif
        
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator ClientPreload()
    {
        yield return StartCoroutine(PreloadShaders());
        yield return StartCoroutine(PreloadAssets());
    }

    IEnumerator ServerPreload()
    {
        yield return StartCoroutine(PreloadAssets());
    }
    
    IEnumerator PreloadAssets()
    {
        stage.text = "Preloading Assets...";
    
        string[] scenesToPreload = { "MainMenu","CutScene", "Game", "GameOver" };
    
        // Load each scene additively with activation disabled
        List<AsyncOperation> preloadOps = new List<AsyncOperation>();

        foreach (string scene in scenesToPreload)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            op.allowSceneActivation = false;
            preloadOps.Add(op);
        }

        // Wait for all scenes to reach 0.9 (preloaded)
        bool allReady = false;
        while (!allReady)
        {
            float totalProgress = 0f;
            allReady = true;

            foreach (AsyncOperation op in preloadOps)
            {
                totalProgress += op.progress;
                if (op.progress < 0.9f)
                    allReady = false;
            }

            fill.fillAmount = totalProgress / (0.9f * scenesToPreload.Length);
            yield return null;
        }

        Debug.Log("All scenes preloaded (0.9). Now unloading...");

        // Unload all preloaded scenes
        foreach (string scene in scenesToPreload)
        {
            yield return SceneManager.UnloadSceneAsync(scene);
        }

        Debug.Log("Preloaded scenes unloaded. Ready to start game!");
    }

    IEnumerator PreloadShaders()
    {
        stage.text = "Preloading shaders...";
        yield return new WaitForEndOfFrame();
        while (!collection.WarmUpProgressively(5))
        {
            float warmed = collection.warmedUpVariantCount;
            float prog = warmed / collection.shaderCount;
            fill.fillAmount = prog;
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForEndOfFrame();
        fill.fillAmount = 1;
        Debug.Log(collection.shaderCount);
    }
}
