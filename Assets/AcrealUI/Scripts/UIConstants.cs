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

namespace AcrealUI
{
    public static class UIConstants
    {
        public const string INDENT = "    ";

        public const int LEGACY_MAGIC_TOKEN_ID = 1016;

        public const int MIN_HIT_CHANCE = 3;
        public const int MAX_HIT_CHANCE = 97;
        public const int BACKSTAB_DAMAGE_MODIFIER = 3;
        public const int ENEMY_STAT_HIT_CHANCE_DIVISOR = 10;
        public const int ENEMY_ARMOR_MULTIPLIER = 5;

        public const int MAX_SKILL_LEVEL = 100;
        public const int ENEMY_BASE_SKILL_LEVEL = 30;
        public const int ENEMY_SKILL_POINTS_PER_LEVEL = 5;
        public const int ENEMY_SKILL_HIT_CHANCE_DIVISOR = 4;

        public const short DEFAULT_DUNGEON_WATER_LEVEL = 10000;

        public const int MAX_RECENT_DIALOGUE_ENTRIES = 4;
        public const int MAX_OLD_DIALOGUE_ENTRIES = 64 - MAX_RECENT_DIALOGUE_ENTRIES;

        public const float TOOLTIP_OFFSET_ITEM = 20f;
    }
}
