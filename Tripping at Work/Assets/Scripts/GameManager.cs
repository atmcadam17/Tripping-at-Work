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
    public int minigamesWon = 0;
    
    public List<Sound> soundList;
    private AudioSource source;

    [SerializeField] private string winSceneName;
    [SerializeField] private string loseSceneName;
    
    private void Awake()
    {
        Debug.Log(Instance);
    }
    
    void OnEnable()
    {
        Debug.Log("manager enabled");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    // called every time a new scene loads
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
        _dialogueRunner = GameObject.Find("Canvas").transform.Find("Dialogue").transform.Find("DialogueRunner")
            .GetComponent<DialogueRunner>();

        StartCoroutine(LoadNewCommands(.01f));
    }

    [System.Serializable]
    public struct Minigame
    {
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
        _dialogueRunner = GameObject.Find("Canvas").transform.Find("Dialogue").transform.Find("DialogueRunner")
            .GetComponent<DialogueRunner>();
        
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public IEnumerator LoadNewCommands(float t)
    {
        yield return new WaitForSeconds(t);
        
        _dialogueRunner.AddCommandHandler("load_next", LoadNextScene);
        _dialogueRunner.AddCommandHandler("set_result", SetGameResult);
        
        _dialogueRunner.AddCommandHandler("play_music", PlayMusic);
        _dialogueRunner.AddCommandHandler("stop_music", StopMusic);
        _dialogueRunner.AddCommandHandler("play_sound", PlaySound);
        
        _dialogueRunner.AddCommandHandler("end_game", EndGame);
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
        var result = parameters[0];
        Minigame currentMinigame;
        
        foreach (var game in minigameList)
        {
            if (SceneManager.GetActiveScene().name == game.sceneName)
            {
                currentMinigame = game;
            }
        }

        // sets result based on command parameter (newGame)
        Debug.Log("setting result");
        switch (result)
        {
            case "win":
                currentMinigame.won = true;
                minigamesWon++;
                Debug.Log("result set true");
                break;
            case "lose":
                currentMinigame.won = false;
                Debug.Log("result set false");
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

    private void EndGame(string[] parameters)
    {
        var gamesWon = 0;
        for (int i = 0; i < minigameList.Count; i++)
        {
            if (minigameList[i].won)
            {
                gamesWon++;
            }
        }

        if (minigamesWon >= 1)
        {
            SceneManager.LoadScene(winSceneName);
        }
        else
        {
            SceneManager.LoadScene(loseSceneName);
        }
    }

    private void LoadByName(string[] parameters)
    {
        SceneManager.LoadScene(parameters[0]);
    }
}
