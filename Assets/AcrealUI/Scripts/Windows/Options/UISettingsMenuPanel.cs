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
    public class UISettingsMenuPanel : UIPanel
    {
        #region Variables
        [SerializeField] private string _gameObjName_button_generalSettings = null;
        [SerializeField] private string _gameObjName_button_interfaceSettings = null;
        [SerializeField] private string _gameObjName_button_videoSettings = null;
        [SerializeField] private string _gameObjName_button_audioSettings = null;
        [SerializeField] private string _gameObjName_button_controlSettings = null;

        private UIButton _button_generalSettings = null;
        private UIButton _button_interfaceSettings = null;
        private UIButton _button_videoSettings = null;
        private UIButton _button_audioSettings = null;
        private UIButton _button_controlSettings = null;
        #endregion


        #region Events
        public System.Action Event_OnButtonClicked_GeneralSettings = null;
        public System.Action Event_OnButtonClicked_InterfaceSettings = null;
        public System.Action Event_OnButtonClicked_VideoSettings = null;
        public System.Action Event_OnButtonClicked_AudioSettings = null;
        public System.Action Event_OnButtonClicked_ControlSettings = null;
        #endregion


        #region Initialization
        public override void Initialize()
        {
            base.Initialize();

            Transform generalTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_generalSettings);
            if (generalTform != null)
            {
                _button_generalSettings = generalTform.GetComponent<UIButton>();
                if (_button_generalSettings != null)
                {
                    _button_generalSettings.Initialize();
                    _button_generalSettings.Event_OnAnyClick += (_, _1) => { Event_OnButtonClicked_GeneralSettings?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI.UISettingsMenuPanel] Failed to load UIButton Script on GameObject \"" + _gameObjName_button_generalSettings + "\""); }
            }

            Transform interfaceTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_interfaceSettings);
            if (interfaceTform != null)
            {
                _button_interfaceSettings = interfaceTform.GetComponent<UIButton>();
                if (_button_interfaceSettings != null)
                {
                    _button_interfaceSettings.Initialize();
                    _button_interfaceSettings.Event_OnAnyClick += (_, _1) => { Event_OnButtonClicked_InterfaceSettings?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI.UISettingsMenuPanel] Failed to load UIButton Script on GameObject \"" + _gameObjName_button_interfaceSettings + "\""); }
            }

            Transform videoTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_videoSettings);
            if (videoTform != null)
            {
                _button_videoSettings = videoTform.GetComponent<UIButton>();
                if (_button_videoSettings != null)
                {
                    _button_videoSettings.Initialize();
                    _button_videoSettings.Event_OnAnyClick += (_, _1) => { Event_OnButtonClicked_VideoSettings?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI.UISettingsMenuPanel] Failed to load UIButton Script on GameObject \"" + _gameObjName_button_videoSettings + "\""); }
            }

            Transform audioTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_audioSettings);
            if (audioTform != null)
            {
                _button_audioSettings = audioTform.GetComponent<UIButton>();
                if (_button_audioSettings != null)
                {
                    _button_audioSettings.Initialize();
                    _button_audioSettings.Event_OnAnyClick += (_, _1) => { Event_OnButtonClicked_AudioSettings?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI.UISettingsMenuPanel] Failed to load UIButton Script on GameObject \"" + _gameObjName_button_audioSettings + "\""); }
            }

            Transform controlTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_controlSettings);
            if (controlTform != null)
            {
                _button_controlSettings = controlTform.GetComponent<UIButton>();
                if (_button_controlSettings != null)
                {
                    _button_controlSettings.Initialize();
                    _button_controlSettings.Event_OnAnyClick += (_, _1) => { Event_OnButtonClicked_ControlSettings?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI.UISettingsMenuPanel] Failed to load UIButton Script on GameObject \"" + _gameObjName_button_controlSettings + "\""); }
            }
        }
        #endregion
    }
}
