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

        // Background panel matching game's light theme
        panel = new GameObject("Panel");
        panel.transform.SetParent(canvas.transform, false);
        Image bg = panel.AddComponent<Image>();
        // Light beige/cream to match game background
        bg.color = new Color(0.95f, 0.94f, 0.90f, 0.98f);
        RectTransform rect = panel.GetComponent<RectTransform>();
        // Make the keyboard panel anchored to the bottom center
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.sizeDelta = new Vector2(720, 220); // Proper size to fit all keys
        rect.anchoredPosition = new Vector2(0, 0); // sits at bottom edge

        // Create a container for all keyboard content
        GameObject content = new GameObject("KeyboardContent");
        content.transform.SetParent(panel.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        // Header text matching game theme
        GameObject headerObj = new GameObject("Header");    
        headerObj.transform.SetParent(content.transform, false);
        capsStatus = headerObj.AddComponent<Text>();
        capsStatus.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        capsStatus.fontSize = 14;
        capsStatus.fontStyle = FontStyle.Bold;
        capsStatus.alignment = TextAnchor.MiddleCenter;
        capsStatus.color = new Color(0.3f, 0.3f, 0.35f, 1f); // Dark gray text for light background
        RectTransform headerRect = capsStatus.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0.5f, 1);
        headerRect.anchorMax = new Vector2(0.5f, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.sizeDelta = new Vector2(500, 20);
        headerRect.anchoredPosition = new Vector2(0, -5);

        // Close button
        GameObject close = CreateButton("×", Vector2.zero, content.transform, 25.0f);
        RectTransform closeRect = close.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.pivot = new Vector2(1, 1);
        closeRect.anchoredPosition = new Vector2(-5, -8);
        close.GetComponent<Button>().onClick.AddListener(HideKeyboard);

        // Grid of keys with compact spacing
        GameObject gridObj = new GameObject("Grid");
        gridObj.transform.SetParent(content.transform, false);
        RectTransform gRect = gridObj.AddComponent<RectTransform>();
        gRect.anchorMin = new Vector2(0.5f, 0.5f);
        gRect.anchorMax = new Vector2(0.5f, 0.5f);
        gRect.pivot = new Vector2(0.5f, 0.5f);
        gRect.sizeDelta = new Vector2(680, 155);
        gRect.anchoredPosition = new Vector2(0, -20);
        GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(64, 36); // Optimized key size
        grid.spacing = new Vector2(4, 3); // Tight spacing
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 10;

        // Keys - cleaner layout with essential keys only
        string[] rows = {
            "0 1 2 3 4 5 6 7 8 9",
            "Q W E R T Y U I O P",
            "A S D F G H J K L DEL",
            "Z X C V B N M @ . CAPS"
        };
        foreach (string row in rows)
        {
            foreach (string k in row.Split(' '))
            {
                if (string.IsNullOrWhiteSpace(k)) continue;
                GameObject key = CreateButton(k, Vector2.zero, gridObj.transform, 64.0f);
                Button btn = key.GetComponent<Button>();
                
                // Style special keys differently to match light theme
                if (k == "CAPS" || k == "DEL")
                {
                    Image img = key.GetComponent<Image>();
                    img.color = new Color(0.5f, 0.6f, 0.8f, 1f); // Light blue for special keys
                    Text txt = key.GetComponentInChildren<Text>();
                    if (txt != null) txt.color = Color.white;
                    
                    ColorBlock cb = btn.colors;
                    cb.normalColor = new Color(0.5f, 0.6f, 0.8f, 1f);
                    cb.highlightedColor = new Color(0.6f, 0.7f, 0.9f, 1f);
                    cb.pressedColor = new Color(0.4f, 0.5f, 0.7f, 1f);
                    btn.colors = cb;
                }
                
                btn.onClick.AddListener(() => OnKeyPress(k));
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
        
        // Light colored buttons matching game theme
        img.color = new Color(1f, 1f, 1f, 1f); // White buttons
        
        RectTransform rect = b.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(size, size);
        rect.anchoredPosition = pos;

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(b.transform, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = 16; // Compact font size
        txt.fontStyle = FontStyle.Bold;
        txt.color = new Color(0.2f, 0.2f, 0.25f, 1f); // Dark text for readability
        RectTransform tRect = txt.GetComponent<RectTransform>();
        tRect.sizeDelta = new Vector2(size, size);

        // Button colors matching light game theme
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(1f, 1f, 1f, 1f); // White
        cb.highlightedColor = new Color(0.85f, 0.9f, 0.95f, 1f); // Light blue-gray
        cb.pressedColor = new Color(0.75f, 0.8f, 0.85f, 1f);
        cb.selectedColor = new Color(0.9f, 0.92f, 0.96f, 1f);
        cb.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.2f; // Smooth color transitions
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
        {
            capsStatus.text = caps ? "● CAPS ON" : "○ Caps Off";
        }
    }
}
