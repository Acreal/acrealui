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

using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using System.Collections.Generic;

namespace AcrealUI
{
    public static class UITextStrings
    {
        //NOTE(Acreal): Any container below that has the first parameter defined, but
        //the second parameter as null, is NOT localized
        //the two parameters should be reversed once a localizationKey is available, such
        //that the first parameter is null, and the second is defined

        public static LocalizedText Global_Label_None = new LocalizedText("None", null, null);
        public static LocalizedText Global_Label_Paused = new LocalizedText("Paused", null, null);
        public static LocalizedText Global_Label_Back = new LocalizedText("Back", null, null);
        public static LocalizedText Global_Label_Primary = new LocalizedText("Primary", null, null);
        public static LocalizedText Global_Label_Secondary = new LocalizedText("Secondary", null, null);

        public static LocalizedText ConfirmationWindow_Title_SetDefaults = new LocalizedText("Set Defaults", null, null);
        public static LocalizedText ConfirmationWindow_Title_ClearBinding = new LocalizedText("Clear Action", null, null);
        public static LocalizedText ConfirmationWindow_Title_WaitingForInput = new LocalizedText("Waiting For Input", null, null);
        public static LocalizedText ConfirmationWindow_Title_DuplicateBinding = new LocalizedText("Duplicate Binding", null, null);

        public static LocalizedText ConfirmationWindow_Body_SetDefaults = new LocalizedText("Reset keybinds to default values?", null, null);
        public static LocalizedText ConfirmationWindow_Body_ClearBinding = new LocalizedText("Do you want to clear the binding for '{0}'?", null, null);
        public static LocalizedText ConfirmationWindow_Body_RebindAction = new LocalizedText("Rebinding {0} Action: {1}", null, null);
        public static LocalizedText ConfirmationWindow_Body_PressAnyKey = new LocalizedText("Press Any Key...\n('Esc' to Cancel)", null, null);
        public static LocalizedText ConfirmationWindow_Body_DuplicateBinding = new LocalizedText("'{0}' is already bound to '{1}'. Do you want to duplicate this binding?\n(Warning: This could cause unintended behaviour.)", null, null);


        public static LocalizedText OptionsWindow_Title_Settings = new LocalizedText("Settings", null, null);
        public static LocalizedText OptionsWindow_Title_General = new LocalizedText("General", null, null);
        public static LocalizedText OptionsWindow_Title_Interface = new LocalizedText("Interface", null, null);
        public static LocalizedText OptionsWindow_Title_NewWindows = new LocalizedText("New Windows", null, null);
        public static LocalizedText OptionsWindow_Title_Video = new LocalizedText("Video", null, null);
        public static LocalizedText OptionsWindow_Title_Window = new LocalizedText("Window", null, null);
        public static LocalizedText OptionsWindow_Title_Audio = new LocalizedText("Audio", null, null);
        public static LocalizedText OptionsWindow_Title_Controls = new LocalizedText("Controls", null, null);
        public static LocalizedText OptionsWindow_Title_Gameplay = new LocalizedText("Gameplay", null, null);
        public static LocalizedText OptionsWindow_Title_Rendering = new LocalizedText("Rendering", null, null);
        public static LocalizedText OptionsWindow_Title_Textures = new LocalizedText("Textures", null, null);
        public static LocalizedText OptionsWindow_Title_AntiAliasing = new LocalizedText(null, "antialiasing", null);
        public static LocalizedText OptionsWindow_Title_Camera = new LocalizedText("Camera", null, null);
        public static LocalizedText OptionsWindow_Title_Shadows = new LocalizedText("Shadows", null, null);
        public static LocalizedText OptionsWindow_Title_Volume = new LocalizedText("Volume", null, null);
        public static LocalizedText OptionsWindow_Title_Movement = new LocalizedText("Movement", null, null);
        public static LocalizedText OptionsWindow_Title_Combat = new LocalizedText("Combat", null, null);
        public static LocalizedText OptionsWindow_Title_Interaction = new LocalizedText("Interaction", null, null);
        public static LocalizedText OptionsWindow_Title_System = new LocalizedText("System", null, null);
        public static LocalizedText OptionsWindow_Title_AxisBindings = new LocalizedText("Axis Bindings", null, null);
        public static LocalizedText OptionsWindow_Title_JoystickControls = new LocalizedText("Joystick Controls", null, null);

        public static LocalizedText OptionsWindow_Label_HeadBobbing = new LocalizedText("Head Bobbing", null, null);
        public static LocalizedText OptionsWindow_Label_Resolution = new LocalizedText("Resolution", null, null);
        public static LocalizedText OptionsWindow_Label_Fullscreen = new LocalizedText("Fullscreen", null, null);
        public static LocalizedText OptionsWindow_Label_ExclusiveFullscreen = new LocalizedText("Exclusive Fullscreen", null, null);
        public static LocalizedText OptionsWindow_Label_Vsync = new LocalizedText("Vsync", null, null);
        public static LocalizedText OptionsWindow_Label_MaxFramerate = new LocalizedText("Max Framerate", null, null);
        public static LocalizedText OptionsWindow_Label_RenderQuality = new LocalizedText("Render Quality", null, null);
        public static LocalizedText OptionsWindow_Label_MainFilter = new LocalizedText("Main Filter", null, null);
        public static LocalizedText OptionsWindow_Label_GuiFilter = new LocalizedText("GUI Filter", null, null);
        public static LocalizedText OptionsWindow_Label_VideoFilter = new LocalizedText("Video Filter", null, null);
        public static LocalizedText OptionsWindow_Label_DungeonTextureMode = new LocalizedText("Dungeon Texture Mode", null, null);
        public static LocalizedText OptionsWindow_Label_EnableTextureArrays = new LocalizedText("Enable Texture Arrays", null, null);
        public static LocalizedText OptionsWindow_Label_AmbientLitInteriors = new LocalizedText("Ambient Lit Interiors", null, null);
        public static LocalizedText OptionsWindow_Label_FieldOfView = new LocalizedText("Field of View", null, null);
        public static LocalizedText OptionsWindow_Label_ShadowResolution = new LocalizedText("Shadow Resolution", null, null);
        public static LocalizedText OptionsWindow_Label_ExteriorShadowDistance = new LocalizedText("Exterior Shadow Distance", null, null);
        public static LocalizedText OptionsWindow_Label_InteriorShadowDistance = new LocalizedText("Interior Shadow Distance", null, null);
        public static LocalizedText OptionsWindow_Label_DungeonShadowDistance = new LocalizedText("Dungeon Shadow Distance", null, null);
        public static LocalizedText OptionsWindow_Label_ExteriorLightsCastShadows = new LocalizedText("Exterior Lights Cast Shadows", null, null);
        public static LocalizedText OptionsWindow_Label_InteriorLightsCastShadows = new LocalizedText("Interior Lights Cast Shadows", null, null);
        public static LocalizedText OptionsWindow_Label_DungeonLightsCastShadows = new LocalizedText("Dungeon Lights Cast Shadows", null, null);
        public static LocalizedText OptionsWindow_Label_NpcsCastShadows = new LocalizedText("NPCs Cast Shadows", null, null);
        public static LocalizedText OptionsWindow_Label_ObjectBillboardsCastShadows = new LocalizedText("Object Billboards Cast Shadows", null, null);
        public static LocalizedText OptionsWindow_Label_FoliageBillboardsCastShadows = new LocalizedText("Foliage Billboards Cast Shadows", null, null);
        public static LocalizedText OptionsWindow_Label_SoundEffects = new LocalizedText("Sound Effects", null, null);
        public static LocalizedText OptionsWindow_Label_Music = new LocalizedText("Music", null, null);
        public static LocalizedText OptionsWindow_Label_InvertLook = new LocalizedText("Invert Look", null, null);
        public static LocalizedText OptionsWindow_Label_MovementAcceleration = new LocalizedText("Movement Acceleration", null, null);
        public static LocalizedText OptionsWindow_Label_BowsDrawAndRelease = new LocalizedText("Bows Draw and Release", null, null);
        public static LocalizedText OptionsWindow_Label_ToggleSneak = new LocalizedText("Toggle Sneak", null, null);
        public static LocalizedText OptionsWindow_Label_Invert = new LocalizedText("Invert", null, null);
        public static LocalizedText OptionsWindow_Label_LookSensitivity = new LocalizedText("Look Sensitivity", null, null);
        public static LocalizedText OptionsWindow_Label_UIMouseSensitivity = new LocalizedText("UI Mouse Sensitivity", null, null);
        public static LocalizedText OptionsWindow_Label_MouseSmoothingFactor = new LocalizedText("Mouse Smoothing Factor", null, null);
        public static LocalizedText OptionsWindow_Label_MaximumMovementThreshold = new LocalizedText("Maximum Movement Threshold", null, null);
        public static LocalizedText OptionsWindow_Label_Deadzone = new LocalizedText("Deadzone", null, null);
        public static LocalizedText OptionsWindow_Label_Unlimited = new LocalizedText("Unlimited", null, null);
        public static LocalizedText OptionsWindow_Label_RetroModeOff = new LocalizedText(null, "retroModeOff", null);
        public static LocalizedText OptionsWindow_Label_RetroMode320 = new LocalizedText(null, "retroMode320x200", null);
        public static LocalizedText OptionsWindow_Label_RetroMode640 = new LocalizedText(null, "retroMode640x400", null);
        public static LocalizedText OptionsWindow_Label_Fxaa = new LocalizedText(null, "fxaa", null);
        public static LocalizedText OptionsWindow_Label_Smaa = new LocalizedText(null, "smaa", null);
        public static LocalizedText OptionsWindow_Label_Taa = new LocalizedText(null, "taa", null);

        public static LocalizedText OptionsWindow_Title_RetroMode = new LocalizedText(null, "retroMode", null);
        public static LocalizedText OptionsWindow_Title_DepthOfField = new LocalizedText(null, "depthOfField", null);
        public static LocalizedText OptionsWindow_Title_Bloom = new LocalizedText(null, "bloom", null);
        public static LocalizedText OptionsWindow_Title_AmbientOcclusion = new LocalizedText(null, "ambientOcclusion", null);
        public static LocalizedText OptionsWindow_Title_MotionBlur = new LocalizedText(null, "motionBlur", null);
        public static LocalizedText OptionsWindow_Title_Vignette = new LocalizedText(null, "vignette", null);
        public static LocalizedText OptionsWindow_Title_ColorBoost = new LocalizedText(null, "colorBoost", null);

        public static LocalizedText OptionsWindow_Label_Enable = new LocalizedText(null, "enable", null);
        public static LocalizedText OptionsWindow_Label_Method = new LocalizedText(null, "method", null);
        public static LocalizedText OptionsWindow_Label_Mode = new LocalizedText(null, "mode", null);
        public static LocalizedText OptionsWindow_Label_Quality = new LocalizedText(null, "quality", null);
        public static LocalizedText OptionsWindow_Label_SmaaQuality = new LocalizedText(null, "smaaQuality", null);
        public static LocalizedText OptionsWindow_Label_FxaaFastMode = new LocalizedText(null, "fxaaFastMode", null);
        public static LocalizedText OptionsWindow_Label_PostProcess = new LocalizedText(null, "postProcess", null);
        public static LocalizedText OptionsWindow_Label_RetroModeAspectCorrection = new LocalizedText(null, "retroModeAspectCorrection", null);
        public static LocalizedText OptionsWindow_Label_FocusDistance = new LocalizedText(null, "focusDistance", null);
        public static LocalizedText OptionsWindow_Label_Aperture = new LocalizedText(null, "aperture", null);
        public static LocalizedText OptionsWindow_Label_FocalLength = new LocalizedText(null, "focalLength", null);
        public static LocalizedText OptionsWindow_Label_MaxBlurSize = new LocalizedText(null, "maxBlurSize", null);
        public static LocalizedText OptionsWindow_Label_Intensity = new LocalizedText(null, "intensity", null);
        public static LocalizedText OptionsWindow_Label_Threshold = new LocalizedText(null, "threshold", null);
        public static LocalizedText OptionsWindow_Label_Diffusion = new LocalizedText(null, "diffusion", null);
        public static LocalizedText OptionsWindow_Label_BloomFastMode = new LocalizedText(null, "bloomFastMode", null);
        public static LocalizedText OptionsWindow_Label_Radius = new LocalizedText(null, "radius", null);
        public static LocalizedText OptionsWindow_Label_Thickness = new LocalizedText(null, "thickness", null);
        public static LocalizedText OptionsWindow_Label_ShutterAngle = new LocalizedText(null, "shutterAngle", null);
        public static LocalizedText OptionsWindow_Label_SampleCount = new LocalizedText(null, "sampleCount", null);
        public static LocalizedText OptionsWindow_Label_Smoothness = new LocalizedText(null, "smoothness", null);
        public static LocalizedText OptionsWindow_Label_Rounded = new LocalizedText(null, "rounded", null);
        public static LocalizedText OptionsWindow_Label_Roundness = new LocalizedText(null, "roundness", null);
        public static LocalizedText OptionsWindow_Label_InteriorScale = new LocalizedText(null, "interiorScale", null);
        public static LocalizedText OptionsWindow_Label_ExteriorScale = new LocalizedText(null, "exteriorScale", null);
        public static LocalizedText OptionsWindow_Label_DungeonScale = new LocalizedText(null, "dungeonScale", null);
        public static LocalizedText OptionsWindow_Label_DungeonFalloff = new LocalizedText(null, "dungeonFalloff", null);
        public static LocalizedText OptionsWindow_Label_Dither = new LocalizedText(null, "dither", null);
        public static LocalizedText OptionsWindow_Label_WeaponSwingMode = new LocalizedText(null, "weaponSwingMode", "MainMenu");
        public static LocalizedText OptionsWindow_Label_TaaSharpness = new LocalizedText(null, "taaSharpness", null);
        public static LocalizedText OptionsWindow_Label_Off = new LocalizedText(null, "off", null);
        public static LocalizedText OptionsWindow_Label_Lowest = new LocalizedText(null, "lowest", null);
        public static LocalizedText OptionsWindow_Label_Low = new LocalizedText(null, "low", null);
        public static LocalizedText OptionsWindow_Label_Medium = new LocalizedText(null, "medium", null);
        public static LocalizedText OptionsWindow_Label_High = new LocalizedText(null, "high", null);
        public static LocalizedText OptionsWindow_Label_Ultra = new LocalizedText(null, "ultra", null);
        public static LocalizedText OptionsWindow_Label_Small = new LocalizedText(null, "small", null);
        public static LocalizedText OptionsWindow_Label_Large = new LocalizedText(null, "large", null);
        public static LocalizedText OptionsWindow_Label_VeryLarge = new LocalizedText(null, "veryLarge", null);
        public static LocalizedText OptionsWindow_Label_AO_ScalableAmbient = new LocalizedText(null, "scalableAmbient", null);
        public static LocalizedText OptionsWindow_Label_AO_MultiScaleVolumentric = new LocalizedText(null, "multiScaleVolumetric", null);
        public static LocalizedText OptionsWindow_Label_Posterization_Full = new LocalizedText(null, "posterizationFull", null);
        public static LocalizedText OptionsWindow_Label_Posterization_MinusSky = new LocalizedText(null, "posterizationMinusSky", null);
        public static LocalizedText OptionsWindow_Label_Palettization_Full = new LocalizedText(null, "palettizationFull", null);
        public static LocalizedText OptionsWindow_Label_Palettization_MinusSky = new LocalizedText(null, "palettizationMinusSky", null);
        public static LocalizedText OptionsWindow_Label_AspectCorrection_FourThree = new LocalizedText(null, "FourThree", null);
        public static LocalizedText OptionsWindow_Label_AspectCorrection_SixteenTen = new LocalizedText(null, "SixteenTen", null);

        public static LocalizedTextArray OptionsWindow_TextArray_WeaponSwingModes = new LocalizedTextArray(null, "weaponSwingModes", TextCollections.TextSettings);
        public static LocalizedTextArray OptionsWindow_TextArray_ShadowResolutionModes = new LocalizedTextArray(null, "shadowResolutionModes", TextCollections.TextSettings);
        public static LocalizedTextArray OptionsWindow_TextArray_FilterModes = new LocalizedTextArray(null, "filterModes", TextCollections.TextSettings);
        public static LocalizedTextArray OptionsWindow_TextArray_DungeonTextureModes = new LocalizedTextArray(null, "dungeonTextureModes", TextCollections.TextSettings);


        public static LocalizedText InventoryWindow_Label_Armor = new LocalizedText("Armor:", null, null);
        public static LocalizedText InventoryWindow_Label_CombatSkills = new LocalizedText("Combat Skills", null, null);
        public static LocalizedText InventoryWindow_Label_Condition = new LocalizedText("Condition:", null, null);
        public static LocalizedText InventoryWindow_Label_Damage = new LocalizedText("Damage:", null, null);
        public static LocalizedText InventoryWindow_Label_Inventory = new LocalizedText("Inventory", null, null);
        public static LocalizedText InventoryWindow_Label_LanguageSkills = new LocalizedText("Language Skills", null, null);
        public static LocalizedText InventoryWindow_Label_MagicSkills = new LocalizedText("Magic Skills", null, null);
        public static LocalizedText InventoryWindow_Label_MovementSkills = new LocalizedText("Movement Skills", null, null);
        public static LocalizedText InventoryWindow_Label_PrimaryStats = new LocalizedText("Primary Stats", null, null);
        public static LocalizedText InventoryWindow_Label_SocialSkills = new LocalizedText("Social Skills", null, null);
        public static LocalizedText InventoryWindow_Label_SplitStack = new LocalizedText("Split Stack", null, null);
        public static LocalizedText InventoryWindow_Label_StealthSkills = new LocalizedText("Stealth Skills", null, null);
        public static LocalizedText InventoryWindow_Label_Value = new LocalizedText("Value:", null, null);
        public static LocalizedText InventoryWindow_Label_Weight = new LocalizedText("Weight:", null, null);


        public static LocalizedText Abbreviation_Grams = new LocalizedText("g", null, null);
        public static LocalizedText Abbreviation_Kilograms = new LocalizedText("Kg", null, null);


        private static Dictionary<UIWindowType, LocalizedText> _windowTypeToNameDict = new Dictionary<UIWindowType, LocalizedText>
        {
            { UIWindowType.Banking, new LocalizedText("Banking", null, null) },
            { UIWindowType.BankPurchasePopup, new LocalizedText("Bank Purchase", null, null) },
            { UIWindowType.BookReader, new LocalizedText("Book Reader", null, null) },
            { UIWindowType.Court, new LocalizedText("Court", null, null) },
            { UIWindowType.DaedraSummoned, new LocalizedText("Daedra Summoned", null, null) },
            { UIWindowType.GuildServicePopup, new LocalizedText("Guild Service", null, null) },
            { UIWindowType.GuildServiceCureDisease, new LocalizedText("Guild Service: Cure Disease", null, null) },
            { UIWindowType.GuildServiceDonation, new LocalizedText("Guild Service: Donation", null, null) },
            { UIWindowType.GuildServiceTraining, new LocalizedText("Guild Service: Training", null, null) },
            { UIWindowType.Inventory, new LocalizedText("Inventory", null, null) },
            { UIWindowType.ItemMaker, new LocalizedText("Item Maker", null, null) },
            { UIWindowType.MerchantRepairPopup, new LocalizedText("Merchant Repair", null, null) },
            { UIWindowType.MerchantServicePopup, new LocalizedText("Merchant Service", null, null) },
            { UIWindowType.PauseOptions, new LocalizedText("Pause", null, null) },
            { UIWindowType.PlayerHistory, new LocalizedText("Player History", null, null) },
            { UIWindowType.PotionMaker, new LocalizedText("Potion Maker", null, null) },
            { UIWindowType.QuestJournal, new LocalizedText("Quest Journal", null, null) },
            { UIWindowType.QuestOffer, new LocalizedText("Quest Offer", null, null) },
            { UIWindowType.Rest, new LocalizedText("Rest", null, null) },
            { UIWindowType.SpellBook, new LocalizedText("Spell Book", null, null) },
            { UIWindowType.SpellMaker, new LocalizedText("Spell Maker", null, null) },
            { UIWindowType.Talk, new LocalizedText("Conversation", null, null) },
            { UIWindowType.Tavern, new LocalizedText("Tavern", null, null) },
            { UIWindowType.TeleportPopUp, new LocalizedText("Teleport", null, null) },
            { UIWindowType.Trade, new LocalizedText("Trade", null, null) },
            { UIWindowType.UnitySaveGame, new LocalizedText("Save/Load Game", null, null) },
            { UIWindowType.UseMagicItem, new LocalizedText("Use Magic Item", null, null) },
            { UIWindowType.WitchesCovenPopup, new LocalizedText("Witches Coven", null, null) },
        };


        public static string GetWindowTypeText(UIWindowType windowType)
        {
            _windowTypeToNameDict.TryGetValue(windowType, out LocalizedText textContainer);
            return textContainer.GetText();
        }
    }
}
