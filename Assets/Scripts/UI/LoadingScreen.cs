// https://fish-networking.gitbook.io/docs/tutorials/simple/making-a-loading-screen
using FishNet;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    private void Start()
    {
        if (InstanceFinder.NetworkManager.HasInstance<LoadingScreen>())
        {
            Destroy(gameObject);
            return;
        }

        InstanceFinder.NetworkManager.RegisterInstance(this);
        DontDestroyOnLoad(gameObject);
        HideLoadingScreen();
    }

    public static void ShowLoadingScreen()
    {
        if (InstanceFinder.NetworkManager.TryGetInstance(out LoadingScreen loadingScreen))
            loadingScreen.gameObject.SetActive(true);
    }

    public static void HideLoadingScreen()
    {
        if (InstanceFinder.NetworkManager.TryGetInstance(out LoadingScreen loadingScreen))
            loadingScreen.gameObject.SetActive(false);
    }
}