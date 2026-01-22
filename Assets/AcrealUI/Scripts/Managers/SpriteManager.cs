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

using System.Collections.Generic;
using UnityEngine;

using DaggerfallWorkshop;

namespace AcrealUI
{
    public static class SpriteManager
    {
        private static Dictionary<string, Sprite> spriteKeyToSpriteDict = null;


        public static void Initialize()
        {
            if(spriteKeyToSpriteDict == null)
            {
                spriteKeyToSpriteDict = new Dictionary<string, Sprite>();
            }
        }

        public static void Shutdown()
        {
            if(spriteKeyToSpriteDict != null)
            {
                foreach(KeyValuePair<string, Sprite> kvp in spriteKeyToSpriteDict)
                {
                    if(kvp.Value != null)
                    {
                        Object.Destroy(kvp.Value);
                    }
                }
            }
            spriteKeyToSpriteDict = null;
        }

        public static string CreateSpriteKeyFromImageData(ImageData imageData)
        {
            string spriteKey = string.Empty;
            if(!string.IsNullOrEmpty(imageData.filename))
            {
                spriteKey = imageData.filename + "." + imageData.record.ToString();
                //Debug.Log("Created Sprite Key: " + spriteKey);
            }
            return spriteKey;
        }

        public static bool SpriteExists(string spriteKey)
        {
            return spriteKeyToSpriteDict != null && spriteKeyToSpriteDict.ContainsKey(spriteKey);
        }

        public static Sprite GetSprite(string spriteKey)
        {
            if(spriteKeyToSpriteDict == null) { return null; }

            Sprite sprite = null;
            if (spriteKeyToSpriteDict != null)
            {
                //Debug.Log("Retrieved Sprite: " + (sprite != null ? sprite.name : "NULL") + "  with Key: " + spriteKey);
                spriteKeyToSpriteDict.TryGetValue(spriteKey, out sprite);
            }
            return sprite;
        }

        public static Sprite GetOrCreateSprite(ImageData imageData)
        {
            if(spriteKeyToSpriteDict == null) { return null; }

            Sprite sprite = null;
            string spriteKey = CreateSpriteKeyFromImageData(imageData);
            if (!string.IsNullOrEmpty(spriteKey))
            {
                sprite = GetSprite(spriteKey);
                if (sprite == null)
                {
                    sprite = CreateSprite(imageData);
                    spriteKeyToSpriteDict[spriteKey] = sprite;
                }
            }
            return sprite;
        }

        public static Sprite CreateSprite(ImageData imageData)
        {
            //Debug.Log("Create Sprite From Texture: " + (!string.IsNullOrEmpty(imageData.filename) ? imageData.filename : "NULL"));
            return Sprite.Create(imageData.texture, new Rect(0, 0, imageData.texture.width, imageData.texture.height), new Vector2(0.5f, 0.5f), 100);
        }
    }
}