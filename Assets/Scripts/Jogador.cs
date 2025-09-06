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
    private int ultimaDirecao = -1;
    private bool ultimoEstaAtacando = false;
    private bool ultimoEstaEsquivando = false;
    private bool precionouAtaque = false; // Variavel para controlar se o jogador esta atacando
    private bool estaAcando = false;
    private bool estaEsquivando = false;// Variavel para controlar se o jogador esta esquivando
    private bool podeMover = true; // Variavel para controlar se o jogador pode se mover
    private float ultimaMagnitudeVelocidade = 0f;
	private float horizontal; // Variavel para armazenar input horizontal
    private float vertical; // Variavel para armazenar input vertical
    private float cooldownAtaque = 1.5f; // Variavel para controlar o cooldown entre ataques
    private float tempoEsquiva = 0f; // Variavel para controlar o tempo de esquiva
    private float duracaoEsquiva = 0.3f; // Tempo de duracao da esquiva em segundos
    private float duracaoAtaque = 0.5f; // Tempo de duracao do ataque em segundos

    public Transform projetil; // Objeto projetil
    public float velocidadeMovimento = 5f; // Velocidade maxima de movimentacao do jogador
    public float forcaEsquiva = 10f; // Forca do impulso durante a esquiva
    public bool vivo;  // Variavel publica para saber se esta vivo ou morto
    public int vidaAtual = 10; // Vida atual do jogador
    public int vidaMaxima = 10; // Vida maxima do jogador
    public int focaAtaque; // Foca de ataque do jogador
    public int quantidadeEsquivas = 0; // Contador de vezes que o jogador esquivou
    public int quantidadeAtaques = 0; // Contador de vezes que o jogador atacou
    public int quantidadeDanoRecebido = 0; // Contador de vezes que o jogador foi atingido


    // Metodo chamado quando o script e inicializado
    void Start()
    {        
        corpo = GetComponent<Rigidbody2D>(); // Obtem o componente Rigidbody2D do GameObject
        contato = GetComponent<BoxCollider2D>();
        animador = GetComponent<Animator>(); // Obtem o componente Animator do GameObject
        
        areaDeAtaqueCollider = transform.Find("AreaDeAtaque").GetComponent<BoxCollider2D>(); // Encontra o objeto filho chamado "AreaDeAtaque" e obtem seu BoxCollider2D
        areaDeAtaque = GameObject.Find("AreaDeAtaque");
        //areaDeAtaqueCollider.enabled = false; // Desativa a colisao da area de ataque no inicio
        areaDeAtaqueCollider.enabled = true;
        direcao = 1;
        vidaAtual = vidaMaxima; // Inicializa a vida atual com o valor maximo
        vivo = true; // inicia o personagem vivo
        focaAtaque = 1; // Foca de ataque inicial
	}

    // Metodo chamado a cada frame
    void Update()
    {
        // Garante que a vida nao fique negativa
        if (vidaAtual < 0)
        {
            vidaAtual = 0;
        }

        if (!vivo)
        {
            Destroy(transform.gameObject);
            return;
        }

        // Se o jogador pode se mover, processa as entradas
        if (podeMover)
        {
            ProcessarEntradas();
        }
		
        if (vidaAtual == 0)
        {

            vivo = false;
        }

        VerificarMudancasEstado(); // Verifica mudancas de estado para atualizar animacoes

        
    }

    // Metodo chamado em intervalos fixos de tempo para fisica
    void FixedUpdate()
    {
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

    string GetNomeAnimacaoAtual()
    {
        if (precionouAtaque)
        {
            switch (direcao)
            {
                case 0: return "atacandoCima";
                case 1: return "atacandoBaixo";
                case 2: return "atacandoEsq";
                case 3: return "atacandoDir";
            }
        }
        else if (estaEsquivando)
        {
            switch (direcao)
            {
                case 0: return "esquivandoCima";
                case 1: return "esquivandoBaixo";
                case 2: return "esquivandoEsq";
                case 3: return "esquivandoDir";
            }
        }
        else if (corpo.linearVelocity.magnitude > 0.1f)
        {
            switch (direcao)
            {
                case 0: return "andandoCima";
                case 1: return "andandoBaixo";
                case 2: return "andandoEsq";
                case 3: return "andandoDir";
            }
        }
        else
        {
            switch (direcao)
            {
                case 0: return "paradoCima";
                case 1: return "paradoBaixo";
                case 2: return "paradoEsq";
                case 3: return "paradoDir";
            }
        }

        return "paradoBaixo"; // fallback
    }

    // Processa as entradas do teclado/controle
    void ProcessarEntradas()
    {
        horizontal = Input.GetAxisRaw("Horizontal"); // Obtem input horizontal (A/D ou setas horizontais)
        vertical = Input.GetAxisRaw("Vertical"); // Obtem input vertical (W/S ou setas verticais)
        
        AtualizarDirecao(); // Atualiza a direcao do jogador baseada nos inputs
        
        // Verifica se a tecla de esquiva (espaco) foi pressionada
        if (Input.GetKeyDown(KeyCode.Space) && !estaEsquivando && !precionouAtaque)
        {
            IniciarEsquiva();
            return;
        }
        
        // Verifica se a tecla de ataque (J) foi pressionada
        if (Input.GetKeyDown(KeyCode.Return) && !precionouAtaque && !estaEsquivando && cooldownAtaque <= 0)
        {
            IniciarAtaque();
        }
    }

    // Move o jogador baseado nos inputs
    void Movimentar()
    {
        // Se esta atacando ou esquivando, nao permite movimento adicional
        if (precionouAtaque || estaEsquivando)
        {
            return;
        }

        Vector2 movimento = new Vector2(horizontal, vertical); // Cria um vetor de movimento baseado nos inputs
        
        // Normaliza o vetor para movimento diagonal nao ser mais rapido
        if (movimento.magnitude > 1)
        {
            movimento.Normalize();
        }
		
        corpo.linearVelocity = movimento * velocidadeMovimento; // Aplica a velocidade ao Rigidbody2D

        if (!vivo)
        {
            // Para o movimento residual
            corpo.linearVelocity = Vector2.zero;
        }
    }

    // Atualiza a direcao do jogador baseada nos inputs
    void AtualizarDirecao()
    {
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
                areaDeAtaque.transform.position = new Vector2(transform.position.x, transform.position.y + 0.7f);
            }
            // Para baixo
            else
            {
                direcao = 1;
                areaDeAtaque.transform.position = new Vector2(transform.position.x, transform.position.y - 0.7f);
            }
        }
        else if (horizontal != 0)
        {
            // Para esquerda
            if (horizontal < 0)
            {
                direcao = 2;
                areaDeAtaque.transform.position = new Vector2(transform.position.x - 0.6f, transform.position.y);
            }
            // Para direita
            else
            {
                direcao = 3;
                areaDeAtaque.transform.position = new Vector2(transform.position.x + 0.6f, transform.position.y);
            }
        }
    }

	private void VerificarMudancasEstado()
    {
        bool estadoMudou = false;

        // Verifica se algum parametro relevante mudou
        if (ultimaDirecao != direcao ||
            ultimoEstaAtacando != precionouAtaque ||
            ultimoEstaEsquivando != estaEsquivando ||
            Mathf.Abs(ultimaMagnitudeVelocidade - corpo.linearVelocity.magnitude) > 0.1f)
        {
            estadoMudou = true;
        }

        if (estadoMudou)
        {
            // Atualiza as animacoes baseadas no estado do jogador
            AtualizarAnimacoes();

            string estadoAtual = GetNomeAnimacaoAtual();

            Debugando();

            // Atualiza os valores de referencia
            ultimaDirecao = direcao;
            ultimoEstaAtacando = precionouAtaque;
            ultimoEstaEsquivando = estaEsquivando;
            ultimaMagnitudeVelocidade = corpo.linearVelocity.magnitude;
        }
    }

    // Atualiza os parametros do Animator baseados no estado do jogador
    void AtualizarAnimacoes()
    {
        // Determina qual animacao deve ser reproduzida baseada no estado e direcao
        if (precionouAtaque)
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

            switch (direcao)
            {
                case 0: SetAnimacaoUnica("atacandoCima"); animador.SetBool("atacandoBaixo", false);  animador.SetBool("atacandoEsq", false);  animador.SetBool("atacandoDir", false); break;
                case 1: SetAnimacaoUnica("atacandoBaixo"); animador.SetBool("atacandoCima", false);  animador.SetBool("atacandoEsq", false);  animador.SetBool("atacandoDir", false); break;
                case 2: SetAnimacaoUnica("atacandoEsq"); animador.SetBool("atacandoCima", false);  animador.SetBool("atacandoBaixo", false);  animador.SetBool("atacandoDir", false); break;
                case 3: SetAnimacaoUnica("atacandoDir"); animador.SetBool("atacandoCima", false);  animador.SetBool("atacandoBaixo", false);  animador.SetBool("atacandoEsq", false); break;
            }
            return;
        }
        else if (estaEsquivando)
        {
            animador.SetBool("paradoCima", false);
            animador.SetBool("paradoBaixo", false);
            animador.SetBool("paradoEsq", false);
            animador.SetBool("paradoDir", false);
            animador.SetBool("andandoCima", false);
            animador.SetBool("andandoBaixo", false);
            animador.SetBool("andandoEsq", false);
            animador.SetBool("andandoDir", false);
            animador.SetBool("atacandoCima", false);
            animador.SetBool("atacandoBaixo", false);
            animador.SetBool("atacandoEsq", false);
            animador.SetBool("atacandoDir", false);

            switch (direcao)
            {
                case 0: SetAnimacaoUnica("esquivandoCima"); animador.SetBool("esquivandoBaixo", false);  animador.SetBool("esquivandoEsq", false);  animador.SetBool("esquivandoDir", false); break;
                case 1: SetAnimacaoUnica("esquivandoBaixo"); animador.SetBool("esquivandoCima", false);  animador.SetBool("esquivandoEsq", false);  animador.SetBool("esquivandoDir", false); break;
                case 2: SetAnimacaoUnica("esquivandoEsq"); animador.SetBool("esquivandoCima", false);  animador.SetBool("esquivandoBaixo", false);  animador.SetBool("esquivandoDir", false); break;
                case 3: SetAnimacaoUnica("esquivandoDir"); animador.SetBool("esquivandoCima", false);  animador.SetBool("esquivandoBaixo", false);  animador.SetBool("esquivandoEsq", false); break;
            }
            return;
        }
        else if (corpo.linearVelocity.magnitude > 0.1f)
        {
            animador.SetBool("paradoCima", false);
            animador.SetBool("paradoBaixo", false);
            animador.SetBool("paradoEsq", false);
            animador.SetBool("paradoDir", false);
            animador.SetBool("esquivandoCima", false);
            animador.SetBool("esquivandoBaixo", false);
            animador.SetBool("esquivandoEsq", false);
            animador.SetBool("esquivandoDir", false);
            animador.SetBool("atacandoCima", false);
            animador.SetBool("atacandoBaixo", false);
            animador.SetBool("atacandoEsq", false);
            animador.SetBool("atacandoDir", false);

            switch (direcao)
            {
                case 0: SetAnimacaoUnica("andandoCima"); animador.SetBool("andandoBaixo", false);  animador.SetBool("andandoEsq", false);  animador.SetBool("andandoDir", false); break;
                case 1: SetAnimacaoUnica("andandoBaixo"); animador.SetBool("andandoCima", false);  animador.SetBool("andandoEsq", false);  animador.SetBool("andandoDir", false); break;
                case 2: SetAnimacaoUnica("andandoEsq"); animador.SetBool("andandoCima", false);  animador.SetBool("andandoBaixo", false);  animador.SetBool("andandoDir", false); break;
                case 3: SetAnimacaoUnica("andandoDir"); animador.SetBool("andandoCima", false);  animador.SetBool("andandoBaixo", false);  animador.SetBool("andandoEsq", false); break;
            }
            return;
        }
        else
        {
            animador.SetBool("paradoCima", false);
            animador.SetBool("paradoBaixo", false);
            animador.SetBool("paradoEsq", false);
            animador.SetBool("paradoDir", false);
            animador.SetBool("andandoCima", false);
            animador.SetBool("andandoBaixo", false);
            animador.SetBool("andandoEsq", false);
            animador.SetBool("andandoDir", false);
            animador.SetBool("atacandoCima", false);
            animador.SetBool("atacandoBaixo", false);
            animador.SetBool("atacandoEsq", false);
            animador.SetBool("atacandoDir", false);

            switch (direcao)
            {
                case 0: SetAnimacaoUnica("paradoCima"); animador.SetBool("paradoBaixo", false);  animador.SetBool("paradoEsq", false);  animador.SetBool("paradoDir", false); break;
                case 1: SetAnimacaoUnica("paradoBaixo"); animador.SetBool("paradoCima", false);  animador.SetBool("paradoEsq", false);  animador.SetBool("paradoDir", false); break;
                case 2: SetAnimacaoUnica("paradoEsq"); animador.SetBool("paradoCima", false);  animador.SetBool("paradoBaixo", false);  animador.SetBool("paradoDir", false); break;
                case 3: SetAnimacaoUnica("paradoDir"); animador.SetBool("paradoCima", false);  animador.SetBool("paradoBaixo", false);  animador.SetBool("paradoEsq", false); break;
            }
        }
    }

	private void SetAnimacaoUnica(string animacaoAtual)
	{
		// Ativa apenas a animacao desejada
		animador.SetBool(animacaoAtual, true);
    }

    // Inicia a acao de esquiva
    void IniciarEsquiva()
    {
        estaEsquivando = true; // Marca que o jogador esta esquivando
        podeMover = false; // Impede movimento durante a esquiva
        corpo.linearVelocity = Vector2.zero; // Para o movimento residual
        tempoEsquiva = duracaoEsquiva; // Configura o tempo de duracao da esquiva
        quantidadeEsquivas++; // Incrementa o contador de esquivas
        Vector2 direcaoEsquiva = Vector2.zero; // Aplica um impulso na direcao atual
        
        switch (direcao)
        {
            case 0: direcaoEsquiva = Vector2.up; break;
            case 1: direcaoEsquiva = Vector2.down; break;
            case 2: direcaoEsquiva = Vector2.left; break;
            case 3: direcaoEsquiva = Vector2.right; break;
        }

        corpo.AddForce(direcaoEsquiva * forcaEsquiva, ForceMode2D.Impulse); // Aplica forca de impulso na esquiva
        transform.GetComponent<BoxCollider2D>().enabled = false; // Desativar colisao
    }

    // Finaliza a acao de esquiva
    void FinalizarEsquiva()
    {
        estaEsquivando = false; // Marca que o jogador nao esta mais esquivando
        podeMover = true; // Permite movimento novamente
        transform.GetComponent<BoxCollider2D>().enabled = true; // reativar colisao
        corpo.linearVelocity = Vector2.zero; // Para o movimento residual da esquiva
    }

    // Inicia a acao de ataque
    void IniciarAtaque()
    {
        if (precionouAtaque)
        {
            return;
        }
		
        precionouAtaque = true; // Marca que o jogador esta atacando
        cooldownAtaque = duracaoAtaque; // Configura o cooldown do ataque
        quantidadeAtaques++; // Incrementa o contador de ataques
        podeMover = false; // Impede movimento durante o ataque
        corpo.linearVelocity = Vector2.zero; // Para o movimento do jogador

        Invoke("AtivarAreaDeAtaque", 0.25f); // Aguarda um tempo antes de ativar a area de ataque
    }

    // Corrotina para ativar a area de ataque temporariamente
     void AtivarAreaDeAtaque()
    {
        estaAcando = true;
        // areaDeAtaqueCollider.enabled = true; // Ativa o collider da area de ataque
        Invoke("FinalizarAtaque", cooldownAtaque); // Aguarda um quarto de segundo antes de ativar a area de ataque
    }

    void FinalizarAtaque()
    {
        estaAcando = false;
        // areaDeAtaqueCollider.enabled = false; // Desativa o collider da area de ataque
        precionouAtaque = false; // Finaliza o ataque
        podeMover = true; // Permite movimento novamente
    }
	
    
    // Metodo chamado quando a area de ataque colide com outro objeto
    void OnTriggerStay2D(Collider2D outro)
    {
        // Verifica se o objeto colidido e um inimigo
        if (outro.gameObject.name.Contains("Inimigo"))
        {
            if (estaAcando)
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

    void Debugando()
    {  
        string estaVivo = vivo == true ? "Esta vivo" : "Morreu!";
        Debug.Log($"{estaVivo}");
        Debug.Log($"Estado: {GetNomeAnimacaoAtual()}");
        Debug.Log($"Velocidade: {corpo.linearVelocity.magnitude}");
        Debug.Log($"Atacando: {precionouAtaque}");
        Debug.Log($"Esquivando: {estaEsquivando}");
    }
}