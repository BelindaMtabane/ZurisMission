using UnityEngine;

/// <summary>
/// Stores Story 5 and Story 6 content and their separate Level 3 progression.
/// </summary>
public static class StoryNarrativeData3
{
    const string Story5UnlockedKey = "DropByDrop.Narrative.Story5Unlocked";
    const string Story5CompletedKey = "DropByDrop.Narrative.Story5Completed";
    const string Story5ReadKey = "DropByDrop.Narrative.Story5Read";
    const string Story6UnlockedKey = "DropByDrop.Narrative.Story6Unlocked";
    const string Story6CompletedKey = "DropByDrop.Narrative.Story6Completed";
    const string Story6ReadKey = "DropByDrop.Narrative.Story6Read";
    const string Level3UnlockedKey = "DropByDrop.Narrative.Level3Unlocked";
    const string Story5BestScoreKey = "DropByDrop.Narrative.Story5BestQuizScore";
    const string Story6BestScoreKey = "DropByDrop.Narrative.Story6BestQuizScore";

    public const string Story5Title = "THE FOREST'S SECRET";
    public const string Story5MissionTitle = "PROTECT THE FOREST!";
    public const string Story5CompleteTitle = "STORY 5 COMPLETED!";
    public const string Story6Title = "THE POLLUTION PUZZLE";
    public const string Story6MissionTitle = "SOLVE THE POLLUTION PUZZLE!";
    public const string Story6CompleteTitle = "STORY 6 COMPLETED!";

    public static readonly StoryDialogueLine[] Story5Opening =
    {
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "Zuri travels into a lush, green forest with tall trees, flowing streams and plenty of visible water."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Wow! Look at this forest! It's so green and beautiful! There is water everywhere!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Welcome to the forest, Zuri! It's quite different from the village, isn't it?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "It really is! Look at that stream! And all those trees! Does this mean everyone here has enough water?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Not necessarily. Seeing water around us doesn't always mean people have safe drinking water."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Really? But the stream looks so clean!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Water in rivers and streams can contain germs, dirt or other harmful things that we cannot always see."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So even clear-looking water might not be safe to drink?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Exactly! Natural water sources need protection, and water must be treated and tested before people drink it."),
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "Zuri and Kaelo notice how trees, plants, soil and water are connected."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Kaelo, why are there so many trees and plants growing around here?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Healthy forests help protect the environment. Trees and plants help the soil absorb and hold water."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So the forest helps look after water too?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Yes! Trees, soil, rivers and streams are all part of an ecosystem. They work together to support plants, animals and people."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "What happens if people cut down too many trees or throw rubbish into the streams?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "It can damage the environment, pollute water sources and harm the living things that depend on them."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Oh no! Then we need to protect the forest and keep the water clean!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's right! Protecting nature also helps protect our water sources."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Look over there! Someone has left rubbish beside the stream!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's dangerous for the environment. Rain can wash rubbish and pollution into rivers and streams."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And that could make the water unsafe for animals and people!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Exactly! We should never throw rubbish into or near natural water sources."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We can help by keeping the forest clean, protecting the trees and looking after the streams!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Wonderful! But remember, even when we protect a water source, we must still make sure the water is safe before drinking it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I understand now! Having water nearby is not the same as having safe drinking water."),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "You've discovered the forest's secret, Zuri! Healthy ecosystems help protect the water that life depends on."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then let's become Forest Water Protectors!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "PROTECT THE FOREST!")
    };

    public static readonly StoryDialogueLine[] Story5Ending =
    {
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Kaelo, I never realised how much the forest does to help protect our water!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "It does a lot! Healthy trees, plants and soil help support the natural water systems around us."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And if we pollute rivers and streams, we can hurt the animals, plants and people who depend on them."),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's right! Protecting nature is one way we help protect our water."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "But we must also remember that water from rivers and streams isn't automatically safe to drink."),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Exactly! It needs proper treatment and testing before people can safely drink it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I understand now! We need to protect the environment and make sure people have access to safe water."),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "You've learned an important lesson, Zuri. Water and nature need one another."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then I'll do my part to protect every stream, every tree and every drop!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's the spirit! The forest is lucky to have a Water Detective like you!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Haha! Come on, Kaelo! Our adventure isn't over yet!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "Healthy ecosystems help protect our natural water sources. Keep nature clean, protect water and remember that clear-looking water is not always safe to drink.")
    };

    const string Story5CorrectFeedback =
        "Excellent work! You're becoming a Forest Water Protector!";
    const string Story5IncorrectFeedback =
        "Not quite! Look closely at the clues and try again, Water Detective! 😂";

    public static readonly StoryQuizQuestion[] Story5Questions =
    {
        new StoryQuizQuestion(
            "Why should we protect forests and natural water sources?",
            "Because healthy ecosystems support water, plants, animals and people.",
            "Because forests do not need water.",
            "Because rubbish helps trees grow.",
            0, Story5CorrectFeedback, Story5IncorrectFeedback),
        new StoryQuizQuestion(
            "Is clear-looking river or stream water always safe to drink?",
            "Yes, because clear water never contains germs.",
            "No, natural water may contain harmful germs or pollution and must be treated and tested.",
            "Yes, as long as it comes from a forest.",
            1, Story5CorrectFeedback, Story5IncorrectFeedback),
        new StoryQuizQuestion(
            "How can we help protect natural water sources?",
            "Throw rubbish into streams.",
            "Cut down all the trees near rivers.",
            "Keep the environment clean and prevent pollution.",
            2, Story5CorrectFeedback, Story5IncorrectFeedback)
    };

    public static readonly StoryDialogueLine[] Story6Opening =
    {
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "Zuri and Kaelo arrive at a water source with rubbish and unusual substances nearby."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Kaelo, wait! Something doesn't look right over here!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "What is it, Zuri?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Look at the stream! There is rubbish lying beside it, and the water looks different from before."),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Oh no! It looks like this water source might have been polluted."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Polluted? Does that mean something has made the water dirty?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's right! Water pollution happens when harmful things enter water and make it unsafe for people, animals and plants."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "But how did all this rubbish get here?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Sometimes people throw rubbish near rivers, spill harmful substances or do things that allow pollution to reach water sources."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "That's terrible! We need to find out what's happening before more damage is done!"),
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "They investigate and notice more signs of pollution."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I can see plastic bottles, food wrappers and other rubbish scattered everywhere!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Yes. When it rains, rubbish and harmful substances can be washed into streams and rivers."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So even rubbish left on the ground can end up in the water?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Exactly! Some pollution is easy to see, but other harmful substances may be invisible."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "That means water can look clean but still be unsafe!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "You've got it! Pollution can affect water quality even when we cannot see it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "What happens to the animals that drink from polluted water?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "They can become sick, and pollution can also harm plants and the people who depend on that water."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Now I understand why we must protect every water source!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "We can help prevent pollution by disposing of rubbish properly and keeping harmful substances away from rivers and streams."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We should also make sure rubbish doesn't block drains and flow into our water sources!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Exactly! Communities must work together to prevent pollution and protect water quality."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And if a water source becomes polluted, we shouldn't assume it's safe just because it looks clear."),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Correct! The water needs to be assessed and treated appropriately before it can be used for drinking."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I see the bigger picture now! Protecting water means stopping pollution before it spreads."),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's the secret to solving the Pollution Puzzle!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then let's investigate, clean up what we safely can and help protect our water!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Yes. We can help with visible rubbish, but unknown chemicals and polluted water must be left for responsible adults."),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "SOLVE THE POLLUTION PUZZLE!")
    };

    public static readonly StoryDialogueLine[] Story6Ending =
    {
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Kaelo, I think we've finally solved the Pollution Puzzle!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "We have, Zuri! Now we understand how rubbish and harmful substances can contaminate our water."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I never realised that something left on the ground could eventually reach a river or stream!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's why preventing pollution is so important. Once water becomes contaminated, it can be difficult to restore."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We must keep rubbish out of nature and make sure harmful substances don't enter our water sources."),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Exactly! Everyone has a responsibility to protect the water we share."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And if we notice pollution, we should report it so the right people can help!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Wonderful thinking! Clean and safe water is important for people, animals and the environment."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I'm glad we investigated this mystery. We have to protect our water before more pollution happens!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's the spirit, Zuri! You're becoming an excellent Water Detective!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Another mystery solved! Let's keep our forests and water sources clean!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "Water pollution happens when harmful substances contaminate water. Preventing pollution helps protect people, animals, plants and the environment.")
    };

    const string Story6CorrectFeedback =
        "Brilliant detective work! You've uncovered the pollution clues!";
    const string Story6IncorrectFeedback =
        "Not quite! Look carefully at the clues and try again! 😂";

    public static readonly StoryQuizQuestion[] Story6Questions =
    {
        new StoryQuizQuestion(
            "What is water pollution?",
            "When harmful substances or waste contaminate water.",
            "When water flows through a river.",
            "When rain falls into a forest.",
            0, Story6CorrectFeedback, Story6IncorrectFeedback),
        new StoryQuizQuestion(
            "How can rubbish on the ground reach rivers and streams?",
            "It disappears before it reaches the water.",
            "Rain can wash rubbish and harmful substances into water sources.",
            "Trees turn rubbish into clean water.",
            1, Story6CorrectFeedback, Story6IncorrectFeedback),
        new StoryQuizQuestion(
            "What is the best way to help prevent water pollution?",
            "Throw rubbish near rivers.",
            "Pour harmful substances into drains.",
            "Dispose of waste properly and keep harmful substances away from water sources.",
            2, Story6CorrectFeedback, Story6IncorrectFeedback)
    };

    public static bool Story5Completed => PlayerPrefs.GetInt(Story5CompletedKey, 0) == 1;
    public static bool Story6Unlocked => PlayerPrefs.GetInt(Story6UnlockedKey, 0) == 1;
    public static bool Story6Completed => PlayerPrefs.GetInt(Story6CompletedKey, 0) == 1;
    public static bool HasReadStory5 => PlayerPrefs.GetInt(Story5ReadKey, 0) == 1 || Story5Completed;
    public static bool HasReadStory6 => PlayerPrefs.GetInt(Story6ReadKey, 0) == 1 || Story6Completed;

    public static void MarkStory5Read()
    {
        PlayerPrefs.SetInt(Story5ReadKey, 1);
        PlayerPrefs.Save();
    }

    public static void MarkStory6Read()
    {
        PlayerPrefs.SetInt(Story6ReadKey, 1);
        PlayerPrefs.Save();
    }
    public static bool Level3Unlocked => PlayerPrefs.GetInt(Level3UnlockedKey, 0) == 1;
    public static int Story5BestScore => PlayerPrefs.GetInt(Story5BestScoreKey, 0);
    public static int Story6BestScore => PlayerPrefs.GetInt(Story6BestScoreKey, 0);

    public static bool HasStory5Content =>
        Story5Opening.Length > 0 && Story5Ending.Length > 0 &&
        Story5Questions.Length == 3;

    public static bool HasStory6Content =>
        Story6Opening.Length > 0 && Story6Ending.Length > 0 &&
        Story6Questions.Length == 3;

    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(Story5UnlockedKey);
        PlayerPrefs.DeleteKey(Story5CompletedKey);
        PlayerPrefs.DeleteKey(Story5ReadKey);
        PlayerPrefs.DeleteKey(Story6UnlockedKey);
        PlayerPrefs.DeleteKey(Story6CompletedKey);
        PlayerPrefs.DeleteKey(Story6ReadKey);
        PlayerPrefs.DeleteKey(Level3UnlockedKey);
        PlayerPrefs.DeleteKey(Story5BestScoreKey);
        PlayerPrefs.DeleteKey(Story6BestScoreKey);
        PlayerPrefs.Save();
        EnsureInitialized();
    }

    public static void EnsureInitialized()
    {
        if (!PlayerPrefs.HasKey(Story5UnlockedKey))
            PlayerPrefs.SetInt(Story5UnlockedKey, 1);
        if (!PlayerPrefs.HasKey(Story5CompletedKey))
            PlayerPrefs.SetInt(Story5CompletedKey, 0);
        if (!PlayerPrefs.HasKey(Story6UnlockedKey))
            PlayerPrefs.SetInt(Story6UnlockedKey, 0);
        if (!PlayerPrefs.HasKey(Story6CompletedKey))
            PlayerPrefs.SetInt(Story6CompletedKey, 0);
        if (!PlayerPrefs.HasKey(Level3UnlockedKey))
            PlayerPrefs.SetInt(Level3UnlockedKey,
                Story5Completed || Story6Unlocked ? 1 : 0);

        PlayerPrefs.Save();
    }

    public static void SaveStory5QuizResult(int score)
    {
        int safeScore = Mathf.Clamp(score, 0, 3);
        PlayerPrefs.SetInt(Story5BestScoreKey,
            Mathf.Max(Story5BestScore, safeScore));

        if (safeScore >= 2)
        {
            PlayerPrefs.SetInt(Story5CompletedKey, 1);
            PlayerPrefs.SetInt(Level3UnlockedKey, 1);
        }

        if (safeScore == 3)
            PlayerPrefs.SetInt(Story6UnlockedKey, 1);

        PlayerPrefs.Save();
    }

    public static void SaveStory6QuizResult(int score)
    {
        int safeScore = Mathf.Clamp(score, 0, 3);
        PlayerPrefs.SetInt(Story6BestScoreKey,
            Mathf.Max(Story6BestScore, safeScore));

        if (safeScore >= 2)
            PlayerPrefs.SetInt(Story6CompletedKey, 1);

        if (safeScore == 3)
            StoryNarrativeData4.UnlockStory7FromStory6();

        PlayerPrefs.Save();
    }

    public static StoryQuizQuestion GetStory6Question(
        int questionIndex, bool question1Correct, bool question2Correct)
    {
        int safeIndex = Mathf.Clamp(questionIndex, 0, 2);
        return Story6Questions[safeIndex];
    }
}
