using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks Level 1 layout slots so hazards, pickups, and water never share the same lane + progress.
/// </summary>
public static class Level1LayoutPlacement
{
    struct Slot
    {
        public int Lane;
        public float Progress;
    }

    static readonly List<Slot> Occupied = new List<Slot>(160);

    public static void Reset()
    {
        Occupied.Clear();
    }

    public static bool IsFree(int lane, float progress)
    {
        lane = Mathf.Clamp(lane, 0, LevelLanes.Count - 1);
        float gap = Level1Config.MinSameLaneProgressGap;

        for (int i = 0; i < Occupied.Count; i++)
        {
            if (Occupied[i].Lane == lane && Mathf.Abs(Occupied[i].Progress - progress) < gap)
            {
                return false;
            }
        }

        return true;
    }

    public static bool AreLanesFree(int[] lanes, float progress)
    {
        if (lanes == null || lanes.Length == 0) return true;

        for (int i = 0; i < lanes.Length; i++)
        {
            if (!IsFree(lanes[i], progress))
            {
                return false;
            }
        }

        return true;
    }

    public static void Reserve(int lane, float progress)
    {
        lane = Mathf.Clamp(lane, 0, LevelLanes.Count - 1);
        progress = Mathf.Clamp01(progress);
        Occupied.Add(new Slot { Lane = lane, Progress = progress });
    }

    public static void ReserveLanes(int[] lanes, float progress)
    {
        if (lanes == null) return;

        progress = Mathf.Clamp01(progress);
        for (int i = 0; i < lanes.Length; i++)
        {
            Reserve(lanes[i], progress);
        }
    }

    public static bool TryReserve(int lane, float progress, out float usedProgress, out int usedLane)
    {
        usedLane = Mathf.Clamp(lane, 0, LevelLanes.Count - 1);
        usedProgress = Mathf.Clamp01(progress);

        if (TryReserveAt(usedLane, usedProgress))
        {
            return true;
        }

        const float step = 0.005f;
        for (int n = 1; n <= 10; n++)
        {
            float forward = Mathf.Clamp01(progress + n * step);
            if (TryReserveAt(usedLane, forward))
            {
                usedProgress = forward;
                return true;
            }

            float backward = Mathf.Clamp01(progress - n * step);
            if (TryReserveAt(usedLane, backward))
            {
                usedProgress = backward;
                return true;
            }
        }

        for (int l = 0; l < LevelLanes.Count; l++)
        {
            if (l == usedLane) continue;
            if (TryReserveAt(l, progress))
            {
                usedLane = l;
                usedProgress = progress;
                return true;
            }
        }

        Debug.LogWarning($"[Level1] Skipped spawn — lane {lane} @ {progress:P1} overlaps an existing object.");
        return false;
    }

    public static bool TryReserveLanes(int[] lanes, float progress, out float usedProgress)
    {
        usedProgress = Mathf.Clamp01(progress);
        if (lanes == null || lanes.Length == 0) return true;

        const float step = 0.005f;
        for (int n = 0; n <= 10; n++)
        {
            float candidate = Mathf.Clamp01(progress + n * step);
            if (AreLanesFree(lanes, candidate))
            {
                ReserveLanes(lanes, candidate);
                usedProgress = candidate;
                return true;
            }
        }

        Debug.LogWarning($"[Level1] Skipped multi-lane spawn @ {progress:P1} — lanes overlap existing objects.");
        return false;
    }

    public static int[] GetRollingLogLanes(int lane, int laneSpan)
    {
        int span = Mathf.Clamp(laneSpan, 2, 3);
        int leftLane = Mathf.Clamp(lane, 0, LevelLanes.Count - span);
        int[] lanes = new int[span];
        for (int i = 0; i < span; i++)
        {
            lanes[i] = leftLane + i;
        }

        return lanes;
    }

    public static float ProgressFromWorldZ(float worldZ)
    {
        return Level1Progress.Normalized(worldZ);
    }

    static bool TryReserveAt(int lane, float progress)
    {
        if (!IsFree(lane, progress)) return false;
        Reserve(lane, progress);
        return true;
    }
}
