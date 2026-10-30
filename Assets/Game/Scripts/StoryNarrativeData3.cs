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
            "WHOOOOA! Look at this place! It's so green! There are trees everywhere! I think I've walked into a giant salad!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Haha! Welcome to the forest, Zuri. Quite different from the village, isn't it?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Different? This place has more water than my entire village! Look at that stream! Look at those trees! Even the plants look like they're having a party!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "It is beautiful, but seeing water everywhere doesn't mean everyone has safe drinking water."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Wait... what? But that stream looks so clean! I can practically see my reflection!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Natural water can contain germs, dirt and other harmful things that we cannot always see."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So the water could be hiding tiny troublemakers? Even if it looks like a sparkling swimming pool?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Exactly! Clear-looking water is not automatically safe to drink."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Well, that's sneaky! My detective eyes are going to need extra training!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Haha! That's why natural water sources must be protected, and water must be properly treated and tested before drinking."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Understood! No mystery sips from the forest stream!"),
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "Zuri and Kaelo notice how trees, plants, soil and water are connected."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Kaelo, why are there so many trees and plants here? It's like the whole forest is growing at once!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Healthy forests help protect the environment. Trees and plants help the soil absorb and hold water."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So the trees are secretly working as water protectors? I knew they were more than just giant umbrellas!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's one way to think about it! Trees, soil, rivers and streams are all part of an ecosystem."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "An eco-what-now?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "An ecosystem! It's where living things and their environment work together."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Oh! So the trees, plants, animals, soil and water are all part of one big team?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Exactly! They depend on one another."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then what happens if someone cuts down too many trees or throws rubbish into the streams?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "It can damage the environment, pollute water sources and harm the living things that depend on them."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Oh no! That's like breaking the whole team's equipment before the game even starts!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Haha! That's why we must protect the forest and keep its water sources clean."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then Forest Detective Zuri is officially on duty!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Kaelo! Emergency! I've spotted something suspicious beside the stream!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "What have you found?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "RUBBISH! Plastic bottles, wrappers... Who brought their messy picnic and forgot to clean up?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's a serious problem. Rain can wash rubbish and pollution from the ground into rivers and streams."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Wait! So rubbish doesn't just sit there looking ugly? It can travel all the way into the water?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Exactly! It can pollute the water and harm animals, plants and people."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "That's terrible! The forest is not a giant rubbish bin!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "You're right. We should dispose of waste properly and keep rubbish away from natural water sources."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "We should protect the trees, keep the forest clean and look after the streams!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Wonderful! But remember, even when we protect a water source, we still need to make sure its water is safe before drinking."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Got it! Protect nature, prevent pollution and never trust a mystery drink!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Haha! You've discovered the forest's secret, Zuri. Healthy ecosystems help protect the water that life depends on."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then let's become the greatest Forest Water Protectors this forest has ever seen!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "PROTECT THE FOREST!")
    };

    public static readonly StoryDialogueLine[] Story5Ending =
    {
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Kaelo, I think I've discovered the forest's biggest secret!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "And what would that be, Zuri?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "The trees are water protectors, the soil helps hold water, and the streams are NOT secret drinking fountains!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Haha! You've learned quite a lot!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I never realised how much the forest does to help protect our water!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Healthy trees, plants and soil all help support natural water systems."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And if people throw rubbish into the forest or cut down too many trees, they can damage the whole environment!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's right! Pollution can reach rivers and streams and harm the living things that depend on them."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So we must keep the forest clean and protect every stream!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Exactly! But there's one more important thing to remember."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Let me guess... no eating mysterious forest berries?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That too! But I was thinking about drinking water. River and stream water isn't automatically safe, even when it looks clear."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Oh yes! Tiny germs and harmful substances can hide where our eyes can't see!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Correct! Natural water must be properly treated and tested before people drink it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I understand now! Just because water looks clean doesn't mean it is safe!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "You've become a very clever Forest Water Protector."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Thank you! I think I deserve a badge... maybe a leaf-shaped one!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Haha! I think we can arrange that!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "But seriously, I'll do my part to protect every stream, every tree and every precious drop!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's the spirit! The forest is lucky to have a Water Detective like you."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Come on, Kaelo! Our adventure isn't over yet. There's still more to discover!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "LEARNING TAKEAWAY: HEALTHY ECOSYSTEMS HELP PROTECT OUR WATER SOURCES!")
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
            "Kaelo! Stop right there! My detective senses are tingling!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Your detective senses? What have you discovered now?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Look at the stream! Something is wrong. The water doesn't look the same as before!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "You're right, Zuri. It looks like this water source might have been polluted."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Polluted?! Does that mean someone has been playing dirty tricks on our water?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Not exactly. Water pollution happens when harmful substances or waste enter water and make it unsafe for people, animals and plants."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Oh no! So the water has been attacked by invisible troublemakers?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Haha! You could say that. Pollution isn't always easy to see."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then this is a serious case! Detective Zuri is opening an investigation!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Good thinking. But first, we need to find out what might have caused the pollution."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Aha! A mystery! I knew my detective hat would come in handy!"),
        new StoryDialogueLine(StorySpeaker.StageDirection,
            "They investigate and notice more signs of pollution."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Look at all this rubbish! Plastic bottles, food wrappers and other litter! Who made this mess?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "It may have been left by people who didn't dispose of their waste properly."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "This place is not a rubbish bin! Did nobody teach them how to clean up after themselves?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Unfortunately, rubbish can cause bigger problems than just making a place look untidy."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "How? Can rubbish move by itself?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Not exactly! When it rains, water can wash rubbish and harmful substances from the ground into streams and rivers."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "WAIT! So the rain can carry all this rubbish straight into the water?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's right. And some pollution can be invisible, which means water might look clean but still be unsafe."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "That's sneaky! My eyes can't even catch all the clues!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Exactly. Pollution can affect water quality even when we cannot see it."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And what happens to the animals that depend on this water?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "They can become sick, and pollution can also harm plants and people."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "That's it! We have to protect this stream before the pollution causes more damage!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "We can help prevent pollution by disposing of rubbish properly and keeping harmful substances away from rivers and streams."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So no throwing bottles, wrappers or mystery liquids into the water?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Exactly! We should also make sure rubbish doesn't block drains and get washed into water sources."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I understand! Even rubbish left far away from a river can become a problem when it rains!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's right. Communities must work together to prevent pollution and protect water quality."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "And what if the water looks clean again? Can we drink it?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "No, Zuri. We must never assume polluted or potentially contaminated water is safe just because it looks clear. It needs to be assessed and treated appropriately before it can be used for drinking."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Got it! No mystery drinking, no mystery chemicals and definitely no mystery rubbish!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Haha! You've learned an important lesson."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then let's solve this Pollution Puzzle, protect the stream and help keep the forest healthy!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's the spirit, Detective Zuri!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "SOLVE THE POLLUTION PUZZLE!")
    };

    public static readonly StoryDialogueLine[] Story6Ending =
    {
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Kaelo! Case closed! I think we've finally solved the Pollution Puzzle!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Excellent work, Zuri! What have you discovered?"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "That rubbish doesn't magically disappear! It can travel through rainwater and end up in rivers and streams!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's right. And harmful substances can contaminate water even when we cannot see them."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I never realised that a little wrapper left on the ground could cause such a big problem!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's why we must dispose of waste properly and keep pollution away from natural water sources."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "From now on, my detective eyes will be watching for litter everywhere!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Haha! Just remember, you don't need to investigate every piece of rubbish by yourself."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "I know! We should work together and ask responsible adults to help when something is dangerous."),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Exactly! Never touch unknown chemicals, dangerous waste or potentially contaminated water."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "So if we find something suspicious, we report it instead of trying to be a superhero?"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "That's a very smart detective decision!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Good! I prefer being a detective with all my fingers still attached!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Haha! And remember, if water may be polluted, we must not assume it is safe to drink just because it looks clear."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "It needs proper assessment and treatment before people can safely drink it!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "You've got it! Preventing pollution helps protect people, animals, plants and the environment."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Then let's make a promise to keep our forests clean and protect every water source!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Agreed! A healthy environment means healthier water for everyone."),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Another mystery solved! The Pollution Puzzle didn't stand a chance against Detective Zuri and her very clever assistant!"),
        new StoryDialogueLine(StorySpeaker.Kaelo,
            "Assistant?! I thought we were partners!"),
        new StoryDialogueLine(StorySpeaker.Zuri,
            "Fine, fine! Detective Zuri and Detective Kaelo! The greatest forest investigation team ever!"),
        new StoryDialogueLine(StorySpeaker.TitleCard,
            "LEARNING TAKEAWAY: PREVENT POLLUTION. PROTECT OUR WATER!")
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
