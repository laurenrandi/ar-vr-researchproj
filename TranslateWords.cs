using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections;

public class TranslateWords : MonoBehaviour
{
    public TextMeshPro label;
    public TextMeshPro resultText; // TextMeshPro object to display the result

    public Image wordImage; // Image component to display corresponding image

    public Button translationToggleButton;
    public Button Know;
    public Button[] quizButtons; // Array to hold quiz toggles


    public GameObject canvas;
    public GameObject alternateCanvas;

    private bool isAlternateCanvasActive = false;
    private bool isWaitingForSecondClick = false;

    // Static fields to hold translation dictionaries
    private static Dictionary<string, string> englishToSpanishDictionary = new Dictionary<string, string>();
    private static Dictionary<string, string> spanishToEnglishDictionary = new Dictionary<string, string>();
    private Dictionary<string, Sprite> wordImageMap = new Dictionary<string, Sprite>(); // Map words to images

    private static bool dictionariesLoaded = false;

    private bool isEnglish = true;
    private int incorrect = 0;
    private int correct = 0;


    void Start()
    {
        if (!dictionariesLoaded)
        {
            LoadTranslations();
            dictionariesLoaded = true;
            Debug.Log("loaded");
        }

        // Load translations only if dictionaries haven't been loaded yet

        // Associate words with images
        Sprite[] images = Resources.LoadAll<Sprite>("images");
        foreach (var image in images)
        {
            string word = image.name; // Assuming image name matches word
            wordImageMap.Add(word, image);

        }

        SetTextMeshProToRandomEnglishWord();
        UpdateWordImage(label.text);
        canvas.SetActive(false);
        translationToggleButton.onClick.AddListener(delegate { ToggleTranslation(); });


        // Add click event listeners to quiz toggles
        foreach (Button button in quizButtons)
        {
            button.onClick.AddListener(delegate { CheckQuizAnswer(button); });
        }

        // Add click event listener to the submit button
        // submitButton.onClick.AddListener(SubmitQuiz);
        UpdateWordImage(label.text);
        SetAlternateCanvasButtonsWithRandomSpanishWords();
        SetRandomButtonWithTranslatedText();
        //alternateCanvas.SetActive(false);
        //translationToggleButton.onClick.AddListener(delegate { ToggleTranslation(); });

    }

    void ToggleTranslation()
    {
        // Get the current word displayed in the TextMeshPro label
        string currentWord = label.text;

        // Translate the current word
        string translatedWord = Translate(currentWord);

        isEnglish = !isEnglish;

        // Check if translation is found
        if (translatedWord != null)
        {
            // Update the TextMeshPro label with the translated word
            label.text = translatedWord;
            if (!isEnglish)
            {
                canvas.SetActive(true);
                alternateCanvas.SetActive(false);
            }
            else
            {
                canvas.SetActive(false);

            }

            // Update the image corresponding to the translated word
            // UpdateWordImage(translatedWord);
        }
        else
        {
            Debug.LogWarning("Translation not found for word: " + currentWord);
        }

    }

    public void StartAlternateCanvasTimer()
    {
        // Call this method when the button is clicked to start the timer coroutine
        StartCoroutine(ShowAlternateCanvasAfterDelay(5f));
    }

    IEnumerator ShowAlternateCanvasAfterDelay(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        isWaitingForSecondClick = true;

        while (isWaitingForSecondClick)
        {
            yield return null;
            if (Input.GetMouseButtonDown(0)) // Assuming left mouse button
            {
                // If the "know" button is clicked again, set isWaitingForSecondClick to false and break out of the loop
                isWaitingForSecondClick = false;
                break;
            }

        }

        // Show the alternate canvas

        ShowAlternateCanvas();
    }

    void ShowAlternateCanvas()
    {
        // Disable the normal canvas
        canvas.SetActive(false);

        // Enable the alternate canvas
        alternateCanvas.SetActive(true);

        // Set the flag to indicate the alternate canvas is active
        isAlternateCanvasActive = true;

        label.gameObject.SetActive(false);
    }

    public void CloseAlternateCanvas()
    {
        // Disable the alternate canvas
        alternateCanvas.SetActive(false);

        // Enable the normal canvas
        canvas.SetActive(true);

        // Reset the flag
        isAlternateCanvasActive = false;
    }

    void SetAlternateCanvasButtonsWithRandomSpanishWords()
    {
        // Create a list to hold all Spanish words from the CSV file
        List<string> allSpanishWords = new List<string>(spanishToEnglishDictionary.Keys);

        // Create a list to hold the random Spanish words
        List<string> randomSpanishWords = new List<string>();

        // Get random Spanish words from the list until the list has enough unique words
        while (randomSpanishWords.Count < 3) // Assuming you have only three buttons
        {
            // Get a random index within the range of all Spanish words list
            int randomIndex = Random.Range(0, allSpanishWords.Count);

            // Get a random Spanish word using the random index
            string randomSpanishWord = allSpanishWords[randomIndex];

            // Check if the word is not already in the list
            if (!randomSpanishWords.Contains(randomSpanishWord))
            {
                // Add the word to the list
                randomSpanishWords.Add(randomSpanishWord);
            }
        }

        // Iterate over the alternate canvas buttons and set their labels to random Spanish words
        for (int i = 0; i < 3; i++) // Assuming you have only three buttons
        {
            // Set the label of the button to a random Spanish word from the list
            quizButtons[i].GetComponentInChildren<TextMeshPro>().text = randomSpanishWords[i];
        }
    }



    string GetCurrentWordFromImage()
    {
        // Get the name of the current sprite assigned to the image component
        string spriteName = wordImage.sprite.name;

        // Assuming the sprite name matches the word
        return spriteName;
    }


    void CheckQuizAnswer(Button button)
    {
        // Clear the result text when a quiz button is clicked
        resultText.text = "";

        // Get the word associated with the image
        string currentWord = label.text;

        // Translate the current word
        string translatedWord = Translate(currentWord);

        Debug.Log("SubmitQuiz function called");
        Debug.Log("Translated word: " + translatedWord);


        // Get the text of the clicked button
        string buttonText = button.GetComponentInChildren<TextMeshPro>().text;

        // Check if the clicked button's text matches the translated word
        if (translatedWord == buttonText)
        {
            // Set the result text to "Correct" if the selected answer is correct
            resultText.text = "Correct";
            correct++;
        }
        else
        {
            // Set the result text to "Incorrect" if the selected answer is incorrect
            resultText.text = "Incorrect";
            incorrect++;
        }
    }


    void LoadTranslations()
    {
        string csvFilePath = @"C:\Users\randi\arlangproj2\Assets\CSVFiles\Words.csv";
        // Provide the path to your CSV file
        if (!File.Exists(csvFilePath))
        {
            Debug.LogError("CSV file not found at path: " + csvFilePath);
            return;
        }

        string[] lines = File.ReadAllLines(csvFilePath);

        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            if (parts.Length >= 4) // Ensure that the CSV file contains enough columns
            {
                string englishWord = parts[0].Trim();
                string spanishTranslation = parts[1].Trim();
                string incorrectTranslation = parts[2].Trim(); // Get the value from the "incorrect" column
                string correctTranslation = parts[3].Trim(); // Get the value from the "correct" column

                if (!englishToSpanishDictionary.ContainsKey(englishWord))
                {
                    englishToSpanishDictionary.Add(englishWord, spanishTranslation);
                }
                else
                {
                    Debug.LogWarning("Duplicate entry for English word: " + englishWord);
                }

                if (!spanishToEnglishDictionary.ContainsKey(spanishTranslation))
                {
                    spanishToEnglishDictionary.Add(spanishTranslation, englishWord);
                }
                else
                {
                    Debug.LogWarning("Duplicate entry for Spanish word: " + spanishTranslation);
                }

                // Store the incorrect and correct translations based on the current index label
                if (englishWord == label.text) // Assuming label.text contains the current index label
                {
                    englishToSpanishDictionary.Add(englishWord, incorrectTranslation);
                    spanishToEnglishDictionary.Add(spanishTranslation, correctTranslation);
                }
            }
            else
            {
                Debug.LogWarning("Invalid format in line: " + line);
            }
        }
    }


    public void UpdateTranslation()
    {
        string currentWord = label.text;
        string translatedWord = Translate(currentWord);
        if (translatedWord != null)
        {
            label.text = translatedWord;
            // label.text = "lala";
        }
        else
        {
            Debug.LogWarning("Translation not found for word: " + currentWord);
        }
    }

    private string Translate(string word)
    {
        if (isEnglish)
        {
            if (englishToSpanishDictionary.ContainsKey(word))
            {
                return englishToSpanishDictionary[word];
            }
            else
            {
                Debug.LogWarning("Translation not found for word: " + word);
                return null;
            }
        }
        else
        {
            if (spanishToEnglishDictionary.ContainsKey(word))
            {
                return spanishToEnglishDictionary[word];
            }
            else
            {
                Debug.LogWarning("Translation not found for word: " + word);
                return null;
            }
        }
    }


    void UpdateWordImage(string word)
    {
        if (wordImageMap.ContainsKey(word))
        {
            // Update the image component with the corresponding sprite
            wordImage.sprite = wordImageMap[word];
        }
        else
        {
            Debug.LogWarning("Image not found for word: " + word);
        }
    }
    void SetTextMeshProToRandomEnglishWord()
    {

        label.text = "";
        // Get a random index within the range of the English words dictionary
        int randomIndex = Random.Range(0, englishToSpanishDictionary.Count);

        // Get a random English word using the random index
        string randomEnglishWord = englishToSpanishDictionary.Keys.ElementAt(randomIndex);

        // Set the TextMeshPro label to the random English word
        label.text = randomEnglishWord;
    }

    void SetRandomButtonWithTranslatedText()
    {
        // Get a random index within the range of the quiz buttons array
        int randomIndex = Random.Range(0, quizButtons.Length);

        // Get the translated word
        string translatedWord = Translate(label.text);

        // Set the text of the random button to the translated word
        quizButtons[randomIndex].GetComponentInChildren<TextMeshPro>().text = translatedWord;
    }
}
