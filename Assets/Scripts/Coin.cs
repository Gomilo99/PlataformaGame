using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public int valor = 1;
    public AudioClip sonidoMoneda;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.SumarPuntos(valor);
            Debug.Log("Colision");
            Destroy(this.gameObject);
            AudioManager.Instance.ReproducirSonido(sonidoMoneda);
        }

    }
}
