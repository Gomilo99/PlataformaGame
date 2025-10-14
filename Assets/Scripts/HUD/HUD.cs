using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    public TextMeshProUGUI puntos;
    public int puntosTotales = 0;

    private void Start()
    {
        // Suscribirse al evento de recogida de monedas
        EventBus<int>.Subscribe(GameEvent.CoinCollected, HandleOnCoinCollected);

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
    
}
