using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public HUD hud;
    public static GameManager Instance { get; private set; }
    public int PuntosTotales { get; private set; }
    public int vidas = 3;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("Más de un Game Manager en escena!");
        }
    }
    public void SumarPuntos(int punstosASumar)
    {
        PuntosTotales += punstosASumar;
        hud.ActualizarPuntos(PuntosTotales);
        Debug.Log("puntosTotales");
    }
    public void PerderVida()
    {
        vidas -= 1;
        if (vidas == 0)
        {
            //Reiniciar Nivel
            SceneManager.LoadScene(0);
        }
        hud.DesactivarVida(vidas);
    }
    public bool GanarVida()
    {
        if (vidas == 3) return false;

        hud.ActivarVida(vidas);
        vidas += 1;
        return true;
    }
}
