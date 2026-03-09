using DaggerfallWorkshop.Game.Utility.ModSupport;
using UnityEngine;
using UnityEngine.UI;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIDialogueEntry : UIInteractiveElement
    {
        [SerializeField] private string _gameObjName_portraitRawImage = null;

        private RawImage _portraitRawImage = null;


        public override void Initialize()
        {
            base.Initialize();

            Transform portraitTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_portraitRawImage);
            if(portraitTform != null )
            {
                _portraitRawImage = portraitTform.GetComponent<RawImage>();
            }
        }

        public void SetPortraitTexture(Texture2D portrait)
        {
            if(_portraitRawImage != null )
            {
                _portraitRawImage.texture = portrait;
            }
        }
    }
}
