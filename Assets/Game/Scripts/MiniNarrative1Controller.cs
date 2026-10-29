using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Runs the first narrative and its water-safety quiz using the UI already
/// placed in MiniNarrative1. Narrative Water Drops are intentionally isolated
/// from every gameplay resource.
/// </summary>
public sealed class MiniNarrative1Controller : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterForSceneLoads()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureCurrentScene()
    {
        EnsureController(SceneManager.GetActiveScene());
    }

    static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureController(scene);
    }

    static void EnsureController(Scene scene)
    {
        if (scene.name != SceneCatalog.MiniNarrative1) return;
        if (FindFirstObjectByType<MiniNarrative1Controller>() != null) return;

        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
            canvas.AddComponent<MiniNarrative1Controller>();
        else
            Debug.LogError("[MiniNarrative1] Existing Canvas was not found. No replacement UI was created.");
    }

    const string WaterDropsKey = "DropByDrop.Narrative.WaterDrops";
    const string RewardClaimedKey = "DropByDrop.Narrative.MiniNarrative1RewardClaimed";
    const string Story1UnlockedKey = "DropByDrop.Narrative.Story1Unlocked";
    const string Story2UnlockedKey = "DropByDrop.Narrative.Story2Unlocked";
    const string Story3UnlockedKey = "DropByDrop.Narrative.Story3Unlocked";
    const float NarrativeNextDelaySeconds = 3f;

    enum Speaker
    {
        Zuri,
        Villagers
    }

    enum Flow
    {
        Opening,
        Quiz,
        Ending,
        Finished
    }

    readonly struct DialogueLine
    {
        public readonly Speaker Speaker;
        public readonly string Text;

        public DialogueLine(Speaker speaker, string text)
        {
            Speaker = speaker;
            Text = text;
        }
    }

    readonly struct QuizQuestion
    {
        public readonly string Prompt;
        public readonly string[] Answers;
        public readonly int CorrectAnswer;
        public readonly string CorrectRemark;
        public readonly string IncorrectRemark;

        public QuizQuestion(string prompt, string answer1, string answer2, string answer3,
            int correctAnswer, string correctRemark, string incorrectRemark)
        {
            Prompt = prompt;
            Answers = new[] { answer1, answer2, answer3 };
            CorrectAnswer = correctAnswer;
            CorrectRemark = correctRemark;
            IncorrectRemark = incorrectRemark;
        }
    }

    static readonly DialogueLine[] OpeningDialogue =
    {
        new DialogueLine(Speaker.Villagers,
            "Zuri, our clean water is running low. Please take the safe path to the collection point."),
        new DialogueLine(Speaker.Zuri,
            "I will! My bucket is ready, my shoes are tied, and I promise not to race any goats."),
        new DialogueLine(Speaker.Villagers,
            "Good. Watch the heat, avoid unsafe puddles, and bring the water home in a clean, covered container."),
        new DialogueLine(Speaker.Zuri,
            "Safe route, heat breaks, clean water, covered bucket. I have it!")
    };

    static readonly DialogueLine[] EndingDialogue =
    {
        new DialogueLine(Speaker.Zuri,
            "I am home! The water is safe, the container stayed covered, and the goats did not win."),
        new DialogueLine(Speaker.Villagers,
            "Well done, Zuri. Rest in shade when the heat makes you dizzy, and never drink water just because it looks clear."),
        new DialogueLine(Speaker.Villagers,
            "Keep collected water clean and covered. Protected wells must be built and repaired by trained adults."),
        new DialogueLine(Speaker.Zuri,
            "Then I will help by making safe choices and carrying hope one drop at a time!"),
        new DialogueLine(Speaker.Villagers,
            "The village still needs more water. Your journey begins now.")
    };

    static readonly QuizQuestion[] Questions =
    {
        new QuizQuestion(
            "Which route should Zuri take to the water collection point?",
            "The marked path where adults can see her",
            "A shortcut through tall grass",
            "The flooded path beside the puddles",
            0,
            "Zuri thinks this route feels safe and easy to follow.",
            "Zuri pauses. This route might hide danger."),
        new QuizQuestion(
            "Zuri feels dizzy during a heatwave. What should she do?",
            "Run faster so the trip ends sooner",
            "Stop in shade, rest, and tell an adult",
            "Ignore it and keep walking",
            1,
            "Zuri thinks taking care of herself is the wise choice.",
            "Zuri feels this choice could make the heat worse."),
        new QuizQuestion(
            "Why might clear-looking puddle water still be unsafe?",
            "It may contain germs or pollution that cannot be seen",
            "Clear water is always safe",
            "Only muddy water contains germs",
            0,
            "Zuri remembers that some dangers are too small to see.",
            "Zuri wonders if clear-looking water can still hide danger."),
        new QuizQuestion(
            "How should collected drinking water be stored?",
            "In an open bucket beside animals",
            "In any container, even if it is dirty",
            "In a clean container with a secure cover",
            2,
            "Zuri thinks this will keep the water protected.",
            "Zuri worries the water may not stay clean."),
        new QuizQuestion(
            "Zuri has 3 litres of water. How many 1-litre containers can she fill?",
            "2 containers",
            "3 containers",
            "4 containers",
            1,
            "Zuri counts carefully and smiles.",
            "Zuri thinks she should count the containers again.")
    };

    GameObject narrativePanel;
    GameObject questionPanel;
    GameObject otherCharacterImage;
    GameObject mainCharacterImage;
    TMP_Text narrativeTitle;
    TMP_Text zuriText;
    TMP_Text npcText;
    TMP_Text questionTitle;
    TMP_Text questionText;
    TMP_Text commentaryTitle;
    TMP_Text commentaryText;
    TMP_Text rewardText;
    Button narrativeNextButton;
    Button nextQuestionButton;
    readonly Button[] answerButtons = new Button[3];
    readonly Button[] storyButtons = new Button[3];

    Flow flow;
    int dialogueIndex;
    int questionIndex;
    int correctAnswers;
    bool answerSelected;
    int waterDrops;
    Coroutine narrativeButtonRoutine;

    void Awake()
    {
        if (!ResolveExistingUi())
        {
            enabled = false;
            return;
        }

        ConfigureListeners();
        LoadSavedNarrativeState();
        ApplyExistingFont();
        BeginStory();
    }

    bool ResolveExistingUi()
    {
        narrativePanel = FindObject("NarrativePanel");
        questionPanel = FindObject("QuestionPanel");
        otherCharacterImage = FindObject("NarrativePanel/OtherCharactersImage");
        mainCharacterImage = FindObject("NarrativePanel/MainCharacterImage (1)");

        narrativeTitle = FindComponent<TMP_Text>("NarrativePanel/TitleNarrative");
        npcText = FindComponent<TMP_Text>("NarrativePanel/OtherCharactersImage/NPCtext");
        zuriText = FindComponent<TMP_Text>("NarrativePanel/MainCharacterImage (1)/ZURItext");
        narrativeNextButton = FindComponent<Button>("NarrativePanel/NextButton");

        questionTitle = FindComponent<TMP_Text>("QuestionPanel/TitleQuestion");
        questionText = FindComponent<TMP_Text>("QuestionPanel/QuestionAnswer");
        commentaryTitle = FindComponent<TMP_Text>("QuestionPanel/CommentaryPanel/CommentaryTitle");
        commentaryText = FindComponent<TMP_Text>("QuestionPanel/CommentaryPanel/CommentaryText");
        nextQuestionButton = FindComponent<Button>("QuestionPanel/NextQuestionButton");
        rewardText = FindComponent<TMP_Text>("RewardsPanel/RewardText");

        answerButtons[0] = FindComponent<Button>("QuestionPanel/Answer1");
        answerButtons[1] = FindComponent<Button>("QuestionPanel/Answer2");
        answerButtons[2] = FindComponent<Button>("QuestionPanel/Answer3");
        storyButtons[0] = FindComponent<Button>("StoryPanel/Story1");
        storyButtons[1] = FindComponent<Button>("StoryPanel/Story2");
        storyButtons[2] = FindComponent<Button>("StoryPanel/Story3");

        bool complete = narrativePanel != null && questionPanel != null &&
                        otherCharacterImage != null && mainCharacterImage != null &&
                        narrativeTitle != null && npcText != null && zuriText != null &&
                        narrativeNextButton != null && nextQuestionButton != null &&
                        questionTitle != null && questionText != null &&
                        commentaryTitle != null && commentaryText != null &&
                        rewardText != null;

        for (int i = 0; i < answerButtons.Length; i++)
            complete &= answerButtons[i] != null;
        for (int i = 0; i < storyButtons.Length; i++)
            complete &= storyButtons[i] != null;

        if (!complete)
            Debug.LogError("[MiniNarrative1] Required existing UI references are missing. No replacement UI was created.");

        return complete;
    }

    GameObject FindObject(string relativePath)
    {
        Transform found = transform.Find(relativePath);
        if (found != null) return found.gameObject;

        Debug.LogError($"[MiniNarrative1] Missing existing UI object: Canvas/{relativePath}");
        return null;
    }

    T FindComponent<T>(string relativePath, bool required = true) where T : Component
    {
        Transform found = transform.Find(relativePath);
        T component = found != null ? found.GetComponent<T>() : null;
        if (required && component == null)
            Debug.LogError($"[MiniNarrative1] Missing {typeof(T).Name}: Canvas/{relativePath}");
        return component;
    }

    void ConfigureListeners()
    {
        narrativeNextButton.onClick.RemoveListener(AdvanceNarrative);
        narrativeNextButton.onClick.AddListener(AdvanceNarrative);
        nextQuestionButton.onClick.RemoveListener(HandleNextQuestion);
        nextQuestionButton.onClick.AddListener(HandleNextQuestion);

        answerButtons[0].onClick.RemoveListener(SelectAnswer1);
        answerButtons[0].onClick.AddListener(SelectAnswer1);
        answerButtons[1].onClick.RemoveListener(SelectAnswer2);
        answerButtons[1].onClick.AddListener(SelectAnswer2);
        answerButtons[2].onClick.RemoveListener(SelectAnswer3);
        answerButtons[2].onClick.AddListener(SelectAnswer3);

        storyButtons[0].onClick.RemoveListener(BeginStory);
        storyButtons[0].onClick.AddListener(BeginStory);
    }

    void LoadSavedNarrativeState()
    {
        waterDrops = Mathf.Max(0, PlayerPrefs.GetInt(WaterDropsKey, 0));
        if (!PlayerPrefs.HasKey(Story1UnlockedKey))
            PlayerPrefs.SetInt(Story1UnlockedKey, 1);
        if (!PlayerPrefs.HasKey(Story2UnlockedKey))
            PlayerPrefs.SetInt(Story2UnlockedKey, 0);
        if (!PlayerPrefs.HasKey(Story3UnlockedKey))
            PlayerPrefs.SetInt(Story3UnlockedKey, 0);

        bool story1Unlocked = PlayerPrefs.GetInt(Story1UnlockedKey, 1) == 1;
        bool story2Unlocked = PlayerPrefs.GetInt(Story2UnlockedKey, 0) == 1;
        bool story3Unlocked = PlayerPrefs.GetInt(Story3UnlockedKey, 0) == 1;

        storyButtons[0].interactable = story1Unlocked;
        storyButtons[1].interactable = story2Unlocked;
        storyButtons[2].interactable = story3Unlocked;

        rewardText.text = waterDrops.ToString();
        PlayerPrefs.Save();
    }

    void ApplyExistingFont()
    {
        TMP_FontAsset font = questionText.font != null ? questionText.font : narrativeTitle.font;
        if (font == null) return;

        narrativeTitle.font = font;
        zuriText.font = font;
        npcText.font = font;
        questionTitle.font = font;
        questionText.font = font;
        commentaryTitle.font = font;
        commentaryText.font = font;
        rewardText.font = font;
    }

    void BeginStory()
    {
        flow = Flow.Opening;
        dialogueIndex = 0;
        questionIndex = 0;
        correctAnswers = 0;
        answerSelected = false;
        ShowNarrative(OpeningDialogue[dialogueIndex]);
    }

    void AdvanceNarrative()
    {
        if (!narrativeNextButton.interactable) return;
        narrativeNextButton.interactable = false;

        switch (flow)
        {
            case Flow.Opening:
                dialogueIndex++;
                if (dialogueIndex < OpeningDialogue.Length)
                    ShowNarrative(OpeningDialogue[dialogueIndex]);
                else
                    BeginQuiz();
                break;

            case Flow.Ending:
                dialogueIndex++;
                if (dialogueIndex < EndingDialogue.Length)
                    ShowNarrative(EndingDialogue[dialogueIndex]);
                else
                    LoadLevelOne();
                break;
        }
    }

    void HandleNextQuestion()
    {
        if (flow != Flow.Quiz || !answerSelected) return;

        if (questionIndex < Questions.Length - 1)
        {
            questionIndex++;
            ShowQuestion();
        }
        else
        {
            FinishQuiz();
        }
    }

    void ShowNarrative(DialogueLine line)
    {
        questionPanel.SetActive(false);
        narrativePanel.SetActive(true);
        narrativeTitle.gameObject.SetActive(true);
        otherCharacterImage.SetActive(true);
        mainCharacterImage.SetActive(true);
        narrativeNextButton.gameObject.SetActive(true);
        narrativeNextButton.interactable = false;

        narrativeTitle.text = "DROP BY DROP: FLOW OF HOPE";
        zuriText.text = line.Speaker == Speaker.Zuri ? $"Zuri:\n{line.Text}" : string.Empty;
        npcText.text = line.Speaker == Speaker.Villagers ? $"Villagers:\n{line.Text}" : string.Empty;
        RestartNarrativeButtonTimer();
    }

    void RestartNarrativeButtonTimer()
    {
        if (narrativeButtonRoutine != null)
            StopCoroutine(narrativeButtonRoutine);
        narrativeButtonRoutine = StartCoroutine(EnableNarrativeButtonAfterDelay());
    }

    IEnumerator EnableNarrativeButtonAfterDelay()
    {
        yield return new WaitForSecondsRealtime(NarrativeNextDelaySeconds);
        narrativeButtonRoutine = null;

        if (flow != Flow.Opening && flow != Flow.Ending) yield break;
        narrativeNextButton.interactable = true;
        narrativeNextButton.Select();
    }

    void BeginQuiz()
    {
        flow = Flow.Quiz;
        questionIndex = 0;
        ShowQuestion();
    }

    void ShowQuestion()
    {
        if (narrativeButtonRoutine != null)
        {
            StopCoroutine(narrativeButtonRoutine);
            narrativeButtonRoutine = null;
        }

        narrativePanel.SetActive(false);
        questionPanel.SetActive(true);
        questionTitle.text = $"QUESTION {questionIndex + 1} OF {Questions.Length}";

        QuizQuestion question = Questions[questionIndex];
        questionText.text =
            $"{question.Prompt}\n\n" +
            $"1. {question.Answers[0]}\n\n" +
            $"2. {question.Answers[1]}\n\n" +
            $"3. {question.Answers[2]}";

        commentaryTitle.text = "CHOOSE ONE ANSWER";
        commentaryText.text = string.Empty;
        answerSelected = false;
        SetAnswerButtonsInteractable(true);
        nextQuestionButton.interactable = false;
    }

    void SelectAnswer1() => SelectAnswer(0);
    void SelectAnswer2() => SelectAnswer(1);
    void SelectAnswer3() => SelectAnswer(2);

    void SelectAnswer(int selectedAnswer)
    {
        if (flow != Flow.Quiz || answerSelected) return;

        answerSelected = true;
        QuizQuestion question = Questions[questionIndex];
        bool correct = selectedAnswer == question.CorrectAnswer;
        if (correct) correctAnswers++;

        commentaryTitle.text = correct ? "ZURI FEELS CONFIDENT" : "ZURI IS THINKING";
        commentaryText.text = correct ? question.CorrectRemark : question.IncorrectRemark;

        SetAnswerButtonsInteractable(false);
        nextQuestionButton.interactable = true;
    }

    void SetAnswerButtonsInteractable(bool interactable)
    {
        for (int i = 0; i < answerButtons.Length; i++)
            answerButtons[i].interactable = interactable;
    }

    void FinishQuiz()
    {
        int earned = 0;
        bool alreadyClaimed = PlayerPrefs.GetInt(RewardClaimedKey, 0) == 1;

        if (!alreadyClaimed)
        {
            if (correctAnswers <= 1)
                earned = Random.Range(1, 4);
            else if (correctAnswers <= 3)
                earned = Random.Range(4, 7);
            else
                earned = Random.Range(7, 11);

            waterDrops += earned;
            PlayerPrefs.SetInt(WaterDropsKey, waterDrops);
            PlayerPrefs.SetInt(RewardClaimedKey, 1);
            PlayerPrefs.Save();
        }

        rewardText.text = waterDrops.ToString();
        flow = Flow.Ending;
        dialogueIndex = 0;
        ShowNarrative(EndingDialogue[dialogueIndex]);

        string result = alreadyClaimed
            ? $"Quiz complete: {correctAnswers}/5 correct. Your earlier story reward is already saved."
            : $"Quiz complete: {correctAnswers}/5 correct. You earned {earned} Water Drops!";
        zuriText.text = $"Zuri:\n{result}\n\n{EndingDialogue[0].Text}";
        npcText.text = string.Empty;
    }

    void LoadLevelOne()
    {
        if (flow == Flow.Finished) return;
        flow = Flow.Finished;
        SceneManager.LoadScene(SceneCatalog.MainGame);
    }
}
