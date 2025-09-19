using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DesplazarEnemigoHorizontal : MonoBehaviour
{
    public float minX;
    public float maxX;
    public float tiempoEspera = 2f;
    public float velocidadMove = 1f;
    [SerializeField] private GameObject _LugarObjetivo;
    

    
    void Start()
    {
        UpdateObjetivo();
        StartCoroutine(Patrullar());
    }
    private void UpdateObjetivo()
    {
        // Si es la primera vez inicia el patrullaje hacia la izquierda
        if (_LugarObjetivo == null)
        {
            _LugarObjetivo = new GameObject("Sitio_objetivo");
            _LugarObjetivo.transform.position = new Vector2(minX, transform.position.y);
            transform.localScale = new Vector3(-1, 1, 1);
            return;
        }
        // Iniciar patrullaje hacia la derecha
        if (_LugarObjetivo.transform.position.x == minX)
        {
            _LugarObjetivo.transform.position = new Vector2(maxX, transform.position.y);
            transform.localScale = new Vector3(1, 1, 1);
        }
        // Cambio de sentido de derecha a izquierda
        else if (_LugarObjetivo.transform.position.x == maxX)
        {
            _LugarObjetivo.transform.position = new Vector2(minX, transform.position.y);
            transform.localScale = new Vector3(-1, 1, 1);
        }

    }
    private IEnumerator Patrullar()
    {
        // Co-Rutina para mover el enemigo
        while (Vector2.Distance(transform.position, _LugarObjetivo.transform.position) > 0.05f)
        {
            // Se desplazará hasta el sitio objetivo
            Vector2 direccion = _LugarObjetivo.transform.position - transform.position;
            transform.Translate(direccion.normalized * velocidadMove * Time.deltaTime);
            yield return null;
        }

        // En este punto, se alcanzó el objetivo, se reestablece nuestra posicion en la del objetivo.
        Debug.Log("Se alcanzo el Objetivo.");
        transform.position = new Vector2(_LugarObjetivo.transform.position.x, transform.position.y);

        // Esperamos un momento antes de volver a movernos.
        Debug.Log("Esperando " + tiempoEspera + " segundos");
        yield return new WaitForSeconds(tiempoEspera);

        // Se espera lo necesario para que termine y vuelve a empezar el movimiento.
        UpdateObjetivo();
        StartCoroutine(Patrullar());
    }
    
    
    

}