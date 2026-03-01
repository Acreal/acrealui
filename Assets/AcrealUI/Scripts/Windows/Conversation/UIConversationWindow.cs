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
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIConversationWindow : UIWindow
    {
        #region Variables
        [SerializeField] private string _gameObjName_okayButton = null;
        [SerializeField] private string _gameObjName_historyButton = null;
        [SerializeField] private string _gameObjName_topicDivider = null;
        [SerializeField] private string _gameObjName_topicTypeParent = null;
        [SerializeField] private string _gameObjName_topicEntryParent = null;
        [SerializeField] private string _gameObjName_dialogueEntryParent = null;

        private UIButton _okayButton = null;
        private UIButton _historyButton = null;
        private GameObject _topicDivider = null;
        private Transform _topicTypeParent = null;
        private Transform _topicEntryParent = null;
        private Transform _dialogueEntryParent = null;

        private List<UIButton> _topicButtonEntries = null;
        private List<UIDialogueEntry> _dialogueEntries = null;
        #endregion


        #region Events
        public event System.Action OnSubmitDialogueEntry = null;
        public event System.Action OnOpenDialogueHistory = null;
        #endregion


        #region Initialization
        public override void Initialize()
        {
            base.Initialize();

            Transform okayTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_okayButton);
            if(okayTform != null)
            {
                _okayButton = okayTform.GetComponent<UIButton>();
                if(_okayButton != null)
                {
                    _okayButton.Event_OnClicked += (UIButton btn, PointerEventData data) =>
                    {
                        if (data.button == PointerEventData.InputButton.Left && data.clickCount == 1)
                        {
                            OnSubmitDialogueEntry?.Invoke();
                        }
                    };
                }
            }

            Transform historyTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_historyButton);
            if (historyTform != null)
            {
                _historyButton = historyTform.GetComponent<UIButton>();
                if (_historyButton != null)
                {
                    _historyButton.Event_OnClicked += (UIButton btn, PointerEventData data) =>
                    {
                        if (data.button == PointerEventData.InputButton.Left && data.clickCount == 1)
                        {
                            OnOpenDialogueHistory?.Invoke();
                        }
                    };
                }
            }

            Transform divTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_topicDivider);
            if (divTform != null)
            {
                _topicDivider = divTform.gameObject;
            }

            _topicTypeParent = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_topicTypeParent);
            if (_topicTypeParent != null)
            {
                UIButton tellMeBtn = AddTopicTypeButton("Tell me about..."); // TODO(Acreal): localize string
                if (tellMeBtn != null)
                {

                }

                UIButton whereBtn = AddTopicTypeButton("Where is..."); // TODO(Acreal): localize string
                if (whereBtn != null)
                {
                    
                }
            }

            _topicEntryParent = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_topicEntryParent);
            _dialogueEntryParent = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_dialogueEntryParent);
        }
        #endregion


        #region Public API
        public UIDialogueEntry AddDialogueEntry(DialogueInfo dialogue)
        {
            if (_dialogueEntries != null)
            {
                UIDialogueEntry dialogueEntry = Instantiate(UIManager.referenceManager.prefab_dialogueEntry, _dialogueEntryParent);
                if (dialogueEntry != null)
                {
                    dialogueEntry.Initialize();
                    _dialogueEntries.Add(dialogueEntry);
                    return dialogueEntry;
                }
            }
            return null;
        }

        public UIButton AddTopicEntry()
        {
            if(_topicButtonEntries != null)
            {
                
            }
            return null;
        }
        #endregion


        #region Topic Entry Management
        private UIButton AddTopicTypeButton(string buttonLabel)
        {
            UIButton topicTypeBtn = Instantiate(UIManager.referenceManager.prefab_button, _topicTypeParent);
            if (topicTypeBtn != null)
            {
                topicTypeBtn.Initialize();
                topicTypeBtn.SetDisplayValue(buttonLabel);
                topicTypeBtn.SetDisplayValueTextSize(28);

                LayoutElement layoutElem = topicTypeBtn.GetComponent<LayoutElement>();
                if (layoutElem != null)
                {
                    layoutElem.minWidth = 0;
                    layoutElem.minHeight = 42;
                    layoutElem.flexibleWidth = 1f;
                    layoutElem.flexibleHeight = 0f;
                }
            }
            return topicTypeBtn;
        }
        #endregion
    }
}
