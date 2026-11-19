using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace ProjectRogue.Movement
{
    [RequireComponent(
        typeof(ClassTraversal),
        typeof(ElementPassives),
        typeof(PlayerClassController))]
    public sealed class PrototypeLoadoutPanel : MonoBehaviour
    {
        [FormerlySerializedAs("weaponIcons")]
        [SerializeField] private Sprite[] classIcons = new Sprite[3];

        [SerializeField] private Sprite[] elementIcons = new Sprite[3];

        private ClassTraversal traversal;
        private ElementPassives elements;
        private PlayerClassController classController;

        private UnityEngine.UI.Text status;
        private GameObject canvasObject;
        private GameObject ownedEventSystem;

        private readonly UnityEngine.UI.Image[] buttons =
            new UnityEngine.UI.Image[6];

        private Font font;


        private void Start()
        {
            traversal = GetComponent<ClassTraversal>();
            elements = GetComponent<ElementPassives>();
            classController = GetComponent<PlayerClassController>();

            font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf");

            CreateInterface();
        }


        private void CreateInterface()
        {
            canvasObject = new GameObject(
                "Prototype Loadout",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(UnityEngine.UI.CanvasScaler),
                typeof(UnityEngine.UI.GraphicRaycaster));

            Canvas canvas =
                canvasObject.GetComponent<Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = 20;

            var scaler =
                canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();

            scaler.uiScaleMode =
                UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(1920, 1080);

            scaler.matchWidthOrHeight = 0.5f;


            // Main panel
            var panel = Rect(
                "Panel",
                canvasObject.transform,
                Vector2.zero,
                new Vector2(680, 260));

            // Anchor the entire panel to the TOP RIGHT.
            panel.anchorMin = new Vector2(1f, 1f);
            panel.anchorMax = new Vector2(1f, 1f);
            panel.pivot = new Vector2(1f, 1f);

            panel.anchoredPosition =
                new Vector2(-20f, -20f);

            var panelBackground =
                panel.gameObject
                    .AddComponent<UnityEngine.UI.Image>();

            panelBackground.color =
                new Color(
                    0.055f,
                    0.065f,
                    0.09f,
                    0.96f);


            // Header
            UnityEngine.UI.Text title = Label(
                "PROJECT ROGUE - LOADOUT",
                panel,
                new Vector2(18, -12),
                new Vector2(640, 30),
                20);

            title.fontStyle =
                FontStyle.Bold;


            // Row labels
            UnityEngine.UI.Text classLabel = Label(
                "CLASS",
                panel,
                new Vector2(18, -55),
                new Vector2(75, 35),
                16);

            classLabel.fontStyle =
                FontStyle.Bold;

            UnityEngine.UI.Text elementLabel = Label(
                "ELEMENT",
                panel,
                new Vector2(18, -105),
                new Vector2(75, 35),
                16);

            elementLabel.fontStyle =
                FontStyle.Bold;


            string[] classes =
            {
                "Knight",
                "Mage",
                "Thief"
            };

            string[] elementNames =
            {
                "Fire",
                "Water",
                "Wind"
            };


            // Buttons
            for (int i = 0; i < 3; i++)
            {
                int index = i;

                MakeButton(
                    classes[i],
                    classIcons[i],
                    panel,
                    new Vector2(
                        105 + i * 180,
                        -52),
                    i,
                    () => classController.SelectClass(index));

                MakeButton(
                    elementNames[i],
                    elementIcons[i],
                    panel,
                    new Vector2(
                        105 + i * 180,
                        -102),
                    i + 3,
                    () => elements.SelectElement(index));
            }


            // Status background
            var statusBackground = Rect(
                "Status Background",
                panel,
                new Vector2(18, -158),
                new Vector2(644, 84));

            statusBackground.gameObject
                .AddComponent<UnityEngine.UI.Image>()
                .color = new Color(
                    0.025f,
                    0.03f,
                    0.045f,
                    0.95f);


            status = Label(
                "",
                statusBackground,
                new Vector2(12, -8),
                new Vector2(620, 70),
                15);

            status.alignment =
                TextAnchor.UpperLeft;

            status.horizontalOverflow =
                HorizontalWrapMode.Wrap;

            status.verticalOverflow =
                VerticalWrapMode.Overflow;


            if (FindFirstObjectByType<EventSystem>() == null)
            {
                ownedEventSystem = new GameObject(
                    "Prototype EventSystem",
                    typeof(EventSystem),
                    typeof(InputSystemUIInputModule));

                ownedEventSystem
                    .GetComponent<InputSystemUIInputModule>()
                    .AssignDefaultActions();
            }
        }


        private void Update()
        {
            if (status == null)
                return;

            ClassKind selectedClass =
                classController.CurrentClass != null
                    ? classController.CurrentClass.classKind
                    : traversal.SelectedClass;


            // Button selection colors
            for (int i = 0; i < buttons.Length; i++)
            {
                bool selected =
                    i < 3
                        ? i == (int)selectedClass
                        : i - 3 == (int)elements.SelectedElement;

                buttons[i].color =
                    selected
                        ? new Color(
                            0.18f,
                            0.42f,
                            0.58f)
                        : new Color(
                            0.12f,
                            0.14f,
                            0.19f);
            }


            string traversalText =
                selectedClass == ClassKind.Mage
                    ? "Airborne Shift: downward slam"
                    : selectedClass == ClassKind.Thief
                        ? "Grounded Shift: backward hop"
                        : "Shift: forward dash";


            string readyText =
                traversal.IsUsingUtility
                    ? "ACTIVE"
                    : traversal.CooldownRemaining > 0
                        ? $"Cooldown {traversal.CooldownRemaining:0.0}s"
                        : "Ready";


            string elementText =
                elements.SelectedElement == ElementKind.Fire
                    ? "Fire - burn effect placeholder"
                    : elements.SelectedElement == ElementKind.Wind
                        ? "Wind - attack speed placeholder"
                        : "Water - passive placeholder";


            string profileText =
                classController.CurrentClass != null
                    ? BuildProfileText(
                        classController.CurrentClass)
                    : "No class profile selected.";


            status.text =
                $"{profileText}\n" +
                $"Traversal: {traversalText} [{readyText}]\n" +
                $"Element: {elementText}";
        }


        private static string BuildProfileText(
            ClassDefinition definition)
        {
            return
                $"{definition.displayName}  |  " +
                $"VIG {definition.vigor}   " +
                $"STR {definition.strength}   " +
                $"DEX {definition.dexterity}   " +
                $"MND {definition.mind}   " +
                $"CON {definition.constitution}  |  " +
                $"HP {definition.maxHealth:0}   " +
                $"STA {definition.maxStamina:0}   " +
                $"MANA {definition.maxMana:0}";
        }


        private static RectTransform Rect(
            string name,
            Transform parent,
            Vector2 position,
            Vector2 size)
        {
            RectTransform rect =
                new GameObject(
                    name,
                    typeof(RectTransform))
                    .GetComponent<RectTransform>();

            rect.SetParent(
                parent,
                false);

            rect.anchorMin =
                new Vector2(0f, 1f);

            rect.anchorMax =
                new Vector2(0f, 1f);

            rect.pivot =
                new Vector2(0f, 1f);

            rect.anchoredPosition =
                position;

            rect.sizeDelta =
                size;

            return rect;
        }


        private UnityEngine.UI.Text Label(
            string text,
            Transform parent,
            Vector2 position,
            Vector2 size,
            int fontSize)
        {
            UnityEngine.UI.Text label =
                Rect(
                    "Label",
                    parent,
                    position,
                    size)
                    .gameObject
                    .AddComponent<UnityEngine.UI.Text>();

            label.font =
                font;

            label.fontSize =
                fontSize;

            label.text =
                text;

            label.color =
                Color.white;

            label.raycastTarget =
                false;

            return label;
        }


        private void MakeButton(
            string name,
            Sprite sprite,
            Transform parent,
            Vector2 position,
            int index,
            UnityEngine.Events.UnityAction action)
        {
            RectTransform rect = Rect(
                name,
                parent,
                position,
                new Vector2(
                    165,
                    38));


            UnityEngine.UI.Image image =
                rect.gameObject
                    .AddComponent<UnityEngine.UI.Image>();

            buttons[index] =
                image;


            UnityEngine.UI.Button button =
                rect.gameObject
                    .AddComponent<UnityEngine.UI.Button>();

            button.targetGraphic =
                image;

            button.navigation =
                new UnityEngine.UI.Navigation
                {
                    mode =
                        UnityEngine.UI.Navigation.Mode.None
                };

            button.onClick.AddListener(
                action);


            // Icon
            UnityEngine.UI.Image icon = Rect(
                "Icon",
                rect,
                new Vector2(
                    7,
                    -4),
                new Vector2(
                    30,
                    30))
                .gameObject
                .AddComponent<UnityEngine.UI.Image>();

            icon.sprite =
                sprite;

            icon.color =
                GetIconColor(index);

            icon.preserveAspect =
                true;

            icon.raycastTarget =
                false;


            UnityEngine.UI.Text buttonLabel = Label(
                name,
                rect,
                new Vector2(
                    45,
                    -6),
                new Vector2(
                    112,
                    27),
                17);

            buttonLabel.alignment =
                TextAnchor.MiddleLeft;
        }


        private static Color GetIconColor(int index)
        {
            switch (index)
            {
                // Fire
                case 3:
                    return new Color(
                        1f,
                        0.35f,
                        0.08f);

                // Water
                case 4:
                    return new Color(
                        0.15f,
                        0.6f,
                        1f);

                // Wind
                case 5:
                    return new Color(
                        0.55f,
                        0.95f,
                        0.75f);

                // Thief
                case 2:
                    return new Color(
                        0.75f,
                        0.55f,
                        0.32f);

                default:
                    return new Color(
                        0.85f,
                        0.87f,
                        0.9f);
            }
        }


        private void OnDestroy()
        {
            if (canvasObject != null)
                Destroy(canvasObject);

            if (ownedEventSystem != null)
                Destroy(ownedEventSystem);
        }
    }
}