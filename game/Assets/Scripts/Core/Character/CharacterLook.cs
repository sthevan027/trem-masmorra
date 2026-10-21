using System;

namespace TremMasmorra.Core.Character
{
    public enum Gender { Masculino, Feminino }

    /// <summary>Aparencia do personagem. Nao altera nenhum atributo de combate.</summary>
    public class CharacterLook
    {
        public const float MinHeight = 0.85f;
        public const float MaxHeight = 1.2f;
        public const int HairStyles = 3;
        public const int SkinTones = 4;

        float height = 1f;
        int hairStyle;
        int skinTone;

        public Gender Gender { get; set; }

        public float Height
        {
            get => height;
            set => height = Math.Max(MinHeight, Math.Min(MaxHeight, value));
        }

        public int HairStyle
        {
            get => hairStyle;
            set => hairStyle = ((value % HairStyles) + HairStyles) % HairStyles;
        }

        public int SkinTone
        {
            get => skinTone;
            set => skinTone = ((value % SkinTones) + SkinTones) % SkinTones;
        }
    }
}
