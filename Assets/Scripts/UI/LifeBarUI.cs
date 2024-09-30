using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeBarUI : MonoBehaviour
{
    [Header("Color Lifebar")]
    [SerializeField] private List<Color> _colorLifeBar = new List<Color>();
    
    [Header("Player")]
    [SerializeField] private RawImage _playerFGLifeBar;
    [SerializeField] private Image _playerLifeBarColor;
    [SerializeField] private Slider _playerLifeBar;
    [SerializeField] private List<Texture> _playerLifeBarSpriteList = new List<Texture>();

    [Header("Foe")]
    [SerializeField] private RawImage _foeFGLifeBar;
    [SerializeField] private Image _foeLifeBarColor;
    [SerializeField] private Slider _foeLifeBar;
    [SerializeField] private List<Texture> _foeLifeBarSpriteList = new List<Texture>();
    
    public enum MiniGame
    {
        MiniGameOne,
        MinigameTwo,
        MinigameThree,
        MinigameFour
    }

    private static LifeBarUI instance;

    private LifeBarUI() { }

    public static LifeBarUI GetInstance
    {
        get { return instance; }
        private set { instance = value; }
    }

	private void Awake()
	{
        if (instance != null)
        {
            gameObject.SetActive(false);
            Debug.Log("This instance of " + GetType().Name + " already exists, delete the last one added");
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
	}

    /// <summary>
    /// Change the color and the front image of the lifebar and reset current value of lifebar
    /// </summary>
    /// <param name="pCurrentGame">The minigame playing</param>
    public void ChangeLifeBar(MiniGame pCurrentGame)
    {
        _playerFGLifeBar.texture = _playerLifeBarSpriteList[(int)pCurrentGame];
        _foeFGLifeBar.texture = _foeLifeBarSpriteList[(int)pCurrentGame];

        _playerLifeBarColor.color = _colorLifeBar[(int)pCurrentGame];
        _foeLifeBarColor.color = _colorLifeBar[(int)pCurrentGame];

        ChangePlayerLifeBar(5);
        ChangeFoeLifeBar(120);
    }

    /// <summary>
    /// Change lifebar slider value
    /// </summary>
    /// <param name="pSlider">Slider you want to change</param>
    /// <param name="pValue"></param>
    private void ChangeLifeBarValue(Slider pSlider, int pValue)
    {
        pSlider.value = pValue;
    }

    /// <summary>
    /// Change the player lifebar value
    /// </summary>
    /// <param name="pValue"></param>
    public void ChangePlayerLifeBar(int pValue)
    {
        ChangeLifeBarValue(_playerLifeBar, pValue);
    }

    /// <summary>
    /// Change foe lifebar value
    /// </summary>
    /// <param name="pValue"></param>
    public void ChangeFoeLifeBar(int pValue)
    {
        ChangeLifeBarValue(_foeLifeBar, pValue);
    }

    /// <summary>
    /// Change Foe slider min and max value
    /// </summary>
    /// <param name="pMin"></param>
    /// <param name="pMax"></param>
    public void ChangeFoeMinMaxLifeBar(int pMin, int pMax)
    {
    }
}
