using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    public TextMeshProUGUI puntos;
    public GameObject[] vidas;

    // Update is called once per frame
    void Update()
    {
        puntos.text = GameManager.Instance.PuntosTotales.ToString();
    }
    public void ActualizarPuntos(int PuntosTotales)
    {
        puntos.text = PuntosTotales.ToString();
    }
    public void DesactivarVida(int indice)
    {
        vidas[indice].SetActive(false);
    }
    public void ActivarVida(int indice)
    {
        vidas[indice].SetActive(true);
    }
}
