using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public string LoadScene;

    void OnTriggerEnter()
    {
        SceneManager.LoadScene(LoadScene);
    }

}
