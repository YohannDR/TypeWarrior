using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("SFX Event")]
    [field: SerializeField] private EventReference _sfxSucces; //{ get; private set; }
    [field: SerializeField] private EventReference _sfxDeath; //{ get; private set; }
    [field: SerializeField] private EventReference _sfxDamagePlayer; //{ get; private set; }
    [field: SerializeField] private EventReference _sfxDamageFoe; //{ get; private set; }
    [field: SerializeField] private EventReference _sfxWin; //{ get; private set; }
    [field: SerializeField] private EventReference _sfxKeyboard; //{ get; private set; }
    [field: SerializeField] private EventReference _sfxValidateWord; //{ get; private set; }
    [field: SerializeField] private EventReference _sfxJumpNSlide; //{ get; private set; }

    [Header("Music Event")]
    [field: SerializeField] private EventReference _music;

    private const string MUSIC_STATE_PARAMETERS = "STATE";

    private EventInstance _musicEventInstance;

    //Enums of which music should be played with FMOD
    public enum MusicState
    {
        Intro,
        LevelOne,
        LevelTwo,
        LevelThree,
        LevelFour,
        Outro
    }

    private static SoundManager instance;

    private SoundManager() { }

    public static SoundManager GetInstance
    {
        get { return instance; }
        private set { instance = value; }
    }
	private void Awake()
	{
		if(instance != null)
        {
            Destroy(this);
            Debug.Log("This instance of" + GetType().Name + " already exist, delete the last one added.");
            return;
        }
        else instance = this;

        DontDestroyOnLoad(gameObject);
	}
    
    // Start is called before the first frame update
    void Start()
    {
        PlaySucces(transform.position);
        InitializeMusic(_music);
        SwitchMusicState(MusicState.LevelOne);
    }

    private EventInstance CreateInstance(EventReference pEventMusic)
    {
        EventInstance lEventInstance = RuntimeManager.CreateInstance(pEventMusic);
        return lEventInstance;
    }

    /// <summary>
    /// Play the instance of the music you give it
    /// </summary>
    /// <param name="pMusicEventRef">Reference of the music that will be played</param>
    private void InitializeMusic(EventReference pMusicEventRef)
    {
        _musicEventInstance = CreateInstance(pMusicEventRef);
        _musicEventInstance.start();
    }

    /// <summary>
    /// Change the state parameters of the music event
    /// </summary>
    /// <param name="pMusicState">part of the music that should be play</param>
    private void SwitchMusicState(MusicState pMusicState)
    {
        _musicEventInstance.setParameterByName(MUSIC_STATE_PARAMETERS, (int)pMusicState);
    }

    private void PlaySFX(EventReference pSound, Vector3 pPosition)
    {
        RuntimeManager.PlayOneShot(pSound, pPosition);
    }

    public void SwitchMusicStateTwo()
    {
        SwitchMusicState(MusicState.LevelTwo);
    }

    public void SwitchMusicStateThree()
    {
        SwitchMusicState(MusicState.LevelThree);
    }

    public void PlaySucces(Vector3 pPosition) 
    {
        PlaySFX(_sfxSucces, transform.position);
    }
}
