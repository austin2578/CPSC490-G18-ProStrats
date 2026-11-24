using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace ProjectRogue.UI
{
    // One persistent owner for menu navigation, loading and pause. No run data persists here.
    [DefaultExecutionOrder(-1000)]
    public sealed class MenuFlow : MonoBehaviour
    {
        public enum Screen { Title, Playing, Pause, Settings, ConfirmQuit, Death, Complete, Loading }
        public string titleScene = "Assets/Scenes/TitleScreen.unity";
        public string gameplayScene = "Assets/PlayableDemo/PlayableArena.unity";
        public static MenuFlow Instance { get; private set; }
        public Screen Current { get; private set; }
        public bool GameplayInputAllowed => Current == Screen.Playing && !waitingForRelease;
        public bool IsLoading { get; private set; }

        readonly Dictionary<Screen, GameObject> panels = new Dictionary<Screen, GameObject>();
        readonly Dictionary<Screen, GameObject> firstFocus = new Dictionary<Screen, GameObject>();
        Camera menuCamera;
        Canvas canvas;
        Screen settingsReturn;
        GameObject settingsOpener;
        UnityEngine.UI.Slider volume;
        UnityEngine.UI.Text volumeText, settingsNotice;
        float resumeScale = 1, previousVolume;
        bool previousListenerPause, ownsPause, waitingForRelease;
        Coroutine resumeRoutine;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            previousVolume = AudioListener.volume;
            AudioListener.volume = PlayerPrefs.GetFloat("ProjectRogue.MasterVolume", previousVolume);
            BuildUI();
            Show(Screen.Title);
        }

        void Update()
        {
            if (!IsLoading && Current != Screen.Playing && Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                var selected = EventSystem.current.currentSelectedGameObject;
                var selectable = selected != null ? selected.GetComponent<UnityEngine.UI.Selectable>() : null;
                if (selectable != null)
                {
                    var next = Keyboard.current.shiftKey.isPressed ? selectable.FindSelectableOnUp() : selectable.FindSelectableOnDown();
                    if (next != null) EventSystem.current.SetSelectedGameObject(next.gameObject);
                }
            }
            if (IsLoading || Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;
            switch (Current)
            {
                case Screen.Playing: PauseGame(); break;
                case Screen.Pause: ResumeGame(); break;
                case Screen.Settings: CloseSettings(); break;
                case Screen.ConfirmQuit: CancelQuit(); break;
            }
        }

        public void StartGame() { if (!IsLoading && Current == Screen.Title) StartCoroutine(Load(gameplayScene, true)); }
        public void Retry() { if (!IsLoading && (Current == Screen.Death || Current == Screen.Complete)) StartCoroutine(Load(gameplayScene, true)); }
        public void PauseGame()
        {
            if (Current != Screen.Playing || IsLoading) return;
            AcquirePause(); Show(Screen.Pause);
        }

        public void ResumeGame()
        {
            if (Current != Screen.Pause || IsLoading || resumeRoutine != null) return;
            // Consume the menu-closing frame and held attack/movement inputs before releasing gameplay.
            resumeRoutine = StartCoroutine(ResumeAfterRelease());
        }

        IEnumerator ResumeAfterRelease()
        {
            waitingForRelease = true;
            Show(Screen.Playing);
            yield return null;
            while (GameplayKeysHeld()) yield return null;
            ReleasePause(); waitingForRelease = false; resumeRoutine = null;
        }

        static bool GameplayKeysHeld()
        {
            var k = Keyboard.current;
            return k != null && (k.aKey.isPressed || k.dKey.isPressed || k.leftArrowKey.isPressed ||
                k.rightArrowKey.isPressed || k.spaceKey.isPressed || k.leftShiftKey.isPressed ||
                k.jKey.isPressed || k.kKey.isPressed || k.digit1Key.isPressed || k.digit2Key.isPressed ||
                k.digit3Key.isPressed || k.mKey.isPressed || k.nKey.isPressed);
        }

        void AcquirePause()
        {
            if (ownsPause) return;
            resumeScale = Time.timeScale;
            previousListenerPause = AudioListener.pause;
            ownsPause = true; Time.timeScale = 0; AudioListener.pause = true;
        }

        void ReleasePause()
        {
            if (!ownsPause) return;
            Time.timeScale = resumeScale; AudioListener.pause = previousListenerPause; ownsPause = false;
        }

        public void OpenSettings()
        {
            if (Current != Screen.Title && Current != Screen.Pause) return;
            settingsReturn = Current;
            settingsOpener = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            volume.SetValueWithoutNotify(AudioListener.volume);
            UpdateVolumeText(volume.value); settingsNotice.text = "Back discards changes that have not been applied.";
            Show(Screen.Settings);
        }

        public void ApplySettings()
        {
            if (Current != Screen.Settings) return;
            AudioListener.volume = volume.value;
            PlayerPrefs.SetFloat("ProjectRogue.MasterVolume", volume.value); PlayerPrefs.Save();
            settingsNotice.text = "Master volume applied and saved.";
        }

        public void CloseSettings()
        {
            if (Current != Screen.Settings) return;
            Show(settingsReturn);
            if (settingsOpener != null && settingsOpener.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(settingsOpener);
        }

        public void RequestQuitToTitle() { if (Current == Screen.Pause) Show(Screen.ConfirmQuit); }
        public void CancelQuit() { if (Current == Screen.ConfirmQuit) Show(Screen.Pause); }
        public void ConfirmQuit() { if (Current == Screen.ConfirmQuit && !IsLoading) StartCoroutine(Load(titleScene, false)); }
        public void ResultsToTitle()
        {
            if ((Current == Screen.Death || Current == Screen.Complete) && !IsLoading)
                StartCoroutine(Load(titleScene, false));
        }

        public void EndDemo(bool completed)
        {
            if (Current != Screen.Playing || IsLoading) return;
            AcquirePause(); Show(completed ? Screen.Complete : Screen.Death);
        }

        public void QuitApplication()
        {
            if (Current != Screen.Title) return;
            Application.Quit();
        }

        IEnumerator Load(string path, bool gameplay)
        {
            if (!Application.CanStreamedLevelBeLoaded(path))
            {
                Debug.LogError("Menu scene is missing from Build Profiles: " + path, this); yield break;
            }
            IsLoading = true; AcquirePause(); Show(Screen.Loading);
            var operation = SceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
            while (!operation.isDone) yield return null;
            // Scene destruction discards all arena actors, cooldowns and resources.
            ReleasePause(); Time.timeScale = 1;
            IsLoading = false; waitingForRelease = true;
            Show(gameplay ? Screen.Playing : Screen.Title);
            yield return null;
            while (GameplayKeysHeld()) yield return null;
            waitingForRelease = false;
        }

        void Show(Screen screen)
        {
            Current = screen;
            foreach (var item in panels) item.Value.SetActive(item.Key == screen);
            menuCamera.enabled = screen == Screen.Title || (screen == Screen.Settings && settingsReturn == Screen.Title) || screen == Screen.Loading;
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(firstFocus.TryGetValue(screen, out var focus) ? focus : null);
        }

        void BuildUI()
        {
            var cameraGo = new GameObject("Menu backdrop camera"); cameraGo.transform.SetParent(transform);
            menuCamera = cameraGo.AddComponent<Camera>(); menuCamera.clearFlags = CameraClearFlags.SolidColor;
            cameraGo.AddComponent<AudioListener>();
            menuCamera.backgroundColor = new Color(.13f,.14f,.23f); menuCamera.cullingMask = 0;
            var canvasGo = new GameObject("Menu Canvas", typeof(RectTransform)); canvasGo.transform.SetParent(transform, false);
            canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280,720); scaler.matchWidthOrHeight = .5f;
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            var events = new GameObject("Menu EventSystem"); events.transform.SetParent(transform);
            events.AddComponent<EventSystem>();
            var input = events.AddComponent<InputSystemUIInputModule>(); input.AssignDefaultActions();

            var title = Panel(Screen.Title, "PROJECT ROGUE", "PLAYABLE PROTOTYPE");
            Button(title, "Start game", StartGame);
            Button(title, "Settings", OpenSettings);
            var quit = Button(title, "Quit", QuitApplication);
            if (Application.isEditor) { quit.interactable = false; Text(title, "Quit is available in the standalone build.", 18, 36); }
            Text(title, "Move · Jump · Fight · Reach the exit", 20, 38);

            var pause = Panel(Screen.Pause, "PAUSED", "Your current room is preserved.");
            Button(pause, "Resume", ResumeGame);
            Button(pause, "Controls", OpenSettings);
            Button(pause, "Settings", OpenSettings);
            Button(pause, "Quit to menu", RequestQuitToTitle);

            var settings = Panel(Screen.Settings, "SETTINGS & CONTROLS", "Keyboard prototype · Escape returns to the previous screen");
            volumeText = Text(settings, "Master volume", 22, 34);
            volume = Slider(settings);
            volume.onValueChanged.AddListener(UpdateVolumeText);
            Text(settings, "Music / effects mixing: unavailable in this demo", 18, 32);
            Text(settings, "A/D or arrows  Move     Space  Jump     Shift  Dash\nJ  Slash     K  Arc strike     Escape  Pause\nArrows / Tab  Menu focus     Enter / Space  Select", 20, 82);
            settingsNotice = Text(settings, "", 18, 40);
            Button(settings, "Apply", ApplySettings);
            Button(settings, "Back", CloseSettings);
            firstFocus[Screen.Settings] = volume.gameObject;

            var confirm = Panel(Screen.ConfirmQuit, "LEAVE THIS ROOM?", "Current room progress will be lost. There is no run save.");
            Button(confirm, "Cancel", CancelQuit);
            Button(confirm, "Quit to menu", ConfirmQuit);
            var death = Panel(Screen.Death, "YOU DIED", "Start fresh with full health, mana and three enemies.");
            Button(death, "Try again", Retry); Button(death, "Quit to menu", ResultsToTitle);
            var complete = Panel(Screen.Complete, "ROOM COMPLETE", "Arena cleared. This prototype is one room, not a full run.");
            Button(complete, "Play again", Retry); Button(complete, "Quit to menu", ResultsToTitle);
            Panel(Screen.Loading, "LOADING", "Preparing the scene…");
            WireNavigation();
        }

        RectTransform Panel(Screen key, string heading, string subtitle)
        {
            var overlay = Rect("Screen " + key, (RectTransform)canvas.transform);
            overlay.anchorMin = Vector2.zero; overlay.anchorMax = Vector2.one; overlay.offsetMin = overlay.offsetMax = Vector2.zero;
            overlay.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.08f,.09f,.15f,.96f);
            var card = Rect("Panel", overlay); card.anchorMin = card.anchorMax = card.pivot = new Vector2(.5f,.5f);
            card.sizeDelta = new Vector2(640,650);
            card.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.2f,.22f,.35f);
            var layout = card.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.padding = new RectOffset(32,32,20,20); layout.spacing = 9;
            layout.childAlignment = TextAnchor.MiddleCenter; layout.childControlWidth = true; layout.childForceExpandWidth = true;
            layout.childControlHeight = false; layout.childForceExpandHeight = false;
            panels[key] = overlay.gameObject;
            Text(card, heading, 48, 60); Text(card, subtitle, 22, 48);
            return card;
        }

        UnityEngine.UI.Button Button(RectTransform parent, string title, UnityEngine.Events.UnityAction action)
        {
            var rect = Rect(title, parent); rect.sizeDelta = new Vector2(550,52);
            rect.gameObject.AddComponent<UnityEngine.UI.Image>().color = Color.white;
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
            var colors = button.colors;
            colors.normalColor = new Color(.11f,.12f,.2f); colors.highlightedColor = new Color(.32f,.36f,.57f);
            colors.selectedColor = new Color(.34f,.4f,.64f); colors.pressedColor = new Color(.19f,.6f,.64f);
            colors.disabledColor = new Color(.16f,.17f,.22f); button.colors = colors;
            button.onClick.AddListener(action);
            var label = Text(rect, title, 24, 52); label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
            var screen = parent.parent.gameObject;
            foreach (var pair in panels) if (pair.Value == screen && !firstFocus.ContainsKey(pair.Key)) firstFocus[pair.Key] = button.gameObject;
            return button;
        }

        UnityEngine.UI.Slider Slider(RectTransform parent)
        {
            var rect = Rect("Master volume", parent); rect.sizeDelta = new Vector2(550,34);
            var slider = rect.gameObject.AddComponent<UnityEngine.UI.Slider>(); slider.minValue = 0; slider.maxValue = 1;
            var track = Rect("Track", rect); track.anchorMin = new Vector2(0,.3f); track.anchorMax = new Vector2(1,.7f); track.offsetMin = track.offsetMax = Vector2.zero;
            track.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.1f,.11f,.2f);
            var handle = Rect("Handle", rect); handle.sizeDelta = new Vector2(26,34);
            handle.anchorMin = handle.anchorMax = new Vector2(.5f,.5f);
            var image = handle.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = new Color(.45f,.8f,.87f);
            slider.handleRect = handle; slider.targetGraphic = image; slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            return slider;
        }

        void WireNavigation()
        {
            foreach (var panel in panels.Values)
            {
                var controls = new List<UnityEngine.UI.Selectable>(panel.GetComponentsInChildren<UnityEngine.UI.Selectable>(true));
                controls.RemoveAll(s => !s.interactable);
                for (int i=0; i<controls.Count; i++)
                {
                    var nav = controls[i].navigation; nav.mode = UnityEngine.UI.Navigation.Mode.Explicit;
                    nav.selectOnUp = controls[(i+controls.Count-1)%controls.Count]; nav.selectOnDown = controls[(i+1)%controls.Count];
                    controls[i].navigation = nav;
                }
            }
        }

        void UpdateVolumeText(float value) { volumeText.text = "Master volume  " + Mathf.RoundToInt(value*100) + "%"; }
        static RectTransform Rect(string name, RectTransform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false); return rect;
        }
        static UnityEngine.UI.Text Text(RectTransform parent, string value, int size, float height)
        {
            var rect = Rect("Text", parent); rect.sizeDelta = new Vector2(550,height);
            var text = rect.gameObject.AddComponent<UnityEngine.UI.Text>(); text.font = Resources.Load<Font>("Fonts/Jersey10-Regular");
            text.fontSize = size; text.color = Color.white; text.text = value; text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
            return text;
        }
        void OnDestroy()
        {
            if (Instance != this) return;
            ReleasePause(); AudioListener.volume = previousVolume; Instance = null;
        }
    }
}
