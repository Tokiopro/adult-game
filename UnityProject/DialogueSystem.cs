using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SchoolLoveSimulator
{
    [System.Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public int nextDialogueId;
        public int affectionChange;
        public string requiredFlag;
    }

    [System.Serializable]
    public class DialogueLine
    {
        public int id;
        public string characterName;
        public string dialogueText;
        public Sprite characterSprite;
        public AudioClip voiceClip;
        public List<DialogueChoice> choices;
        public int nextDialogueId = -1;
        public string setFlag;
    }

    public class DialogueSystem : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject dialoguePanel;
        public Text characterNameText;
        public Text dialogueText;
        public Image characterImage;
        public GameObject choiceButtonPrefab;
        public Transform choiceButtonContainer;
        public float textSpeed = 0.05f;

        [Header("Dialogue Data")]
        public List<DialogueLine> currentDialogue;
        private int currentLineIndex = 0;
        private bool isTyping = false;
        private bool canProceed = false;
        private Coroutine typingCoroutine;

        [Header("Audio")]
        public AudioSource voiceAudioSource;
        public AudioSource sfxAudioSource;
        public AudioClip textSound;

        private Dictionary<string, bool> gameFlags = new Dictionary<string, bool>();
        private CharacterManager characterManager;
        private SaveLoadSystem saveLoadSystem;

        void Start()
        {
            characterManager = FindObjectOfType<CharacterManager>();
            saveLoadSystem = FindObjectOfType<SaveLoadSystem>();
            dialoguePanel.SetActive(false);
        }

        void Update()
        {
            if (dialoguePanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    if (isTyping)
                    {
                        StopTyping();
                    }
                    else if (canProceed && currentDialogue[currentLineIndex].choices.Count == 0)
                    {
                        NextLine();
                    }
                }
            }
        }

        public void StartDialogue(List<DialogueLine> dialogue)
        {
            currentDialogue = dialogue;
            currentLineIndex = 0;
            dialoguePanel.SetActive(true);
            DisplayCurrentLine();
        }

        private void DisplayCurrentLine()
        {
            if (currentLineIndex >= currentDialogue.Count)
            {
                EndDialogue();
                return;
            }

            DialogueLine line = currentDialogue[currentLineIndex];
            
            characterNameText.text = line.characterName;
            characterImage.sprite = line.characterSprite;

            if (line.voiceClip != null && voiceAudioSource != null)
            {
                voiceAudioSource.clip = line.voiceClip;
                voiceAudioSource.Play();
            }

            ClearChoices();
            
            if (line.choices != null && line.choices.Count > 0)
            {
                DisplayChoices(line.choices);
                dialogueText.text = line.dialogueText;
                canProceed = false;
            }
            else
            {
                typingCoroutine = StartCoroutine(TypeText(line.dialogueText));
            }

            if (!string.IsNullOrEmpty(line.setFlag))
            {
                SetFlag(line.setFlag, true);
            }
        }

        private IEnumerator TypeText(string text)
        {
            isTyping = true;
            canProceed = false;
            dialogueText.text = "";

            foreach (char letter in text)
            {
                dialogueText.text += letter;
                
                if (textSound != null && sfxAudioSource != null)
                {
                    sfxAudioSource.PlayOneShot(textSound, 0.5f);
                }
                
                yield return new WaitForSeconds(textSpeed);
            }

            isTyping = false;
            canProceed = true;
        }

        private void StopTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            
            dialogueText.text = currentDialogue[currentLineIndex].dialogueText;
            isTyping = false;
            canProceed = true;
        }

        private void DisplayChoices(List<DialogueChoice> choices)
        {
            foreach (DialogueChoice choice in choices)
            {
                if (!string.IsNullOrEmpty(choice.requiredFlag) && !HasFlag(choice.requiredFlag))
                {
                    continue;
                }

                GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceButtonContainer);
                Text buttonText = choiceButton.GetComponentInChildren<Text>();
                buttonText.text = choice.choiceText;

                Button button = choiceButton.GetComponent<Button>();
                int choiceIndex = choices.IndexOf(choice);
                button.onClick.AddListener(() => MakeChoice(choiceIndex));
            }
        }

        private void MakeChoice(int choiceIndex)
        {
            DialogueChoice choice = currentDialogue[currentLineIndex].choices[choiceIndex];
            
            if (choice.affectionChange != 0 && characterManager != null)
            {
                string characterName = currentDialogue[currentLineIndex].characterName;
                characterManager.ChangeAffection(characterName, choice.affectionChange);
            }

            ClearChoices();

            if (choice.nextDialogueId >= 0)
            {
                currentLineIndex = FindDialogueById(choice.nextDialogueId);
                DisplayCurrentLine();
            }
            else
            {
                NextLine();
            }
        }

        private int FindDialogueById(int id)
        {
            for (int i = 0; i < currentDialogue.Count; i++)
            {
                if (currentDialogue[i].id == id)
                {
                    return i;
                }
            }
            return -1;
        }

        private void ClearChoices()
        {
            foreach (Transform child in choiceButtonContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void NextLine()
        {
            DialogueLine currentLine = currentDialogue[currentLineIndex];
            
            if (currentLine.nextDialogueId >= 0)
            {
                currentLineIndex = FindDialogueById(currentLine.nextDialogueId);
            }
            else
            {
                currentLineIndex++;
            }
            
            DisplayCurrentLine();
        }

        private void EndDialogue()
        {
            dialoguePanel.SetActive(false);
            currentDialogue = null;
            currentLineIndex = 0;
            
            if (voiceAudioSource != null)
            {
                voiceAudioSource.Stop();
            }
        }

        public void SetFlag(string flagName, bool value)
        {
            gameFlags[flagName] = value;
        }

        public bool HasFlag(string flagName)
        {
            return gameFlags.ContainsKey(flagName) && gameFlags[flagName];
        }

        public Dictionary<string, bool> GetAllFlags()
        {
            return new Dictionary<string, bool>(gameFlags);
        }

        public void LoadFlags(Dictionary<string, bool> flags)
        {
            gameFlags = new Dictionary<string, bool>(flags);
        }
    }
}