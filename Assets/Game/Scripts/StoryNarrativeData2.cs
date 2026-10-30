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
            "Wow! Look at this place! There are so many green plants growing here!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "It looks much better than before, doesn't it? The plants are finally getting some water again."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I'm so happy! I thought once our village had more water, everything would be perfect!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Having access to water is a big step, Zuri, but we still have to use it wisely."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "What do you mean? If we have water, shouldn't we use as much as we need?"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "We should use what we need, but wasting water can cause problems for everyone."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Even when there is water available?"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Yes! Remember, people, animals and plants all depend on water to survive."),
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "Zuri and Naledi notice a small plant that looks weak and dry."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Oh no! Naledi, look at that little plant! Why does it look so sad?"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "It hasn't been getting enough water. Plants need water to grow, stay healthy and produce food."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So without enough water, they can become weak and dry?"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "That's right! And when plants struggle, animals and people can also be affected."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I never thought about how many living things depend on one little drop of water!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "There's something else we should look at. See that watering can over there?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Oh! It's leaking! Water is dripping out even though nobody is using it!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Exactly! That's water being wasted without helping anyone."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We have to do something! We can't let all that water go to waste!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "There are many simple ways we can save water."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Like what?"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "We can close taps properly, report leaking pipes and use only the water we need."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And when watering plants, we should make sure the water reaches them instead of wasting it on the ground!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Exactly! Small actions can make a big difference."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So getting water is important, but protecting it and using it carefully is just as important!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "You've got it, Zuri! Water is valuable, and everyone has a part to play in saving it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then let's show everyone that every drop really does go a long way!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "EVERY DROP COUNTS!")
    };

    public static readonly StoryDialogueLine[] Story3Ending =
    {
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Naledi, I finally understand! Having water available doesn't mean we should waste it."),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "That's right! Water is precious, and we must look after it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "If we leave taps running or ignore leaks, we could waste water that someone else needs!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Exactly! And remember, plants, animals and people all need water to survive."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "From now on, I'll close taps properly, report leaks and use only the water I need!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "That's wonderful, Zuri! Imagine what could happen if everyone in the village did the same."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We could save so much water together!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "And help keep our environment healthy too!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Another mystery solved! But our journey isn't over yet!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Come on, Water Detective! There are still more ways we can help our community!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "Every drop matters. Saving water helps protect people, plants, animals and the environment.")
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
            "Naledi, look at this beautiful garden! There are so many vegetables and flowers!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "It used to be even greener, Zuri. But lately, the plants haven't been getting enough water."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Oh no! Some of the leaves are turning brown. What happened?"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "The community has been struggling to get enough water for the garden."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "But we need plants to grow food! What will happen if the vegetables don't survive?"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "That's the problem. Gardens help provide food for families, and healthy plants are important for our community."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then we have to help! We can't let the whole garden disappear!"),
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "They notice that the little water supply they have is not enough to support all the plants."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We could bring more water in buckets, but there are so many plants!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "That might help for a little while, but we need a more reliable water supply."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Do you mean finding another place to get water?"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Exactly! Remember what we learned about groundwater? A properly planned and constructed borehole could help the community access water beneath the ground."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "A borehole! So we could bring water closer to the garden?"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Yes, if there is enough suitable groundwater and the borehole is properly planned, built, protected and maintained. The water must also be tested."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "That sounds like a great idea! But we must still be careful not to waste the water we find."),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "You're right! Having a better water supply doesn't mean we should use more water than we need."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So we need both a reliable water source and a smart way to use it!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "That's exactly what the garden needs. We must protect the water supply and make sure every drop is used wisely."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We could water the plants carefully, repair leaking pipes and make sure the water reaches the roots!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Wonderful ideas! We should also keep the water source clean and make sure the borehole is properly maintained."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I understand now! A good water supply helps us grow food, but looking after that water is just as important."),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "And when the community works together, we can protect the garden and help families too."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then let's rescue this garden and help it grow again!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "RESCUE THE GARDEN!")
    };

    public static readonly StoryDialogueLine[] Story4Ending =
    {
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We did it, Naledi! Now I understand how important this garden is to everyone!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Yes! Healthy gardens help provide food, and they need a reliable supply of clean water."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And a properly planned borehole could help the community get the water it needs!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "That's right! But we must protect the water source, test the water, and keep using it wisely."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We should also repair leaks and make sure the plants only get the water they need!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "Exactly! When we combine a reliable water supply with responsible water use, we can help the whole community."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Imagine all the vegetables and flowers growing here again!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "And imagine the families who will benefit from the food this garden provides."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I'm so proud of what we've learned together! We aren't just finding water; we're learning how to protect it!"),
        new StoryDialogueLine(StorySpeaker.Naledi,
            "That's what makes a real Water Detective, Zuri!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "The garden has hope again! Let's keep helping our community, one drop at a time!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "Clean water helps gardens grow and communities thrive. A reliable water supply and responsible water use can make a lasting difference.")
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
