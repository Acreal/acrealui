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
                -need to compare performance of both options in a test script first

        -UI data should always flow from Object -> Controller -> UI
        -Player Input should always flow from UI -> Controller -> Object
        -UI scripts should not contain any Daggerfall scripts or namespaces
            -they are only used to take in basic data and put it on the screen
            -excludes 'DaggerfallWorkshop.Game.Utility.ModSupport' which is needed by [ImportedComponent]
*/

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop;

namespace AcrealUI
{
    public class UIManager : MonoBehaviour
    {
        public static Mod mod = null;

        public static UIReferenceManager referenceManager { get; private set; }
        public static UITooltipManager tooltipManager { get; private set; }

        [Invoke(StateManager.StateTypes.Start)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            GameObject go = new GameObject("UIManager");
            go.AddComponent<UIManager>();
        }

        private void Awake()
        {
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

            
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.PauseOptions, typeof(UIPauseWindowController));
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Inventory, typeof(UIInventoryWindowController));
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.UnitySaveGame, typeof(UISaveWindowController));


            

            mod.IsReady = true;
        }

        private void Start()
        {
            //if (referenceManager.prefab_hud != null)
            //{
            //    Instantiate(referenceManager.prefab_hud);
            //}

            //DaggerfallUI.Instance.DaggerfallHUD.Enabled = false;

            /////////////////////////////////////////////////////////////////////////////////////////////////
            //**************************************[NEW WINDOWS]******************************************//
            /////////////////////////////////////////////////////////////////////////////////////////////////
            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.PauseOptions, typeof(UIPauseWindowController));
            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Inventory, typeof(UIInventoryWindowController));
            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.UnitySaveGame, typeof(UISaveWindowController));

            /////////////////////////////////////////////////////////////////////////////////////////////////
            //**************************[WINDOWS THAT NEED TO BE REPLACED]*********************************//
            /////////////////////////////////////////////////////////////////////////////////////////////////
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Automap, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Banking, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.BankPurchasePopup, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.BookReader, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Court, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.DaedraSummoned, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.EffectSettingsEditor, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.ExteriorAutomap, null);
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
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Start, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.StartNewGameWizard, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Talk, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Tavern, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.TeleportPopUp, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Trade, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Transport, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.TravelMap, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.TravelPopUp, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.UseMagicItem, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.VidPlayer, null);
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.WitchesCovenPopup, null);


            /////////////////////////////////////////////////////////////////////////////////////////////////
            //**********************************[DEPRECATED WINDOWS]***************************************//
            /////////////////////////////////////////////////////////////////////////////////////////////////
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.UnityMouseControls, null); <-- now rolled into the pause window
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.JoystickControls, null);   <-- now rolled into the pause window
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Controls, null;            <-- now rolled into the pause window
            //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.CharacterSheet, null);     <-- now rolled into the inventory window
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
    }
}
