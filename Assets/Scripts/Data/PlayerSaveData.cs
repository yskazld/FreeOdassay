using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData
{
    public string version = "0.0.1";
    public string jobId = "designer";
    public int level = 1;
    public float currentXp = 0f;
    public int coins = 0;
    public int skillPoints = 0;
    public int roomLevel = 1;
    public List<string> unlockedSkills = new();
    public long lastSavedTicks = DateTime.UtcNow.Ticks;
    public long lastExitTicks = DateTime.UtcNow.Ticks;
    public int bestPuzzleScore = 0;
}
