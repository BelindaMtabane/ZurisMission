using UnityEngine;

public static class Level2MudMonsterDirector
{
    static Level2MudMonster activeMonster;

    public static bool TryStartMonster(Level2MudMonster monster)
    {
        if (activeMonster != null && activeMonster != monster)
        {
            return false;
        }

        activeMonster = monster;
        return true;
    }

    public static void ReleaseMonster(Level2MudMonster monster)
    {
        if (activeMonster == monster)
        {
            activeMonster = null;
        }
    }
}
