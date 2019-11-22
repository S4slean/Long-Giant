using System;
using System.Threading.Tasks;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class SnackbarManager : MonoBehaviour
{
    public GameObject m_snackbar;

    private bool m_isShown = false;
    private RectTransform rt;
    private UnityEngine.UI.Text m_textObject;

    private void Awake()
    {
        rt = m_snackbar.GetComponent<RectTransform>();
        m_textObject = rt.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>();
    }

    public void Open(string text)
    {
        if (m_isShown)
        {
            rt.DOAnchorPosY(rt.rect.height, 1f).SetEase(Ease.InOutBack).OnComplete(() =>
            {
                m_textObject.text = text;
                m_isShown = false;
                rt.DOAnchorPosY(0, 1f).SetEase(Ease.InOutBack).OnComplete(() =>
                {
                    m_isShown = true;
                });
            });
            return;
        }

        m_textObject.text = text;
        rt.DOAnchorPosY(0, 1f).SetEase(Ease.InOutBack);
        m_isShown = true;
    }

    public void Close()
    {
        if (!m_isShown)
        {
            return;
        }

        rt.DOAnchorPosY(rt.rect.height, 1f).SetEase(Ease.InOutBack).OnComplete(() =>
        {
            m_isShown = false;
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Open("Set the center of the world.");
        }
    }
}