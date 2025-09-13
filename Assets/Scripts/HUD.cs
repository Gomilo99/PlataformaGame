using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    public TextMeshProUGUI puntos;
    public int puntosTotales = 0;
    public GameObject[] vidas;

    private void Start()
    {
        // Suscribirse al evento de recogida de monedas
        EventBus<int>.Subscribe(GameEvent.CoinCollected, HandleOnCoinCollected);

        EventBus<int>.Subscribe(GameEvent.VidaGanada, HandleOnVidaActivada);
        EventBus<int>.Subscribe(GameEvent.VidaPerdida, HandleOnVidaDesactivada);

    }
    // Update is called once per frame
    void Update()
    {
        //puntos.text = GameManager.Instance.PuntosTotales.ToString();
    }
    private void HandleOnCoinCollected(int coinValue)
    {
        puntosTotales += coinValue;
        Debug.Log($"Coins collected: {puntosTotales}");
        puntos.text = puntosTotales.ToString();
    }

    public void HandleOnVidaDesactivada(int indice)
    {
        vidas[indice].SetActive(false);
    }
    public void HandleOnVidaActivada(int indice)
    {
        vidas[indice].SetActive(true);
    }
}
