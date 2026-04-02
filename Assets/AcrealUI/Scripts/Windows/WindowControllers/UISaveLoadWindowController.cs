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

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using System.Collections.Generic;
using UnityEngine;

namespace AcrealUI
{
    public class UISaveLoadWindowController : DaggerfallUnitySaveGameWindow, IWindowController
    {
        #region Definitions
        public enum SaveWindowState
        {
            None = 0,
            Save = 1,
            Load = 2,
            SelectCharacter = 3,
            ImportSave = 4,
        }
        #endregion


        #region Variables
        private UIWindowSaveLoad _saveLoadGameWindowInstance = null;
        private UISaveGameData _selectedSaveGameData = new UISaveGameData();
        private SaveWindowState _currentState = SaveWindowState.None;
        #endregion


        #region Constructor
        public UISaveLoadWindowController(IUserInterfaceManager uiManager, Modes mode, DaggerfallBaseWindow previous = null, bool displayMostRecentChar = false) : base(uiManager, mode, previous, displayMostRecentChar)
        {
            CreateWindow();
        }
        #endregion


        #region Base Class Overrides
        public override void OnPush()
        {
            currentPlayerName = GameManager.Instance != null && GameManager.Instance.PlayerEntity != null ? GameManager.Instance.PlayerEntity.Name : null;

            // TODO(Acreal): find a better way to determine this rather than checking for "Nameless"
            // assuming players can name themselves this intentionally
            if (string.IsNullOrWhiteSpace(currentPlayerName) || currentPlayerName == "Nameless")
            {
                currentPlayerName = UIUtilityFunctions.GetMostRecentSaveGameData().characterName;
            }

            if (!_selectedSaveGameData.isValid)
            {
                _selectedSaveGameData = UIUtilityFunctions.GetMostRecentSaveGameData(currentPlayerName);
            }

            if (mode == Modes.SaveGame)
            {
                SetState(SaveWindowState.Save);
            }
            else
            {
                SetState(SaveWindowState.Load);
            }

            ShowWindow();
        }

        public override void OnPop()
        {
            base.OnPop();

            _selectedSaveGameData = new UISaveGameData();

            // TODO(Acreal): keep track of window instances internally so there's no need
            // to destroy every time
            if (_saveLoadGameWindowInstance != null)
            {
                _saveLoadGameWindowInstance.Hide();
                Object.Destroy(_saveLoadGameWindowInstance.gameObject);
            }
            _saveLoadGameWindowInstance = null;
        }

        protected override void Setup()
        {
            ParentPanel.BackgroundColor = ScreenDimColor;
        }

        protected void UpdateSavesEntries()
        {
            if(_saveLoadGameWindowInstance == null) { return; }

            _saveLoadGameWindowInstance.ClearSaveEntries();

            switch(_currentState)
            {
                case SaveWindowState.Save:
                case SaveWindowState.Load:
                    bool hasSaves = false;
                    if (!string.IsNullOrWhiteSpace(currentPlayerName))
                    {
                        // Get list of all save files (and any relevant meta info) for this character name
                        List<UISaveGameData> allSaveData = UIUtilityFunctions.GetAllCharacterSaveGameData(currentPlayerName);
                        if (allSaveData != null)
                        {
                            hasSaves = allSaveData.Count > 0;
                            for (int i = 0; i < allSaveData.Count; i++)
                            {
                                UISaveGameData saveData = allSaveData[i];
                                if (saveData.saveKey < 0)
                                {
                                    return;
                                }

                                // Use saveKey.ToString() here (rather than saveName) so that we can
                                // retrieve the correct entry even if the saveName changes
                                UISaveGameEntry saveEntry = _saveLoadGameWindowInstance.GetOrAddSaveEntry(saveData.saveKey.ToString());
                                if (saveEntry != null)
                                {
                                    saveEntry.SetSaveData(saveData);
                                    saveEntry.SetDisplayName(saveData.saveName);

                                    saveEntry.transform.SetSiblingIndex(i);

                                    saveEntry.Event_OnToggledOn += (UIToggle toggle) =>
                                    {
                                        _saveLoadGameWindowInstance.SetInputFieldValue(toggle.displayName, false);

                                        UISaveGameEntry gameEntry = toggle as UISaveGameEntry;
                                        UISaveGameData data = gameEntry.GetSaveData();
                                        _selectedSaveGameData = data;
                                        _saveLoadGameWindowInstance.SetScreenshotTexture(data.screenshot);
                                        _saveLoadGameWindowInstance.SetTimestampText(data.realTimestampString, data.gameTimestampString);
                                        _saveLoadGameWindowInstance.SetVersionText(data.gameVersion);
                                    };
                                }
                            }

                            if (_selectedSaveGameData.saveKey >= 0)
                            {
                                UISaveGameEntry selectedEntry = _saveLoadGameWindowInstance.GetSaveEntryBySaveName(_selectedSaveGameData.saveName);
                                if(selectedEntry != null)
                                {
                                    selectedEntry.isToggledOn = true;
                                }
                            }
                        }
                    }

                    if (_saveLoadGameWindowInstance != null)
                    {
                        _saveLoadGameWindowInstance.SetNoSavesTextActive(!hasSaves);
                    }
                    break;

                case SaveWindowState.SelectCharacter:
                    string[] allChars = UIUtilityFunctions.GetAllSavedCharacterNames();
                    if (allChars != null)
                    {
                        // Populate list with character names instead of save file names
                        foreach (string charName in allChars)
                        {
                            UISaveGameData saveData = new UISaveGameData
                            {
                                isValid = true,
                                characterName = charName,
                            };

                            UISaveGameEntry saveEntry = _saveLoadGameWindowInstance.GetOrAddSaveEntry(saveData.characterName);
                            if (saveEntry != null)
                            {
                                saveEntry.SetSaveData(saveData);
                                saveEntry.SetDisplayName(saveData.characterName);

                                saveEntry.Event_OnToggledOn += (UIToggle toggle) =>
                                {
                                    UISaveGameEntry entry = toggle as UISaveGameEntry;
                                    if (entry != null)
                                    {
                                        UISaveGameData data = entry.GetSaveData();
                                        currentPlayerName = data.characterName;
                                        _selectedSaveGameData = UIUtilityFunctions.GetMostRecentSaveGameData(currentPlayerName);
                                        
                                        if (mode == Modes.SaveGame)
                                        {
                                            SetState(SaveWindowState.Save);
                                        }
                                        else
                                        {
                                            SetState(SaveWindowState.Load);
                                        }
                                    }
                                };
                            }
                        }
                    }
                    break;
            }
        }

        protected override void RenameSaveButton_OnGotUserInput(DaggerfallInputMessageBox sender, string saveName)
        {
            if (string.IsNullOrEmpty(saveName))
            {
                return;
            }

            // NOTE(Acreal): base func used 'saveNameTextBox.Text' here so we can't call it
            int key = GameManager.Instance.SaveLoadManager.FindSaveFolderByNames(currentPlayerName, _selectedSaveGameData.saveName);
            if (key == -1)
            {
                return;
            }

            GameManager.Instance.SaveLoadManager.Rename(key, saveName);
            _selectedSaveGameData.saveName = saveName;
            UpdateSavesEntries();
        }

        protected override void ConfirmDelete_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Delete)
            {
                // Get save key and delete file
                int key = GameManager.Instance.SaveLoadManager.FindSaveFolderByNames(_selectedSaveGameData.characterName, _selectedSaveGameData.saveName);
                if (key == -1)
                {
                    return;
                }
                GameManager.Instance.SaveLoadManager.DeleteSaveFolder(key);

                UpdateSavesEntries();

                // Reselect save entry or indicate no saves are found
                UISaveGameData saveData = UIUtilityFunctions.GetMostRecentSaveGameData();
                UISaveGameEntry saveEntry = _saveLoadGameWindowInstance.GetOrAddSaveEntry(saveData.saveKey.ToString());
                if (saveEntry != null)
                {
                    saveEntry.isToggledOn = true;
                }
                else
                {
                    _selectedSaveGameData = new UISaveGameData();
                    _saveLoadGameWindowInstance.SetNoSavesTextActive(_saveLoadGameWindowInstance.numSaveEntries == 0);
                    _saveLoadGameWindowInstance.SetScreenshotTexture(null);
                    _saveLoadGameWindowInstance.SetTimestampText(null, null);
                    _saveLoadGameWindowInstance.SetVersionText(null);
                }
            }

            CloseWindow();
        }

        public override void CancelWindow()
        {
            switch(_currentState)
            {
                case SaveWindowState.Save:
                case SaveWindowState.Load:
                    HideWindow();
                    base.CancelWindow();
                    break;

                case SaveWindowState.ImportSave:
                case SaveWindowState.SelectCharacter:
                    if(mode == Modes.SaveGame) { SetState(SaveWindowState.Save); }
                    else { SetState(SaveWindowState.Load); }
                    break;
            }
        }

        // NOTE(Acreal): I would normally override this instead of using UpdateSaveEntries(),
        // but the compiler gets angry if I do that, so we empty this function and use a
        // separate one instead
        protected override void RefreshSavesList() { }
        public override void Draw() { }
        #endregion


        #region IWindowController
        public void ShowWindow()
        {
            if(PreviousWindow != null)
            {
                IWindowController prev = PreviousWindow as IWindowController;
                if(prev != null)
                {
                    prev.HideWindow();
                }
            }

            if(_saveLoadGameWindowInstance != null)
            {
                _saveLoadGameWindowInstance.Show();
            }
        }

        public void HideWindow()
        {
            if (_saveLoadGameWindowInstance != null)
            {
                _saveLoadGameWindowInstance.Hide();
            }

            if (PreviousWindow != null)
            {
                IWindowController prev = PreviousWindow as IWindowController;
                if (prev != null)
                {
                    prev.ShowWindow();
                }
            }
        }

        public void CreateWindow()
        {
            if (_saveLoadGameWindowInstance == null && UIManager.referenceManager.prefab_saveLoadWindow != null)
            {
                _saveLoadGameWindowInstance = Object.Instantiate(UIManager.referenceManager.prefab_saveLoadWindow);
                if (_saveLoadGameWindowInstance != null)
                {
                    _saveLoadGameWindowInstance.Initialize();
                    _saveLoadGameWindowInstance.SetIsSaving(mode == Modes.SaveGame);

                    _saveLoadGameWindowInstance.Event_ButtonClick_CloseWindow += () =>
                    {
                        if (PreviousWindow != null)
                        {
                            DaggerfallUI.Instance.PopToHUD();
                        }
                        else
                        {
                            CancelWindow();
                        }
                    };

                    _saveLoadGameWindowInstance.Event_ButtonClick_PrevWindow += () =>
                    {
                        switch (_currentState)
                        {
                            case SaveWindowState.SelectCharacter:
                            case SaveWindowState.ImportSave:
                                if (_saveLoadGameWindowInstance.isSaving)
                                {
                                    SetState(SaveWindowState.Save);
                                }
                                else
                                {
                                    SetState(SaveWindowState.Load);
                                }
                                break;

                            default:
                                CancelWindow();
                                break;
                        }
                    };

                    _saveLoadGameWindowInstance.Event_ButtonClick_SaveGame += () =>
                    {
                        string characterName = GameManager.Instance.PlayerEntity.Name;
                        string saveName = _saveLoadGameWindowInstance.inputFieldValue;

                        if (string.IsNullOrWhiteSpace(saveName))
                        {
                            DaggerfallUI.MessageBox(TextManager.Instance.GetLocalizedText("youMustEnterASaveName"));
                            return;
                        }

                        int key = GameManager.Instance.SaveLoadManager.FindSaveFolderByNames(characterName, saveName);
                        if (key != -1)
                        {
                            DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                            messageBox.SetText(new string[] { TextManager.Instance.GetLocalizedText("confirmOverwriteSave"), "" });
                            messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
                            messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
                            messageBox.OnButtonClick += (DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton) =>
                            {
                                if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
                                {
                                    GameManager.Instance.SaveLoadManager.Save(characterName, saveName);
                                    DaggerfallUI.Instance.PopToHUD();
                                }
                                else
                                {
                                    CloseWindow();
                                }
                            };
                            uiManager.PushWindow(messageBox);
                        }
                        else
                        {
                            GameManager.Instance.SaveLoadManager.Save(characterName, saveName);
                            DaggerfallUI.Instance.PopToHUD();
                        }
                    };

                    _saveLoadGameWindowInstance.Event_ButtonClick_LoadGame += () =>
                    {
                        if (_selectedSaveGameData.saveKey >= 0)
                        {
                            GameManager.Instance.SaveLoadManager.Load(_selectedSaveGameData.saveKey);
                            DaggerfallUI.Instance.PopToHUD();
                        }
                    };

                    _saveLoadGameWindowInstance.Event_ButtonClick_RenameSave += () =>
                    {
                        // Must have a save selected
                        if (_selectedSaveGameData.saveKey < 0)
                        {
                            return;
                        }

                        // Input save name
                        DaggerfallInputMessageBox messageBox = new DaggerfallInputMessageBox(uiManager, this);
                        messageBox.SetTextBoxLabel(TextManager.Instance.GetLocalizedText("enterSaveName") + ": ");
                        messageBox.TextBox.Text = _selectedSaveGameData.saveName;
                        messageBox.OnGotUserInput += RenameSaveButton_OnGotUserInput;
                        uiManager.PushWindow(messageBox);
                    };

                    _saveLoadGameWindowInstance.Event_ButtonClick_DeleteSave += () =>
                    {
                        // Must have a save selected
                        if (string.IsNullOrWhiteSpace(_selectedSaveGameData.saveName))
                        {
                            return;
                        }

                        // Confirmation
                        DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                        messageBox.SetText(new string[] { TextManager.Instance.GetLocalizedText("confirmDeleteSave"), "" });
                        messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Delete);
                        messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Cancel);
                        messageBox.OnButtonClick += ConfirmDelete_OnButtonClick;
                        uiManager.PushWindow(messageBox);
                    };

                    _saveLoadGameWindowInstance.Event_ButtonClick_ImportSave += () =>
                    {
                        SetState(SaveWindowState.ImportSave);
                    };

                    _saveLoadGameWindowInstance.Event_ButtonClick_SelectCharacter += () =>
                    {
                        SetState(SaveWindowState.SelectCharacter);
                    };

                    _saveLoadGameWindowInstance.Event_InputFieldChanged_SaveFileName += (string saveFileName) =>
                    {
                        UISaveGameEntry saveEntry = _saveLoadGameWindowInstance.GetSaveEntryBySaveName(saveFileName);
                        if (saveEntry != null)
                        {
                            saveEntry.isToggledOn = true;

                            if (mode == Modes.SaveGame)
                            {
                                _saveLoadGameWindowInstance.SetSaveOrLoadButtonEnabled(true);
                            }
                            else
                            {
                                _saveLoadGameWindowInstance.SetSaveOrLoadButtonEnabled(saveEntry.GetSaveData().saveKey >= 0);
                            }
                        }
                        else
                        {
                            _saveLoadGameWindowInstance.SetScreenshotTexture(null);
                            _saveLoadGameWindowInstance.SetTimestampText(null, null);
                            _saveLoadGameWindowInstance.SetVersionText(null);

                            _saveLoadGameWindowInstance.toggleGroup.SetActiveToggle(null);
                            _saveLoadGameWindowInstance.SetSaveOrLoadButtonEnabled(mode == Modes.SaveGame);
                        }
                    };
                }
            }
        }

        public void DestroyWindow()
        {
            if (_saveLoadGameWindowInstance != null)
            {
                Object.Destroy(_saveLoadGameWindowInstance.gameObject);
            }
            _saveLoadGameWindowInstance = null;
        }
        #endregion


        #region State Management
        private void SetState(SaveWindowState stateToSet)
        {
            if (_currentState != stateToSet)
            {
                SaveWindowState prevState = _currentState;
                _currentState = stateToSet;

                if (_saveLoadGameWindowInstance != null)
                {
                    _saveLoadGameWindowInstance.SetScrollListActive(_currentState != SaveWindowState.ImportSave);

                    switch (_currentState)
                    {
                        case SaveWindowState.Save:
                        case SaveWindowState.Load:
                            _saveLoadGameWindowInstance.SetSize(new Vector2(770, 606));
                            _saveLoadGameWindowInstance.SetInputFieldValue(null);
                            _saveLoadGameWindowInstance.SetSaveInfoHeaderActive(true);
                            _saveLoadGameWindowInstance.SetSaveDetailsActive(true);
                            _saveLoadGameWindowInstance.SetRenameDeleteButtonsActive(true);
                            _saveLoadGameWindowInstance.SetImportButtonActive(_currentState == SaveWindowState.Load);
                            _saveLoadGameWindowInstance.SetSelectCharacterButtonActive(_currentState == SaveWindowState.Load);

                            string characterText = "<color=#FFFF00>" + _selectedSaveGameData.characterName + "</color>";
                            string headerText = null;
                            string promptText = null;
                            if (_currentState == SaveWindowState.Save)
                            {
                                headerText = UIUtilityFunctions.GetLocalizedText("saveButton");
                                promptText = UIUtilityFunctions.GetLocalizedText("savePrompt");
                            }
                            else
                            {
                                headerText = UIUtilityFunctions.GetLocalizedText("loadButton");
                                promptText = UIUtilityFunctions.GetLocalizedText("loadPrompt");
                            }
                            promptText = string.Format(UIUtilityFunctions.GetLocalizedText("saveLoadPromptFormat"), promptText, characterText);

                            _saveLoadGameWindowInstance.SetHeaderText(headerText);
                            _saveLoadGameWindowInstance.SetSaveLoadPromptText(promptText);
                            _saveLoadGameWindowInstance.SetSaveLoadButtonText(headerText); //reuse header text
                            break;

                        case SaveWindowState.ImportSave:
                        case SaveWindowState.SelectCharacter:
                            _saveLoadGameWindowInstance.SetSize(new Vector2(350, 450));
                            _saveLoadGameWindowInstance.SetSaveInfoHeaderActive(false);
                            _saveLoadGameWindowInstance.SetSaveDetailsActive(false);
                            _saveLoadGameWindowInstance.SetImportButtonActive(false);
                            _saveLoadGameWindowInstance.SetRenameDeleteButtonsActive(false);

                            if(_currentState == SaveWindowState.ImportSave)
                            {
                                //TODO(Acreal): localize this string
                                _saveLoadGameWindowInstance.SetHeaderText("Import Classic Save");
                            }
                            else
                            {
                                //TODO(Acreal): localize this string
                                _saveLoadGameWindowInstance.SetHeaderText("Select Character");
                            }
                            break;
                    }

                    UpdateSavesEntries();
                }
            }
        }
        #endregion
    }
}
