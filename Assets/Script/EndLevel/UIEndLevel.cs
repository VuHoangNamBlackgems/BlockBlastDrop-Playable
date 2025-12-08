using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIEndLevel : MonoBehaviour
{
    public static UIEndLevel Instance;
    public CanvasGroup canvasEndLevel;
    [SerializeField] GameObject WinPanel, LosePanel;
    [SerializeField] Button BtnPlayMore;
    [SerializeField] Button BtnTryAgain;

    [SerializeField] ParticleSystem[] lsConfetti;

    BoardController boardController => BoardController.Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (BtnPlayMore)
        {
            BtnPlayMore.onClick.RemoveAllListeners();
            BtnPlayMore.onClick.AddListener(OnStore);
        }
        if (BtnTryAgain)
        {
            BtnTryAgain.onClick.RemoveAllListeners();
            BtnTryAgain.onClick.AddListener(OnStore);
        }
    }

    public void Show(bool isWin)
    {
        BtnPlayMore.transform.DOKill();
        BtnPlayMore.transform.localScale = Vector3.one;
        BtnTryAgain.transform.DOKill();
        BtnTryAgain.transform.localScale = Vector3.one;
        WinPanel.SetActive(isWin);
        LosePanel.SetActive(!isWin);
        if (isWin)
            AudioController.Instance.LevelWinSound();
        else
            AudioController.Instance.LevelFailSound();

        gameObject.SetActive(true);
        canvasEndLevel.DOFade(1, 0.2f).OnComplete(() =>
        {
            if (isWin)
            {
                OnConfetti();
                BtnPlayMore.transform.DOScale(1.03f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }
            else
            {
                BtnTryAgain.transform.DOScale(1.03f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }
        });
    }
    public void Hide()
    {
        gameObject.SetActive(false);
        canvasEndLevel.alpha = 0;
    }

    void OnConfetti()
    {
        foreach (var confetti in lsConfetti)
            confetti.Play();
    }

    void OnStore()
    {

    }
}
