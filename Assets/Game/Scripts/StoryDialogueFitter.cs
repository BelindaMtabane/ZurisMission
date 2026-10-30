using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;

/// <summary>
/// Splits dialogue lines that would render taller than the tallest Story 1/2 line into
/// consecutive pages for the same speaker, so long script lines stay inside the
/// Narrative Scene 1 dialogue boxes without resizing or restyling them.
/// </summary>
public static class StoryDialogueFitter
{
    static readonly Dictionary<StoryDialogueLine[], StoryDialogueLine[]> Cache = new();
    static float maxHeight = -1f;

    public static StoryDialogueLine[] Fit(StoryDialogueLine[] lines, TMP_Text box)
    {
        if (lines == null || box == null) return lines;
        if (Cache.TryGetValue(lines, out StoryDialogueLine[] fitted)) return fitted;

        float limit = MaxHeight(box);
        var result = new List<StoryDialogueLine>(lines.Length);
        foreach (StoryDialogueLine line in lines)
        {
            if (line.Speaker == StorySpeaker.TitleCard || Height(box, line.Speaker, line.Text) <= limit)
            {
                result.Add(line);
                continue;
            }

            foreach (string page in Paginate(box, line.Speaker, line.Text, limit))
                result.Add(new StoryDialogueLine(line.Speaker, page));
        }

        fitted = result.ToArray();
        Cache[lines] = fitted;
        return fitted;
    }

    static float MaxHeight(TMP_Text box)
    {
        if (maxHeight > 0f) return maxHeight;

        StoryDialogueLine[][] reference =
        {
            StoryNarrativeData.Story1Opening, StoryNarrativeData.Story1Ending,
            StoryNarrativeData.Story2Opening, StoryNarrativeData.Story2Ending
        };
        foreach (StoryDialogueLine[] set in reference)
            foreach (StoryDialogueLine line in set)
                if (line.Speaker != StorySpeaker.TitleCard)
                    maxHeight = UnityEngine.Mathf.Max(maxHeight, Height(box, line.Speaker, line.Text));

        return maxHeight;
    }

    static List<string> Paginate(TMP_Text box, StorySpeaker speaker, string text, float limit)
    {
        var pages = new List<string>();
        string current = "";
        foreach (string sentence in Regex.Split(text.Trim(), @"(?<=[.!?])\s+"))
        {
            string candidate = current.Length == 0 ? sentence : current + " " + sentence;
            if (Height(box, speaker, candidate) <= limit)
            {
                current = candidate;
                continue;
            }

            if (current.Length > 0)
            {
                pages.Add(current);
                current = "";
            }

            if (Height(box, speaker, sentence) <= limit)
            {
                current = sentence;
                continue;
            }

            List<string> pieces = SplitBalanced(box, speaker, sentence, limit);
            for (int i = 0; i < pieces.Count - 1; i++) pages.Add(pieces[i]);
            current = pieces[pieces.Count - 1];
        }

        if (current.Length > 0) pages.Add(current);
        return pages;
    }

    /// <summary>Splits one over-long sentence into the fewest even pieces, preferring breaks after commas.</summary>
    static List<string> SplitBalanced(TMP_Text box, StorySpeaker speaker, string sentence, float limit)
    {
        string[] words = sentence.Split(' ');
        for (int count = 2; count <= words.Length; count++)
        {
            var pieces = new List<string>(count);
            int start = 0;
            bool fits = true;
            for (int p = 1; p <= count && fits; p++)
            {
                int end = p == count ? words.Length : NearestCommaBreak(words, words.Length * p / count, start + 1);
                string piece = string.Join(" ", words, start, end - start);
                fits = Height(box, speaker, piece) <= limit;
                pieces.Add(piece);
                start = end;
            }

            if (fits) return pieces;
        }

        return new List<string> { sentence };
    }

    static int NearestCommaBreak(string[] words, int target, int min)
    {
        for (int offset = 0; offset <= 3; offset++)
        {
            foreach (int i in new[] { target - offset, target + offset })
                if (i >= min && i < words.Length && words[i - 1].EndsWith(",")) return i;
        }

        return UnityEngine.Mathf.Clamp(target, min, words.Length - 1);
    }

    static float Height(TMP_Text box, StorySpeaker speaker, string text)
    {
        return box.GetPreferredValues(Displayed(speaker, text), box.rectTransform.rect.width, 0f).y;
    }

    static string Displayed(StorySpeaker speaker, string text)
    {
        switch (speaker)
        {
            case StorySpeaker.Zuri: return $"Zuri:\n{text}";
            case StorySpeaker.Naledi: return $"Naledi:\n{text}";
            case StorySpeaker.Kaelo: return $"Kaelo:\n{text}";
            case StorySpeaker.Villager1: return $"Villager 1:\n{text}";
            case StorySpeaker.Villager2: return $"Villager 2:\n{text}";
            case StorySpeaker.Everyone: return $"Everyone:\n{text}";
            case StorySpeaker.StageDirection: return $"<i>{text}</i>";
            default: return text;
        }
    }
}
