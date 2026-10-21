using UnityEngine;

namespace TremMasmorra.Game
{
    /// <summary>Unico componente da cena Main: cria o GameManager, que monta todo o resto por codigo.</summary>
    public class GameBootstrap : MonoBehaviour
    {
        void Awake()
        {
            if (FindFirstObjectByType<GameManager>() == null)
                new GameObject("GameManager").AddComponent<GameManager>();
        }
    }
}
