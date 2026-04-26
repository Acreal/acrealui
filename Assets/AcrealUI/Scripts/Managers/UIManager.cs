/*
Copyright (c) 2025-2026 Acreal (https://github.com/acreal)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
documentation files (the "Software"), to deal in the Software without restriction, including without 
limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
the Software, and to permit persons to whom the Software is furnished to do so, subject to the following 
conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions 
of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE.
*/


/*
UIManager.cs

Represents the main management component of this mod. Instantiated and added at setup.

NOTES(Acreal):
    -Some key architecture rules:
        -references to objects assigned in the editor will not be valid after packaging
         the mod. the workaround is to use Transform.Find(string) to get the component's
         GameObject and then use GetComponent<T> to get the actual reference.
            -mods will not compile if they contain a serialized reference
            -Transform.Find may be a bottleneck to performance in the future. a possible optimization
             would be to unwind back through an object's transform hierarchy after using
             Transform.Find(string) once, and then caching sibling indexes of every transform
             all the way back to the root object. future instances could then use that list
             of indexes to find their way to the target transform using transform.GetChild(int)
             instead of Transform.Find(string)
*/

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using Wenzil.Console;
#endif

namespace AcrealUI
{
    public class UIManager : MonoBehaviour
    {
        #region Definitions
        private struct CoroutineContainer
        {
            public int objectId;
            public int instanceId;
            public IEnumerator<float> coroutine;
            public Action<bool> endCallback;
        }
        #endregion


        #region Variables
        public static Mod mod = null;

        private static UIManager _instance = null;

        private List<UIWindowInstanceType> _coreWindowInstanceTypesList = null;
        private Dictionary<UIWindowInstanceType, UIWindow> _windowTypeToInstanceDict = null;
        private Dictionary<UIWindowType, Type> _defaultCoreWindowTypes = null;
        private Dictionary<UIWindowType, bool> _enabledCoreWindowsDict = null;
        private List<CoroutineContainer> _runningCoroutines = null;
        private bool _runningMasterRoutine = false;
        #endregion


        #region Properties
        public static UIManager Instance { get { return _instance; } }
        public static UIReferenceManager referenceManager { get; private set; }
        public static UITooltipManager tooltipManager { get; private set; }
        public static UIPopupManager popupManager{ get; private set; }
        #endregion


        #region Initialization
        [Invoke(StateManager.StateTypes.Start)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            if (_instance == null)
            {
                _instance = new GameObject("UIManager").AddComponent<UIManager>();
            }
        }
        #endregion


        #region MonoBehaviour
        private void Awake()
        {
            _runningCoroutines = new List<CoroutineContainer>(16);

            _coreWindowInstanceTypesList = new List<UIWindowInstanceType>
            {
                UIWindowInstanceType.PauseOptions,
                UIWindowInstanceType.Conversation,
                UIWindowInstanceType.Inventory,
                UIWindowInstanceType.SaveOrLoadGame,
            };
            _windowTypeToInstanceDict = new Dictionary<UIWindowInstanceType, UIWindow>();
            _defaultCoreWindowTypes = new Dictionary<UIWindowType, Type>();
            _enabledCoreWindowsDict = new Dictionary<UIWindowType, bool>();

            UISpriteManager.Initialize();

            referenceManager = new UIReferenceManager();
            referenceManager.Initialize(mod);

            tooltipManager = new UITooltipManager();
            tooltipManager.Initialize();

            popupManager = new UIPopupManager();

            SaveLoadManager.OnStartLoad += PreLoadGame;
            SaveLoadManager.OnLoad += PostLoadGame;

            #if UNITY_EDITOR
            ConsoleCommandsDatabase.RegisterCommand("resetkeybinds", "Reset All Keybinds to Defaults", string.Empty, (_) =>
            {
                try
                {
                    UIUtilityFunctions.SetDefaultKeybinds();
                    return "Successfully reset all keybinds to default values!";
                }
                catch
                {
                    return "Something went terribly wrong resetting the keybinds. :(";
                }
            });
            #endif

            mod.IsReady = true;
        }

        private void Start()
        {
            /////////////////////////////////////////////////////////////////////////////////////////////////
            //**************************************[NEW WINDOWS]******************************************//
            /////////////////////////////////////////////////////////////////////////////////////////////////
            _defaultCoreWindowTypes[UIWindowType.PauseOptions] = typeof(DaggerfallPauseOptionsWindowExtended);

            IUserInterfaceWindow inventoryWindow = UIWindowFactory.GetInstance(UIWindowType.Inventory, DaggerfallUI.UIManager, null);
            _defaultCoreWindowTypes[UIWindowType.Inventory] = inventoryWindow != null ? inventoryWindow.GetType() : typeof(DaggerfallInventoryWindow);

            IUserInterfaceWindow saveWindow = UIWindowFactory.GetInstanceWithArgs(UIWindowType.UnitySaveGame, new object[] { DaggerfallUI.UIManager, DaggerfallUnitySaveGameWindow.Modes.SaveGame, null, false });
            _defaultCoreWindowTypes[UIWindowType.UnitySaveGame] = saveWindow != null ? saveWindow.GetType() : typeof(DaggerfallUnitySaveGameWindow);

            IUserInterfaceWindow talkWindow = UIWindowFactory.GetInstance(UIWindowType.Talk, DaggerfallUI.UIManager, null);
            _defaultCoreWindowTypes[UIWindowType.Talk] = talkWindow != null ? talkWindow.GetType() : typeof(DaggerfallTalkWindow);

            EnableCoreWindow(UIWindowType.PauseOptions);
            EnableCoreWindow(UIWindowType.Inventory);
            EnableCoreWindow(UIWindowType.Talk);
            EnableCoreWindow(UIWindowType.UnitySaveGame);
            ApplyCoreWindowChanges();

            //daggerfall unity starts with a depth clear only,
            //but the load game window does not have a background,
            //so we set it to solid color before we get to that point
            Camera.main.clearFlags = CameraClearFlags.SolidColor;

            /////////////////////////////////////////////////////////////////////////////////////////////////
            //**************************[WINDOWS THAT NEED TO BE REPLACED]*********************************//
            /////////////////////////////////////////////////////////////////////////////////////////////////
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Banking, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.BankPurchasePopup, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.BookReader, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Court, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.DaedraSummoned, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.EffectSettingsEditor, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.GuildServiceCureDisease, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.GuildServiceDonation, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.GuildServicePopup, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.GuildServiceTraining, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.ItemMaker, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.MerchantRepairPopup, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.MerchantServicePopup, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.PlayerHistory, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.PotionMaker, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.QuestJournal, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.QuestOffer, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Rest, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.SpellBook, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.SpellIconPicker, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.SpellMaker, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Tavern, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.TeleportPopUp, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Trade, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Transport, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.TravelMap, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.TravelPopUp, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.UseMagicItem, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.WitchesCovenPopup, null);
        }

        private void Update()
        {
            if(tooltipManager != null)
            {
                tooltipManager.Update();
            }
        }

        private void LateUpdate()
        {
            if (tooltipManager != null)
            {
                tooltipManager.LateUpdate();
            }
        }

        private void OnDestroy()
        {
            SaveLoadManager.OnStartLoad -= PreLoadGame;
            SaveLoadManager.OnLoad -= PostLoadGame;

            StopAllCoroutines();

            if(tooltipManager != null)
            {
                tooltipManager.Shutdown();
            }
            tooltipManager = null;

            if (referenceManager != null)
            {
                referenceManager.Shutdown();
            }
            referenceManager = null;

            UISpriteManager.Shutdown();
        }
        #endregion


        #region Public API
        public UIWindow GetWindowInstance(UIWindowInstanceType windowType, bool autoCreate = true)
        {
            _windowTypeToInstanceDict.TryGetValue(windowType, out UIWindow window);
            if (window == null && autoCreate)
            {
                switch (windowType)
                {
                    case UIWindowInstanceType.PauseOptions: window = Instantiate(referenceManager.prefab_pauseWindow); break;
                    case UIWindowInstanceType.Conversation: window = Instantiate(referenceManager.prefab_conversationWindow); break;
                    case UIWindowInstanceType.Inventory: window = Instantiate(referenceManager.prefab_inventoryWindow); break;
                    case UIWindowInstanceType.SaveOrLoadGame: window = Instantiate(referenceManager.prefab_saveLoadWindow); break;
                    case UIWindowInstanceType.Confirmation: window = Instantiate(referenceManager.prefab_confirmationWindow); break;
                    case UIWindowInstanceType.SliderConfirmation: window = Instantiate(referenceManager.prefab_sliderConfirmationWindow); break;
                    case UIWindowInstanceType.Trade: window = Instantiate(referenceManager.prefab_tradeWindow); break;
                }

                if (window != null)
                {
                    DontDestroyOnLoad(window.gameObject);
                    _windowTypeToInstanceDict[windowType] = window;
                }
            }
            return window;
        }

        public void EnableCoreWindow(UIWindowType windowInstanceType)
        {
            _enabledCoreWindowsDict[windowInstanceType] = true;
        }
        
        public void DisableCoreWindow(UIWindowType windowInstanceType)
        {
            _enabledCoreWindowsDict[windowInstanceType] = false;
        }

        public bool IsCoreWindowEnabled(UIWindowType windowType)
        {
            _enabledCoreWindowsDict.TryGetValue(windowType, out bool isEnabled);
            return isEnabled;
        }

        public void PreLoadGame(SaveData_v1 saveData)
        {
            DaggerfallUI.Instance.PopToHUD();
            CleanupAllWindows();
        }

        protected void PostLoadGame(SaveData_v1 saveData)
        {
            ApplyCoreWindowChanges();
        }

        public void CleanupAllWindows()
        {
            foreach (UIWindowInstanceType windowInstType in _coreWindowInstanceTypesList)
            {
                if (_windowTypeToInstanceDict.TryGetValue(windowInstType, out UIWindow windowInst))
                {
                    windowInst?.Hide();
                    windowInst?.Cleanup();
                }
            }
        }

        public void ApplyCoreWindowChanges()
        {
            CleanupAllWindows();

            foreach (KeyValuePair<UIWindowType, bool> kvp in _enabledCoreWindowsDict)
            {
                if (kvp.Value)
                {
                    switch (kvp.Key)
                    {
                        case UIWindowType.PauseOptions:
                            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.PauseOptions, typeof(UIPauseWindowController));
                            break;

                        case UIWindowType.Inventory:
                            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Inventory, typeof(UIInventoryWindowController));
                            break;

                        case UIWindowType.Talk:
                            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Talk, typeof(UIConversationWindowController));
                            break;

                        case UIWindowType.UnitySaveGame:
                            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.UnitySaveGame, typeof(UISaveLoadWindowController));
                            break;
                    }
                }
                else if (_defaultCoreWindowTypes.TryGetValue(kvp.Key, out Type windowType))
                {
                    if (windowType != null)
                    {
                        UIWindowFactory.RegisterCustomUIWindow(kvp.Key, windowType);
                    }
                }
            }
        }

        public void ExecuteDelayed(int objectId, int instanceId, float delay, System.Action funcToExecute)
        {
            if(funcToExecute != null)
            {
                RunCoroutine(objectId, instanceId, ExecuteDelayedRoutine(delay, funcToExecute));
            }
        }

        public void RunCoroutine(int objectId, int instanceId, IEnumerator<float> coroutine, Action<bool> onEndCallback = null)
        {
            if (coroutine != null)
            {
                CoroutineContainer routineContainer = new CoroutineContainer
                {
                    objectId = objectId,
                    instanceId = instanceId,
                    coroutine = coroutine,
                    endCallback = onEndCallback,
                };

                bool overwrote = false;
                for(int i = 0; i < _runningCoroutines.Count; i++)
                {
                    int objId = _runningCoroutines[i].objectId;
                    int instId = _runningCoroutines[i].instanceId;

                    if (objId == objectId && instId == instanceId)
                    {
                        _runningCoroutines[i].endCallback?.Invoke(true);
                        _runningCoroutines[i] = new CoroutineContainer
                        {
                            objectId = objId,
                            instanceId = instId,
                            coroutine = coroutine,
                            endCallback = onEndCallback,
                        };
                        overwrote = true;
                        break;
                    }
                }

                if (!overwrote)
                {
                    _runningCoroutines.Add(routineContainer);
                }

                if(!_runningMasterRoutine)
                {
                    _runningMasterRoutine = true;
                    StartCoroutine(MasterRoutine());
                }
            }
        }

        public void StopCoroutine(int objectId, int instanceId)
        {
            for (int i = 0; i < _runningCoroutines.Count; i++)
            {
                int objId = _runningCoroutines[i].objectId;
                int instId = _runningCoroutines[i].instanceId;

                if (objId == objectId && instId == instanceId)
                {
                    _runningCoroutines[i].endCallback?.Invoke(true);
                    _runningCoroutines[i] = new CoroutineContainer
                    {
                        objectId = objId,
                        instanceId = instId,
                        coroutine = null,
                        endCallback = null,
                    };
                    break;
                }
            }
        }
        #endregion


        #region Coroutines
        private IEnumerator<float> MasterRoutine()
        {
            while (_runningCoroutines != null && _runningCoroutines.Count > 0)
            {
                for (int i = 0; i < _runningCoroutines.Count; i++)
                {
                    if (_runningCoroutines[i].coroutine == null || !_runningCoroutines[i].coroutine.MoveNext())
                    {
                        _runningCoroutines[i].endCallback?.Invoke(false);
                        _runningCoroutines.RemoveAt(i);
                        i--;
                    }
                }
                yield return 0f;
            }
            _runningMasterRoutine = false;
        }

        private static IEnumerator<float> ExecuteDelayedRoutine(float delay, System.Action funcToExecute)
        {
            float t = delay;
            while(t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                yield return 0;
            }

            try
            {
                funcToExecute();
            }
            catch(System.Exception e)
            {
                Debug.LogError("[AcrealUI] Unable to execute function in UIManager.ExecuteDelayedRoutine! Reason: " + e.Message);
            }
        }
        #endregion
    }
}
