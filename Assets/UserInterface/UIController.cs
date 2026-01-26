using UnityEngine;
using UnityEngine.UIElements;

namespace UserInterface
{
    public class UIController : MonoBehaviour
    {
        private VisualElement ui;
        private Button generateMapBtn;
        private Slider tileSize;
    
        private WaveFunctionCollapseManager collapseManager;
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            ui = GetComponent<UIDocument>().rootVisualElement;
            collapseManager = FindFirstObjectByType<WaveFunctionCollapseManager>();
        }

        private void OnEnable()
        {
            generateMapBtn = ui.Q<Button>("btn-generate");
            tileSize = ui.Q<Slider>("tile-size");
        
            generateMapBtn.clicked += () => collapseManager.StartMapGeneration(tileSize.value);
        }
    }
}
