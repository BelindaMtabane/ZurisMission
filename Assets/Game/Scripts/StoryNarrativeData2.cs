using UnityEngine;

/// <summary>
/// Stores Story 3 and Story 4 content and their narrative progression.
/// </summary>
public static class StoryNarrativeData2
{
    const string Story3UnlockedKey = "DropByDrop.Narrative.Story3Unlocked";
    const string Story3CompletedKey = "DropByDrop.Narrative.Story3Completed";
    const string Story3ReadKey = "DropByDrop.Narrative.Story3Read";
    const string Story4UnlockedKey = "DropByDrop.Narrative.Story4Unlocked";
    const string Story4CompletedKey = "DropByDrop.Narrative.Story4Completed";
    const string Story4ReadKey = "DropByDrop.Narrative.Story4Read";
    const string Story3BestScoreKey = "DropByDrop.Narrative.Story3BestQuizScore";
    const string Story4BestScoreKey = "DropByDrop.Narrative.Story4BestQuizScore";

    public const string Story3Title = "A DROP GOES A LONG WAY";
    public const string Story3MissionTitle = "EVERY DROP COUNTS!";
    public const string Story3CompleteTitle = "STORY 3 COMPLETED!";
    public const string Story4Title = "THE GARDEN RESCUE";
    public const string Story4MissionTitle = "RESCUE THE GARDEN!";
    public const string Story4CompleteTitle = "STORY 4 COMPLETED!";

    public static readonly StoryDialogueLine[] Story3Opening =
    {
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "Zuri arrives where green plants are beginning to return."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "WOW! Look at all this greenery! It's so beautiful! I think I've accidentally walked into a giant salad!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Haha! Welcome to the greener side of our village, Zuri!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I love it! Look at those plants! They're finally growing again. I bet they're having the best time!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Some are, but not all of them. A few plants are still struggling because water is limited."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Wait... plants can struggle too? I thought they just stood around looking pretty!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Oh, Zuri! Plants need water to grow and stay healthy, just like people need water to survive."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So even plants get thirsty? Poor things! And here I thought I was the only one who complained when I was thirsty!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "The important thing is that we must use the water we have carefully."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "You mean we can't celebrate having water by splashing it everywhere?"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Definitely not!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "There goes my idea for a village water party!"),
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "Zuri and Naledi notice a small plant that looks weak and dry."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Naledi! Come quickly! I think that little plant needs a doctor!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "A doctor? Why?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Look at it! Its leaves are drooping! It looks like it has had a very long Monday!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Haha! I think it needs water, not a doctor. Plants need enough water to grow and stay healthy."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Oh! So it's thirsty! I understand. Sometimes I look exactly like that when I haven't had water!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "And look over there. That watering can has a leak!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "WHAT?! It's giving away our water for free without asking permission!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "That's right! Water is dripping out even when nobody is using it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "That's a sneaky little leak! We need to stop it before the watering can becomes completely empty!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Exactly! Wasting water can happen in small ways that we don't always notice."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then my detective eyes are officially on leak patrol!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "There are lots of simple ways we can save water, Zuri."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I'm listening! Should I stand next to every tap and watch it all day?"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Haha! That would be a very tiring job! We can close taps properly, fix leaks and use only the water we need."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I can do that! No more taps running when nobody is using them!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "We should also water plants carefully so the water reaches their roots instead of being wasted."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So no giving the entire garden an accidental swimming pool?"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Exactly! Small actions can make a big difference."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I get it now! We must save water even when we have enough for today."),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "That's right! People, animals and plants all depend on water."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then I have a new mission! I'm becoming a Water-Saving Superstar!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Haha! I like that! Let's show everyone how every drop goes a long way!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "EVERY DROP COUNTS!")
    };

    public static readonly StoryDialogueLine[] Story3Ending =
    {
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Naledi, I think I've finally figured it out! Water is precious, even when we have plenty of it!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "That's right! Having water available doesn't mean we should waste it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I used to think that if there was water in the village, we could just use as much as we wanted!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "But remember, people, animals and plants all need water to survive."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And that little plant taught me something! Even plants need us to look after them. I think it deserves a tiny thank-you speech!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "A thank-you speech? For a plant?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Of course! It's been working very hard at being green!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Haha! You're impossible, Zuri!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "But seriously, we must close taps properly, fix leaks and use only the water we need."),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Exactly! Small actions can make a big difference when everyone does their part."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Imagine if everyone in the village saved just a little water every day!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "We could protect so much more water for people, animals and the environment."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then I'm officially opening the Water Detective Leak Patrol! Our first rule: no sneaky dripping taps!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "And our second rule?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Never let a plant go thirsty if we can help it!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Haha! Together, we can help everyone understand why every drop matters."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "That's the spirit! Let's save water, protect our plants and become the greatest Water-Saving Superstars this village has ever seen!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "LEARNING TAKEAWAY: EVERY DROP MATTERS!")
    };

    const string Story3CorrectFeedback =
        "Brilliant! You're becoming a Water-Saving Superstar!";
    const string Story3IncorrectFeedback =
        "Not quite! Every good Water Detective checks the clues again! 😂";

    public static readonly StoryQuizQuestion[] Story3Questions =
    {
        new StoryQuizQuestion(
            "Why should we conserve water?",
            "Because people, plants and animals all depend on water.",
            "Because water is only useful during rainy days.",
            "Because plants do not need water.",
            0, Story3CorrectFeedback, Story3IncorrectFeedback),
        new StoryQuizQuestion(
            "What happens when plants do not receive enough water?",
            "They grow faster.",
            "They can become weak and dry.",
            "They turn into animals.",
            1, Story3CorrectFeedback, Story3IncorrectFeedback),
        new StoryQuizQuestion(
            "Which action helps prevent water waste?",
            "Leaving taps running.",
            "Ignoring leaking pipes.",
            "Closing taps properly and fixing leaks.",
            2, Story3CorrectFeedback, Story3IncorrectFeedback)
    };

    public static readonly StoryDialogueLine[] Story4Opening =
    {
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "Zuri and Naledi discover a community garden. Many plants are beginning to wilt."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "WOW! Look at this garden! So many vegetables and flowers! I think I've found the village's biggest salad!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Haha! It is lovely, isn't it? But this garden used to look even greener."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Wait... what happened? Did someone forget to water the plants? Or did the vegetables go on strike?"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Neither! The community has been struggling to get enough water for the garden."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Oh no! Look at those leaves! They're drooping like they've just heard some terrible news!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "The plants aren't getting enough water, Zuri. Gardens need water to grow healthy food for families."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "But we need those vegetables! I cannot imagine a village without delicious carrots!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "And many families depend on gardens for fresh food."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then this is serious! Detective Zuri is officially accepting the Garden Rescue mission!"),
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "They notice that the little water supply they have is not enough to support all the plants."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Right! I have a plan. We can bring water in buckets!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "That might help for a little while, but look at how many plants there are."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Hmm... you're right. My arms are already complaining just thinking about it!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "We need a more reliable water supply for the community."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Wait a minute! Didn't we learn about groundwater and wells?"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "We did! A properly planned and constructed borehole could help the community access groundwater beneath the ground."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "A borehole! So we could bring water closer to the garden?"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Yes, if there is enough suitable groundwater and the borehole is properly planned, built and maintained."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "That sounds amazing! Imagine not having to carry a million buckets!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Haha! But remember, even with a better water supply, we must still use water carefully."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Of course! We don't want to rescue the garden today and waste all our water tomorrow!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Exactly! We need a reliable water supply and a smart way to use it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We could water the plants carefully, fix leaking pipes and make sure the water reaches their roots!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Wonderful ideas! We must also keep the water source clean and make sure the borehole is properly maintained."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So no leaking pipes, no wasted water and no plants left feeling thirsty!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "That's right! Healthy plants help provide food for families, and the whole community benefits."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then let's get to work! These vegetables are counting on us!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "And remember, Zuri, a garden needs more than just water. It needs people who care for it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Don't worry! I've got my detective hat, my rescue plan and my very serious gardening face!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Your gardening face?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Yes! Look! Very serious... very professional... slightly hungry!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Haha! Come on, Garden Rescue Hero! Let's save this garden!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "RESCUE THE GARDEN!")
    };

    public static readonly StoryDialogueLine[] Story4Ending =
    {
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We did it, Naledi! I think our garden rescue mission was a success!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "You did wonderfully, Zuri! Now we understand how important water is for growing food."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And I learned that vegetables don't grow just because we stare at them and say, 'Please grow!'"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Haha! No, they need enough water, healthy soil and proper care."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And a reliable water supply could make a big difference for our garden!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "That's right! A properly planned borehole could help the community access groundwater if suitable groundwater is available."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "But we must protect the water source, maintain the equipment and use the water wisely."),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Exactly! Having more reliable access to water doesn't mean we should waste it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We should also fix leaks and water the plants carefully. I don't want our water escaping through sneaky pipes again!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "You've remembered everything! That's what makes a good Water Detective."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And imagine all the vegetables that could grow here! Carrots, tomatoes, spinach... I'm getting hungry just thinking about it!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Zuri! We're rescuing the garden, not eating the whole garden!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I know, I know! I'll wait until the vegetables are ready!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Haha! When the community works together, we can protect the garden and help families too."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then let's keep looking after our water and helping our garden grow!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "One healthy plant and one precious drop at a time!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Garden Rescue Heroes forever!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "LEARNING TAKEAWAY: CLEAN WATER HELPS GARDENS AND COMMUNITIES THRIVE!")
    };

    const string Story4CorrectFeedback =
        "Fantastic! You're a true Garden Rescue Hero!";
    const string Story4IncorrectFeedback =
        "Not quite! Think carefully, Water Detective! Every clue helps us protect the garden! 😂";

    public static readonly StoryQuizQuestion[] Story4Questions =
    {
        new StoryQuizQuestion(
            "Why are community gardens important?",
            "They help provide food for families and support healthy plants.",
            "They use water without giving anything back.",
            "They only grow flowers.",
            0, Story4CorrectFeedback, Story4IncorrectFeedback),
        new StoryQuizQuestion(
            "How can a properly planned borehole help a community?",
            "It creates rain whenever people need it.",
            "It can provide access to groundwater when suitable groundwater is available.",
            "It makes plants grow without water.",
            1, Story4CorrectFeedback, Story4IncorrectFeedback),
        new StoryQuizQuestion(
            "What should a community do to protect its garden and water supply?",
            "Use as much water as possible.",
            "Ignore leaks and leave taps running.",
            "Maintain the water source and use water carefully.",
            2, Story4CorrectFeedback, Story4IncorrectFeedback)
    };

    public static bool Story3Completed => PlayerPrefs.GetInt(Story3CompletedKey, 0) == 1;
    public static bool Story4Unlocked => PlayerPrefs.GetInt(Story4UnlockedKey, 0) == 1;
    public static bool Story4Completed => PlayerPrefs.GetInt(Story4CompletedKey, 0) == 1;
    public static bool HasReadStory3 => PlayerPrefs.GetInt(Story3ReadKey, 0) == 1 || Story3Completed;
    public static bool HasReadStory4 => PlayerPrefs.GetInt(Story4ReadKey, 0) == 1 || Story4Completed;

    public static void MarkStory3Read()
    {
        PlayerPrefs.SetInt(Story3ReadKey, 1);
        PlayerPrefs.Save();
    }

    public static void MarkStory4Read()
    {
        PlayerPrefs.SetInt(Story4ReadKey, 1);
        PlayerPrefs.Save();
    }
    public static int Story3BestScore => PlayerPrefs.GetInt(Story3BestScoreKey, 0);
    public static int Story4BestScore => PlayerPrefs.GetInt(Story4BestScoreKey, 0);

    public static bool HasStory3Content =>
        Story3Opening.Length > 0 && Story3Ending.Length > 0 &&
        Story3Questions.Length == 3;

    public static bool HasStory4Content =>
        Story4Opening.Length > 0 && Story4Ending.Length > 0 &&
        Story4Questions.Length == 3;

    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(Story3UnlockedKey);
        PlayerPrefs.DeleteKey(Story3CompletedKey);
        PlayerPrefs.DeleteKey(Story3ReadKey);
        PlayerPrefs.DeleteKey(Story4UnlockedKey);
        PlayerPrefs.DeleteKey(Story4CompletedKey);
        PlayerPrefs.DeleteKey(Story4ReadKey);
        PlayerPrefs.DeleteKey(Story3BestScoreKey);
        PlayerPrefs.DeleteKey(Story4BestScoreKey);
        PlayerPrefs.Save();
        EnsureInitialized();
    }

    public static void EnsureInitialized()
    {
        if (!PlayerPrefs.HasKey(Story3UnlockedKey))
            PlayerPrefs.SetInt(Story3UnlockedKey, 1);
        if (!PlayerPrefs.HasKey(Story3CompletedKey))
            PlayerPrefs.SetInt(Story3CompletedKey, 0);
        if (!PlayerPrefs.HasKey(Story4UnlockedKey))
            PlayerPrefs.SetInt(Story4UnlockedKey, 0);
        if (!PlayerPrefs.HasKey(Story4CompletedKey))
            PlayerPrefs.SetInt(Story4CompletedKey, 0);
        PlayerPrefs.Save();
    }

    public static void SaveStory3QuizResult(int score)
    {
        int safeScore = Mathf.Clamp(score, 0, 3);
        PlayerPrefs.SetInt(Story3BestScoreKey,
            Mathf.Max(Story3BestScore, safeScore));

        if (safeScore >= 2)
            PlayerPrefs.SetInt(Story3CompletedKey, 1);

        if (safeScore == 3)
            PlayerPrefs.SetInt(Story4UnlockedKey, 1);

        PlayerPrefs.Save();
    }

    public static void SaveStory4QuizResult(int score)
    {
        int safeScore = Mathf.Clamp(score, 0, 3);
        PlayerPrefs.SetInt(Story4BestScoreKey,
            Mathf.Max(Story4BestScore, safeScore));

        if (safeScore >= 2)
            PlayerPrefs.SetInt(Story4CompletedKey, 1);

        PlayerPrefs.Save();
    }

    public static StoryQuizQuestion GetStory4Question(
        int questionIndex, bool question1Correct, bool question2Correct)
    {
        int safeIndex = Mathf.Clamp(questionIndex, 0, 2);
        return Story4Questions[safeIndex];
    }
}
