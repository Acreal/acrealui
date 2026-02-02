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

using DaggerfallWorkshop.Game.Utility.ModSupport;
using UnityEngine;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIPausePanel : UIPanel
    {
        #region Variables
        [SerializeField] private string _gameObjName_button_saveGame = null;
        [SerializeField] private string _gameObjName_button_loadGame = null;
        [SerializeField] private string _gameObjName_button_exitGame = null;
        [SerializeField] private string _gameObjName_button_settings = null;
        [SerializeField] private string _gameObjName_button_continue = null;

        private UIButton _button_saveGame = null;
        private UIButton _button_loadGame = null;
        private UIButton _button_exitGame = null;
        private UIButton _button_settings = null;
        private UIButton _button_continue = null;
        #endregion


        #region Events
        public event System.Action Event_OnButtonClicked_Continue = null;
        public event System.Action Event_OnButtonClicked_SaveGame = null;
        public event System.Action Event_OnButtonClicked_LoadGame = null;
        public event System.Action Event_OnButtonClicked_ExitGame = null;
        public event System.Action Event_OnButtonClicked_Settings = null;
        #endregion


        #region Init/Cleanup
        public override void Initialize()
        {
            base.Initialize();

            Transform continueTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_continue);
            _button_continue = continueTform != null ? continueTform.GetComponent<UIButton>() : null;
            if (_button_continue != null)
            {
                _button_continue.Initialize();
                _button_continue.Event_OnClicked += (_) => { Event_OnButtonClicked_Continue?.Invoke(); };
            }
            else { Debug.LogError("[AcrealUI.UIPausePanel] Failed to load UIButton Script on GameObject \"" + (continueTform != null ? continueTform.name : "NULL") + "\""); }

            Transform saveTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_saveGame);
            _button_saveGame = saveTform != null ? saveTform.GetComponent<UIButton>() : null;
            if (_button_saveGame != null)
            {
                _button_saveGame.Initialize();
                _button_saveGame.Event_OnClicked += (_) => { Event_OnButtonClicked_SaveGame?.Invoke(); };
            }
            else { Debug.LogError("[AcrealUI.UIPausePanel] Failed to load UIButton Script on GameObject \"" + (saveTform != null ? saveTform.name : "NULL") + "\""); }

            Transform loadTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_loadGame);
            _button_loadGame = loadTform != null ? loadTform.GetComponent<UIButton>() : null;
            if (_button_loadGame != null)
            {
                _button_loadGame.Initialize();
                _button_loadGame.Event_OnClicked += (_) => { Event_OnButtonClicked_LoadGame?.Invoke(); };
            }
            else { Debug.LogError("[AcrealUI.UIPausePanel] Failed to load UIButton Script on GameObject \"" + (loadTform != null ? loadTform.name : "NULL") + "\""); }

            Transform exitTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_exitGame);
            _button_exitGame = exitTform != null ? exitTform.GetComponent<UIButton>() : null;
            if (_button_exitGame != null)
            {
                _button_exitGame.Initialize();
                _button_exitGame.Event_OnClicked += (_) => { Event_OnButtonClicked_ExitGame?.Invoke(); };
            }
            else { Debug.LogError("[AcrealUI.UIPausePanel] Failed to load UIButton Script on GameObject \"" + (exitTform != null ? exitTform.name : "NULL") + "\""); }

            Transform settingsTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_settings);
            _button_settings = settingsTform != null ? settingsTform.GetComponent<UIButton>() : null;
            if (_button_settings != null)
            {
                _button_settings.Initialize();
                _button_settings.Event_OnClicked += (_) => { Event_OnButtonClicked_Settings?.Invoke(); };
            }
            else { Debug.LogError("[AcrealUI.UIPausePanel] Failed to load UIButton Script on GameObject \"" + (settingsTform != null ? settingsTform.name : "NULL") + "\""); }
        }
        #endregion
    }
}
