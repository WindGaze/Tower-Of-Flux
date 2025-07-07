using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class VoskDialogText : MonoBehaviour 
{
    public VoskSpeechToText VoskSpeechToText;
    public Text DialogText;

	// Regular expressions for English voice commands
	Regex hi_regex = new Regex(@"hello");
	Regex who_regex = new Regex(@"who are you");
	Regex pass_regex = new Regex("(okay|let's go)");
	Regex help_regex = new Regex("help me");

	Regex goat_regex = new Regex(@"(goat|start with goat)");
	Regex wolf_regex = new Regex(@"(wolf)");
	Regex cabbage_regex = new Regex(@"(cabbage|start with cabbage)");

	Regex goat_back_regex = new Regex(@"(goat back|bring goat back|return goat)");
	Regex wolf_back_regex = new Regex(@"(wolf back|bring wolf back|return wolf)");
	Regex cabbage_back_regex = new Regex(@"(cabbage back|bring cabbage back|return cabbage)");

	Regex forward_regex = new Regex("move forward");
	Regex back_regex = new Regex("(go back|return back)");

	// State
	bool goat_left;
	bool wolf_left;
	bool cabbage_left;
	bool man_left;

    void Awake()
    {
        VoskSpeechToText.OnTranscriptionResult += OnTranscriptionResult;
		ResetState();
    }

	void ResetState()
	{
		goat_left = true;
		wolf_left = true;
		cabbage_left = true;
		man_left = true;
	}

	void CheckState() {
		if (goat_left && wolf_left && !man_left) {
			AddFinalResponse("The wolf ate the goat, start again");
			return;
		}
		if (goat_left && cabbage_left && !man_left) {
			AddFinalResponse("The goat ate the cabbage, start again");
			return;
		}
		if (!goat_left && !wolf_left && man_left) {
			AddFinalResponse("The wolf ate the goat, start again");
			return;
		}
		if (!goat_left && !cabbage_left && man_left) {
			AddFinalResponse("The goat ate the cabbage, start again");
			return;
		}
		if (!goat_left && !wolf_left && !cabbage_left && !man_left) {
			AddFinalResponse("Great job, let's do it again!");
			return;
		}

		AddResponse("Okay, what next?");
	}

	void Say(string response)
	{
		System.Diagnostics.Process.Start("/usr/bin/say", response); 
	}

	void AddFinalResponse(string response) {
		Say(response);
		DialogText.text = response + "\n";
		ResetState();
	}

	void AddResponse(string response) {
        Say(response);
		DialogText.text = response + "\n\n";

		DialogText.text += "The farmer is " + (man_left ? "on the left bank" : "on the right bank") + "\n";
		DialogText.text += "The wolf is " + (wolf_left ? "on the left bank" : "on the right bank") + "\n";
		DialogText.text += "The goat is " + (goat_left ? "on the left bank" : "on the right bank") + "\n";
		DialogText.text += "The cabbage is " + (cabbage_left ? "on the left bank" : "on the right bank") + "\n";

		DialogText.text += "\n";
	}

    private void OnTranscriptionResult(string obj)
    {
		// Save to file

        Debug.Log(obj);
        var result = new RecognitionResult(obj);
        foreach (RecognizedPhrase p in result.Phrases)
        {
			if (hi_regex.IsMatch(p.Text))
			{
				AddResponse("Hello to you");
				return;
			}
			if (who_regex.IsMatch(p.Text))
			{
				AddResponse("I am a teaching robot");
				return;
			}
			if (pass_regex.IsMatch(p.Text))
			{
                AddResponse("Alright");
				return;
			}
			if (help_regex.IsMatch(p.Text))
			{
				AddResponse("Think for yourself");
				return;
			}
			if (goat_back_regex.IsMatch(p.Text)) {
				if (goat_left == true) {
					AddResponse("The goat is still on the left bank");
				} else if (man_left == true) {
					AddResponse("The farmer is still on the left bank");
				} else {
					goat_left = true;
					man_left = true;
					CheckState();
				}
				return;
			}

			if (wolf_back_regex.IsMatch(p.Text)) {
				if (wolf_left == true) {
					AddResponse("The wolf is still on the left bank");
				} else if (man_left == true) {
					AddResponse("The farmer is still on the left bank");
				} else {
					wolf_left = true;
					man_left = true;
					CheckState();
				}
				return;
			}

			if (cabbage_back_regex.IsMatch(p.Text)) {
				if (cabbage_left == true) {
					AddResponse("The cabbage is still on the left bank");
				} else if (man_left == true) {
					AddResponse("The farmer is still on the left bank");
				} else {
					cabbage_left = true;
					man_left = true;
					CheckState();
				}
				return;
			}

			if (goat_regex.IsMatch(p.Text)) {
				if (goat_left == false) {
					AddResponse("The goat is already on the right bank");
				} else if (man_left == false) {
					AddResponse("The farmer is already on the right bank");
				} else {
					goat_left = false;
					man_left = false;
					CheckState();
				}
				return;
			}

			if (wolf_regex.IsMatch(p.Text)) {
				if (wolf_left == false) {
					AddResponse("The wolf is already on the right bank");
				} else if (man_left == false) {
					AddResponse("The farmer is already on the right bank");
				} else {
					wolf_left = false;
					man_left = false;
					CheckState();
				}
				return;
			}

			if (cabbage_regex.IsMatch(p.Text)) {
				if (cabbage_left == false) {
					AddResponse("The cabbage is already on the right bank");
				} else if (man_left == false) {
					AddResponse("The farmer is already on the right bank");
				} else {
					cabbage_left = false;
					man_left = false;
					CheckState();
				}
				return;
			}

			if (forward_regex.IsMatch(p.Text)) {
				if (man_left == false) {
					AddResponse("The farmer is already on the right bank");
				} else {
					man_left = false;
					CheckState();
				}
				return;
			}
		
			if (back_regex.IsMatch(p.Text)) {
				if (man_left == true) {
					AddResponse("The farmer is still on the left bank");
				} else {
					man_left = true;
					CheckState();
				}
				return;
			}
        }
		if (result.Phrases.Length > 0 && result.Phrases[0].Text != "") {
			AddResponse("I don't understand you");
		}
    }
}
