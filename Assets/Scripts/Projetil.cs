using UnityEngine;

public class Projetil : MonoBehaviour
{
    public Vector3 direcao;
    public float velocidade;
    public bool TiroOriginal = false;

    private void Start()
    {
        if (!TiroOriginal)
        {
            Destroy(transform.gameObject, 0.2f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += direcao * velocidade * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Projetilc tocou" + other);

        // Verifica se o objeto colidido está na layer "Moeda"
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstaculo"))
        {
            //Destruir o outro bojeto
            Destroy(other.transform.gameObject);
        }

        Destroy(transform.gameObject);

    }
}
