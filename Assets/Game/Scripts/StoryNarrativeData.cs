using UnityEngine;

public enum StorySpeaker
{
    Zuri,
    Villager1,
    Villager2,
    StageDirection,
    TitleCard
}

public enum StoryQuizDifficulty
{
    Easy,
    Medium,
    Hard
}

public readonly struct StoryDialogueLine
{
    public readonly StorySpeaker Speaker;
    public readonly string Text;

    public StoryDialogueLine(StorySpeaker speaker, string text)
    {
        Speaker = speaker;
        Text = text;
    }
}

public readonly struct StoryQuizQuestion
{
    public readonly string Prompt;
    public readonly string[] Answers;
    public readonly int CorrectAnswer;
    public readonly string CorrectRemark;
    public readonly string IncorrectRemark;
    public readonly string Hint;
    public readonly StoryQuizDifficulty Difficulty;

    public StoryQuizQuestion(string prompt, string answer1, string answer2, string answer3,
        int correctAnswer, string correctRemark, string incorrectRemark)
        : this(prompt, answer1, answer2, answer3, correctAnswer, correctRemark,
            incorrectRemark, string.Empty, StoryQuizDifficulty.Medium)
    {
    }

    public StoryQuizQuestion(string prompt, string answer1, string answer2, string answer3,
        int correctAnswer, string correctRemark, string incorrectRemark,
        string hint, StoryQuizDifficulty difficulty)
    {
        Prompt = prompt;
        Answers = new[] { answer1, answer2, answer3 };
        CorrectAnswer = correctAnswer;
        CorrectRemark = correctRemark;
        IncorrectRemark = incorrectRemark;
        Hint = hint;
        Difficulty = difficulty;
    }
}

/// <summary>
/// Stores Story 1 and Story 2 narrative content, quiz content and saved unlock data.
/// Story 2 ending content stays empty until its approved dialogue is provided.
/// </summary>
public static class StoryNarrativeData
{
    const string Story1UnlockedKey = "DropByDrop.Narrative.Story1Unlocked";
    const string Story1CompletedKey = "DropByDrop.Narrative.Story1Completed";
    const string Story2UnlockedKey = "DropByDrop.Narrative.Story2Unlocked";
    const string Level1UnlockedKey = "DropByDrop.Narrative.Level1Unlocked";
    const string Story1BestScoreKey = "DropByDrop.Narrative.Story1BestQuizScore";

    public const string Story1Title = "THE GREAT WATER MYSTERY";
    public const string Story2Title = "THE HIDDEN WATER CLUE";

    public static readonly StoryDialogueLine[] Story1Opening =
    {
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Good morning! Who's ready for a BIG bucket of water?"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Not so fast, Zuri! Our village is running low on water."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "What?! Did someone drink it all?"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Haha! No. We haven't had enough rain, and our water sources are running low."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Oh no! Even the ground looks like a giant cracked biscuit! I'll investigate!"),
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "Zuri notices a small puddle nearby."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "WAIT! WATER! Mystery solved!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Stop, Zuri! Don't drink that puddle water!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "But it looks clean!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Harmful germs can hide in water even when it looks clean."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Invisible germs?! That's just rude! And is that a frog?! No thank you!"),
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "Zuri wipes her forehead because it is hot."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Phew! It's really hot today!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Find some shade, rest and ask for help if you feel unwell."),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Our village needs help finding a safe and reliable water supply."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then I'm your girl! I'll collect supplies and help our village!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Good luck, Zuri!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Adventure awaits! Bucket ready, let's go!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "Ready to become a Water Detective?")
    };

    public static readonly StoryDialogueLine[] Story1Ending =
    {
        new StoryDialogueLine(StorySpeaker.Zuri,
            "YES! We did it! I'm officially a Water Detective!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Well done, Zuri! But our village still needs your help."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Another mystery? You know I'm always ready for an adventure!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "We need you to collect some important supplies: sand, bricks, a hammer and water for your bucket."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Sand, bricks, a hammer and water? Are we building something?"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "You'll find out soon! These materials will help us prepare for a better water solution."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Aha! A new mission! Don't worry, I'll collect everything we need!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Be careful on your journey, Zuri. Watch out for obstacles and remember to stay safe."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Mission accepted! Bucket ready, adventure loading! Let's go!")
    };

    public static readonly StoryQuizQuestion[] Story1Questions =
    {
        new StoryQuizQuestion(
            "The village has received very little rain for several weeks. What could happen to its water supply?",
            "The amount of available water may decrease.",
            "The rivers will automatically fill themselves.",
            "The clouds will send water directly into people's buckets.",
            0,
            "Correct! You've cracked the drought mystery! Detective Zuri approves!",
            "Not quite! Even the clouds are confused by that answer! 😂"),
        new StoryQuizQuestion(
            "Zuri has been walking in the hot sun. She feels dizzy and very tired. What is safest?",
            "Keep walking until she reaches her destination.",
            "Find shade, rest and ask someone for help.",
            "Run faster so she can finish before getting hotter.",
            1,
            "Brilliant! Zuri stays cool, safe and un-toasted!",
            "Uh-oh! We don't want a crispy Zuri! Try again! 😂"),
        new StoryQuizQuestion(
            "Puddle water looks clear, has no strange smell and appears clean. What should Zuri understand?",
            "Water is safe if it looks and smells clean.",
            "Water from nature is always safe to drink.",
            "Harmful germs can be present even when water looks and smells clean.",
            2,
            "Spot on! Invisible germs won't fool you today!",
            "Oops! The frog says that's a terrible idea! 😂")
    };

    public static readonly StoryDialogueLine[] Story2Opening =
    {
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Hello again! Your favourite Water Detective is back!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Welcome, Zuri! We've been looking for a more reliable water source."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Hmm... Have we checked everywhere? Even underneath our feet?"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "That's a clever thought! Water can collect underground. It's called groundwater."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "UNDERGROUND?! So water is hiding from me?!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Haha! When rain falls, some of it soaks into the ground and moves through soil and rocks."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Wow! The ground has its own secret water supply!"),
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "Villager 2 shows Zuri a picture of a well."),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "A properly built well can help us reach groundwater and bring it to the surface."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So we could build a well for our village?"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Yes! But first, we must find a suitable location and make sure the water is safe."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So I can't just dig a random hole and hope for a swimming pool?"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Haha! No, Zuri. A well needs proper planning and construction."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Got it! We need a clever plan, not a giant hole!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Our village will need the right materials and people to help build a reliable water source."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then let's work together! Teamwork makes the water dream work!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "Remember, we must also protect and maintain our water source."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Absolutely! No pollution, no careless digging and definitely no frog soup!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Good luck, Zuri!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Let's uncover the hidden water clue!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "THE HIDDEN WATER CLUE!")
    };

    public static readonly StoryDialogueLine[] Story2Ending =
    {
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Well done, Zuri! Now we know how groundwater could help our village."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And I found the secret water hiding beneath our feet! Detective Zuri strikes again!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "But finding water is only the beginning. We need to plan a safe and reliable well."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So, what's our next move?"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "We must find a suitable location and gather the right building materials."),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "We'll need bricks, sand, cement, a shovel and other tools to prepare for construction."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Bricks, sand and tools? Looks like this detective is becoming a builder too!"),
        new StoryDialogueLine(StorySpeaker.Villager1,
            "Haha! And remember, the well must be properly built, protected and its water tested."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Don't worry! We'll build it safely and work together to bring water closer to our village!"),
        new StoryDialogueLine(StorySpeaker.Villager2,
            "That's the spirit, Zuri! Our community is counting on you."),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "A WELL OF HOPE")
    };

    const string Story2CorrectFeedback =
        "Brilliant! You're uncovering the clues like a real Water Detective!";
    const string Story2IncorrectFeedback =
        "Not quite! Every good detective checks the clues again! 😂";

    // Each row contains Easy, Medium and Hard versions of one learning objective.
    public static readonly StoryQuizQuestion[][] Story2QuestionBank =
    {
        new[]
        {
            new StoryQuizQuestion(
                "What is groundwater?",
                "Water found beneath Earth's surface.",
                "Water floating inside clouds.",
                "Water collected only in rivers.",
                0, Story2CorrectFeedback, Story2IncorrectFeedback,
                "Think about where groundwater is found.", StoryQuizDifficulty.Easy),
            new StoryQuizQuestion(
                "How does rainwater become groundwater?",
                "It evaporates and stays inside clouds.",
                "It soaks into soil and moves underground.",
                "It flows over roads into drains.",
                1, Story2CorrectFeedback, Story2IncorrectFeedback,
                "Remember what happens when rain soaks into soil.", StoryQuizDifficulty.Medium),
            new StoryQuizQuestion(
                "Why can groundwater sometimes remain available during a dry period?",
                "Some groundwater is stored underground and can move slowly.",
                "The ground creates new water whenever it gets hot.",
                "Groundwater only comes from ocean waves.",
                0, Story2CorrectFeedback, Story2IncorrectFeedback,
                "Think about water stored below the surface.", StoryQuizDifficulty.Hard)
        },
        new[]
        {
            new StoryQuizQuestion(
                "What does a well help people do?",
                "Reach groundwater.",
                "Make rain fall.",
                "Clean every river.",
                0, Story2CorrectFeedback, Story2IncorrectFeedback,
                "A well reaches below the ground.", StoryQuizDifficulty.Easy),
            new StoryQuizQuestion(
                "Why must a community choose a suitable location for a well?",
                "To make sure groundwater can be reached safely.",
                "Because every patch of ground contains drinkable water.",
                "So the well is closer to the tallest tree.",
                0, Story2CorrectFeedback, Story2IncorrectFeedback,
                "Think about reaching safe groundwater.", StoryQuizDifficulty.Medium),
            new StoryQuizQuestion(
                "What could happen if a well is poorly located or constructed?",
                "It may not reach enough water or could become contaminated.",
                "It will always produce clean water.",
                "It will create more rainfall.",
                0, Story2CorrectFeedback, Story2IncorrectFeedback,
                "Poor planning can affect water amount and safety.", StoryQuizDifficulty.Hard)
        },
        new[]
        {
            new StoryQuizQuestion(
                "How should a community look after a well?",
                "Keep it clean and protected.",
                "Throw rubbish nearby.",
                "Leave it broken.",
                0, Story2CorrectFeedback, Story2IncorrectFeedback,
                "Safe water sources need care.", StoryQuizDifficulty.Easy),
            new StoryQuizQuestion(
                "Why must groundwater be protected from pollution?",
                "Pollutants can seep into the ground and contaminate water.",
                "Pollution only affects water above ground.",
                "Underground water cannot be affected by people.",
                0, Story2CorrectFeedback, Story2IncorrectFeedback,
                "Pollution can move through soil.", StoryQuizDifficulty.Medium),
            new StoryQuizQuestion(
                "A new well has been built. What should happen before people drink its water?",
                "Have the water tested and ensure the well is properly maintained.",
                "Assume it is safe because it looks clear.",
                "Let anyone add chemicals to it.",
                0, Story2CorrectFeedback, Story2IncorrectFeedback,
                "Looking clear does not prove water is safe.", StoryQuizDifficulty.Hard)
        }
    };

    // Medium defaults are available for UI previews; runtime selection uses GetStory2Question.
    public static readonly StoryQuizQuestion[] Story2Questions =
    {
        Story2QuestionBank[0][1],
        Story2QuestionBank[1][1],
        Story2QuestionBank[2][1]
    };

    public static StoryQuizQuestion GetStory2Question(
        int questionIndex, bool question1Correct, bool question2Correct)
    {
        int safeIndex = Mathf.Clamp(questionIndex, 0, 2);
        StoryQuizDifficulty difficulty;

        if (safeIndex == 0)
        {
            difficulty = StoryQuizDifficulty.Medium;
        }
        else if (safeIndex == 1)
        {
            difficulty = question1Correct
                ? StoryQuizDifficulty.Hard
                : StoryQuizDifficulty.Medium;
        }
        else
        {
            int previousCorrect = (question1Correct ? 1 : 0) +
                                  (question2Correct ? 1 : 0);
            difficulty = previousCorrect == 2
                ? StoryQuizDifficulty.Hard
                : previousCorrect == 1
                    ? StoryQuizDifficulty.Medium
                    : StoryQuizDifficulty.Easy;
        }

        return Story2QuestionBank[safeIndex][(int)difficulty];
    }

    public static bool Story1Completed => PlayerPrefs.GetInt(Story1CompletedKey, 0) == 1;
    public static bool Story2Unlocked => PlayerPrefs.GetInt(Story2UnlockedKey, 0) == 1;
    public static bool Level1Unlocked => PlayerPrefs.GetInt(Level1UnlockedKey, 0) == 1;
    public static int Story1BestScore => PlayerPrefs.GetInt(Story1BestScoreKey, 0);

    public static void EnsureInitialized()
    {
        if (!PlayerPrefs.HasKey(Story1UnlockedKey))
            PlayerPrefs.SetInt(Story1UnlockedKey, 1);
        if (!PlayerPrefs.HasKey(Story1CompletedKey))
            PlayerPrefs.SetInt(Story1CompletedKey, 0);
        if (!PlayerPrefs.HasKey(Story2UnlockedKey))
            PlayerPrefs.SetInt(Story2UnlockedKey, 0);
        if (!PlayerPrefs.HasKey(Level1UnlockedKey))
            PlayerPrefs.SetInt(Level1UnlockedKey,
                Story1Completed || Story2Unlocked ? 1 : 0);

        PlayerPrefs.Save();
    }

    public static void SaveStory1QuizResult(int score)
    {
        int safeScore = Mathf.Clamp(score, 0, 3);
        PlayerPrefs.SetInt(Story1BestScoreKey,
            Mathf.Max(Story1BestScore, safeScore));

        if (safeScore >= 2)
        {
            PlayerPrefs.SetInt(Story1CompletedKey, 1);
            PlayerPrefs.SetInt(Level1UnlockedKey, 1);
        }

        if (safeScore == 3)
            PlayerPrefs.SetInt(Story2UnlockedKey, 1);

        PlayerPrefs.Save();
    }
}
