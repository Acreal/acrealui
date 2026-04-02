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
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AcrealUI
{
    public class UIConversationWindowController : DaggerfallTalkWindow, IWindowController
    {
        #region Variables
        private UIConversationWindow _conversationWindowInstance = null;
        private Dictionary<int, TalkManager.ListItem> _instanceIdToTopicListItemDict = null;
        private Stack<List<TalkManager.ListItem>> _topicListStack = null;
        private SpeakingStyle _currentSpeakingStyle = SpeakingStyle.Normal;
        private string _pendingDialogueText = null;
        private int _selectedTopicInstanceId = 0;
        //private bool _speakingInProgress = false;
        #endregion


        #region Constructor
        public UIConversationWindowController(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null) : base(uiManager, previous)
        {
            listboxConversation = new ListBox();

            _topicListStack = new Stack<List<TalkManager.ListItem>>();
            _instanceIdToTopicListItemDict = new Dictionary<int, TalkManager.ListItem>();
            _selectedTopicInstanceId = 0;

            CreateWindow();
        }
        #endregion


        #region IWindowController
        public void ShowWindow()
        {
            if (_conversationWindowInstance != null)
            {
                TalkManager.Instance.ForceTopicListsUpdate();

                #region Populate Topics
                //NOTE(Acreal): since the topics are dynamic and based on the NPC you're talking to,
                //this list has to be created when the conversation is started, and cannot be cached
                //at Initialization time
                TalkManager.ListItem tellMeAbt = new TalkManager.ListItem
                {
                    type = TalkManager.ListItemType.ItemGroup,
                    caption = "Tell Me About",//UIUtilityFunctions.GetLocalizedText("button_tellmeabout"),
                    listChildItems = TalkManager.Instance.ListTopicTellMeAbout,
                };

                TalkManager.ListItem location = new TalkManager.ListItem
                {
                    type = TalkManager.ListItemType.ItemGroup,
                    caption = "Location",//UIUtilityFunctions.GetLocalizedText("button_location"),
                    listChildItems = TalkManager.Instance.ListTopicLocation,
                };

                TalkManager.ListItem people = new TalkManager.ListItem
                {
                    type = TalkManager.ListItemType.ItemGroup,
                    caption = "People",//UIUtilityFunctions.GetLocalizedText("button_people"),
                    listChildItems = TalkManager.Instance.ListTopicPerson,
                };

                TalkManager.ListItem things = new TalkManager.ListItem
                {
                    type = TalkManager.ListItemType.ItemGroup,
                    caption = "Things",//UIUtilityFunctions.GetLocalizedText("button_things"),
                    listChildItems = TalkManager.Instance.ListTopicThings,
                };

                TalkManager.ListItem work = new TalkManager.ListItem
                {
                    type = TalkManager.ListItemType.Item,
                    questionType = TalkManager.QuestionType.Work,
                    caption = "Work",//UIUtilityFunctions.GetLocalizedText("button_work"),
                };

                List<TalkManager.ListItem> whereIsList = new List<TalkManager.ListItem>
                {
                    location,
                    people,
                    things,
                    work,
                };

                TalkManager.ListItem whereIs = new TalkManager.ListItem
                {
                    type = TalkManager.ListItemType.ItemGroup,
                    caption = "Where Is",//UIUtilityFunctions.GetLocalizedText("button_whereis"),
                    listChildItems = whereIsList,
                };

                List<TalkManager.ListItem> defaultTopicList = new List<TalkManager.ListItem>
                {
                    tellMeAbt,
                    whereIs,
                };

                PushTopicList(defaultTopicList);
                #endregion

                SetQuestionAnswerPairInConversationListbox(null, TalkManager.Instance.NPCGreetingText);
                _conversationWindowInstance.SetPendingDialogue(null);
                _conversationWindowInstance.Show();
                _conversationWindowInstance.UpdateTopicPanelSize();
            }
        }

        public void HideWindow()
        {
            if (_conversationWindowInstance != null)
            {
                _conversationWindowInstance.ClearTopics();
                _conversationWindowInstance.ClearDialogue();
                _conversationWindowInstance.Hide();
            }
        }

        public void CreateWindow()
        {
            if (UIManager.referenceManager.prefab_conversationWindow == null) { return; }

            _conversationWindowInstance = Object.Instantiate(UIManager.referenceManager.prefab_conversationWindow);
            _conversationWindowInstance.Initialize();
            _conversationWindowInstance.SetHeaderText("Conversation History"); // TODO(Acreal): localize string
            _conversationWindowInstance.Hide();

            _conversationWindowInstance.Event_ButtonClick_CloseWindow += () => { CancelWindow(); };

            _conversationWindowInstance.normalSpeakingStyleToggle.DataSource_IsToggledOn = (_) => { return _currentSpeakingStyle == SpeakingStyle.Normal; };
            _conversationWindowInstance.politeSpeakingStyleToggle.DataSource_IsToggledOn = (_) => { return _currentSpeakingStyle == SpeakingStyle.Polite; };
            _conversationWindowInstance.bluntSpeakingStyleToggle.DataSource_IsToggledOn = (_) => { return _currentSpeakingStyle == SpeakingStyle.Blunt; };

            _conversationWindowInstance.normalSpeakingStyleToggle.Event_OnToggledOn += (_) => { SetSpeakingStyle(SpeakingStyle.Normal); };
            _conversationWindowInstance.politeSpeakingStyleToggle.Event_OnToggledOn += (_) => { SetSpeakingStyle(SpeakingStyle.Polite); };
            _conversationWindowInstance.bluntSpeakingStyleToggle.Event_OnToggledOn += (_) => { SetSpeakingStyle(SpeakingStyle.Blunt); };

            _conversationWindowInstance.previousTopicButton.DataSource_IsDisabled = (_) => { return _topicListStack == null || _topicListStack.Count <= 1; };
            _conversationWindowInstance.previousTopicButton.Event_OnLeftClick += (_, _1) =>
            {
                PopTopicList();
            };

            _conversationWindowInstance.Event_ButtonClicked_OnSubmitDialogueEntry += () =>
            {
                if (!string.IsNullOrWhiteSpace(_pendingDialogueText))
                {
                    string answer = string.Empty;
                    if (_instanceIdToTopicListItemDict.TryGetValue(_selectedTopicInstanceId, out TalkManager.ListItem listItem))
                    {
                        answer = TalkManager.Instance.GetAnswerText(listItem);
                    }
                    SetQuestionAnswerPairInConversationListbox(_pendingDialogueText, answer);

                    //_speakingInProgress = true;
                    //UIManager.Instance.RunCoroutine(GetHashCode(), 0, DisplayDialogueRoutine(_pendingDialogueInfo));

                    //if (_instanceIdToTopicListItemDict.TryGetValue(_selectedTopicInstanceId, out TalkManager.ListItem listItem))
                    //{
                    //    DialogueInfo reply = new DialogueInfo
                    //    {
                    //        entryPrefab = UIManager.referenceManager.prefab_npcDialogueEntry,
                    //        speakerPortrait = texturePortrait,
                    //        speakerName = TalkManager.Instance.NameNPC,
                    //        dialogueText = TalkManager.Instance.GetAnswerText(listItem),
                    //    };
                    //    UIManager.Instance.RunCoroutine(GetHashCode(), 1, RespondToPlayerRoutine(reply, 0.5f));
                    //}

                    _selectedTopicInstanceId = 0;
                    _pendingDialogueText = null;
                    _conversationWindowInstance.SetPendingDialogue(null);
                }
            };

            _conversationWindowInstance.Event_OnCopyDialogueToNotebook += (UIDialogueEntry dialogueEntry, int dialogueEntryIndex) =>
            {
                if (copyIndexes.Remove(dialogueEntryIndex))
                {
                    if (dialogueEntry.isPlayerDialogue) { dialogueEntry.SetBorderColor(textcolorQuestionBackgroundModernConversationStyle); }
                    else { dialogueEntry.SetBorderColor(textcolorAnswerBackgroundModernConversationStyle); }
                }
                else
                {
                    copyIndexes.Add(dialogueEntryIndex);
                    dialogueEntry.SetBorderColor(textcolorHighlighted);
                }
            };
        }

        public void DestroyWindow()
        {
            if (_conversationWindowInstance != null)
            {
                Object.Destroy(_conversationWindowInstance.gameObject);
            }
            _conversationWindowInstance = null;
        }
        #endregion


        #region Base Class Overrides
        public override void OnPush()
        {
            base.OnPush();
            ShowWindow();
        }

        public override void OnPop()
        {
            base.OnPop();
            HideWindow();
        }

        protected override void SetQuestionAnswerPairInConversationListbox(string question, string answer)
        {
            if (!string.IsNullOrWhiteSpace(question))
            {
                DialogueInfo questionInfo = new DialogueInfo
                {
                    dialogueText = question,
                    entryPrefab = UIManager.referenceManager.prefab_playerDialogueEntry,
                    speakerName = UIUtilityFunctions.GetPlayerName(),
                    speakerPortrait = UIUtilityFunctions.GetPlayerPortrait(),
                };

                _conversationWindowInstance.AddDialogueEntry(questionInfo).SetBorderColor(textcolorQuestionBackgroundModernConversationStyle);
                listboxConversation.AddItem(question, out ListBox.ListItem textLabelQuestion);
                textLabelQuestion.textColor = DaggerfallUI.DaggerfallQuestionTextColor; //used in base.OnPop() to determine if this was the player or NPC talking
            }

            if (!string.IsNullOrWhiteSpace(answer))
            {
                DialogueInfo answerInfo = new DialogueInfo
                {
                    dialogueText = answer,
                    entryPrefab = UIManager.referenceManager.prefab_npcDialogueEntry,
                    speakerName = TalkManager.Instance.NameNPC,
                    speakerPortrait = texturePortrait,
                };
                _conversationWindowInstance.AddDialogueEntry(answerInfo).SetBorderColor(textcolorAnswerBackgroundModernConversationStyle);
                listboxConversation.AddItem(answer, out ListBox.ListItem textLabelAnswer);
            }
        }

        //unused functions
        public override void Draw() { }
        protected override void Setup() { }
        #endregion


        #region Topics
        private void PushTopicList(List<TalkManager.ListItem> topics)
        {
            if (topics != null)
            {
                _selectedTopicInstanceId = 0;
                _topicListStack.Push(topics);
                PopulateTopicsFromList(topics);
                _conversationWindowInstance.previousTopicButton.Refresh();
            }
        }

        private void PopTopicList()
        {
            if (_topicListStack.Count > 1)
            {
                _selectedTopicInstanceId = 0;
                _topicListStack.Pop();
                PopulateTopicsFromList(_topicListStack.Peek());
                _conversationWindowInstance.previousTopicButton.Refresh();
            }
        }

        private void PopulateTopicsFromList(List<TalkManager.ListItem> topics)
        {
            if(_conversationWindowInstance == null) { return; }

            _conversationWindowInstance.ClearTopics();
            _instanceIdToTopicListItemDict.Clear();

            listCurrentTopics = topics;
            if (topics != null && topics.Count > 0)
            {
                List<TalkManager.ListItem> allGroupItems = new List<TalkManager.ListItem>();
                List<TalkManager.ListItem> allSingleItems = new List<TalkManager.ListItem>();

                for (int i = 0; i < topics.Count; i++)
                {
                    if (topics[i].type == TalkManager.ListItemType.ItemGroup)
                    {
                        allGroupItems.Add(topics[i]);
                    }
                    else if(topics[i].type == TalkManager.ListItemType.Item)
                    {
                        allSingleItems.Add(topics[i]);
                    }
                }

                foreach (TalkManager.ListItem item in allGroupItems)
                {
                    CreateTopicButton(item);
                }

                foreach (TalkManager.ListItem item in allSingleItems)
                {
                    CreateTopicButton(item);
                }

                if(allGroupItems.Count > 0 && allSingleItems.Count > 0)
                {
                    _conversationWindowInstance.ActivateTopicDividerWithSiblingIndex(allGroupItems.Count);
                }
                else
                {
                    _conversationWindowInstance.DeactivateTopicDivider();
                }

                _conversationWindowInstance.UpdateTopicPanelSize();
            }
        }

        private void CreateTopicButton(TalkManager.ListItem item)
        {
            if (item.caption == null) // this is a check to detect problems arising from old save data - where caption end up as null
            {
                item.caption = item.key; //  just try to take key as caption then (answers might still be broken)
                if (item.caption == string.Empty)
                {
                    item.caption = TextManager.Instance.GetLocalizedText("resolvingError");
                }
            }
            else if (item.caption == string.Empty)
            {
                item.caption = TextManager.Instance.GetLocalizedText("resolvingError");
            }

            UIButton btnPrefab = item.type == TalkManager.ListItemType.ItemGroup ? UIManager.referenceManager.prefab_button : UIManager.referenceManager.prefab_button_textOnly;
            if (btnPrefab != null)
            {
                UIButton topicBtn = _conversationWindowInstance.AddTopicEntry(item.caption, btnPrefab);
                if (topicBtn != null)
                {
                    _instanceIdToTopicListItemDict.Add(topicBtn.gameObject.GetInstanceID(), item);

                    topicBtn.SetDisplayValue(item.caption);
                    topicBtn.SetDisplayValueTextSize(28);

                    topicBtn.DataSource_IsDisabled = (GameObject btnObj) =>
                    {
                        _instanceIdToTopicListItemDict.TryGetValue(btnObj.GetInstanceID(), out TalkManager.ListItem listItem);
                        return listItem == null || (listItem.type == TalkManager.ListItemType.ItemGroup && (listItem.listChildItems == null || listItem.listChildItems.Count == 0));
                    };

                    topicBtn.Event_OnAnyClick += (UIButton btn, PointerEventData data) =>
                    {
                        _selectedTopicInstanceId = btn.gameObject.GetInstanceID();

                        if (_instanceIdToTopicListItemDict.TryGetValue(_selectedTopicInstanceId, out TalkManager.ListItem listItem))
                        {
                            if (listItem.type == TalkManager.ListItemType.ItemGroup)
                            {
                                List<TalkManager.ListItem> topicList = listItem.listChildItems;
                                if (topicList != null)
                                {
                                    PushTopicList(topicList);
                                }
                            }
                            else
                            {
                                _pendingDialogueText = TalkManager.Instance.GetQuestionText(listItem, UIUtilityFunctions.SpeakingStyleToTalkTone(_currentSpeakingStyle));
                                _conversationWindowInstance.SetPendingDialogue(_pendingDialogueText);
                            }
                        }
                    };

                    LayoutElement layoutElem = topicBtn.GetComponent<LayoutElement>();
                    if (layoutElem != null)
                    {
                        layoutElem.minWidth = 0;
                        layoutElem.minHeight = 42;
                        layoutElem.flexibleWidth = 1f;
                        layoutElem.flexibleHeight = 0f;
                    }

                    topicBtn.Refresh();
                }
            }
        }
        #endregion


        #region TalkTone/SpeakingStyle
        private void SetSpeakingStyle(SpeakingStyle speakingStyle)
        {
            _currentSpeakingStyle = speakingStyle;

            if (_conversationWindowInstance != null)
            {
                _conversationWindowInstance.normalSpeakingStyleToggle?.Refresh();
                _conversationWindowInstance.politeSpeakingStyleToggle?.Refresh();
                _conversationWindowInstance.bluntSpeakingStyleToggle?.Refresh();
            }

            if (_selectedTopicInstanceId != 0 && TalkManager.Instance != null)
            {
                if (_instanceIdToTopicListItemDict.TryGetValue(_selectedTopicInstanceId, out TalkManager.ListItem listItem))
                {
                    _pendingDialogueText = TalkManager.Instance.GetQuestionText(listItem, UIUtilityFunctions.SpeakingStyleToTalkTone(_currentSpeakingStyle));
                    _conversationWindowInstance.SetPendingDialogue(_pendingDialogueText);
                }
            }
        }
        #endregion


        #region Dialogue Routine
        //private IEnumerator<float> DisplayDialogueRoutine(DialogueInfo dialogueInfo)
        //{
        //    int charsPerSecond = 32;
        //    float startTime = Time.unscaledTime;

        //    //disable input while your character is speaking
        //    _conversationWindowInstance.SetOkayButtonEnabled(false);
        //    _conversationWindowInstance.SetInputEnabled(false);

        //    yield return 0f; //give input a chance to clear

        //    UIDialogueEntry entry = _conversationWindowInstance.AddDialogueEntry(dialogueInfo);

        //    int charIdx = 0;
        //    int lastCharIdx = 0;
        //    int maxIdx = dialogueInfo.dialogueText.Length;
        //    while(charIdx < maxIdx)
        //    {
        //        //handle displaying text with a "typewriter" style
        //        charIdx = Mathf.Min(Mathf.RoundToInt((Time.unscaledTime - startTime) * charsPerSecond), maxIdx);
        //        if (lastCharIdx != charIdx)
        //        {
        //            lastCharIdx = charIdx;
        //            string displayValue = dialogueInfo.dialogueText.Substring(0, charIdx);
        //            entry.SetDisplayValue(displayValue);
        //        }

        //        if(InputManager.Instance.GetKeyDown(InputManager.Instance.GetBinding(InputManager.Actions.ActivateCenterObject)))
        //        {
        //            break;
        //        }

        //        yield return 0f;
        //    }

        //    _speakingInProgress = false;
        //    entry.SetDisplayValue(dialogueInfo.dialogueText);
        //    _conversationWindowInstance.SetOkayButtonEnabled(true);
        //    _conversationWindowInstance.SetInputEnabled(true);
        //}

        //private IEnumerator<float> RespondToPlayerRoutine(DialogueInfo dialogue, float delay)
        //{
        //    while(_speakingInProgress)
        //    {
        //        yield return 0f;
        //    }

        //    //disable input until NPC responds
        //    _conversationWindowInstance.SetOkayButtonEnabled(false);
        //    _conversationWindowInstance.SetInputEnabled(false);

        //    yield return 0f; //give input a chance to clear

        //    float t = delay;
        //    while(t > 0f)
        //    {
        //        if (InputManager.Instance.GetKeyDown(InputManager.Instance.GetBinding(InputManager.Actions.ActivateCenterObject)))
        //        {
        //            break;
        //        }

        //        t -= Time.unscaledDeltaTime;
        //        yield return 0f;
        //    }

        //    //add response and re-enable input
        //    _speakingInProgress = true;
        //    UIManager.Instance.RunCoroutine(GetHashCode(), 0, DisplayDialogueRoutine(dialogue));

        //    while (_speakingInProgress)
        //    {
        //        yield return 0f;
        //    }

        //    _conversationWindowInstance.SetOkayButtonEnabled(true);
        //    _conversationWindowInstance.SetInputEnabled(true);
        //}
        #endregion
    }
}
