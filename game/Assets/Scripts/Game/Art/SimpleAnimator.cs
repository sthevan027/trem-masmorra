using UnityEngine;

namespace TremMasmorra.Game
{
    /// <summary>Animacao basica por codigo: andar, atacar, tomar dano, morrer.</summary>
    public class SimpleAnimator : MonoBehaviour
    {
        public Transform ArmL, ArmR, LegL, LegR;
        public Renderer[] Renderers;
        public float MoveSpeed;
        public bool Ghost;

        float attackT = -1f;
        float hurtT;
        float deathT = -1f;
        float walkPhase;
        MaterialPropertyBlock block;

        public bool IsDead => deathT >= 0f;

        public void PlayAttack() => attackT = 0f;
        public void PlayHurt() => hurtT = 0.15f;
        public void PlayDeath() => deathT = 0f;

        void Update()
        {
            float dt = Time.deltaTime;
            if (IsDead)
            {
                deathT += dt;
                float k = Mathf.Clamp01(deathT / 0.4f);
                transform.localRotation = Quaternion.Euler(-90f * k, 0f, 0f);
                transform.localPosition = new Vector3(0f, 0.25f * k, 0f);
                return;
            }

            float walk = Mathf.Clamp01(MoveSpeed / 3f);
            walkPhase += dt * 10f * walk;
            float swing = Mathf.Sin(walkPhase) * 40f * walk;
            Rot(ArmL, swing);
            Rot(LegL, -swing);
            Rot(LegR, swing);

            float armR = -swing;
            if (attackT >= 0f)
            {
                attackT += dt;
                float k = attackT / 0.25f;
                armR = Mathf.Lerp(-120f, 20f, Mathf.Clamp01(k));
                if (k >= 1f) attackT = -1f;
            }
            Rot(ArmR, armR);

            ApplyTint(dt);
        }

        void ApplyTint(float dt)
        {
            if (Renderers == null) return;
            bool flash = hurtT > 0f;
            hurtT -= dt;
            bool hidden = Ghost && Mathf.Sin(Time.time * 50f) > 0f;
            if (block == null) block = new MaterialPropertyBlock();
            foreach (var r in Renderers)
            {
                if (r == null) continue;
                r.enabled = !hidden;
                if (flash)
                {
                    block.SetColor("_Color", Color.red);
                    r.SetPropertyBlock(block);
                }
                else
                {
                    r.SetPropertyBlock(null);
                }
            }
        }

        static void Rot(Transform t, float x)
        {
            if (t != null) t.localRotation = Quaternion.Euler(x, 0f, 0f);
        }
    }
}
