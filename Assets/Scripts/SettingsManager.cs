using DG.Tweening;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public GameObject m_soundMuteButton;
    public GameObject m_restartButton;
    public GameObject m_closeButton;
    public Sprite m_soundMuteImageActive;
    public GameObject m_settingsGear;

    private bool m_isPanelOpen = false;
    private bool m_audioMuted = false;
    private UnityEngine.UI.Image m_soundMuteImageComponent;
    private Sprite m_soundMuteImageInactive;
    private Sequence sequence;

    private void Awake()
    {
        sequence = DOTween.Sequence();
        const float time = 0.27f;
        const float buttonYOffset = 8;
        sequence.Append(m_settingsGear.transform.DORotate(new Vector3(0, 0, 180), time * 3.0f));
        sequence.Join(m_soundMuteButton.transform.DOMoveY(-100 - buttonYOffset, time));
        sequence.Join(m_restartButton.transform.DOMoveY(-200 - buttonYOffset * 2, time).SetDelay(time / 2.0f));
        sequence.Join(m_closeButton.transform.DOMoveY(-300 - buttonYOffset * 3, time).SetDelay(time / 3.0f));
        sequence.SetEase(Ease.InOutBack);
        sequence.SetAutoKill(false);
        sequence.SetRelative(true);
        sequence.OnPlay(() => {
            if (m_isPanelOpen)
            {
                m_soundMuteButton.SetActive(true);
                m_restartButton.SetActive(true);
                m_closeButton.SetActive(true);
            }
        });
        sequence.OnRewind(() => {
            m_soundMuteButton.SetActive(false);
            m_restartButton.SetActive(false);
            m_closeButton.SetActive(false);
        });
        sequence.Pause();

        m_soundMuteImageComponent = m_soundMuteButton.GetComponent<UnityEngine.UI.Image>();
        m_soundMuteImageInactive = m_soundMuteImageComponent.sprite;
        m_soundMuteButton.SetActive(false);
        m_restartButton.SetActive(false);
        m_closeButton.SetActive(false);
    }

    public void OnSettingsButtonClicked()
    {
        m_isPanelOpen = !m_isPanelOpen;

        if (m_isPanelOpen)
        {
            sequence.PlayForward();
        }
        else
        {
            sequence.PlayBackwards();
        }
    }

    public void OnSoundMuteButtonClicked()
    {
        m_audioMuted = !m_audioMuted;
        AudioListener.volume = m_audioMuted ? 0.0f : 1.0f;

        m_soundMuteImageComponent.sprite = m_audioMuted ? m_soundMuteImageActive : m_soundMuteImageInactive;
    }

    public void OnRestartbuttonClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnClosebuttonClicked()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
