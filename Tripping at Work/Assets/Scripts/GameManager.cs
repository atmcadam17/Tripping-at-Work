using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private DialogueRunner _dialogueRunner = null;
    
    public List<string> sceneNames; // list of IN ORDER
    private int currentSceneIndex = 0;
    
    public List<Minigame> minigameList;
    public List<Sound> soundList;
    private AudioSource source;
    
    private void Awake()
    {
        Debug.Log(Instance);
    }
    
    [System.Serializable]
    public struct Minigame
    {
        public string gameName; // called using command
        public string sceneName; // associated scene
        public bool won; // state (false by default)
    }
    
    [System.Serializable]
    public struct Sound
    {
        public string soundName; // called using command
        public AudioClip clip; // clip to play
    }

    void Start()
    {
        _dialogueRunner.AddCommandHandler("load_next", LoadNextScene);
        _dialogueRunner.AddCommandHandler("set_result", SetGameResult);
        
        _dialogueRunner.AddCommandHandler("play_music", PlayMusic);
        _dialogueRunner.AddCommandHandler("stop_music", StopMusic);
        _dialogueRunner.AddCommandHandler("play_sound", PlaySound);
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // switches to the next scene in sceneNames - called by the command load_next
    // no parameters
    private void LoadNextScene(string[] parameters)
    {
        currentSceneIndex++;
        SceneManager.LoadScene(sceneNames[currentSceneIndex]);
    }

    // sets result of minigame - called by the command set_result
    // params: string "win" or "lose"
    private void SetGameResult(string[] parameters)
    {
        // find current minigame in minigameList
        var newGame = parameters[0];
        Minigame currentMinigame;
        
        foreach (var game in minigameList)
        {
            if (SceneManager.GetActiveScene().name == game.sceneName)
            {
                currentMinigame = game;
            }
        }

        // sets result based on command parameter (newGame)
        switch (newGame)
        {
            case "win":
                currentMinigame.won = true;
                break;
            case "lose":
                currentMinigame.won = false;
                break;
            default:
                Debug.Log("Command set_result takes only param \"win\" or \"lose\".");
                break;
        }
    }

    // plays oneshot
    // takes 1 parameter: name
    private void PlaySound(string[] parameters)
    {
        var soundName = parameters[0];
        source.PlayOneShot(GetSoundFromList(soundName));
    }

    // plays music
    // takes 1 parameter: name
    private void PlayMusic(string[] parameters)
    {
        var soundName = parameters[0];
        source.clip = GetSoundFromList(soundName);
        source.Play();
        Debug.Log("music");
    }

    private void StopMusic(string[] parameters)
    {
        source.Stop();
    }

    private AudioClip GetSoundFromList(string soundName)
    {
        foreach (var sound in soundList)
        {
            if (sound.soundName == soundName)
            {
                return sound.clip;
            }
        }
        return null;
    }
}
