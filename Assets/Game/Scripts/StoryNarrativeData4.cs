using UnityEngine;

/// <summary>
/// Stores Story 7 and Story 8 content.
/// Story 8 is the final narrative and records journey completion.
/// </summary>
public static class StoryNarrativeData4
{
    const string Story7UnlockedKey = "DropByDrop.Narrative.Story7Unlocked";
    const string Story7CompletedKey = "DropByDrop.Narrative.Story7Completed";
    const string Story7ReadKey = "DropByDrop.Narrative.Story7Read";
    const string Story8UnlockedKey = "DropByDrop.Narrative.Story8Unlocked";
    const string Story8CompletedKey = "DropByDrop.Narrative.Story8Completed";
    const string Story8ReadKey = "DropByDrop.Narrative.Story8Read";
    const string Story7BestScoreKey = "DropByDrop.Narrative.Story7BestQuizScore";
    const string Story8BestScoreKey = "DropByDrop.Narrative.Story8BestQuizScore";
    const string AllStoriesCompletedKey = "DropByDrop.Narrative.AllStoriesCompleted";
    const string ChampionKey = "DropByDrop.Narrative.WaterDetectiveChampion";

    public const string Story7Title = "THE VILLAGE COMES TOGETHER";
    public const string Story7MissionTitle = "WORK TOGETHER FOR WATER!";
    public const string Story7CompleteTitle = "STORY 7 COMPLETED!";
    public const string Story8Title = "EVERY DROP COUNTS";
    public const string Story8MissionTitle = "PROTECT OUR WATER. PROTECT OUR FUTURE!";
    public const string Story8CompleteTitle = "ALL STORIES COMPLETED!";
    public const string Story8JourneyTitle = "ZURI'S WATER DETECTIVE JOURNEY COMPLETE!";
    public const string Story8ChampionTitle = "WATER DETECTIVE CHAMPION!";
    public const string Story8Takeaway =
        "Clean water is essential for life. Protecting water, preventing pollution, maintaining water systems and using every drop wisely can help create a healthier and more sustainable future.";

    public static readonly StoryDialogueLine[] Story7Opening =
    {
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "Zuri returns to the village. Villagers are working around the water points and discussing how to protect their water supply."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I'm back! It feels good to see everyone again!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Welcome home, Zuri! We've heard about all the things you've learned on your journey."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I've learned so much! Water is precious, and we must protect it wherever we find it."),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "That's an important lesson. Our village needs more than just a water source. We need a plan to look after it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "What do you mean? Don't we just need to build a well or borehole?"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Building one is a good start, but we also need pipes, tanks and taps to help deliver water to people."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Oh! So all these parts must work together to bring water to our homes!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Exactly! A water system needs proper planning, teamwork and regular care."),
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "Villagers work together to inspect a water tank, pipes and taps."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "It looks like everyone has a job to do!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "That's right! Some people help check the pipes, while others look after the tanks and taps."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "What happens if a pipe breaks or a tap starts leaking?"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "We must report the problem so trained people can repair it before more water is wasted."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And we should keep the area around our water sources clean too!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Wonderful thinking! Protecting the water source is just as important as maintaining the equipment."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So even after a well or borehole is built, we cannot simply forget about it?"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "That's correct. Water systems need regular inspections, repairs and responsible management to keep working properly."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I understand! Building something is only the beginning. We must look after it!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Our village will be stronger if everyone takes responsibility for protecting our water."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We can all help by avoiding waste, reporting leaks and keeping water points clean!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Yes! We should also make sure that our water is properly tested by trained people and that our equipment is maintained."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And nobody should waste water just because it is easier to get now!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Exactly! We must think about our neighbours, our children and future generations."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I used to think solving water problems was about finding water. Now I know it's also about protecting it and sharing responsibility."),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "You've learned an important lesson, Zuri. A community can achieve much more when people work together."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then let's bring everyone together and build a better future for our village!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "WORK TOGETHER FOR WATER!")
    };

    public static readonly StoryDialogueLine[] Story7Ending =
    {
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I think I've finally discovered one of the biggest secrets to solving water problems!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "And what might that be, Zuri?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Working together! Everyone has a role in protecting our water."),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "That's right! We must look after our wells, boreholes, pipes, tanks and taps."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And if something breaks, we must report it and make sure it gets repaired!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Exactly! We must also keep our water sources clean and use water responsibly."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I have learned that building a water system is only the beginning. Looking after it is what keeps it working!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "You've made us proud, Zuri. Our village has a much better chance when everyone works together."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then let's make a promise to protect our water, help one another and never take a single drop for granted!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "We promise!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Together, we can build a stronger and healthier community!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "That's the spirit! The whole village is now part of the Water Detective team!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "Protecting water is everyone's responsibility. Communities must work together to maintain water systems, prevent waste and ensure reliable access to safe water.")
    };

    const string Story7CorrectFeedback =
        "Wonderful teamwork! You're helping the whole village become Water Champions!";
    const string Story7IncorrectFeedback =
        "Not quite! Remember, a strong community works together! Try again! 😂";

    public static readonly StoryQuizQuestion[] Story7Questions =
    {
        new StoryQuizQuestion(
            "Why is teamwork important when managing water?",
            "Because everyone can help protect and maintain the community's water supply.",
            "Because only one person should look after all the water.",
            "Because communities do not need to work together.",
            0, Story7CorrectFeedback, Story7IncorrectFeedback),
        new StoryQuizQuestion(
            "What should happen when a water pipe or tap breaks?",
            "Ignore it and let the water continue leaking.",
            "Report the problem and arrange for it to be repaired.",
            "Stop using every water source forever.",
            1, Story7CorrectFeedback, Story7IncorrectFeedback),
        new StoryQuizQuestion(
            "Why must water systems be maintained after they are built?",
            "Because maintenance helps keep the system working and prevents water waste.",
            "Because water systems never need repairs.",
            "Because maintenance makes water disappear.",
            0, Story7CorrectFeedback, Story7IncorrectFeedback)
    };

    public static readonly StoryDialogueLine[] Story8Opening =
    {
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "Zuri returns to the village and sees the improvements made throughout her journey. Villagers are working together and caring for their water systems."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Wow! Look at our village! So much has changed since the day I started my journey!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "It certainly has, Zuri! You've helped us learn so much about protecting our water."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I remember when all I wanted was to find water for our village."),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "And now you understand that finding water is only one part of the challenge."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Yes! We need clean and safe water, we must protect our environment and we must never waste what we have."),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "You've learned some very important lessons along the way."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I've also learned that we cannot do everything alone. Our whole community must work together!"),
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "They walk past the village well, borehole, water pipes, storage tanks and taps."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Look at all the things we've worked on! The well, the borehole, the pipes and the water tanks!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Each part has an important job. Together, they help bring water closer to our homes."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "But we must keep checking and repairing them so they continue working properly!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Exactly! We also need to make sure our water is tested and safe before people drink it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And we must protect rivers, streams and other natural water sources from pollution."),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "That's right! We must also use water carefully because people, plants and animals all depend on it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So everything we've learned is connected! Finding water, keeping it clean, protecting nature and maintaining our water systems!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "You've understood the bigger picture, Zuri. Clean water is important for everyone's health and future."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "When I first became a Water Detective, I didn't realise how much work went into protecting water."),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Now you know that every action matters, even the small ones."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We can close taps, report leaks, keep rubbish away from water sources and look after our water systems!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "And we can teach others why clean water and a healthy environment are so important."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Because everyone deserves access to safe water!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "That's right, Zuri. Clean water supports healthy families, strong communities and a better future."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then I want everyone to make a promise with me!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "What kind of promise?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "A promise to protect our water, care for our environment and never take a single drop for granted!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "We promise, Zuri!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Wonderful! Then let's finish our journey together and show that EVERY DROP COUNTS!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "PROTECT OUR WATER. PROTECT OUR FUTURE!")
    };

    public static readonly StoryDialogueLine[] Story8Ending =
    {
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We did it! I can't believe how far we've come!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "You have helped our village understand just how precious water really is."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I started this journey looking for water, but I discovered something much bigger."),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "And what was that, Zuri?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "That clean water means health, hope and a better future for everyone!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "You've also shown us that protecting water is something we must do together."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We must keep our water sources clean, maintain our wells and pipes, prevent pollution and never waste water."),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "And pass these lessons on to the next generation!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Because every person can make a difference, no matter how small their actions may seem."),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Our village is proud of you, Zuri!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Thank you, everyone! But this isn't the end of our mission. We must continue protecting water every single day!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Then let us make that promise together!"),
        new StoryDialogueLine(StorySpeaker.Everyone,
            "EVERY DROP COUNTS!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            Story8CompleteTitle),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            Story8JourneyTitle),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            Story8Takeaway),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            Story8ChampionTitle)
    };

    const string Story8CorrectFeedback =
        "AMAZING! You've mastered the lessons of a true Water Detective!";
    const string Story8IncorrectFeedback =
        "Not quite! Think about everything you've learned on your journey. You can do it! 😂";

    public static readonly StoryQuizQuestion[] Story8Questions =
    {
        new StoryQuizQuestion(
            "What is the most important lesson about water?",
            "Finding water is all that matters.",
            "Water must be accessed safely, protected and used responsibly.",
            "Water should only be saved during rainy days.",
            1, Story8CorrectFeedback, Story8IncorrectFeedback),
        new StoryQuizQuestion(
            "Why must communities maintain their water systems?",
            "To keep water systems working and help provide reliable access to water.",
            "Because water systems never break.",
            "To make people use more water.",
            0, Story8CorrectFeedback, Story8IncorrectFeedback),
        new StoryQuizQuestion(
            "Who is responsible for protecting water?",
            "Only Water Detectives.",
            "Only people who build boreholes.",
            "Everyone has a role in protecting and using water responsibly.",
            2, Story8CorrectFeedback, Story8IncorrectFeedback)
    };

    public static bool Story7Unlocked => PlayerPrefs.GetInt(Story7UnlockedKey, 0) == 1;
    public static bool Story7Completed => PlayerPrefs.GetInt(Story7CompletedKey, 0) == 1;
    public static bool Story8Unlocked => PlayerPrefs.GetInt(Story8UnlockedKey, 0) == 1;
    public static bool Story8Completed => PlayerPrefs.GetInt(Story8CompletedKey, 0) == 1;
    public static bool HasReadStory7 => PlayerPrefs.GetInt(Story7ReadKey, 0) == 1 || Story7Completed;
    public static bool HasReadStory8 => PlayerPrefs.GetInt(Story8ReadKey, 0) == 1 || Story8Completed;

    public static void MarkStory7Read()
    {
        PlayerPrefs.SetInt(Story7ReadKey, 1);
        PlayerPrefs.Save();
    }

    public static void MarkStory8Read()
    {
        PlayerPrefs.SetInt(Story8ReadKey, 1);
        PlayerPrefs.Save();
    }
    public static bool AllStoriesCompleted => PlayerPrefs.GetInt(AllStoriesCompletedKey, 0) == 1;
    public static bool WaterDetectiveChampion => PlayerPrefs.GetInt(ChampionKey, 0) == 1;
    public static int Story7BestScore => PlayerPrefs.GetInt(Story7BestScoreKey, 0);
    public static int Story8BestScore => PlayerPrefs.GetInt(Story8BestScoreKey, 0);

    public static bool HasStory7Content =>
        Story7Opening.Length > 0 && Story7Ending.Length > 0 &&
        Story7Questions.Length == 3;

    public static bool HasStory8Content =>
        Story8Opening.Length > 0 && Story8Ending.Length > 0 &&
        Story8Questions.Length == 3;

    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(Story7UnlockedKey);
        PlayerPrefs.DeleteKey(Story7CompletedKey);
        PlayerPrefs.DeleteKey(Story7ReadKey);
        PlayerPrefs.DeleteKey(Story8UnlockedKey);
        PlayerPrefs.DeleteKey(Story8CompletedKey);
        PlayerPrefs.DeleteKey(Story8ReadKey);
        PlayerPrefs.DeleteKey(Story7BestScoreKey);
        PlayerPrefs.DeleteKey(Story8BestScoreKey);
        PlayerPrefs.DeleteKey(AllStoriesCompletedKey);
        PlayerPrefs.DeleteKey(ChampionKey);
        PlayerPrefs.Save();
        EnsureInitialized();
    }

    public static void EnsureInitialized()
    {
        if (!PlayerPrefs.HasKey(Story7UnlockedKey))
            PlayerPrefs.SetInt(Story7UnlockedKey, 0);
        if (!PlayerPrefs.HasKey(Story7CompletedKey))
            PlayerPrefs.SetInt(Story7CompletedKey, 0);
        if (!PlayerPrefs.HasKey(Story8UnlockedKey))
            PlayerPrefs.SetInt(Story8UnlockedKey, 0);
        if (!PlayerPrefs.HasKey(Story8CompletedKey))
            PlayerPrefs.SetInt(Story8CompletedKey, 0);
        if (!PlayerPrefs.HasKey(AllStoriesCompletedKey))
            PlayerPrefs.SetInt(AllStoriesCompletedKey, 0);
        if (!PlayerPrefs.HasKey(ChampionKey))
            PlayerPrefs.SetInt(ChampionKey, 0);
        PlayerPrefs.Save();
    }

    public static void UnlockStory7FromStory6()
    {
        PlayerPrefs.SetInt(Story7UnlockedKey, 1);
        PlayerPrefs.Save();
    }

    public static void SaveStory7QuizResult(int score)
    {
        int safeScore = Mathf.Clamp(score, 0, 3);
        PlayerPrefs.SetInt(Story7BestScoreKey,
            Mathf.Max(Story7BestScore, safeScore));

        if (safeScore >= 2)
            PlayerPrefs.SetInt(Story7CompletedKey, 1);

        if (safeScore == 3)
            PlayerPrefs.SetInt(Story8UnlockedKey, 1);

        PlayerPrefs.Save();
    }

    public static void SaveStory8QuizResult(int score)
    {
        int safeScore = Mathf.Clamp(score, 0, 3);
        PlayerPrefs.SetInt(Story8BestScoreKey,
            Mathf.Max(Story8BestScore, safeScore));

        if (safeScore >= 2)
        {
            PlayerPrefs.SetInt(Story8CompletedKey, 1);
            PlayerPrefs.SetInt(AllStoriesCompletedKey, 1);
            PlayerPrefs.SetInt(ChampionKey, 1);
        }

        PlayerPrefs.Save();
    }

    public static StoryQuizQuestion GetStory8Question(
        int questionIndex, bool question1Correct, bool question2Correct)
    {
        int safeIndex = Mathf.Clamp(questionIndex, 0, 2);
        return Story8Questions[safeIndex];
    }
}
