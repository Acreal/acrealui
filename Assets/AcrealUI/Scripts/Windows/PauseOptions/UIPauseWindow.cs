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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace AcrealUI
{
    [ImportedComponent]
    public class UIPauseWindow : UIWindow
    {
        #region Definitions
        public enum PauseWindowState
        {
            None = 0,
            Default = 1,
            Settings = 2,
            Settings_General = 3,
            Settings_Interface = 4,
            Settings_Video = 5,
            Settings_Audio = 6,
            Settings_Controls = 7,
        }
        #endregion


        #region Variables
        [SerializeField] private string _gameObjName_panelPaused = null;
        [SerializeField] private string _gameObjName_panelSettings = null;
        [SerializeField] private string _gameObjName_panelGeneralSettings = null;
        [SerializeField] private string _gameObjName_panelInterfaceSettings = null;
        [SerializeField] private string _gameObjName_panelAudioSettings = null;
        [SerializeField] private string _gameObjName_panelVideoSettings = null;
        [SerializeField] private string _gameObjName_panelControlSettings = null;

        private UIPausePanel _panelPaused = null;
        private UISettingsMenuPanel _panelSettings = null;
        private UIPanel _panelGeneralSettings = null;
        private UIPanel _panelInterfaceSettings = null;
        private UIPanel _panelAudioSettings = null;
        private UIPanel _panelVideoSettings = null;
        private UIControlOptionsPanel _panelControlSettings = null;

        private PauseWindowState _currentState = PauseWindowState.None;
        private Vector2 _panelSizeOffset = Vector2.zero;
        private UIPanel _activePanel = null;
        #endregion


        #region Properties
        public PauseWindowState currentState { get { return _currentState; } }
        public UIPausePanel panelPaused { get { return _panelPaused; } }
        public UISettingsMenuPanel panelSettings { get { return _panelSettings; } }
        public UIPanel panelGeneralSettings { get { return _panelGeneralSettings; } }
        public UIPanel panelInterfaceSettings { get { return _panelInterfaceSettings; } }
        public UIPanel panelAudioSettings { get { return _panelAudioSettings; } }
        public UIPanel panelVideoSettings { get { return _panelVideoSettings; } }
        public UIControlOptionsPanel panelControlSettings { get { return _panelControlSettings; } }
        #endregion


        #region Initialization/Cleanup
        public override void Initialize()
        {
            base.Initialize();
            _currentState = PauseWindowState.None;

            Transform pauseTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_panelPaused);
            if (pauseTform != null)
            {
                _panelPaused = pauseTform.GetComponent<UIPausePanel>();
                if (_panelPaused)
                {
                    _panelPaused.Initialize();
                }
                else { Debug.LogError("[AcrealUI.UIPauseWindow] Unable to get UIPausePanel script from GameObject \"" + (pauseTform != null ? pauseTform.gameObject.name : "NULL")); }
            }

            Transform settingsTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_panelSettings);
            if (settingsTform != null)
            {
                _panelSettings = settingsTform.GetComponent<UISettingsMenuPanel>();
                if (_panelSettings)
                {
                    _panelSettings.Initialize();
                }
                else { Debug.LogError("[AcrealUI.UIPauseWindow] Unable to get UISettingsMenuPanel script from GameObject \"" + (settingsTform != null ? settingsTform.gameObject.name : "NULL")); }
            }

            Transform generalTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_panelGeneralSettings);
            if (generalTform != null)
            {
                _panelGeneralSettings = generalTform.GetComponent<UIPanel>();
                if (_panelGeneralSettings)
                {
                    _panelGeneralSettings.Initialize();
                }
                else { Debug.LogError("[AcrealUI.UIPauseWindow] Unable to get UIPanel_GeneralSettings script from GameObject \"" + (generalTform != null ? generalTform.gameObject.name : "NULL")); }
            }

            Transform interfaceTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_panelInterfaceSettings);
            if (interfaceTform != null)
            {
                _panelInterfaceSettings = interfaceTform.GetComponent<UIPanel>();
                if (_panelInterfaceSettings)
                {
                    _panelInterfaceSettings.Initialize();
                }
                else { Debug.LogError("[AcrealUI.UIPauseWindow] Unable to get UIPanel_GeneralSettings script from GameObject \"" + (interfaceTform != null ? interfaceTform.gameObject.name : "NULL")); }
            }

            Transform audioTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_panelAudioSettings);
            if (audioTform != null)
            {
                _panelAudioSettings = audioTform.GetComponent<UIPanel>();
                if (_panelAudioSettings)
                {
                    _panelAudioSettings.Initialize();
                }
                else { Debug.LogError("[AcrealUI.UIPauseWindow] Unable to get UIPanel_AudioSettings script from GameObject \"" + (audioTform != null ? audioTform.gameObject.name : "NULL")); }
            }

            Transform videoTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_panelVideoSettings);
            if (videoTform != null)
            {
                _panelVideoSettings = videoTform.GetComponent<UIPanel>();
                if (_panelVideoSettings)
                {
                    _panelVideoSettings.Initialize();
                }
                else { Debug.LogError("[AcrealUI.UIPauseWindow] Unable to get UIPanel_VideoSettings script from GameObject \"" + (videoTform != null ? videoTform.gameObject.name : "NULL")); }
            }

            Transform controlTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_panelControlSettings);
            if (controlTform != null)
            {
                _panelControlSettings = controlTform.GetComponent<UIControlOptionsPanel>();
                if (_panelControlSettings)
                {
                    _panelControlSettings.Initialize();
                }
                else { Debug.LogError("[AcrealUI.UIPauseWindow] Unable to get UIControlOptionsPanel script from GameObject \"" + (controlTform != null ? controlTform.gameObject.name : "NULL")); }
            }
        }

        public override void Cleanup()
        {
            _activePanel?.Hide();
            _activePanel = null;

            _panelPaused?.Cleanup();
            _panelPaused = null;

            _panelSettings?.Cleanup();
            _panelSettings = null;

            _panelGeneralSettings?.Cleanup();
            _panelGeneralSettings = null;

            _panelInterfaceSettings?.Cleanup();
            _panelInterfaceSettings = null;

            _panelAudioSettings?.Cleanup();
            _panelAudioSettings = null;

            _panelVideoSettings?.Cleanup();
            _panelVideoSettings = null;

            _panelControlSettings?.Cleanup();
            _panelControlSettings = null;

            base.Cleanup();
        }

        public override void ResetWindow()
        {
            SetHeaderText(null);
            SetState(PauseWindowState.None);

            _panelPaused?.ResetPanel();
            _panelSettings?.ResetPanel();
            _panelGeneralSettings?.ResetPanel();
            _panelInterfaceSettings?.ResetPanel();
            _panelAudioSettings?.ResetPanel();
            _panelVideoSettings?.ResetPanel();
            _panelControlSettings?.ResetPanel();

            base.ResetWindow();
        }
        #endregion


        #region Public API
        public void SetState(PauseWindowState stateToSet, bool setSizeInstantly = false)
        {
            if (_currentState != stateToSet)
            {
                if (_activePanel != null)
                {
                    _activePanel.Hide();
                }
                _activePanel = null;

                PauseWindowState prevState = currentState;
                _currentState = stateToSet;
                switch (_currentState)
                {
                    case PauseWindowState.Settings:
                        _activePanel = _panelSettings;
                        SetHeaderText("Settings"); //TODO(Acreal): localize this string
                        break;

                    case PauseWindowState.Settings_General:
                        _activePanel = _panelGeneralSettings;
                        SetHeaderText("General"); //TODO(Acreal): localize this string
                        break;

                    case PauseWindowState.Settings_Interface:
                        _activePanel = _panelInterfaceSettings;
                        SetHeaderText("Interface"); //TODO(Acreal): localize this string
                        break;

                    case PauseWindowState.Settings_Video:
                        _activePanel = _panelVideoSettings;
                        SetHeaderText("Video"); //TODO(Acreal): localize this string
                        break;

                    case PauseWindowState.Settings_Audio:
                        _activePanel = _panelAudioSettings;
                        SetHeaderText("Audio"); //TODO(Acreal): localize this string
                        break;

                    case PauseWindowState.Settings_Controls:
                        _activePanel = _panelControlSettings;
                        SetHeaderText("Controls"); //TODO(Acreal): localize this string
                        break;

                    default:
                        _activePanel = _panelPaused;
                        SetHeaderText("Default"); //TODO(Acreal): localize this string
                        break;
                }

                if (_activePanel != null)
                {
                    _activePanel.Show();

                    RectTransform rt = _canvasGroup != null ? _canvasGroup.transform as RectTransform : transform as RectTransform;
                    if (setSizeInstantly)
                    {
                        Vector2 size = _activePanel.panelSize + _panelSizeOffset;

                        LayoutElement layoutElem = _canvasGroup.GetComponent<LayoutElement>();
                        if (layoutElem != null)
                        {
                            layoutElem.minWidth = size.x;
                            layoutElem.minHeight = size.y;
                        }
                        else
                        {
                            rt.sizeDelta = size;
                        }
                    }
                    else
                    {
                        StartCoroutine(TweenSizeRoutine(rt.sizeDelta, _activePanel.panelSize + _panelSizeOffset, 0.1f));
                    }
                }
            }
        }
        #endregion


        #region Panel Size
        private IEnumerator<float> TweenSizeRoutine(Vector2 from, Vector2 to, float duration)
        {
            LayoutElement layoutElem = _canvasGroup.GetComponent<LayoutElement>();
            if (layoutElem != null)
            {
                layoutElem.minWidth = from.x;
                layoutElem.minHeight = from.y;
            }

            float t = duration;
            while(t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                float lerpT = 1f - Mathf.InverseLerp(0f, duration, t);

                if (layoutElem != null)
                {
                    layoutElem.minWidth = Mathf.Lerp(from.x, to.x, lerpT);
                    layoutElem.minHeight = Mathf.Lerp(from.y, to.y, lerpT);
                }

                yield return 0f;
            }

            if (layoutElem != null)
            {
                layoutElem.minWidth = to.x;
                layoutElem.minHeight = to.y;
            }
        }
        #endregion
    }
}
