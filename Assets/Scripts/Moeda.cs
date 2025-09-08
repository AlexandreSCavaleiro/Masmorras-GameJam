using UnityEngine;

public class Moeda : MonoBehaviour
{

    private Rigidbody2D corpo; // Referencia para o componente Rigidbody2D
    [HideInInspector] public Vector3 direcao;
    [HideInInspector] public int sentido;
    public float velocidade = 20;
    public bool MoedaOriginal = false;

    private Transform alvoJogador; // Referencia para o transform do jogador para perseguicao

    private float horizontal; // Variavel para armazenar movimento horizontal calculado
    private float vertical; // Variavel para armazenar movimento vertical calculado

    // Metodo chamado quando o script e inicializado
    void Start()
    {
        GameObject jogadorObj = GameObject.FindGameObjectWithTag("Player"); // Encontra o jogador na cena pela tag
        corpo = GetComponent<Rigidbody2D>(); // Obtem o componente Rigidbody2D do GameObject

        // Se encontrou o jogador, guarda referencia ao transform
        if (jogadorObj != null)
        {
            alvoJogador = jogadorObj.transform;
        }
        if (!MoedaOriginal)
        {
            Destroy(transform.gameObject, 3f);
        }

        // Tenta obter o script Jogador do objeto colidido
        Jogador scriptJogador = jogadorObj.gameObject.GetComponent<Jogador>();

        // Se o objeto tem o script Jogador, adiciona um ponto
        if (scriptJogador != null)
        {
            scriptJogador.AdicionarPontos(1);
        }

    }

    // Update is called once per frame
    void Update()
    {
        ProcessarDecisoes();

        Movimentar();
    }

    void ProcessarDecisoes()
    {

        Vector2 direcaoParaJogador = (alvoJogador.position - transform.position).normalized; // Calcula a direcao para o jogador

        CalcularMovimento(direcaoParaJogador); // Calcula movimento
    }

    
    void CalcularMovimento(Vector2 direcaoParaJogador)
    {
        horizontal = direcaoParaJogador.x;
        vertical = direcaoParaJogador.y;
    }

    void Movimentar()
    {

        // Cria um vetor de movimento baseado nos calculos
        Vector2 movimento = new Vector2(horizontal, vertical);


        // Normaliza o vetor para movimento diagonal nao ser mais rapido
        if (movimento.magnitude > 1)
        {
            movimento.Normalize();
        }

        Vector2 vector2 = new Vector2(horizontal, vertical) * velocidade * Time.deltaTime;
        Vector3 movi3 = new Vector3(vector2.x, vector2.y, 0);

        transform.position += movi3;

        // corpo.linearVelocity = movimento * velocidade;
    }



    void OnTriggerEnter2D(Collider2D outro)
    {
        Debug.Log("Moeda tocou" + outro);

        // Verifica se o objeto colidido est� na layer "Player"
        if (outro.gameObject.CompareTag("Player"))
        {
            Destroy(transform.gameObject);
        }
    }
}

