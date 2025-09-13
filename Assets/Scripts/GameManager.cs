using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public HUD hud;
    public static GameManager Instance { get; private set; }
    public CharacterController player;
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
    public void PerderVida()
    {
        vidas -= 1;
        player.PerderVidaPJ();
        if (vidas == 0)
        {
            //Reiniciar Nivel
            SceneManager.LoadScene(0);
        }
        EventBus<int>.Publish(GameEvent.VidaPerdida, vidas);
    }
    public bool GanarVida()
    {
        if (vidas == 3) return false;

        EventBus<int>.Publish(GameEvent.VidaGanada, vidas);
        vidas += 1;
        return true;
    }
}
