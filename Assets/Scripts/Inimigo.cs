using UnityEngine;
using System.Collections;

// Controla movimento, animacoes, esquiva, ataque e sistema de vida do inimigo
public class Inimigo : MonoBehaviour
{
    private Rigidbody2D corpo; // Referencia para o componente Rigidbody2D do inimigo
    private Animator animador; // Referencia para o componente Animator do inimigo
    private Transform alvoJogador; // Referencia para o transform do jogador para perseguicao

    private int direcao = 1; // Variavel para armazenar a direcao atual do inimigo (0: cima, 1: baixo, 2: esquerda, 3: direita)
    private int direcaoFlanco = 1; // Direcao atual de flanqueamento (-1 ou 1)
    private int danoAplicavel; //Quantidade de dano aplicado conforme nivel

    private float horizontal; // Variavel para armazenar movimento horizontal calculado
    private float vertical; // Variavel para armazenar movimento vertical calculado
    private float cooldownAtaque = 0f; // Variavel para controlar o cooldown entre ataques
    private float tempoEsquiva = 0f; // Variavel para controlar o tempo de esquiva
    private float duracaoEsquiva = 0.3f; // Tempo de duracao da esquiva em segundos
    private float duracaoAtaque = 0.5f; // Tempo de duracao do ataque em segundos
    private float timerDecisaoFlanco = 0f; // Timer para controle de decisoes de flanquear
    private float intervaloDecisaoFlanco = 2f; // Intervalo para mudar direcao de fla

    private bool estaAtacando = false; // Variavel para controlar se o inimigo esta atacando
    private bool estaEsquivando = false; // Variavel para controlar se o inimigo esta esquivando
    private bool podeMover = true; // Variavel para controlar se o inimigo pode se mover
    private bool podeAtacar = true; // Variavel para controlar intervalo entre ataques

    public int vidaAtual = 5; // Vida atual do inimigo (publica para acesso de outros scripts)
    public int vidaMaxima = 5; // Vida maxima do inimigo (publica para acesso de outros scripts)
    public int nivel = 1; // Nivel do inimigo (1 a 4) que afeta aparencia e comportamento
    public int quantidadeEsquivas = 0; // Contador de vezes que o inimigo esquivou
    public int quantidadeAtaques = 0; // Contador de vezes que o inimigo atacou
    public int quantidadeDanoRecebido = 0; // Contador de vezes que o inimigo foi atingido

    public float velocidadeMaxima = 3f; // Velocidade maxima de movimentacao do inimigo (publica para ajuste no Inspector)
    public float forcaEsquiva = 8f; // Forca do impulso durante a esquiva
    public float distanciaManter = 2f; // Distancia ideal que o inimigo deve manter do jogador
    public float distanciaAtaque = 1.5f; // Distancia minima para iniciar ataque
    public float frequenciaEsquiva = 0.3f; // Frequencia de esquiva (0-1, onde 1 = sempre esquiva)

    public bool vivo;
    public bool ataqueCriaProjetil = false; // Define se o inimigo deve criar projetil ao atacar
    public bool deveFlanquear = false; // Define se o inimigo deve flanquear o jogador
    public bool podeEsquivar = false; // Define se o inimigo pode esquivar

    public GameObject projetilPrefab; // Prefab do projetil caso ataque crie projetilnco
    public BoxCollider2D areaDeAtaqueCollider; // Referencia para o BoxCollider2D da area de ataque (objeto filho)
    public GameObject moeda;

    // Tempo que a area de ataque fica ativa durante o ataque
    // private float tempoAreaAtaqueAtiva = 0.25f;

    // Metodo chamado quando o script e inicializado
    void Start()
    {
        corpo = GetComponent<Rigidbody2D>(); // Obtem o componente Rigidbody2D do GameObject
        animador = GetComponent<Animator>(); // Obtem o componente Animator do GameObject
        // areaDeAtaqueCollider = transform.Find("AreaDeAtaque").GetComponent<BoxCollider2D>(); // Encontra o objeto filho chamado "AreaDeAtaque" e obtem seu BoxCollider2D
        areaDeAtaqueCollider.enabled = false; // Desativa a colisao da area de ataque no inicio
        vivo = true;
        vidaMaxima = nivel;
        vidaAtual = vidaMaxima; // Inicializa a vida atual com o valor maximo
        danoAplicavel = nivel; // Quantidade de dano conforme o nível
        GameObject jogadorObj = GameObject.FindGameObjectWithTag("Player"); // Encontra o jogador na cena pela tag

        // Se encontrou o jogador, guarda referencia ao transform
        if (jogadorObj != null)
        {
            alvoJogador = jogadorObj.transform;
        }

        // Atualiza animacoes baseadas no nivel inicial
        //AtualizarAnimacoesPorNivel();
    }

    // Metodo chamado a cada frame
    void Update()
    {
        // Garante que a vida nao fique negativa
        if (vidaAtual < 0)
        {
            vidaAtual = 0;
        }

        if (vidaAtual == 0)
        {

            vivo = false;
        }

        if (!vivo)
        {
            //alvoJogador.transform.;
            Destroy(transform.gameObject);
            return;
        }

        // Se o inimigo pode se mover e tem alvo, processa as decisoes
        if (podeMover && alvoJogador != null)
        {
            ProcessarDecisoes();
        }
    }

    // Metodo chamado em intervalos fixos de tempo para fisica
    void FixedUpdate()
    {
        // Se o inimigo pode se mover e tem alvo, executa a movimentacao
        if (podeMover && alvoJogador != null)
        {
            Movimentar();
        }

        // Atualiza as animacoes baseadas no estado do inimigo
        AtualizarAnimacoes();

        // Gerencia o cooldown do ataque
        if (cooldownAtaque > 0)
        {
            cooldownAtaque -= Time.deltaTime;
        }

        // Gerencia o tempo de esquiva
        if (estaEsquivando)
        {
            tempoEsquiva -= Time.deltaTime;
            if (tempoEsquiva <= 0)
            {
                FinalizarEsquiva();
            }
        }

        // Atualiza timer para decisoes de flanqueamento
        if (deveFlanquear)
        {
            timerDecisaoFlanco -= Time.deltaTime;
            if (timerDecisaoFlanco <= 0)
            {
                // Escolhe nova direcao de flanqueamento aleatoriamente
                direcaoFlanco = Random.Range(0, 2) == 0 ? -1 : 1;
                timerDecisaoFlanco = intervaloDecisaoFlanco;
            }
        }
    }

    // Processa as decisoes de movimento e ataque do inimigo
    void ProcessarDecisoes()
    {

        float distancia = Vector2.Distance(transform.position, alvoJogador.position); // Calcula a distancia atual ate o jogador
        Vector2 direcaoParaJogador = (alvoJogador.position - transform.position).normalized; // Calcula a direcao para o jogador
        AtualizarDirecao(direcaoParaJogador); // Atualiza a direcao do inimigo baseada na posicao do jogador

        // Se esta muito perto, decide se esquiva
        if (podeEsquivar && distancia < distanciaAtaque && !estaEsquivando && !estaAtacando &&
            Random.value < frequenciaEsquiva)
        {
            IniciarEsquiva();
            return;
        }

        if (podeAtacar)
        {
            // Se esta na distancia de ataque e pode atacar, inicia ataque
            if (distancia <= distanciaAtaque && !estaAtacando && !estaEsquivando && cooldownAtaque <= 0)
            {
                podeAtacar = false;
                Invoke("IniciarAtaque", 1);
                return;
            }
        }

        CalcularMovimento(direcaoParaJogador, distancia); // Calcula movimento para manter distancia ideal
    }

    // Calcula o movimento baseado na posicao do jogador
    void CalcularMovimento(Vector2 direcaoParaJogador, float distancia)
    {
        // Se esta muito longe, move-se em direcao ao jogador
        if (distancia > distanciaManter)
        {
            horizontal = direcaoParaJogador.x;
            vertical = direcaoParaJogador.y;
        }
        // Se esta muito perto, move-se para longe do jogador
        else if (distancia < distanciaManter - 0.5f)
        {
            horizontal = -direcaoParaJogador.x;
            vertical = -direcaoParaJogador.y;
        }
        // Se esta na distancia ideal, flanqueia ou para
        else
        {
            if (deveFlanquear)
            {
                // Calcula direcao perpendicular para flanquear
                Vector2 direcaoFlanqueamento = new Vector2(-direcaoParaJogador.y, direcaoParaJogador.x) * direcaoFlanco;
                horizontal = direcaoFlanqueamento.x;
                vertical = direcaoFlanqueamento.y;
            }
            else
            {
                // Para de se mover
                horizontal = 0;
                vertical = 0;
            }
        }
    }

    // Move o inimigo baseado nos calculos
    void Movimentar()
    {
        // Se esta atacando ou esquivando, nao permite movimento adicional
        if (estaAtacando || estaEsquivando)
        {
            return;
        }

        // Cria um vetor de movimento baseado nos calculos
        Vector2 movimento = new Vector2(horizontal, vertical);

        // Normaliza o vetor para movimento diagonal nao ser mais rapido
        if (movimento.magnitude > 1)
        {
            movimento.Normalize();
        }

        // Aplica a velocidade ao Rigidbody2D
        corpo.linearVelocity = movimento * velocidadeMaxima;
    }

    // Atualiza a direcao do inimigo baseada na posicao do jogador
    void AtualizarDirecao(Vector2 direcaoParaJogador)
    {
        // Prioridade: vertical sobre horizontal para direcoes diagonais
        if (Mathf.Abs(direcaoParaJogador.y) > Mathf.Abs(direcaoParaJogador.x))
        {
            // Para cima
            if (direcaoParaJogador.y > 0)
            {
                direcao = 0;
            }
            // Para baixo
            else
            {
                direcao = 1;
            }
        }
        else
        {
            // Para esquerda
            if (direcaoParaJogador.x < 0)
            {
                direcao = 2;
            }
            // Para direita
            else
            {
                direcao = 3;
            }
        }
    }

    // Atualiza os parametros do Animator baseados no estado do inimigo
    void AtualizarAnimacoes()
    {
        // Reseta todos os parametros booleanos do Animator
        animador.SetBool("paradoCima", false);
        animador.SetBool("paradoBaixo", false);
        animador.SetBool("paradoEsq", false);
        animador.SetBool("paradoDir", false);
        animador.SetBool("andandoCima", false);
        animador.SetBool("andandoBaixo", false);
        animador.SetBool("andandoEsq", false);
        animador.SetBool("andandoDir", false);
        animador.SetBool("esquivandoCima", false);
        animador.SetBool("esquivandoBaixo", false);
        animador.SetBool("esquivandoEsq", false);
        animador.SetBool("esquivandoDir", false);
        animador.SetBool("atacandoCima", false);
        animador.SetBool("atacandoBaixo", false);
        animador.SetBool("atacandoEsq", false);
        animador.SetBool("atacandoDir", false);

        // Determina qual animacao deve ser reproduzida baseada no estado e direcao
        if (estaAtacando)
        {
            switch (direcao)
            {
                case 0: animador.SetBool("atacandoCima", true); break;
                case 1: animador.SetBool("atacandoBaixo", true); break;
                case 2: animador.SetBool("atacandoEsq", true); break;
                case 3: animador.SetBool("atacandoDir", true); break;
            }
        }
        else if (estaEsquivando)
        {
            switch (direcao)
            {
                case 0: animador.SetBool("esquivandoCima", true); break;
                case 1: animador.SetBool("esquivandoBaixo", true); break;
                case 2: animador.SetBool("esquivandoEsq", true); break;
                case 3: animador.SetBool("esquivandoDir", true); break;
            }
        }
        else if (corpo.linearVelocity.magnitude > 0.1f)
        {
            switch (direcao)
            {
                case 0: animador.SetBool("andandoCima", true); break;
                case 1: animador.SetBool("andandoBaixo", true); break;
                case 2: animador.SetBool("andandoEsq", true); break;
                case 3: animador.SetBool("andandoDir", true); break;
            }
        }
        else
        {
            switch (direcao)
            {
                case 0: animador.SetBool("paradoCima", true); break;
                case 1: animador.SetBool("paradoBaixo", true); break;
                case 2: animador.SetBool("paradoEsq", true); break;
                case 3: animador.SetBool("paradoDir", true); break;
            }
        }
    }

    // Atualiza animacoes baseadas no nivel do inimigo
    void AtualizarAnimacoesPorNivel()
    {
        // Define parametro de nivel no Animator para controlar variacoes visuais
        //animador.SetInteger("nivel", nivel);
    }

    // Inicia a acao de esquiva
    void IniciarEsquiva()
    {
        // Marca que o inimigo esta esquivando
        estaEsquivando = true;

        // Configura o tempo de duracao da esquiva
        tempoEsquiva = duracaoEsquiva;

        // Incrementa o contador de esquivas
        quantidadeEsquivas++;

        // Aplica um impulso na direcao oposta ao jogador
        Vector2 direcaoEsquiva = Vector2.zero;

        // Calcula direcao oposta ao jogador
        if (alvoJogador != null)
        {
            Vector2 direcaoOposta = (transform.position - alvoJogador.position).normalized;
            direcaoEsquiva = direcaoOposta;
        }
        else
        {
            // Fallback: usa direcao atual se nao tem alvo
            switch (direcao)
            {
                case 0: direcaoEsquiva = Vector2.down; break;
                case 1: direcaoEsquiva = Vector2.up; break;
                case 2: direcaoEsquiva = Vector2.right; break;
                case 3: direcaoEsquiva = Vector2.left; break;
            }
        }

        // Aplica forca de impulso na esquiva
        corpo.AddForce(direcaoEsquiva * forcaEsquiva, ForceMode2D.Impulse);

        // Impede movimento durante a esquiva
        podeMover = false;
    }

    // Finaliza a acao de esquiva
    void FinalizarEsquiva()
    {
        // Marca que o inimigo nao esta mais esquivando
        estaEsquivando = false;

        // Permite movimento novamente
        podeMover = true;

        // Para o movimento residual da esquiva
        corpo.linearVelocity = Vector2.zero;
    }

    // Inicia a acao de ataque
    void IniciarAtaque()
    {
        // Marca que o inimigo esta atacando
        estaAtacando = true;

        // Configura o cooldown do ataque
        cooldownAtaque = duracaoAtaque;

        // Incrementa o contador de ataques
        quantidadeAtaques++;

        // Impede movimento durante o ataque
        podeMover = false;

        // Para o movimento do inimigo
        corpo.linearVelocity = Vector2.zero;

        // Se ataque cria projetil, instancia um
        if (ataqueCriaProjetil && projetilPrefab != null)
        {
            CriarProjetil();
        }

        // Inicia a corrotina para ativar a area de ataque
        StartCoroutine(AtivarAreaDeAtaque());

        // Invoca o resfriamento entre ataques
        int intervaloAtaque = 20 / nivel;

        if (intervaloAtaque < 1)
        {
            intervaloAtaque = 1;
        }

        Invoke("ResfriamentoAtaque", intervaloAtaque);
    }

    void ResfriamentoAtaque()
    {
        FinalizarAtaque();
    }


    // Cria um projetil se configurado
    void CriarProjetil()
    {
        // Instancia o projetil na posicao do inimigo
        GameObject projetil = Instantiate(projetilPrefab, transform.position, Quaternion.identity);

        // Obtem componente Rigidbody2D do projetil
        Rigidbody2D rbProjetil = projetil.GetComponent<Rigidbody2D>();

        // Calcula direcao para o jogador
        Vector2 direcaoProjetil = Vector2.zero;
        if (alvoJogador != null)
        {
            direcaoProjetil = (alvoJogador.position - transform.position).normalized;
        }
        else
        {
            // Usa direcao atual se nao tem alvo
            switch (direcao)
            {
                case 0: direcaoProjetil = Vector2.up; break;
                case 1: direcaoProjetil = Vector2.down; break;
                case 2: direcaoProjetil = Vector2.left; break;
                case 3: direcaoProjetil = Vector2.right; break;
            }
        }

        // Aplica forca ao projetil
        if (rbProjetil != null)
        {
            rbProjetil.AddForce(direcaoProjetil * 10f, ForceMode2D.Impulse);
        }

        Movimentar();
        Invoke("FinalizarAtaque", 2);
    }

    void FinalizarAtaque()
    {
        // Finaliza o ataque
        estaAtacando = false;
        podeAtacar = true;
    }

    // Corrotina para ativar a area de ataque temporariamente
    IEnumerator AtivarAreaDeAtaque()
    {
        // Aguarda um quarto de segundo antes de ativar a area de ataque
        yield return new WaitForSeconds(0.45f);

        // Ativa o collider da area de ataque
        areaDeAtaqueCollider.enabled = true;

        // Aguarda um quarto de segundo com a area ativa
        yield return new WaitForSeconds(0.45f);

        // Desativa o collider da area de ataque
        areaDeAtaqueCollider.enabled = false;

        // Permite movimento novamente
        podeMover = true;
    }

    // Metodo chamado quando a area de ataque colide com outro objeto
    void OnTriggerEnter2D(Collider2D outro)
    {
        // Verifica se a area de ataque esta ativa
        if (areaDeAtaqueCollider.enabled)
        {
            // Verifica se o objeto colidido e o jogador
            if (outro.gameObject.CompareTag("Player"))
            {
                // Tenta obter o script Jogador do objeto colidido
                Jogador scriptJogador = outro.gameObject.GetComponent<Jogador>();

                // Se o objeto tem o script Jogador, causa dano
                if (scriptJogador != null)
                {
                    scriptJogador.DanoAoJogador(danoAplicavel);
                }
            }
        }
    }

    // Metodo para causar dano ao inimigo
    public void ReceberDano(int quantidade)
    {
        // Reduz a vida atual
        vidaAtual -= quantidade;

        // Incrementa o contador de dano recebido
        quantidadeDanoRecebido++;

        // Garante que a vida nao fique negativa
        if (vidaAtual < 0)
        {
            vidaAtual = 0;
        }

        // Se vida chegou a zero, destroi o inimigo
        if (vidaAtual <= 0)
        {
            Destroy(gameObject);
        }
    }

    // Metodo para restaurar vida do inimigo
    public void Curar(int quantidade)
    {
        // Aumenta a vida atual
        vidaAtual += quantidade;

        // Garante que a vida nao ultrapasse o maximo
        if (vidaAtual > vidaMaxima)
        {
            vidaAtual = vidaMaxima;
        }
    }

    // Metodo para definir o nivel do inimigo (pode ser chamado externamente)
    public void DefinirNivel(int novoNivel)
    {
        // Limita o nivel entre 1 e 4
        nivel = Mathf.Clamp(novoNivel, 1, 4);

        // Ajusta estatisticas baseadas no nivel
        vidaMaxima = 3 + nivel * 2;
        vidaAtual = vidaMaxima;
        velocidadeMaxima = 2f + nivel * 0.5f;

        // Atualiza animacoes para refletir novo nivel
        //AtualizarAnimacoesPorNivel();
    }

    void CriarMoeda()
    {
        // Instancia o projetil na posicao do inimigo
        GameObject moeda = Instantiate(projetilPrefab, transform.position, Quaternion.identity);
    }
}