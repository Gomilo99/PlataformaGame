using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Example : MonoBehaviour
{
    public RectTransform panel;
    public Image image;
    public float position = 500;
    public float duration = 1f;
    public Ease ease = Ease.InExpo;
    float escala = 1f;
    public Color color;

    [ContextMenu("MoveRight")]
    public void MoveRight()
    {
        panel.DOAnchorPosX(position, duration).SetEase(ease);
    }
    [ContextMenu("MoveLeft")]
    public void MoveLeft()
    {
        panel.DOAnchorPosX(-position, duration).SetEase(ease);
    }
    [ContextMenu("Scale")]
    public void Scale()
    {

        escala += 1f;
        panel.DOScale(escala, duration).SetEase(ease);
    }
    [ContextMenu("ScaleReset")]
    public void ScaleReset()
    {
        escala = 1f;
        panel.DOScale(escala, duration).SetEase(ease);
    }
    [ContextMenu("Color")]
    public void ColorChange()
    {
        image.DOColor(color, duration).SetEase(ease);

    }
    [ContextMenu("FadeOut")]
    public void FadeOut()
    {
        image.DOFade(0, duration).SetEase(ease);
    }
    [ContextMenu("FadeIn")]
    public void FadeIn()
    {
        image.DOFade(1, duration).SetEase(ease);
    }
    [ContextMenu("Secuence")]
    public void Secuence()
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(panel.DOAnchorPosX(position, duration).SetEase(ease));
        mySequence.Append(panel.DOScale(escala, duration).SetEase(ease).SetLoops(2, LoopType.Yoyo));
        mySequence.Append(image.DOColor(color, duration).SetEase(ease));
        mySequence.Append(image.DOFade(0, duration).SetEase(ease));
        mySequence.Append(image.DOFade(1, duration).SetEase(ease));
        mySequence.Join(panel.DOAnchorPosX(-position, duration).SetEase(ease));
        mySequence.JoinCallback(EndSequence);

        mySequence.Play();
    }
    public void EndSequence()
    {
        Debug.Log("Animacion Finalizada");
    }
}
