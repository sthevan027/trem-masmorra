using System;
using UnityEngine;

namespace TremMasmorra.Game
{
    public class Health : MonoBehaviour
    {
        public int Max = 100;
        public int Current = 100;
        public bool Invulnerable;

        public event Action<int> Damaged;
        public event Action Died;

        public bool IsDead => Current <= 0;
        public float Fraction => Max <= 0 ? 0f : Mathf.Clamp01((float)Current / Max);

        public void Init(int max)
        {
            Max = max;
            Current = max;
        }

        public void Damage(int amount)
        {
            if (IsDead || Invulnerable || amount <= 0) return;
            Current = Mathf.Max(0, Current - amount);
            Damaged?.Invoke(amount);
            if (Current == 0) Died?.Invoke();
        }
    }
}
