using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;  // Usamos TextMesh Pro
using System.Collections;
using System.Collections.Generic;

public class TriviaManager : MonoBehaviour
{
    [System.Serializable]
    public class ApiResponse
    {
        public int response_code;
        public Question[] results;
    }

    [System.Serializable]
    public class Question
    {
        public string category;
        public string type;
        public string difficulty;
        public string question;
        public string correct_answer;
        public string[] incorrect_answers;
    }

    [Header("UI References")]
    public TextMeshProUGUI questionText;  // Question
    public Button[] answerButtons;  // Array for A B C D buttons
    public TextMeshProUGUI resultText;  // Result text
    public Button nextButton;  //Next question 

    private ApiResponse triviaData; // Data from the api
    private int currentQuestionIndex = 0; // Question Counter
    private string correctAnswer;

    void Start()
    {
        nextButton.gameObject.SetActive(false);  // Inicialmente, o botón "Siguiente" estará oculto
        StartCoroutine(FetchQuestions());
    }

    // Coroutine to search for the questions on the api
    IEnumerator FetchQuestions()
    {
        string url = "https://opentdb.com/api.php?amount=10";
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching trivia: " + request.error);
            // Error on the api
        }
        else
        {
            // Parse the JSON response
            string json = request.downloadHandler.text;
            triviaData = JsonUtility.FromJson<ApiResponse>(json);

            if (triviaData != null && triviaData.results.Length > 0)
            {
                ShowQuestion(triviaData.results[currentQuestionIndex]);
            }
        }
    }

    void ShowQuestion(Question q)
    {
        questionText.text = System.Net.WebUtility.HtmlDecode(q.question);
        correctAnswer = q.correct_answer;

        //Combine the correct answer with the incorrect answers
        List<string> answers = new List<string>(q.incorrect_answers);
        answers.Add(q.correct_answer);

        // Mix the responses
        for (int i = 0; i < answers.Count; i++)
        {
            string temp = answers[i];
            int randomIndex = Random.Range(i, answers.Count);
            answers[i] = answers[randomIndex];
            answers[randomIndex] = temp;
        }

        // Add the answers to the buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            string answer = answers[i];
            string buttonLabel = (char)('A' + i) + ": ";  // labels 
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = buttonLabel + System.Net.WebUtility.HtmlDecode(answer); // Mostra a resposta real
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => CheckAnswer(answer));
        }
    }

    // Check if the answer is correct
    void CheckAnswer(string selectedAnswer)
    {
        if (selectedAnswer == correctAnswer)
        {
            resultText.text = "Correct!";  
            resultText.color = Color.green; 
        }
        else
        {
            resultText.text = "Incorrect. The correct answer was: " + correctAnswer;  
            resultText.color = Color.red;  
        }

        nextButton.gameObject.SetActive(true);  
    }

    // next question
    public void LoadNextQuestion()
    {
        // Reset the result text
        resultText.text = "";
        resultText.color = Color.white;

        
        currentQuestionIndex++;

        // Check if there are more questions
        if (currentQuestionIndex < triviaData.results.Length)
        {
            ShowQuestion(triviaData.results[currentQuestionIndex]);
            nextButton.gameObject.SetActive(false);  // Hide next button
        }
        else
        {
            resultText.text = "🎉 You've completed the trivia!";
            resultText.color = Color.cyan;
            nextButton.gameObject.SetActive(false);  //Hide next button on the last question
        }
    }
}
