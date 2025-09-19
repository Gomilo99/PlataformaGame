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
    public void PerderVida(float dano)
    {
        vidas -= dano;
        player.PerderVidaPJ();
        player.AplicarGolpe();
        if (vidas == 0)
        {
            //Reiniciar Nivel
            SceneManager.LoadScene(0);
        }
        EventBus<float>.Publish(GameEvent.VidaPerdida, (int)vidas);
    }
    public bool GanarVida()
    {
        if (vidas == 3) return false;

        EventBus<float>.Publish(GameEvent.VidaGanada, (int)vidas);
        vidas += 1;
        return true;
    }
}