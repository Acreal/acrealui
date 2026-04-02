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

        private Dictionary<UIWindowType, Type> _originalWindowTypesDict = null;
        private Dictionary<UIWindowType, bool> _enabledWindowsDict = null;
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

            UISpriteManager.Initialize();

            if (referenceManager == null)
            {
                referenceManager = new UIReferenceManager();
                referenceManager.Initialize(mod);
            }

            if (tooltipManager == null)
            {
                tooltipManager = new UITooltipManager();
                tooltipManager.Initialize();
            }

            if(_originalWindowTypesDict == null)
            {
                _originalWindowTypesDict = new Dictionary<UIWindowType, Type>();
            }

            if(_enabledWindowsDict == null)
            {
                _enabledWindowsDict = new Dictionary<UIWindowType, bool>();
            }

            if (popupManager == null)
            {
                popupManager = new UIPopupManager();
                popupManager.Initialize();
            }

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
            IUserInterfaceWindow pauseWindow = UIWindowFactory.GetInstance(UIWindowType.PauseOptions, DaggerfallUI.UIManager, null);
            if (pauseWindow != null)
            {
                _originalWindowTypesDict[UIWindowType.PauseOptions] = pauseWindow.GetType();
            }
            else
            {
                _originalWindowTypesDict[UIWindowType.PauseOptions] = typeof(DaggerfallPauseOptionsWindow);
            }

            IUserInterfaceWindow inventoryWindow = UIWindowFactory.GetInstance(UIWindowType.Inventory, DaggerfallUI.UIManager, null);
            if (inventoryWindow != null)
            {
                _originalWindowTypesDict[UIWindowType.Inventory] = inventoryWindow.GetType();
            }
            else
            {
                _originalWindowTypesDict[UIWindowType.Inventory] = typeof(DaggerfallInventoryWindow);
            }

            IUserInterfaceWindow saveWindow = UIWindowFactory.GetInstanceWithArgs(UIWindowType.UnitySaveGame, new object[] { DaggerfallUI.UIManager, DaggerfallUnitySaveGameWindow.Modes.SaveGame, null, false });
            if (saveWindow != null)
            {
                _originalWindowTypesDict[UIWindowType.UnitySaveGame] = saveWindow.GetType();
            }
            else
            {
                _originalWindowTypesDict[UIWindowType.UnitySaveGame] = typeof(DaggerfallUnitySaveGameWindow);
            }

            IUserInterfaceWindow talkWindow = UIWindowFactory.GetInstance(UIWindowType.Talk, DaggerfallUI.UIManager, null);
            if (talkWindow != null)
            {
                _originalWindowTypesDict[UIWindowType.Talk] = talkWindow.GetType();
            }
            else
            {
                _originalWindowTypesDict[UIWindowType.Talk] = typeof(DaggerfallTalkWindow);
            }

            EnableWindow(UIWindowType.Talk);
            EnableWindow(UIWindowType.Inventory);
            EnableWindow(UIWindowType.PauseOptions);
            EnableWindow(UIWindowType.UnitySaveGame);

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
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Talk, null);
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
        public void EnableWindow(UIWindowType windowType)
        {
            _enabledWindowsDict[windowType] = true;

            switch(windowType)
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
        
        public void DisableWindow(UIWindowType windowType)
        {
            _enabledWindowsDict[windowType] = true;

            if (_originalWindowTypesDict.TryGetValue(windowType, out Type t))
            {
                UIWindowFactory.RegisterCustomUIWindow(windowType, t);
            }
        }

        public void ExecuteDelayed(int objectId, int instanceId, float delay, System.Action funcToExecute)
        {
            if(funcToExecute != null)
            {
                RunCoroutine(objectId, instanceId, ExecuteDelayedRoutine(delay, funcToExecute));
            }
        }

        public void RunCoroutine(int objectId, int instanceId, IEnumerator<float> coroutine)
        {
            if (coroutine != null)
            {
                CoroutineContainer routineContainer = new CoroutineContainer
                {
                    objectId = objectId,
                    instanceId = instanceId,
                    coroutine = coroutine,
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
