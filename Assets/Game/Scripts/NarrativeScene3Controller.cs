using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Runs Story 5 and Story 6 using the existing UI placed in MiniNarrative3.
/// Narrative progression is intentionally isolated
/// from every gameplay resource.
/// </summary>
public sealed class NarrativeScene3Controller : MonoBehaviour
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
        if (scene.name != SceneCatalog.MiniNarrative3) return;
        if (FindFirstObjectByType<NarrativeScene3Controller>() != null) return;

        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
            canvas.AddComponent<NarrativeScene3Controller>();
        else
            Debug.LogError("[MiniNarrative3] Existing Canvas was not found. No replacement UI was created.");
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
        RetryStory5Quiz,
        RetryStory6Quiz,
        PlayLevel3AfterEnding,
        ContinueStory6Ending
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
    bool playingStory6;
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
        BeginStory5();
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
            : "SKIP STORY 6";

        answerButtons[0] = FindComponent<Button>("QuestionPanel/Answer1");
        answerButtons[1] = FindComponent<Button>("QuestionPanel/Answer2");
        answerButtons[2] = FindComponent<Button>("QuestionPanel/Answer3");
        storyButtons[0] = FindComponent<Button>("StoryPanel/Story5");
        storyButtons[1] = FindComponent<Button>("StoryPanel/Story6");
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
            Debug.LogError("[MiniNarrative3] Required existing UI references are missing. No replacement UI was created.");

        return complete;
    }

    GameObject FindObject(string relativePath)
    {
        Transform found = transform.Find(relativePath);
        if (found != null) return found.gameObject;

        Debug.LogError($"[MiniNarrative3] Missing existing UI object: Canvas/{relativePath}");
        return null;
    }

    T FindComponent<T>(string relativePath, bool required = true) where T : Component
    {
        Transform found = transform.Find(relativePath);
        T component = found != null ? found.GetComponent<T>() : null;
        if (required && component == null)
            Debug.LogError($"[MiniNarrative3] Missing {typeof(T).Name}: Canvas/{relativePath}");
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

        storyButtons[0].onClick.RemoveListener(BeginStory5);
        storyButtons[0].onClick.AddListener(BeginStory5);
        storyButtons[1].onClick.RemoveListener(SelectStory6);
        storyButtons[1].onClick.AddListener(SelectStory6);
    }

    void LoadSavedNarrativeState()
    {
        StoryNarrativeData3.EnsureInitialized();
        RefreshStoryUi();
    }

    void RefreshStoryUi(string message = null)
    {
        if (!string.IsNullOrEmpty(message) && storyTitle != null)
            storyTitle.text = message;

        if (storyButtons[0] != null)
            storyButtons[0].interactable = true;
        if (storyButtons[1] != null)
            storyButtons[1].interactable = StoryNarrativeData3.Story6Unlocked;
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

    void BeginStory5()
    {
        playingStory6 = false;
        BeginSelectedStory();
    }

    void BeginStory6()
    {
        playingStory6 = true;
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

    void SelectStory6()
    {
        if (StoryNarrativeData3.Story6Unlocked)
        {
            BeginStory6();
            return;
        }

        RefreshStoryUi("Complete Story 5 perfectly to unlock Story 6");
    }

    StoryDialogueLine[] CurrentOpening =>
        playingStory6 ? StoryNarrativeData3.Story6Opening : StoryNarrativeData3.Story5Opening;

    StoryDialogueLine[] CurrentEnding =>
        playingStory6 ? StoryNarrativeData3.Story6Ending : StoryNarrativeData3.Story5Ending;

    void ShowUnavailableStoryMessage()
    {
        flow = Flow.Finished;
        narrativePanel.SetActive(false);
        questionPanel.SetActive(false);
        skipButton.gameObject.SetActive(false);
        RefreshStoryUi(playingStory6
            ? "Story 6 content has not been added yet"
            : "Story 5 content has not been added yet");
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
                    LoadLevelThree();
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

        if (!playingStory6 && flow == Flow.Opening && dialogueIndex == CurrentOpening.Length - 1)
            narrativeTitle.text = StoryNarrativeData3.Story5MissionTitle;
        else if (!playingStory6 && flow == Flow.Ending && dialogueIndex == CurrentEnding.Length - 1)
            narrativeTitle.text = StoryNarrativeData3.Story5CompleteTitle;
        else if (flow == Flow.Opening && dialogueIndex == CurrentOpening.Length - 1)
            narrativeTitle.text = StoryNarrativeData3.Story6MissionTitle;
        else if (flow == Flow.Ending && dialogueIndex == CurrentEnding.Length - 1)
            narrativeTitle.text = StoryNarrativeData3.Story6CompleteTitle;
        else
            narrativeTitle.text = playingStory6
                ? StoryNarrativeData3.Story6Title
                : StoryNarrativeData3.Story5Title;

        if (line.Speaker == StorySpeaker.Zuri)
        {
            mainCharacterImage.SetActive(true);
            otherCharacterImage.SetActive(false);
            zuriText.text = $"Zuri:\n{line.Text}";
            npcText.text = string.Empty;
        }
        else if (line.Speaker == StorySpeaker.Kaelo ||
                 line.Speaker == StorySpeaker.Naledi ||
                 line.Speaker == StorySpeaker.Villager1 ||
                 line.Speaker == StorySpeaker.Villager2)
        {
            mainCharacterImage.SetActive(false);
            otherCharacterImage.SetActive(true);
            zuriText.text = string.Empty;
            string speakerName = line.Speaker == StorySpeaker.Kaelo ? "Kaelo"
                : line.Speaker == StorySpeaker.Naledi ? "Naledi"
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
        bool hasContent = playingStory6
            ? StoryNarrativeData3.HasStory6Content
            : StoryNarrativeData3.HasStory5Content;
        if (!hasContent)
        {
            ShowUnavailableStoryMessage();
            return;
        }

        if (playingStory6)
            StoryNarrativeData3.MarkStory6Read();
        else
            StoryNarrativeData3.MarkStory5Read();

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
        activeQuestion = playingStory6
            ? StoryNarrativeData3.GetStory6Question(
                questionIndex, submittedCorrect[0], submittedCorrect[1])
            : StoryNarrativeData3.Story5Questions[questionIndex];
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
        if (playingStory6)
        {
            StoryNarrativeData3.SaveStory6QuizResult(correctAnswers);
            if (correctAnswers < 2)
            {
                RefreshStoryUi("Keep investigating!");
                ShowStory6FailureResult();
                return;
            }

            RefreshStoryUi("Story 6 complete!");
            ShowStory6PassedResult();
            return;
        }

        StoryNarrativeData3.SaveStory5QuizResult(correctAnswers);

        if (correctAnswers < 2)
        {
            RefreshStoryUi("Keep investigating!");
            ShowFailureResult();
            return;
        }

        if (correctAnswers == 2)
        {
            RefreshStoryUi("Level 3 unlocked!");
            ShowStory5PassedResult(false);
            return;
        }

        RefreshStoryUi("Story 6 unlocked!");
        ShowStory5PassedResult(true);
    }

    void ShowFailureResult()
    {
        resultAction = ResultAction.RetryStory5Quiz;
        flow = Flow.Result;
        narrativePanel.SetActive(false);
        questionPanel.SetActive(true);
        questionTitle.text = $"FIRST-TRY SCORE: {correctAnswers} OF 3";
        questionText.text =
            "Not quite, Water Detective! Every good detective learns from clues. Let's try again!\n\n" +
            "LEVEL 3 — LOCKED\nSTORY 6 — LOCKED";
        commentaryTitle.text = "TRY THE QUIZ AGAIN";
        commentaryText.text = "You need at least 2 out of 3 correct answers.";
        PrepareResultButtons("RETRY QUIZ");
    }

    void ShowStory5PassedResult(bool story6Unlocked)
    {
        resultAction = ResultAction.PlayLevel3AfterEnding;
        flow = Flow.Result;
        narrativePanel.SetActive(false);
        questionPanel.SetActive(true);
        questionTitle.text = $"CONGRATULATIONS — {correctAnswers} OUT OF 3!";
        questionText.text = story6Unlocked
            ? "You unlocked Story 6 and Level 3!\n\n" +
              "Select the Story 6 button to continue the story, or skip to Level 3."
            : "You unlocked Level 3!\n\nStory 6 needs a perfect score.";
        commentaryTitle.text = story6Unlocked
            ? "STORY 6 AND LEVEL 3 UNLOCKED"
            : "LEVEL 3 UNLOCKED";
        commentaryText.text = story6Unlocked
            ? "Story 6 will remain unlocked if you skip it."
            : "Continue to the next mission.";
        PrepareResultButtons(story6Unlocked
            ? authoredSkipLabel
            : "PLAY LEVEL 3");
    }

    void ShowStory6FailureResult()
    {
        resultAction = ResultAction.RetryStory6Quiz;
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

    void ShowStory6PassedResult()
    {
        resultAction = ResultAction.ContinueStory6Ending;
        flow = Flow.Result;
        narrativePanel.SetActive(false);
        questionPanel.SetActive(true);
        questionTitle.text = $"CONGRATULATIONS — {correctAnswers} OUT OF 3!";
        bool story7Unlocked = StoryNarrativeData4.Story7Unlocked;
        questionText.text = story7Unlocked
            ? "You completed Story 6 and unlocked Story 7!\n\nContinue to the ending, then play Level 3."
            : "You completed Story 6!\n\nContinue to the ending, then play Level 3.";
        commentaryTitle.text = story7Unlocked
            ? "STORY 7 UNLOCKED"
            : "STORY 6 COMPLETE";
        commentaryText.text = story7Unlocked
            ? "Story 7 will remain unlocked if you skip it."
            : "Continue through the ending, then play Level 3.";
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
        return playingStory6
            ? StoryNarrativeData3.HasReadStory6
            : StoryNarrativeData3.HasReadStory5;
    }

    void HandleSkipPressed()
    {
        if (flow == Flow.Finished) return;

        if (flow != Flow.Result && CurrentStoryAlreadyRead())
        {
            LoadLevelThree();
            return;
        }

        if (flow != Flow.Result) return;
        skipButton.interactable = false;
        skipButton.gameObject.SetActive(false);

        if (resultAction == ResultAction.RetryStory5Quiz ||
            resultAction == ResultAction.RetryStory6Quiz)
            RestartQuiz();
        else if (resultAction == ResultAction.PlayLevel3AfterEnding)
            LoadLevelThree();
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

    void LoadLevelThree()
    {
        if (flow == Flow.Finished) return;

        StopNarrativeButtonTimer();
        flow = Flow.Finished;
        skipButton.interactable = false;
        skipButton.gameObject.SetActive(false);
        Time.timeScale = 1f;
        SceneLoadOverlay.Load(SceneCatalog.Level3);
    }
}
