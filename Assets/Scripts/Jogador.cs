using UnityEngine;
using System.Collections;

// Controla movimento, animacoes, esquiva, ataque e sistema de vida
public class Jogador : MonoBehaviour
{
    
    private Rigidbody2D corpo; // Referencia para o componente Rigidbody2D do jogador
    private BoxCollider2D contato; // Area de contato do comrpo do personagem
    private Animator animador; // Referencia para o componente Animator do jogador
    private GameObject areaDeAtaque; // Referencia para area de ataque (objeto filho)
    private BoxCollider2D areaDeAtaqueCollider; // Referencia para o BoxCollider2D da area de ataque (objeto filho)
    private int direcao = 1; // Variavel para armazenar a direcao atual do jogador (0: cima, 1: baixo, 2: esquerda, 3: direita)
    private bool estaAtacando = false; // Variavel para controlar se o jogador esta atacando
    private bool estaEsquivando = false;// Variavel para controlar se o jogador esta esquivando
    private bool podeMover = true; // Variavel para controlar se o jogador pode se mover
    private float horizontal; // Variavel para armazenar input horizontal
    private float vertical; // Variavel para armazenar input vertical
    private float cooldownAtaque = 0.35f; // Variavel para controlar o cooldown entre ataques
    private float tempoEsquiva = 0f; // Variavel para controlar o tempo de esquiva
    public float velocidadeMovimento = 5f; // Velocidade maxima de movimentacao do jogador
    public float forcaEsquiva = 10f; // Forca do impulso durante a esquiva
    public bool vivo;  // Variavel publica para saber se está vivo ou morto
    public int vidaAtual = 10; // Vida atual do jogador
    public int vidaMaxima = 10; // Vida maxima do jogador
    public int focaAtaque; // Foça de ataque do jogador
    public int quantidadeEsquivas = 0; // Contador de vezes que o jogador esquivou
    public int quantidadeAtaques = 0; // Contador de vezes que o jogador atacou
    public int quantidadeDanoRecebido = 0; // Contador de vezes que o jogador foi atingido
    private float duracaoEsquiva = 0.3f; // Tempo de duracao da esquiva em segundos
    private float duracaoAtaque = 0.5f; // Tempo de duracao do ataque em segundos


    // Metodo chamado quando o script e inicializado
    void Start()
    {        
        corpo = GetComponent<Rigidbody2D>(); // Obtem o componente Rigidbody2D do GameObject
        contato = GetComponent<BoxCollider2D>();
        animador = GetComponent<Animator>(); // Obtem o componente Animator do GameObject
        
        areaDeAtaqueCollider = transform.Find("AreaDeAtaque").GetComponent<BoxCollider2D>(); // Encontra o objeto filho chamado "AreaDeAtaque" e obtem seu BoxCollider2D
        areaDeAtaque = GameObject.Find("AreaDeAtaque");
        areaDeAtaqueCollider.enabled = false; // Desativa a colisao da area de ataque no inicio

        vidaAtual = vidaMaxima; // Inicializa a vida atual com o valor maximo
        vivo = true; // inicia o personagem vivo
        focaAtaque = 1; // Foça de ataque inicial
		
		RessetAnim();
		// Atualiza as animacoes baseadas no estado do jogador
        AtualizarAnimacoes();
    }

    // Metodo chamado a cada frame
    void Update()
    {
        if (!vivo)
        {
            return;
        }

        // Se o jogador pode se mover, processa as entradas
        if (podeMover)
        {
            ProcessarEntradas();
        }

        // Garante que a vida nao fique negativa
        if (vidaAtual < 0)
        {
            vidaAtual = 0;
        }

        if (vidaAtual == 0)
        {

            vivo = false;
        }
    }

    // Metodo chamado em intervalos fixos de tempo para fisica
    void FixedUpdate()
    {
        if (!vivo)
        {
            return;
        }

        // Se o jogador pode se mover, executa a movimentacao
        if (podeMover)
        {
            Movimentar();
        }
		
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
    }

    // Processa as entradas do teclado/controle
    void ProcessarEntradas()
    {
        // Obtem input horizontal (A/D ou setas horizontais)
        horizontal = Input.GetAxisRaw("Horizontal");
        
        // Obtem input vertical (W/S ou setas verticais)
        vertical = Input.GetAxisRaw("Vertical");
        
        // Atualiza a direcao do jogador baseada nos inputs
        AtualizarDirecao();
        
        // Verifica se a tecla de esquiva (espaço) foi pressionada
        if (Input.GetKeyDown(KeyCode.Space) && !estaEsquivando && !estaAtacando)
        {
            IniciarEsquiva();
            return;
        }
        
        // Verifica se a tecla de ataque (J) foi pressionada
        if (Input.GetKeyDown(KeyCode.J) && !estaAtacando && !estaEsquivando && cooldownAtaque <= 0)
        {
            IniciarAtaque();
        }
    }

    // Move o jogador baseado nos inputs
    void Movimentar()
    {
        // Se esta atacando ou esquivando, nao permite movimento adicional
        if (estaAtacando || estaEsquivando)
        {
            return;
        }

        // Cria um vetor de movimento baseado nos inputs
        Vector2 movimento = new Vector2(horizontal, vertical);
        
        // Normaliza o vetor para movimento diagonal nao ser mais rapido
        if (movimento.magnitude > 1)
        {
            movimento.Normalize();
        }
		
        // Aplica a velocidade ao Rigidbody2D
        corpo.linearVelocity = movimento * velocidadeMovimento;
    }

    // Atualiza a direcao do jogador baseada nos inputs
    void AtualizarDirecao()
    {
        int direacaoAnterior = direcao;

        // Se nao ha input, mantem a direcao atual
        if (horizontal == 0 && vertical == 0)
        {
            return;
        }
        
        // Prioridade: vertical sobre horizontal para direcoes diagonais
        if (vertical != 0)
        {
            // Para cima
            if (vertical > 0)
            {
                direcao = 0;
                areaDeAtaque.transform.position = new Vector2(transform.position.x + 0, transform.position.y + 0.5f);
            }
            // Para baixo
            else
            {
                direcao = 1;
                areaDeAtaque.transform.position = new Vector2(transform.position.x + 0, transform.position.y - 0.5f);
            }
        }
        else if (horizontal != 0)
        {
            // Para esquerda
            if (horizontal < 0)
            {
                direcao = 2;
                areaDeAtaque.transform.position = new Vector2(transform.position.x - 0.4f, transform.position.y + 0);
            }
            // Para direita
            else
            {
                direcao = 3;
                areaDeAtaque.transform.position = new Vector2(transform.position.x + 0.4f, transform.position.y + 0);
            }
        }

        if (direcao != direacaoAnterior)
        {
            RessetAnim();
			// Atualiza as animacoes baseadas no estado do jogador
			AtualizarAnimacoes();
        }
    }

    // Reseta todos os parametros booleanos do Animator
    void RessetAnim()
    {
        
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
    }

    // Atualiza os parametros do Animator baseados no estado do jogador
    void AtualizarAnimacoes()
    {
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
            return;
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
            return;
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
            return;
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

    // Inicia a acao de esquiva
    void IniciarEsquiva()
    {
        // Marca que o jogador esta esquivando
        estaEsquivando = true;
		
		// Atualiza as animacoes baseadas no estado do jogador
		AtualizarAnimacoes();
			
        // Impede movimento durante a esquiva
        podeMover = false;

        // Para o movimento residual
        corpo.linearVelocity = Vector2.zero;

        RessetAnim();
        AtualizarAnimacoes();

        // Configura o tempo de duracao da esquiva
        tempoEsquiva = duracaoEsquiva;
        
        // Incrementa o contador de esquivas
        quantidadeEsquivas++;
        
        // Aplica um impulso na direcao atual
        Vector2 direcaoEsquiva = Vector2.zero;
        
        switch (direcao)
        {
            case 0: direcaoEsquiva = Vector2.up; break;
            case 1: direcaoEsquiva = Vector2.down; break;
            case 2: direcaoEsquiva = Vector2.left; break;
            case 3: direcaoEsquiva = Vector2.right; break;
        }
        
        // Aplica forca de impulso na esquiva
        corpo.AddForce(direcaoEsquiva * forcaEsquiva, ForceMode2D.Impulse);
    }

    // Finaliza a acao de esquiva
    void FinalizarEsquiva()
    {
        // Marca que o jogador nao esta mais esquivando
        estaEsquivando = false;
        
        // Permite movimento novamente
        podeMover = true;
        
        // Para o movimento residual da esquiva
        corpo.linearVelocity = Vector2.zero;

        RessetAnim();
        AtualizarAnimacoes();
    }

    // Inicia a acao de ataque
    void IniciarAtaque()
    {
        if (estaAtacando)
        {
            return;
        }
		RessetAnim();
		
        // Marca que o jogador esta atacando
        estaAtacando = true;

		// Atualiza as animacoes baseadas no estado do jogador
		AtualizarAnimacoes();
			
        // Configura o cooldown do ataque
        cooldownAtaque = duracaoAtaque;

        // Incrementa o contador de ataques
        quantidadeAtaques++;

        // Impede movimento durante o ataque
        podeMover = false;

        // Para o movimento do jogador
        corpo.linearVelocity = Vector2.zero;

        // Aguarda um quarto de segundo antes de ativar a area de ataque
        Invoke("AtivarAreaDeAtaque", 0.15f);
    }

    // Corrotina para ativar a area de ataque temporariamente
     void AtivarAreaDeAtaque()
    {
        // Ativa o collider da area de ataque
        areaDeAtaqueCollider.enabled = true;

        // Aguarda um quarto de segundo antes de ativar a area de ataque
        Invoke("FinalizarAtaque", cooldownAtaque);
    }

    void FinalizarAtaque()
    {
        // Desativa o collider da area de ataque
        areaDeAtaqueCollider.enabled = false;

        // Finaliza o ataque
        estaAtacando = false;

        // Permite movimento novamente
        podeMover = true;

        RessetAnim();
        AtualizarAnimacoes();
    }
	
    
    // Metodo chamado quando a area de ataque colide com outro objeto
    void OnTriggerStay2D(Collider2D outro)
    {
        // Verifica se a area de ataque esta ativa
        if (areaDeAtaqueCollider.enabled)
        {
            // Verifica se o objeto colidido e um inimigo
            if (outro.gameObject.name.Contains("Inimigo"))
            {
                // Tenta obter o script Inimigo do objeto colidido
                Inimigo scriptInimigo = outro.gameObject.GetComponent<Inimigo>();
                
                // Se o objeto tem o script Inimigo, causa dano
                if (scriptInimigo != null)
                {
                    scriptInimigo.vidaAtual -= 1;
                }
            }
        }
    }


    // Metodo para causar dano ao jogador
    public void DanoAoJogador(int quantidade)
    {
        // Reduz a vida atual
        vidaAtual -= quantidade;
        
        // Incrementa o contador de dano recebido
        quantidadeDanoRecebido++;
    }

    // Metodo para restaurar vida do jogador
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
}