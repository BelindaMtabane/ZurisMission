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
            "HELLOOO, EVERYONE! I'M BACK! Did you miss me? Because I definitely missed this village!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Welcome home, Zuri! We've heard all about your adventures!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Of course you have! I've been solving mysteries, protecting forests and fighting sneaky water leaks!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Haha! You've certainly learned a lot. But our village still needs your help."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Another mystery?! My detective hat isn't even fully unpacked!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Our village needs more than just a water source. We need a proper plan to look after it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Wait... I thought we just needed to build a well or borehole and celebrate!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "If only it were that easy! We also need pipes, tanks and taps to help deliver water to people's homes."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Oh! So the well, pipes, tanks and taps all have to work together? It's like a giant water team!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Exactly! And every part needs proper care and maintenance."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then this sounds like a job for... THE VILLAGE WATER DETECTIVE TEAM!"),
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "Villagers work together to inspect a water tank, pipes and taps."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Wow! It looks like everyone has something to do. Who is in charge of the pipes?"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Some people help check the pipes, while others look after the tanks and taps."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And what happens if a pipe breaks or a tap starts leaking?"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "We must report the problem and arrange for it to be repaired before more water is wasted."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So we can't just put a bucket underneath and pretend everything is fine?"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "A bucket might catch some water temporarily, but the leak still needs to be repaired."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I see! We don't want our village turning into a swimming pool!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Haha! That's certainly not the plan."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And what about keeping the area around our water sources clean?"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "That's everyone's responsibility too. We must protect the water source as well as the equipment."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So even after we build a well or borehole, we can't just wave goodbye and forget about it?"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Exactly! Water systems need regular inspections, repairs and responsible management."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Understood! Building it is only the beginning. Looking after it is the real mission!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Our village will be stronger if everyone takes responsibility for protecting our water."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then let's give everyone a job! Pipe patrol, tank patrol and tap patrol!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Haha! Those are good names, but we also need to make sure everyone understands their responsibilities."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We can avoid wasting water, report leaks and keep our water points clean!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Wonderful! We must also make sure our water is properly tested and that our equipment is maintained."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And nobody should waste water just because it's easier to get now!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "That's right! We must think about our neighbours, our children and future generations."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I used to think solving water problems meant finding water. Now I know we must protect it, share responsibility and look after the whole system!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "You've learned an important lesson, Zuri. A community can achieve much more when people work together."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then let's bring everyone together! I'll be the Chief Detective!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "And what will the rest of us be?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "My very important assistant detectives, of course!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Haha! I think we can all be Water Detectives!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Deal! Now, team... LET'S GET TO WORK!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "WORK TOGETHER FOR WATER!")
    };

    public static readonly StoryDialogueLine[] Story7Ending =
    {
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I think I've finally discovered one of the BIGGEST secrets to solving water problems!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "And what might that be, Zuri?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "TEAMWORK! It takes more than one detective to keep an entire village's water safe!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "That's absolutely right! We must look after our wells, boreholes, pipes, tanks and taps."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And if something breaks, we report it and make sure it gets repaired! No pretending that a leaking pipe is a little water fountain!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Haha! Exactly! We must also keep our water sources clean and use water responsibly."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I've learned that building a water system is only the beginning. Looking after it is what keeps it working!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "You've made us proud, Zuri. Our village has a much better chance when everyone works together."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And everyone has a part to play! Even the smallest jobs can make a big difference."),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "That's true. Protecting our water is a responsibility we all share."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then let's make a promise! We will protect our water, help one another and never take a single drop for granted!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "We promise!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Together, we can build a stronger and healthier community!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Wait! Does this mean I'm no longer the only Water Detective?"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "That's right! The whole village is now part of the team!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "YES! Finally! I've got an entire detective squad! I hope everyone brought their detective hats!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Haha! We are ready, Zuri!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then let's work together and protect every precious drop!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "LEARNING TAKEAWAY: EVERYONE HAS A ROLE IN PROTECTING OUR WATER!")
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
            "WOW! Look at our village! I almost don't recognise the place! Did I accidentally walk into a village makeover show?"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Haha! Welcome back, Zuri! A lot has changed since your journey began."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I remember when I first started looking for water. I was just a little detective with a big mystery and a very empty bucket!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "And look at everything you've learned since then!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I've investigated underground water, rescued gardens, explored forests and even caught sneaky pollution clues!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "You've certainly had quite an adventure!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And I've learned that finding water is only the beginning! We must make sure it's clean, safe and available for everyone."),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "That's right. We must also protect our environment and never waste the water we have."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So basically, water needs a whole protection team! Good thing I'm an expert detective!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Haha! It looks like the whole village has become part of your team."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Excellent! I always wanted a very large detective squad!"),
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "They walk past the village well, borehole, water pipes, storage tanks and taps."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Look at everything we've worked on! The well, the borehole, the pipes and the water tanks!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Each part has an important job. Together, they help bring water closer to our homes."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So the well finds the water, the pipes carry it, the tanks store it and the taps help us get it?"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Exactly! But we must keep checking and repairing them so they continue working properly."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "No leaving broken pipes to become surprise waterfalls, then?"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Definitely not! We must also make sure our water is tested and safe before people drink it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And we must protect rivers, streams and other natural water sources from pollution!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "That's right. We must also use water carefully because people, plants and animals all depend on it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Wait! So everything I've learned is connected? Finding water, keeping it clean, protecting nature and maintaining our water systems!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "You've understood the bigger picture, Zuri. Clean water is important for everyone's health and future."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Wow! My brain is officially full of water knowledge! I might need a bigger detective hat!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Haha! There's always room for one more lesson!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "When I first became a Water Detective, I thought my job was simply to find water."),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Now you know that protecting water takes more than just finding it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We must close taps, report leaks, keep rubbish away from water sources and look after our water systems!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "And teach others why clean water and a healthy environment are so important."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Because everyone deserves access to safe water!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "That's right, Zuri. Clean water supports healthy families, strong communities and a better future."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then I have an idea! Let's make a village-wide Water Detective promise!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "What kind of promise?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "A promise to protect our water, care for our environment and never take a single drop for granted!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "That sounds like a promise worth making!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Excellent! Everyone get ready! This is our BIGGEST mission yet!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "We're ready, Zuri!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then let's finish this adventure together and show the world that EVERY DROP COUNTS!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "PROTECT OUR WATER. PROTECT OUR FUTURE!")
    };

    public static readonly StoryDialogueLine[] Story8Ending =
    {
        new StoryDialogueLine(StorySpeaker.Zuri,
            "WE DID IT! I can't believe how far we've come!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "You have helped our village understand just how precious water really is, Zuri."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I started this journey looking for water, but I discovered something much bigger!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "And what was that, Zuri?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "That clean water means health, hope and a better future for everyone!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "You've also shown us that protecting water is something we must do together."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We must keep our water sources clean, maintain our wells and pipes, prevent pollution and never waste water!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "And pass these lessons on to the next generation!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Because every person can make a difference, even with the smallest actions!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Our village is proud of you, Zuri!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Thank you, everyone! But I didn't do this alone. I had my friends, my village and the BEST detective team ever!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Haha! And what happens now, Detective Zuri? Is your adventure finally over?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Over?! Not a chance! My detective hat is staying on!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "So what's our next mission?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "To keep protecting our water every single day! We must close those taps, report those sneaky leaks and keep pollution away from our water sources!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Then let us make that promise together!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "We promise to protect our water!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "We promise to care for our environment!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And we promise never to take a single drop for granted!"),
        new StoryDialogueLine(StorySpeaker.Everyone,
            "EVERY DROP COUNTS!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Mission complete! But remember, Water Detectives... protecting water never stops!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            Story8CompleteTitle),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            Story8JourneyTitle),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "ACHIEVEMENT: " + Story8ChampionTitle)
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
