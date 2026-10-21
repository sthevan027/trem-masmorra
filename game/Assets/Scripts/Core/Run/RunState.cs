using System;

namespace TremMasmorra.Core.Run
{
    public class RunState
    {
        public const int TotalWagons = 9;
        public const int BossEvery = 3;

        public int Wagon { get; set; } = 1;
        public int MonstersAlive { get; private set; }
        public bool DoorOpen { get; private set; }
        public bool Victory { get; private set; }
        public bool Dead { get; private set; }

        public bool IsBossWagon => Wagon % BossEvery == 0;

        public void StartWagon(int monsters)
        {
            MonstersAlive = monsters;
            DoorOpen = monsters <= 0;
        }

        public void OnMonsterKilled()
        {
            if (MonstersAlive > 0) MonstersAlive--;
            if (MonstersAlive == 0) DoorOpen = true;
        }

        public void OnPlayerDied() => Dead = true;

        public void NextWagon()
        {
            if (!DoorOpen) throw new InvalidOperationException("A porta do vagão ainda esta fechada.");
            if (Wagon >= TotalWagons) { Victory = true; return; }
            Wagon++;
            DoorOpen = false;
            MonstersAlive = 0;
        }

        public void Restart()
        {
            Wagon = 1;
            MonstersAlive = 0;
            DoorOpen = false;
            Victory = false;
            Dead = false;
        }
    }
}
