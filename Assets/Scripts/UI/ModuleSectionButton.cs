using UnityEngine;
using UnityEngine.UI;
public class ModuleSectionButton : MonoBehaviour
{
    //buttons
    [SerializeField] private Button button;
    [SerializeField] private ModuleManager moduleManager;
    [SerializeField] private int moduleIndex;
    [SerializeField] private int sectionIndex;
    [SerializeField] private GameObject menuRoot;

    void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    void Start()
    {
        button.onClick.AddListener(OnButtonPressed);
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonPressed);
        }
    }

    private void OnButtonPressed()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;

        //sets the module and section to be played when play button is pressed
        moduleManager.PlayModuleSection(moduleIndex, sectionIndex);

        if (menuRoot != null)
        {
            menuRoot.SetActive(true);
        }
    }
}
