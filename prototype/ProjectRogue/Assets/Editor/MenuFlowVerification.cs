using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectRogue.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Repeatable live-Editor checks. No pipeline dependency or production test shortcuts.
public static class MenuFlowVerification
{
    [MenuItem("Project Rogue/UI/Verify menu flow (Play mode)")]
    public static async void VerifyMenu()
    {
        try { Debug.Log(await Run()); }
        catch (Exception ex) { Debug.LogException(ex); }
    }

    public static async Task<string> Run()
    {
        if (!EditorApplication.isPlaying || MenuFlow.Instance == null || MenuFlow.Instance.Current != MenuFlow.Screen.Title)
            throw new InvalidOperationException("Enter Play from TitleScreen and wait for the title before running verification.");
        var lines = new List<string>();
        var menu = MenuFlow.Instance;
        float originalVolume = AudioListener.volume;
        bool hadPreference = PlayerPrefs.HasKey("ProjectRogue.MasterVolume");
        float originalPreference = PlayerPrefs.GetFloat("ProjectRogue.MasterVolume", originalVolume);
        var originalBackground = InputSystem.settings.backgroundBehavior;
        var originalEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
        var testKeyboard = InputSystem.AddDevice<Keyboard>("MenuVerificationKeyboard");
        try
        {
            // Pipeline commands execute while the OS may focus another app. Restore both settings below.
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.EnableDevice(testKeyboard);
            Check("Title starts with Start focused", EventSystem.current.currentSelectedGameObject.name == "Start game", lines);
            Check("Editor Quit is disabled", !Button("Quit").interactable, lines);
            await Keys(70, Key.Tab);
            Check("Tab moves focus to Settings", EventSystem.current.currentSelectedGameObject.name == "Settings", lines);
            await Keys(70, Key.Enter);
            Check("Keyboard submit opens Settings", menu.Current == MenuFlow.Screen.Settings, lines);
            var slider = UnityEngine.Object.FindFirstObjectByType<UnityEngine.UI.Slider>();
            slider.value = .23f;
            Check("Slider changes are pending before Apply", Mathf.Approximately(AudioListener.volume, originalVolume), lines);
            Click("Apply");
            Check("Apply sets and saves master volume", Mathf.Approximately(AudioListener.volume,.23f) && Mathf.Approximately(PlayerPrefs.GetFloat("ProjectRogue.MasterVolume"),.23f), lines);
            slider.value = .77f; Click("Back");
            Check("Back discards unapplied volume", menu.Current == MenuFlow.Screen.Title && Mathf.Approximately(AudioListener.volume,.23f), lines);
            Check("Settings restores opener focus", EventSystem.current.currentSelectedGameObject.name == "Settings", lines);
            Click("Start game");
            await Wait(() => menu.GameplayInputAllowed && Arena() != null && Arena().Player != null);
            Check("Start loads arena with three enemies", Arena().Remaining == 3, lines);
            CheckUnique(lines);
            float x = Arena().Player.transform.position.x;
            await Keys(220, Key.D);
            Check("Keyboard movement changes player position", Arena().Player.transform.position.x > x+.4f, lines);
            await Keys(140, Key.Space);
            Check("Keyboard jump lifts player", Arena().Player.transform.position.y > -.3f, lines);
            x = Arena().Player.transform.position.x;
            await Keys(220, Key.LeftShift);
            Check("Keyboard dash moves player", Arena().Player.transform.position.x > x+1, lines);
            await Task.Delay(650);
            var player = Arena().Player;
            var combat = player.GetComponent<PlayerCombat>();
            combat.TryUse(combat.ability1);
            // Launch an actual team projectile to verify physics/lifetime freeze and resume.
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_Arrow.prefab");
            var projectileObject = UnityEngine.Object.Instantiate(prefab, new Vector3(-15,3,0), Quaternion.identity);
            var projectile = projectileObject.GetComponent<Projectile>();
            var projectileAbility = ScriptableObject.CreateInstance<AbilityData>(); projectileAbility.projectileLifetime=10;
            projectileAbility.projectileSpeed=3; projectile.Launch(player,projectileAbility,10,0,Vector2.right);
            await Keys(60, Key.Escape);
            Check("Escape opens Pause", menu.Current == MenuFlow.Screen.Pause && Time.timeScale == 0, lines);
            var playerPosition = player.transform.position;
            var enemyPositions = UnityEngine.Object.FindObjectsByType<ArenaFoe>(FindObjectsSortMode.None).Select(f=>f.transform.position).ToArray();
            var projectilePosition = projectile.transform.position;
            float pausedTime=Time.time, pausedMana=player.GetComponent<ResourcePool>().Mana;
            await Keys(400,Key.J,Key.K,Key.D,Key.Space);
            Check("Pause blocks movement/jump", Vector3.Distance(player.transform.position,playerPosition)<.001f, lines);
            Check("Pause freezes enemies", UnityEngine.Object.FindObjectsByType<ArenaFoe>(FindObjectsSortMode.None).Select((f,i)=>Vector3.Distance(f.transform.position,enemyPositions[i])<.001f).All(v=>v),lines);
            Check("Pause freezes projectile", projectile != null && Vector3.Distance(projectile.transform.position,projectilePosition)<.001f,lines);
            Check("Pause freezes game clock and resource regeneration", Time.time==pausedTime && Mathf.Approximately(player.GetComponent<ResourcePool>().Mana,pausedMana),lines);
            Check("Paused ability API rejects spending", !combat.TryUse(combat.ability1),lines);
            Click("Controls");
            Check("Controls opens Settings while paused", menu.Current==MenuFlow.Screen.Settings && Time.timeScale==0,lines);
            await Keys(70,Key.Escape);
            Check("Settings Escape returns to Pause", menu.Current==MenuFlow.Screen.Pause && Time.timeScale==0,lines);
            Click("Quit to menu"); Check("Quit requires confirmation",menu.Current==MenuFlow.Screen.ConfirmQuit,lines);
            Click("Cancel"); Check("Cancel preserves player and pause",Arena().Player==player && menu.Current==MenuFlow.Screen.Pause && Time.timeScale==0,lines);
            InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.J)); await Task.Delay(50);
            Click("Resume"); await Task.Delay(120);
            Check("Held attack cannot leak out of Resume", !menu.GameplayInputAllowed && Time.timeScale==0,lines);
            InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());
            await Wait(()=>menu.GameplayInputAllowed && Time.timeScale>0);
            await Task.Delay(150);
            Check("Resume preserves player and continues projectile", Arena().Player==player && projectile != null && Vector3.Distance(projectile.transform.position,projectilePosition)>.05f,lines);
            UnityEngine.Object.Destroy(projectileObject); UnityEngine.Object.Destroy(projectileAbility);
            var foe=UnityEngine.Object.FindObjectsByType<ArenaFoe>(FindObjectsSortMode.None).First();
            player.transform.position=foe.transform.position+Vector3.left*1.1f; player.transform.localScale=Vector3.one;
            player.GetComponent<ProjectRogue.Movement.PlayerMovement>().ResetMotion(); Physics2D.SyncTransforms();
            float enemyHealth=foe.GetComponent<Health>().Current;
            await Keys(70,Key.J);
            Check("J damages an enemy through combat", foe.GetComponent<Health>().Current<enemyHealth,lines);
            await Keys(70,Key.K);
            Check("K spends mana in gameplay", player.GetComponent<ResourcePool>().Mana<45,lines);
            await Task.Delay(1000);
            Check("Enemy attack damages player", player.GetComponent<Health>().Current<100,lines);
            CombatResolver.Apply(foe.gameObject,combat.basicAttack,1000,player.GetComponent<Health>());
            await Wait(()=>menu.Current==MenuFlow.Screen.Death);
            Check("Lethal damage opens Death and stops time",Time.timeScale==0,lines);
            Click("Try again"); await Wait(()=>menu.GameplayInputAllowed && Arena()!=null && Arena().Player!=null);
            Check("Retry restores health, mana and enemies", Arena().Player.GetComponent<Health>().Current==100 && Arena().Player.GetComponent<ResourcePool>().Mana==50 && Arena().Remaining==3,lines);
            CheckUnique(lines);
            player=Arena().Player; combat=player.GetComponent<PlayerCombat>();
            foreach(var enemy in UnityEngine.Object.FindObjectsByType<ArenaFoe>(FindObjectsSortMode.None))
                CombatResolver.Apply(player,combat.basicAttack,1000,enemy.GetComponent<Health>());
            await Task.Delay(100); player.transform.position=new Vector3(16,-.7f,0);
            await Wait(()=>menu.Current==MenuFlow.Screen.Complete);
            Check("Clear plus exit opens room completion",Time.timeScale==0,lines);
            Click("Play again"); await Wait(()=>menu.GameplayInputAllowed && Arena()!=null && Arena().Player!=null);
            CheckUnique(lines);
            await Keys(70,Key.Escape); Click("Quit to menu"); Click("Quit to menu");
            await Wait(()=>menu.Current==MenuFlow.Screen.Title && !menu.IsLoading);
            Check("Confirmed quit unloads arena",Arena()==null && UnityEngine.Object.FindObjectsByType<ArenaFoe>(FindObjectsSortMode.None).Length==0,lines);
            Check("Returning to Title leaves one menu and EventSystem", UnityEngine.Object.FindObjectsByType<MenuFlow>(FindObjectsSortMode.None).Length==1 && UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1,lines);
            Check("Return to Title restores time and audio pause",Time.timeScale==1 && !AudioListener.pause,lines);
            return string.Join("\n",lines)+"\nFINISHED: "+lines.Count+" checks passed.";
        }
        finally
        {
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());
            InputSystem.RemoveDevice(testKeyboard);
            InputSystem.settings.backgroundBehavior = originalBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = originalEditorInput;
            AudioListener.volume=originalVolume;
            if(hadPreference) PlayerPrefs.SetFloat("ProjectRogue.MasterVolume",originalPreference);
            else PlayerPrefs.DeleteKey("ProjectRogue.MasterVolume");
            PlayerPrefs.Save();
        }
    }

    static PlayableArena Arena()=>UnityEngine.Object.FindFirstObjectByType<PlayableArena>();
    static UnityEngine.UI.Button Button(string title)=>UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Single(b=>b.name==title);
    static void Click(string title)
    {
        var button=Button(title);
        if(!button.interactable) throw new Exception("Disabled button: "+title);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
        ExecuteEvents.Execute(button.gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerClickHandler);
    }
    static async Task Keys(int milliseconds, params Key[] keys)
    {
        InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(keys));
        await Task.Delay(milliseconds);
        InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState()); await Task.Delay(40);
    }
    static async Task Wait(Func<bool> condition)
    {
        var until=DateTime.UtcNow.AddSeconds(8);
        while(!condition()) { if(DateTime.UtcNow>until) throw new TimeoutException("Menu transition timed out"); await Task.Delay(30); }
        await Task.Delay(60);
    }
    static void Check(string title,bool value,List<string> lines)
    {
        if(!value) throw new Exception("FAIL: "+title+"\n"+string.Join("\n",lines));
        lines.Add("PASS: "+title);
    }
    static void CheckUnique(List<string> lines)
    {
        Check("No duplicate menu, EventSystem, arena or player",UnityEngine.Object.FindObjectsByType<MenuFlow>(FindObjectsSortMode.None).Length==1 &&
            UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1 && UnityEngine.Object.FindObjectsByType<PlayableArena>(FindObjectsSortMode.None).Length==1 &&
            UnityEngine.Object.FindObjectsByType<PlayerCombat>(FindObjectsSortMode.None).Length==1,lines);
    }
}
