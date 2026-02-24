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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIWindowSaveLoad : UIWindow
    {
        #region Variables
        [SerializeField] private string _gameObjName_rawImage_saveScreenShot = null;
        [SerializeField] private string _gameObjName_inputField_saveName = null;
        [SerializeField] private string _gameObjName_parent_scrollGroup = null;
        [SerializeField] private string _gameObjName_parent_renameDeleteButtons = null;
        [SerializeField] private string _gameObjName_text_realTime = null;
        [SerializeField] private string _gameObjName_text_gameTime = null;
        [SerializeField] private string _gameObjName_text_version = null;
        [SerializeField] private string _gameObjName_text_saveOrLoad = null;
        [SerializeField] private string _gameObjName_text_importSave = null;
        [SerializeField] private string _gameObjName_text_saveLoadPrompt = null;
        [SerializeField] private string _gameObjName_text_noSaves = null;
        [SerializeField] private string _gameObjName_button_switchChar = null;
        [SerializeField] private string _gameObjName_button_rename = null;
        [SerializeField] private string _gameObjName_button_delete = null;
        [SerializeField] private string _gameObjName_button_saveLoad = null;
        [SerializeField] private string _gameObjName_button_import = null;
        [SerializeField] private string _gameObjName_savePromptParent = null;
        [SerializeField] private string _gameObjName_saveDetailsParent = null;
        [SerializeField] private string _gameObjName_scrollListParent = null;

        private RawImage _saveScreenShotRawImage = null;
        private TMP_InputField _saveNameInputField = null;
        private TextMeshProUGUI _realTimeText = null;
        private TextMeshProUGUI _gameTimeText = null;
        private TextMeshProUGUI _versionText = null;
        private TextMeshProUGUI _saveLoadText = null;
        private TextMeshProUGUI _importSaveText = null;
        private TextMeshProUGUI _saveLoadPromptText = null;
        private TextMeshProUGUI _noSavesText = null;
        private UIButton _switchCharButton = null;
        private UIButton _renameButton = null;
        private UIButton _deleteButton = null;
        private UIButton _saveLoadButton = null;
        private UIButton _importButton = null;
        private UIToggleGroup _saveEntriesToggleGroup = null;
        private GameObject _savePromptParent = null;
        private GameObject _saveDetailsParent = null;
        private GameObject _scrollListParent = null;
        private GameObject _renameDeleteButtonsParent = null;

        private Dictionary<string, UISaveGameEntry> _idToSaveGameDataDict = null;
        private Transform _scrollGroupParent = null;
        private bool _isSaving = false;
        #endregion


        #region Properties
        public int numSaveEntries
        {
            get { return _idToSaveGameDataDict != null ? _idToSaveGameDataDict.Count : 0; }
        }

        public bool isSaving
        {
            get { return _isSaving; }
        }

        public string inputFieldValue
        {
            get { return _saveNameInputField != null ? _saveNameInputField.text : null; }
        }

        public UIToggleGroup toggleGroup
        {
            get { return _saveEntriesToggleGroup; }
        }
        #endregion


        #region Events
        public event System.Action Event_ButtonClick_RenameSave = null;
        public event System.Action Event_ButtonClick_DeleteSave = null;
        public event System.Action Event_ButtonClick_SaveGame = null;
        public event System.Action Event_ButtonClick_LoadGame = null;
        public event System.Action Event_ButtonClick_SelectCharacter = null;
        public event System.Action Event_ButtonClick_ImportSave = null;
        public event System.Action<string> Event_InputFieldChanged_SaveFileName = null;
        #endregion


        #region Initialization/Cleanup
        public override void Initialize()
        {
            base.Initialize();

            if(_canvasComponent != null)
            {
                _canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            _idToSaveGameDataDict = new Dictionary<string, UISaveGameEntry>();

            if (!string.IsNullOrEmpty(_gameObjName_parent_scrollGroup))
            {
                _scrollGroupParent = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_parent_scrollGroup);
                if (_scrollGroupParent != null) 
                {
                    _saveEntriesToggleGroup = _scrollGroupParent.GetComponent<UIToggleGroup>();
                    if (_saveEntriesToggleGroup != null)
                    {
                        _saveEntriesToggleGroup.Initialize();
                        _saveEntriesToggleGroup.canBeToggledOff = false;
                    }
                    else { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find UIToggleGroup component on gameObject: \"" + _scrollGroupParent.name + "\""); }
                }
                else { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find GameObject by name: \"" + _gameObjName_parent_scrollGroup + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_savePromptParent))
            {
                Transform promptParentTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_savePromptParent);
                if (promptParentTform != null) { _savePromptParent = promptParentTform.gameObject; }
                else { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find GameObject by name: \"" + _gameObjName_savePromptParent + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_text_saveLoadPrompt))
            {
                Transform promptTextTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_saveLoadPrompt);
                if (promptTextTform != null) { _saveLoadPromptText = promptTextTform.GetComponent<TextMeshProUGUI>(); }
                if (_saveLoadPromptText == null) { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find TextMeshProUGUI script on GameObject \"" + _gameObjName_text_saveLoadPrompt + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_saveDetailsParent))
            {
                Transform saveDetailsParentTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_saveDetailsParent);
                if (saveDetailsParentTform != null) { _saveDetailsParent = saveDetailsParentTform.gameObject; }
                else { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find GameObject by name: \"" + _gameObjName_saveDetailsParent + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_scrollListParent))
            {
                Transform scrollListTForm = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_scrollListParent);
                if (scrollListTForm != null) { _scrollListParent = scrollListTForm.gameObject; }
                else { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find GameObject by name: \"" + _gameObjName_scrollListParent + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_parent_renameDeleteButtons))
            {
                Transform buttonsTForm = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_parent_renameDeleteButtons);
                if (buttonsTForm != null) { _renameDeleteButtonsParent = buttonsTForm.gameObject; }
                else { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find GameObject by name: \"" + _gameObjName_parent_renameDeleteButtons + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_inputField_saveName))
            {
                Transform inputFieldTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_inputField_saveName);
                if (inputFieldTform != null) { _saveNameInputField = inputFieldTform.GetComponent<TMP_InputField>(); }
                if (_saveNameInputField != null)
                {
                    _saveNameInputField.onValueChanged.AddListener((string inputText) =>
                    {
                        if (_saveNameInputField.isFocused)
                        {
                            Event_InputFieldChanged_SaveFileName?.Invoke(inputText);
                        }
                    });
                }
                else { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find TMP_InputField script on GameObject \"" + _gameObjName_inputField_saveName + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_rawImage_saveScreenShot))
            {
                Transform screenshotTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_rawImage_saveScreenShot);
                if (screenshotTform != null) { _saveScreenShotRawImage = screenshotTform.GetComponent<RawImage>(); }
                else { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find RawImage script on GameObject \"" + _gameObjName_rawImage_saveScreenShot + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_button_switchChar))
            {
                Transform switchTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_switchChar);
                if (switchTform != null) { _switchCharButton = switchTform.GetComponent<UIButton>(); }
                if (_switchCharButton != null)
                {
                    _switchCharButton.Initialize();
                    _switchCharButton.Event_OnClicked += (_, _1) => { Event_ButtonClick_SelectCharacter?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find UIButton script on GameObject \"" + _gameObjName_button_switchChar + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_button_rename))
            {
                Transform renameTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_rename);
                if (renameTform != null) { _renameButton = renameTform.GetComponent<UIButton>(); }
                if (_renameButton != null)
                {
                    _renameButton.Initialize();
                    _renameButton.Event_OnClicked += (_, _1) => { Event_ButtonClick_RenameSave?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find UIButton script on GameObject \"" + _gameObjName_button_rename + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_button_delete))
            {
                Transform deleteTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_delete);
                if (deleteTform != null) { _deleteButton = deleteTform.GetComponent<UIButton>(); }
                if (_deleteButton != null)
                {
                    _deleteButton.Initialize();
                    _deleteButton.Event_OnClicked += (_, _1) => { Event_ButtonClick_DeleteSave?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find UIButton script on GameObject \"" + _gameObjName_button_delete + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_button_saveLoad))
            {
                Transform saveLoadTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_saveLoad);
                if (saveLoadTform != null) { _saveLoadButton = saveLoadTform.GetComponent<UIButton>(); }
                if (_saveLoadButton != null)
                {
                    _saveLoadButton.Initialize();
                    _saveLoadButton.Event_OnClicked += (_, _1) =>
                    {
                        if (_isSaving) { Event_ButtonClick_SaveGame?.Invoke(); }
                        else { Event_ButtonClick_LoadGame?.Invoke(); }
                    };
                }
                else { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find UIButton script on GameObject \"" + _gameObjName_button_saveLoad + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_text_saveOrLoad))
            {
                Transform saveTextTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_saveOrLoad);
                if (saveTextTform != null) { _saveLoadText = saveTextTform.GetComponent<TextMeshProUGUI>(); }
                if (_saveLoadText == null) { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find TextMeshProUGUI script on GameObject \"" + _gameObjName_text_saveOrLoad + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_button_import))
            {
                Transform importTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_import);
                if (importTform != null) { _importButton = importTform.GetComponent<UIButton>(); }
                if (_importButton != null)
                {
                    _importButton.Initialize();
                    _importButton.Event_OnClicked += (_, _1) => { Event_ButtonClick_ImportSave?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find UIButton script on GameObject \"" + _gameObjName_button_import + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_text_realTime))
            {
                Transform rtTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_realTime);
                if (rtTform != null) { _realTimeText = rtTform.GetComponent<TextMeshProUGUI>(); }
                if (_realTimeText == null) { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find TextMeshProUGUI script on GameObject \"" + _gameObjName_text_realTime + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_text_gameTime))
            {
                Transform gtTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_gameTime);
                if (gtTform != null) { _gameTimeText = gtTform.GetComponent<TextMeshProUGUI>(); }
                if (_gameTimeText == null) { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find TextMeshProUGUI script on GameObject \"" + _gameObjName_text_gameTime + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_text_version))
            {
                Transform versTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_version);
                if (versTform != null) { _versionText = versTform.GetComponent<TextMeshProUGUI>(); }
                if (_versionText == null) { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find TextMeshProUGUI script on GameObject \"" + _gameObjName_text_version + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_text_noSaves))
            {
                Transform noSavesTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_noSaves);
                if (noSavesTform != null) { _noSavesText = noSavesTform.GetComponent<TextMeshProUGUI>(); }
                if (_noSavesText != null) { _noSavesText.text = UIUtilityFunctions.GetLocalizedText("noSavesFound"); }
                else { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find TextMeshProUGUI script on GameObject \"" + _gameObjName_text_noSaves + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_text_importSave))
            {
                Transform importTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_importSave);
                if (importTform != null) { _importSaveText = importTform.GetComponent<TextMeshProUGUI>(); }
                if (_importSaveText != null) { _importSaveText.text = UIUtilityFunctions.GetLocalizedText("classicSave"); }
                else { Debug.LogError("[AcrealUI.UIWindowSaveLoad] Failed to find TextMeshProUGUI script on GameObject \"" + _gameObjName_text_importSave + "\""); }
            }
        }
        #endregion


        #region Public API
        public void SetIsSaving(bool saving)
        {
            _isSaving = saving;
        }

        public void SetNoSavesTextActive(bool active)
        {
            if (_noSavesText != null)
            {
                _noSavesText.gameObject.SetActive(active);
            }
        }

        public void SetSaveInfoHeaderActive(bool active)
        {
            if (_savePromptParent != null)
            {
                _savePromptParent.SetActive(active);
            }
        }

        public void SetSaveDetailsActive(bool active)
        {
            if (_saveDetailsParent != null)
            {
                _saveDetailsParent.SetActive(active);
            }
        }

        public void SetScrollListActive(bool active)
        {
            if (_scrollListParent != null)
            {
                _scrollListParent.SetActive(active);
            }
        }

        public void SetImportButtonActive(bool active)
        {
            if (_importButton != null)
            {
                _importButton.gameObject.SetActive(active);
            }
        }

        public void SetSaveOrLoadButtonEnabled(bool enabled)
        {
            if (_saveLoadButton != null)
            {
                _saveLoadButton.isDisabled = !enabled;
            }
        }

        public void SetSelectCharacterButtonActive(bool active)
        {
            if (_switchCharButton != null)
            {
                _switchCharButton.gameObject.SetActive(active);
            }
        }

        public void SetRenameDeleteButtonsActive(bool active)
        {
            if (_renameDeleteButtonsParent != null)
            {
                _renameDeleteButtonsParent.SetActive(active);
            }
        }

        public void SetInputFieldValue(string val, bool setWithoutNotify = true)
        {
            if (_saveNameInputField != null)
            {
                if (setWithoutNotify)
                {
                    _saveNameInputField.SetTextWithoutNotify(val);
                }
                else
                {
                    _saveNameInputField.text = val;
                }
            }
        }

        public void SetSaveLoadPromptText(string promptText)
        {
            if (_saveLoadPromptText != null)
            {
                _saveLoadPromptText.text = promptText;
            }
        }

        public void SetTimestampText(string realTimestampString, string gameTimestampString)
        {
            if (_realTimeText != null)
            {
                _realTimeText.text = realTimestampString;
            }

            if (_gameTimeText != null)
            {
                _gameTimeText.text = gameTimestampString;
            }
        }

        public void SetVersionText(string versionString)
        {
            if (_versionText != null)
            {
                _versionText.text = versionString;
            }
        }

        public void SetScreenshotTexture(Texture2D screenshot)
        {
            if (_saveScreenShotRawImage != null)
            {
                _saveScreenShotRawImage.texture = screenshot;
                _saveScreenShotRawImage.enabled = screenshot != null;

                float aspect = screenshot != null ? (screenshot.width / (float)screenshot.height) : (16f / 9f);

                RectTransform rt = _saveScreenShotRawImage.transform.parent as RectTransform;
                LayoutElement layoutElem = rt != null ? rt.GetComponent<LayoutElement>() : null;
                if (layoutElem != null)
                {
                    layoutElem.minHeight = layoutElem.minWidth / aspect;
                }
                else if (rt != null)
                {
                    Vector2 size = rt.sizeDelta;
                    size.y = size.x / aspect;
                    rt.sizeDelta = size;
                }
            }
        }

        public void SetSaveLoadButtonText(string buttonText)
        {
            if (_saveLoadText != null)
            {
                _saveLoadText.text = buttonText;
            }
        }

        public bool HasSaveEntry(string entryId)
        {
            return _idToSaveGameDataDict.ContainsKey(entryId);
        }

        public UISaveGameEntry GetSaveEntryBySaveName(string filename)
        {
            foreach (UISaveGameEntry saveEntry in _idToSaveGameDataDict.Values)
            {
                UISaveGameData saveData = saveEntry.GetSaveData();
                if (!string.IsNullOrWhiteSpace(saveData.saveName) && saveData.saveName == filename)
                {
                    return saveEntry;
                }
            }
            return null;
        }

        public UISaveGameEntry GetOrAddSaveEntry(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) { return null; }

            UISaveGameEntry entry = null;
            _idToSaveGameDataDict.TryGetValue(name, out entry);
            if (entry == null)
            {
                entry = Instantiate(UIManager.referenceManager.prefab_saveEntry, _scrollGroupParent);
                if (entry != null)
                {
                    entry.Initialize();

                    if (_saveEntriesToggleGroup != null)
                    {
                        _saveEntriesToggleGroup.AddToggle(entry);
                    }

                    _idToSaveGameDataDict[name] = entry;
                }
            }
            else
            {
                Debug.Log("Duplicate entry found for: " + name);
            }
            return entry;
        }

        public void RemoveSaveEntry(string name)
        {
            UISaveGameEntry entry = null;
            if (_idToSaveGameDataDict.TryGetValue(name, out entry))
            {
                if (entry != null)
                {
                    if (_saveEntriesToggleGroup != null)
                    {
                        _saveEntriesToggleGroup.RemoveToggle(entry);
                    }

                    _idToSaveGameDataDict.Remove(name);
                    Destroy(entry.gameObject);
                }
            }
        }

        public void ClearSaveEntries()
        {
            if (_idToSaveGameDataDict.Count > 0)
            {
                List<string> destroyList = new List<string>();
                foreach (KeyValuePair<string, UISaveGameEntry> kvp in _idToSaveGameDataDict)
                {
                    if (kvp.Value != null)
                    {
                        destroyList.Add(kvp.Key);
                    }
                }

                for (int i = 0; i < destroyList.Count; i++)
                {
                    RemoveSaveEntry(destroyList[i]);
                }
            }
        }
        #endregion
    }
}
