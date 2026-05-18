using UnityEngine;
// in built scene manager from Unity
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    // the container is just the thing we referenced in unity as the object in the canvas
    public GameObject container;
    // Update is called once per frame
    void Update()
    {
    }

    public void PauseButton()
    {
        // function for pausing which pauses the whole scene altogether
        Time.timeScale = 0;
        AudioListener.pause = true;
    }

    public void ResumeButton()
    {
        // function for resuming opposite to Pause()
        Time.timeScale = 1;
        AudioListener.pause = false;
    }

    /* will make it so it resumes timescale for the scene but also just restarts the whole scene
    i think that its a bit difficult that this prototype is made in one whole scene but maybe we can create levels later down the line -Randy
    */
    public void MainMenuButton()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainVRScene");
    }

    public void ExitButton()
    {
        Application.Quit();
    }
}
