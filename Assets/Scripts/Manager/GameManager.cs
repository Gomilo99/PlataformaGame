using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public HUD hud;
    public static GameManager Instance { get; private set; }
    public CharacterController player;
    public float vidas = 3;
    public float VidasTotales;
    int actualscene;
    void Awake()
    {
        VidasTotales = vidas;
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("Más de un Game Manager en escena!");
        }
        actualscene = SceneManager.GetActiveScene().buildIndex;
    }
    public void PerderVida(float dano)
    {
        vidas -= dano;
        if (vidas == 0)
        {
            //Reiniciar Nivel
            Debug.Log("Game Over");
            SceneManager.LoadScene(actualscene);
        }
        EventBus<int>.Publish(GameEvent.VidaPerdida, (int)vidas);
    }
    public bool GanarVida()
    {
        if (vidas == VidasTotales) return false;

        EventBus<int>.Publish(GameEvent.VidaGanada, (int)vidas);
        vidas += 1;
        return true;
    }
}