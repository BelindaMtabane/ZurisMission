using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Runs Story 3 and Story 4 using the existing UI placed in MiniNarrative2.
/// Narrative progression is intentionally isolated
/// from every gameplay resource.
/// </summary>
public sealed class NarrativeScene2Controller : MonoBehaviour
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
        if (scene.name != SceneCatalog.MiniNarrative2) return;
        if (FindFirstObjectByType<NarrativeScene2Controller>() != null) return;

        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
            canvas.AddComponent<NarrativeScene2Controller>();
        else
            Debug.LogError("[MiniNarrative2] Existing Canvas was not found. No replacement UI was created.");
    }

    const int QuizQuestionCount = 3;
    const float NarrativeNextDelaySeconds = 3f;

    enum Flow
    {
        Opening,
        Quiz,
        Result,
        Ending,
        Finished
    }

    enum ResultAction
    {
        RetryStory3Quiz,
        RetryStory4Quiz,
        ReturnToStartAfterEnding,
        ContinueStory4Ending
    }

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
    TMP_Text storyTitle;
    TMP_Text skipButtonText;
    string authoredSkipLabel;
    Button narrativeNextButton;
    Button nextQuestionButton;
    Button skipButton;
    readonly Button[] answerButtons = new Button[3];
    readonly Button[] storyButtons = new Button[2];
    readonly TMP_Text[] storyButtonLabels = new TMP_Text[2];
    readonly bool[] questionAttempted = new bool[QuizQuestionCount];
    readonly bool[] submittedCorrect = new bool[QuizQuestionCount];

    Flow flow;
    ResultAction resultAction;
    StoryQuizQuestion activeQuestion;
    int dialogueIndex;
    int questionIndex;
    int correctAnswers;
    bool answerSelected;
    bool playingStory4;
    bool allowNarrativeSkip;
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
        ApplyScene1DialoguePresentation();
        BeginStory3();
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
        storyTitle = FindComponent<TMP_Text>("StoryPanel/StoryTitle");
        skipButton = FindComponent<Button>("StoryPanel/SkipButton");
        skipButtonText = skipButton != null ? skipButton.GetComponentInChildren<TMP_Text>(true) : null;
        authoredSkipLabel = skipButtonText != null && !string.IsNullOrWhiteSpace(skipButtonText.text)
            ? skipButtonText.text
            : "SKIP STORY 4";

        answerButtons[0] = FindComponent<Button>("QuestionPanel/Answer1");
        answerButtons[1] = FindComponent<Button>("QuestionPanel/Answer2");
        answerButtons[2] = FindComponent<Button>("QuestionPanel/Answer3");
        storyButtons[0] = FindComponent<Button>("StoryPanel/Story3");
        storyButtons[1] = FindComponent<Button>("StoryPanel/Story4");
        for (int i = 0; i < storyButtons.Length; i++)
            storyButtonLabels[i] = storyButtons[i] != null
                ? storyButtons[i].GetComponentInChildren<TMP_Text>(true)
                : null;

        bool complete = narrativePanel != null && questionPanel != null &&
                        otherCharacterImage != null && mainCharacterImage != null &&
                        narrativeTitle != null && npcText != null && zuriText != null &&
                        narrativeNextButton != null && nextQuestionButton != null &&
                        questionTitle != null && questionText != null &&
                        commentaryTitle != null && commentaryText != null &&
                        storyTitle != null && skipButton != null && skipButtonText != null;

        for (int i = 0; i < answerButtons.Length; i++)
            complete &= answerButtons[i] != null;
        for (int i = 0; i < storyButtons.Length; i++)
            complete &= storyButtons[i] != null && storyButtonLabels[i] != null;

        if (!complete)
            Debug.LogError("[MiniNarrative2] Required existing UI references are missing. No replacement UI was created.");

        return complete;
    }

    GameObject FindObject(string relativePath)
    {
        Transform found = transform.Find(relativePath);
        if (found != null) return found.gameObject;

        Debug.LogError($"[MiniNarrative2] Missing existing UI object: Canvas/{relativePath}");
        return null;
    }

    T FindComponent<T>(string relativePath, bool required = true) where T : Component
    {
        Transform found = transform.Find(relativePath);
        T component = found != null ? found.GetComponent<T>() : null;
        if (required && component == null)
            Debug.LogError($"[MiniNarrative2] Missing {typeof(T).Name}: Canvas/{relativePath}");
        return component;
    }

    void ConfigureListeners()
    {
        narrativeNextButton.onClick.RemoveListener(AdvanceNarrative);
        narrativeNextButton.onClick.AddListener(AdvanceNarrative);
        nextQuestionButton.onClick.RemoveListener(HandleNextQuestion);
        nextQuestionButton.onClick.AddListener(HandleNextQuestion);
        skipButton.onClick.RemoveAllListeners();
        skipButton.onClick.AddListener(HandleSkipPressed);

        answerButtons[0].onClick.RemoveListener(SelectAnswer1);
        answerButtons[0].onClick.AddListener(SelectAnswer1);
        answerButtons[1].onClick.RemoveListener(SelectAnswer2);
        answerButtons[1].onClick.AddListener(SelectAnswer2);
        answerButtons[2].onClick.RemoveListener(SelectAnswer3);
        answerButtons[2].onClick.AddListener(SelectAnswer3);

        storyButtons[0].onClick.RemoveListener(BeginStory3);
        storyButtons[0].onClick.AddListener(BeginStory3);
        storyButtons[1].onClick.RemoveListener(SelectStory4);
        storyButtons[1].onClick.AddListener(SelectStory4);
    }

    void LoadSavedNarrativeState()
    {
        StoryNarrativeData2.EnsureInitialized();
        RefreshStoryUi();
    }

    void RefreshStoryUi(string message = null)
    {
        if (!string.IsNullOrEmpty(message) && storyTitle != null)
            storyTitle.text = message;

        if (storyButtons[0] != null)
            storyButtons[0].interactable = true;
        if (storyButtons[1] != null)
            storyButtons[1].interactable = StoryNarrativeData2.Story4Unlocked;
    }


    void ApplyScene1DialoguePresentation()
    {
        ApplyScene1TextOverflow(zuriText);
        ApplyScene1TextOverflow(npcText);
        ApplyScene1TextOverflow(narrativeTitle);
        ApplyScene1TextOverflow(questionTitle);
        ApplyScene1TextOverflow(questionText);
        ApplyScene1TextOverflow(commentaryTitle);
        ApplyScene1TextOverflow(commentaryText);
        ApplyScene1TextOverflow(skipButtonText);
        ApplyScene1TextOverflow(storyTitle);
    }

    static void ApplyScene1TextOverflow(TMP_Text text)
    {
        if (text == null) return;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        text.enableAutoSizing = false;
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
        skipButtonText.font = font;
    }

    void BeginStory3()
    {
        playingStory4 = false;
        BeginSelectedStory();
    }

    void BeginStory4()
    {
        playingStory4 = true;
        BeginSelectedStory();
    }

    void BeginSelectedStory()
    {
        RefreshStoryUi();
        flow = Flow.Opening;
        dialogueIndex = 0;
        questionIndex = 0;
        correctAnswers = 0;
        answerSelected = false;
        for (int i = 0; i < questionAttempted.Length; i++)
        {
            questionAttempted[i] = false;
            submittedCorrect[i] = false;
        }
        allowNarrativeSkip = CurrentStoryAlreadyRead();
        if (CurrentOpening.Length == 0)
        {
            ShowUnavailableStoryMessage();
            return;
        }

        ShowNarrative(CurrentOpening[dialogueIndex]);
    }

    void SelectStory4()
    {
        if (StoryNarrativeData2.Story4Unlocked)
        {
            BeginStory4();
            return;
        }

        RefreshStoryUi("Complete Story 3 perfectly to unlock Story 4");
    }

    StoryDialogueLine[] CurrentOpening =>
        playingStory4 ? StoryNarrativeData2.Story4Opening : StoryNarrativeData2.Story3Opening;

    StoryDialogueLine[] CurrentEnding =>
        playingStory4 ? StoryNarrativeData2.Story4Ending : StoryNarrativeData2.Story3Ending;

    void ShowUnavailableStoryMessage()
    {
        flow = Flow.Finished;
        narrativePanel.SetActive(false);
        questionPanel.SetActive(false);
        skipButton.gameObject.SetActive(false);
        RefreshStoryUi(playingStory4
            ? "Story 4 content has not been added yet"
            : "Story 3 content has not been added yet");
    }

    void AdvanceNarrative()
    {
        if (!narrativeNextButton.interactable) return;
        narrativeNextButton.interactable = false;

        switch (flow)
        {
            case Flow.Opening:
                dialogueIndex++;
                if (dialogueIndex < CurrentOpening.Length)
                    ShowNarrative(CurrentOpening[dialogueIndex]);
                else
                    BeginQuiz();
                break;

            case Flow.Ending:
                dialogueIndex++;
                if (dialogueIndex < CurrentEnding.Length)
                    ShowNarrative(CurrentEnding[dialogueIndex]);
                else
                    LoadLevelTwo();
                break;
        }
    }

    void HandleNextQuestion()
    {
        if (flow != Flow.Quiz || !answerSelected) return;

        if (questionIndex < QuizQuestionCount - 1)
        {
            questionIndex++;
            ShowQuestion();
        }
        else
        {
            FinishQuiz();
        }
    }

    void ShowNarrative(StoryDialogueLine line)
    {
        questionPanel.SetActive(false);
        narrativePanel.SetActive(true);
        narrativeTitle.gameObject.SetActive(true);
        narrativeNextButton.gameObject.SetActive(true);
        narrativeNextButton.interactable = false;
        RefreshNarrativeSkipButton();

        if (!playingStory4 && flow == Flow.Opening && dialogueIndex == CurrentOpening.Length - 1)
            narrativeTitle.text = StoryNarrativeData2.Story3MissionTitle;
        else if (!playingStory4 && flow == Flow.Ending && dialogueIndex == CurrentEnding.Length - 1)
            narrativeTitle.text = StoryNarrativeData2.Story3CompleteTitle;
        else if (flow == Flow.Opening && dialogueIndex == CurrentOpening.Length - 1)
            narrativeTitle.text = StoryNarrativeData2.Story4MissionTitle;
        else if (flow == Flow.Ending && dialogueIndex == CurrentEnding.Length - 1)
            narrativeTitle.text = StoryNarrativeData2.Story4CompleteTitle;
        else
            narrativeTitle.text = playingStory4
                ? StoryNarrativeData2.Story4Title
                : StoryNarrativeData2.Story3Title;

        if (line.Speaker == StorySpeaker.Zuri)
        {
            mainCharacterImage.SetActive(true);
            otherCharacterImage.SetActive(false);
            zuriText.text = $"Zuri:\n{line.Text}";
            npcText.text = string.Empty;
        }
        else if (line.Speaker == StorySpeaker.Naledi ||
                 line.Speaker == StorySpeaker.Villager1 ||
                 line.Speaker == StorySpeaker.Villager2)
        {
            mainCharacterImage.SetActive(false);
            otherCharacterImage.SetActive(true);
            zuriText.text = string.Empty;
            string speakerName = line.Speaker == StorySpeaker.Naledi ? "Naledi"
                : line.Speaker == StorySpeaker.Villager1 ? "Villager 1"
                : "Villager 2";
            npcText.text = $"{speakerName}:\n{line.Text}";
        }
        else
        {
            mainCharacterImage.SetActive(true);
            otherCharacterImage.SetActive(false);
            zuriText.text = line.Speaker == StorySpeaker.StageDirection
                ? $"<i>{line.Text}</i>"
                : line.Text;
            npcText.text = string.Empty;
        }
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
        bool hasContent = playingStory4
            ? StoryNarrativeData2.HasStory4Content
            : StoryNarrativeData2.HasStory3Content;
        if (!hasContent)
        {
            ShowUnavailableStoryMessage();
            return;
        }

        if (playingStory4)
            StoryNarrativeData2.MarkStory4Read();
        else
            StoryNarrativeData2.MarkStory3Read();

        flow = Flow.Quiz;
        questionIndex = 0;
        ShowQuestion();
    }

    void RefreshNarrativeSkipButton()
    {
        bool show = CurrentStoryAlreadyRead() && (flow == Flow.Opening || flow == Flow.Ending);
        skipButton.gameObject.SetActive(show);
        skipButton.interactable = show;
        if (show)
            skipButtonText.text = "SKIP";
    }

    void StopNarrativeButtonTimer()
    {
        if (narrativeButtonRoutine == null) return;
        StopCoroutine(narrativeButtonRoutine);
        narrativeButtonRoutine = null;
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
        skipButton.gameObject.SetActive(false);
        nextQuestionButton.gameObject.SetActive(true);
        for (int i = 0; i < answerButtons.Length; i++)
            answerButtons[i].gameObject.SetActive(true);
        activeQuestion = playingStory4
            ? StoryNarrativeData2.GetStory4Question(
                questionIndex, submittedCorrect[0], submittedCorrect[1])
            : StoryNarrativeData2.Story3Questions[questionIndex];
        questionTitle.text =
            $"QUESTION {questionIndex + 1} OF {QuizQuestionCount} — " +
            $"{activeQuestion.Difficulty.ToString().ToUpperInvariant()}";

        questionText.text =
            $"{activeQuestion.Prompt}\n\n" +
            $"A. {activeQuestion.Answers[0]}\n" +
            $"B. {activeQuestion.Answers[1]}\n" +
            $"C. {activeQuestion.Answers[2]}";

        commentaryTitle.text = "CHOOSE ONE ANSWER";
        commentaryText.text = string.IsNullOrEmpty(activeQuestion.Hint)
            ? string.Empty
            : $"Hint: {activeQuestion.Hint}";
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

        bool correct = selectedAnswer == activeQuestion.CorrectAnswer;
        bool firstAttempt = !questionAttempted[questionIndex];
        questionAttempted[questionIndex] = true;
        if (firstAttempt)
            submittedCorrect[questionIndex] = correct;

        if (correct)
        {
            answerSelected = true;
            if (firstAttempt) correctAnswers++;
            commentaryTitle.text = "ZURI FEELS CONFIDENT";
            commentaryText.text = activeQuestion.CorrectRemark;
            SetAnswerButtonsInteractable(false);
            nextQuestionButton.interactable = true;
        }
        else
        {
            commentaryTitle.text = "ZURI IS THINKING";
            commentaryText.text = activeQuestion.IncorrectRemark;
            SetAnswerButtonsInteractable(true);
            nextQuestionButton.interactable = false;
        }
    }

    void SetAnswerButtonsInteractable(bool interactable)
    {
        for (int i = 0; i < answerButtons.Length; i++)
            answerButtons[i].interactable = interactable;
    }

    void FinishQuiz()
    {
        if (playingStory4)
        {
            StoryNarrativeData2.SaveStory4QuizResult(correctAnswers);
            if (correctAnswers < 2)
            {
                RefreshStoryUi("Keep investigating!");
                ShowStory4FailureResult();
                return;
            }

            RefreshStoryUi("Story 4 complete!");
            ShowStory4PassedResult();
            return;
        }

        StoryNarrativeData2.SaveStory3QuizResult(correctAnswers);

        if (correctAnswers < 2)
        {
            RefreshStoryUi("Keep investigating!");
            ShowFailureResult();
            return;
        }

        if (correctAnswers == 2)
        {
            RefreshStoryUi("Story 3 complete!");
            ShowStory3PassedResult(false);
            return;
        }

        RefreshStoryUi("Story 4 unlocked!");
        ShowStory3PassedResult(true);
    }

    void ShowFailureResult()
    {
        resultAction = ResultAction.RetryStory3Quiz;
        flow = Flow.Result;
        narrativePanel.SetActive(false);
        questionPanel.SetActive(true);
        questionTitle.text = $"FIRST-TRY SCORE: {correctAnswers} OF 3";
        questionText.text =
            "Not quite, Water Detective! Every good detective learns from clues. Let's try again!\n\n" +
            "STORY 4 — LOCKED";
        commentaryTitle.text = "TRY THE QUIZ AGAIN";
        commentaryText.text = "You need at least 2 out of 3 correct answers.";
        PrepareResultButtons("RETRY QUIZ");
    }

    void ShowStory3PassedResult(bool story4Unlocked)
    {
        resultAction = ResultAction.ReturnToStartAfterEnding;
        flow = Flow.Result;
        narrativePanel.SetActive(false);
        questionPanel.SetActive(true);
        questionTitle.text = $"CONGRATULATIONS — {correctAnswers} OUT OF 3!";
        questionText.text = story4Unlocked
            ? "You unlocked Story 4!\n\n" +
              "Select the Story 4 button to continue, or play Level 2."
            : "You completed Story 3!\n\nStory 4 needs a perfect score.";
        commentaryTitle.text = story4Unlocked
            ? "STORY 4 UNLOCKED"
            : "STORY 3 COMPLETE";
        commentaryText.text = story4Unlocked
            ? "Story 4 will remain unlocked if you skip it."
            : "Continue through the ending, then play Level 2.";
        PrepareResultButtons(story4Unlocked
            ? authoredSkipLabel
            : "PLAY LEVEL 2");
    }

    void ShowStory4FailureResult()
    {
        resultAction = ResultAction.RetryStory4Quiz;
        flow = Flow.Result;
        narrativePanel.SetActive(false);
        questionPanel.SetActive(true);
        questionTitle.text = $"FIRST-TRY SCORE: {correctAnswers} OF 3";
        questionText.text =
            "Not quite, Water Detective! Every good detective learns from clues. Let's try again!";
        commentaryTitle.text = "TRY THE QUIZ AGAIN";
        commentaryText.text = "You need at least 2 out of 3 correct answers.";
        PrepareResultButtons("RETRY QUIZ");
    }

    void ShowStory4PassedResult()
    {
        resultAction = ResultAction.ContinueStory4Ending;
        flow = Flow.Result;
        narrativePanel.SetActive(false);
        questionPanel.SetActive(true);
        questionTitle.text = $"CONGRATULATIONS — {correctAnswers} OUT OF 3!";
        questionText.text =
            "You completed Story 4!\n\nContinue to the garden ending, then play Level 2.";
        commentaryTitle.text = "STORY 4 COMPLETE";
        commentaryText.text = "Continue through the ending, then play Level 2.";
        PrepareResultButtons("CONTINUE TO ENDING");
    }

    void PrepareResultButtons(string skipLabel)
    {
        nextQuestionButton.gameObject.SetActive(false);
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].gameObject.SetActive(true);
            answerButtons[i].interactable = false;
        }

        skipButtonText.text = skipLabel;
        skipButton.gameObject.SetActive(true);
        skipButton.interactable = true;
        skipButton.Select();
    }

    bool CurrentStoryAlreadyRead()
    {
        return playingStory4
            ? StoryNarrativeData2.HasReadStory4
            : StoryNarrativeData2.HasReadStory3;
    }

    void HandleSkipPressed()
    {
        if (flow == Flow.Finished) return;

        if (flow != Flow.Result && CurrentStoryAlreadyRead())
        {
            LoadLevelTwo();
            return;
        }

        if (flow != Flow.Result) return;
        skipButton.interactable = false;
        skipButton.gameObject.SetActive(false);

        if (resultAction == ResultAction.RetryStory3Quiz ||
            resultAction == ResultAction.RetryStory4Quiz)
            RestartQuiz();
        else if (resultAction == ResultAction.ReturnToStartAfterEnding)
            LoadLevelTwo();
        else
            BeginEnding();
    }

    void RestartQuiz()
    {
        correctAnswers = 0;
        questionIndex = 0;
        answerSelected = false;
        for (int i = 0; i < questionAttempted.Length; i++)
        {
            questionAttempted[i] = false;
            submittedCorrect[i] = false;
        }
        BeginQuiz();
    }

    void BeginEnding()
    {
        flow = Flow.Ending;
        dialogueIndex = 0;
        ShowNarrative(CurrentEnding[dialogueIndex]);
    }

    void LoadLevelTwo()
    {
        if (flow == Flow.Finished) return;

        StopNarrativeButtonTimer();
        flow = Flow.Finished;
        skipButton.interactable = false;
        skipButton.gameObject.SetActive(false);
        Time.timeScale = 1f;
        SceneLoadOverlay.Load(SceneCatalog.Level2);
    }
}
