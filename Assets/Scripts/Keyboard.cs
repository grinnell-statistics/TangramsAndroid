using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // uses UnityEngine.UI.InputField now
using UnityEngine.EventSystems;
public class DynamicKeyboard : MonoBehaviour
{
    [Header("Input fields that trigger keyboard")]
    public InputField playerInput;
    public InputField groupInput;

    private Canvas canvas;
    private GameObject panel;
    private Text capsStatus;
    private bool caps;
    private InputField activeField;

void Awake()
{
    // Ensure no field is auto-selected on scene load
    if (EventSystem.current != null)
        EventSystem.current.SetSelectedGameObject(null);

    if (canvas != null)
        canvas.gameObject.SetActive(false);
}

void Start()
{
    AddClickListener(playerInput);
    AddClickListener(groupInput);
    HideKeyboard();
    StartCoroutine(ForceDeselect());
    
}

System.Collections.IEnumerator ForceDeselect()
{
    yield return new WaitForSeconds(0.01f);
    EventSystem.current.SetSelectedGameObject(null);
    canvas.gameObject.SetActive(false);
    
}



void AddClickListener(InputField field)
{
    EventTrigger trigger = field.gameObject.GetComponent<EventTrigger>();
    if (trigger == null)
        trigger = field.gameObject.AddComponent<EventTrigger>();

    var entry = new EventTrigger.Entry();
    entry.eventID = EventTriggerType.Select;
    entry.callback.AddListener((data) => { ShowKeyboard(field); });
    trigger.triggers.Add(entry);
}


    void ShowKeyboard(InputField target)
    {
        activeField = target;
        if (canvas == null) CreateKeyboard();
        canvas.gameObject.SetActive(true);
        UpdateCapsText();
    }

    void HideKeyboard()
    {
        if (canvas) canvas.gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    void CreateKeyboard()
    {
        // Create the keyboard canvas
        GameObject c = new GameObject("DynamicKeyboard");
        canvas = c.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // ensure it's on top
        c.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        c.AddComponent<GraphicRaycaster>();

        // Background panel
        panel = new GameObject("Panel");
        panel.transform.SetParent(canvas.transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.85f, 0.83f, 0.92f, 0.95f);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        // Make the keyboard panel larger and anchored to the bottom center
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.sizeDelta = new Vector2(700, 360); // wide enough to show everything
        rect.anchoredPosition = new Vector2(0, 50); // sits slightly above bottom edge

        // Create a container for all keyboard content
        GameObject content = new GameObject("KeyboardContent");
        content.transform.SetParent(panel.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        // Header text
        GameObject headerObj = new GameObject("Header");    
        headerObj.transform.SetParent(content.transform, false);
        capsStatus = headerObj.AddComponent<Text>();
        capsStatus.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        capsStatus.fontSize = 36;
        capsStatus.alignment = TextAnchor.MiddleCenter;
        RectTransform headerRect = capsStatus.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0.5f, 1);
        headerRect.anchorMax = new Vector2(0.5f, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.sizeDelta = new Vector2(800, 60);
        headerRect.anchoredPosition = new Vector2(0, -20);

        // Close button
        GameObject close = CreateButton("X", Vector2.zero, content.transform, 60);
        RectTransform closeRect = close.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.pivot = new Vector2(1, 1);
        closeRect.anchoredPosition = new Vector2(-20, -20);
        close.GetComponent<Button>().onClick.AddListener(HideKeyboard);

        // Grid of keys
        GameObject gridObj = new GameObject("Grid");
        gridObj.transform.SetParent(content.transform, false);
        RectTransform gRect = gridObj.AddComponent<RectTransform>();
        gRect.anchorMin = new Vector2(0.5f, 0.5f);
        gRect.anchorMax = new Vector2(0.5f, 0.5f);
        gRect.pivot = new Vector2(0.5f, 0.5f);
        gRect.sizeDelta = new Vector2(760, 260);
        gRect.anchoredPosition = new Vector2(0, -60);
        GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(60, 60);
        grid.spacing = new Vector2(5, 5);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 10;

        // Keys
        string[] rows = {
            "0 1 2 3 4 5 6 7 8 9",
            "Q W E R T Y U I O P",
            "A S D F G H J K L DEL",
            "Z X C V B N M @ . - _ CAPS"
        };
        foreach (string row in rows)
        {
            foreach (string k in row.Split(' '))
            {
                if (string.IsNullOrWhiteSpace(k)) continue;
                GameObject key = CreateButton(k, Vector2.zero, gridObj.transform, 70);
                key.GetComponent<Button>().onClick.AddListener(() => OnKeyPress(k));
            }
        }

        UpdateCapsText();
        canvas.gameObject.SetActive(false);

        
    }

    GameObject CreateButton(string label, Vector2 pos, Transform parent, float size)
    {
        GameObject b = new GameObject(label);
        b.transform.SetParent(parent, false);
        Button btn = b.AddComponent<Button>();
        Image img = b.AddComponent<Image>();
        img.color = Color.white;
        RectTransform rect = b.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(size, size);
        rect.anchoredPosition = pos;

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(b.transform, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = 24;
        txt.color = Color.black;
        RectTransform tRect = txt.GetComponent<RectTransform>();
        tRect.sizeDelta = new Vector2(size, size);

        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.8f, 0.8f, 0.8f);
        btn.colors = cb;

        return b;
    }

    void OnKeyPress(string key)
    {
        if (activeField == null) return;

        switch (key)
        {
            case "DEL":
                if (activeField.text.Length > 0)
                    activeField.text = activeField.text.Remove(activeField.text.Length - 1);
                break;
            case "CAPS":
                caps = !caps;
                UpdateCapsText();
                break;
            case "ENTER":
                HideKeyboard();
                break;
            default:
                string value = caps ? key.ToUpper() : key.ToLower();
                activeField.text += value;
                break;
        }
    }

    void UpdateCapsText()
    {
        if (capsStatus != null)
            capsStatus.text = caps ? "Caps Lock is now ON" : "Caps Lock is now off";
    }
}
