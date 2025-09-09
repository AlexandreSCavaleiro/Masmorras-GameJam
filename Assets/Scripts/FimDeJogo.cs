using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FimDeJogo : MonoBehaviour
{
    // Referencia para o Canvas que contem a interface do questionario
    public Canvas canvasQuestionario;
    // Referencia para o componente de texto que mostra a pergunta/instrucao
    public Text textoamostra;
    // Referencia para o campo de entrada de texto onde o usuario digita respostas
    public InputField txtInsert;
    // Array para armazenar os botoes de resposta numerica (1 a 5)
    public Button[] botoesResposta;
    // Referencia para o botao de prosseguir
    public Button botaoProsseguir;

    GameObject jogadorObj;

    // Variavel para controlar o indice da pergunta atual
    private int perguntaAtual = 0;
    // Lista para armazenar as respostas do usuario
    private List<string> respostas = new List<string>();
    // Variavel para armazenar dados do jogador coletados inicialmente
    private string dadosJogador;

    // Perguntas do questionario
    private string[] perguntas = new string[]
    {
        "Escreva seu nome (Pode ser apenas o primeiro nome ou apelido, se voce preferir)",
        "Qual a sua idade? (Apenas quantos anos voce tem atualmente)",
        "Quanto voce se divertiu ao jogar? (Marque de 1 a 5, sendo 1 igual a 'nada' e 5 igual a 'divertiu-se muito')",
        "Quanto o jogo foi facil? (Marque de 1 a 5, sendo 1 igual a 'muito dificil' e 5 igual a 'muito facil')",
        "Quanto voce entendeu os objetivos do jogo? (Marque de 1 a 5, sendo 1 igual a 'nao entendeu' e 5 igual a 'entendeu plenamente')",
        "Quanto o visual do jogo foi agradavel? (Marque de 1 a 5, sendo 1 igual a 'nao foi agradavel' e 5 igual a 'muito agradavel')",
        "Quanto a jogabilidade foi fluida? (Marque de 1 a 5, sendo 1 igual a 'horrivel' e 5 igual a 'muito fluida')",
        "Quantos bugs(erros) voce encontrou ao jogar? (Marque de 1 a 5, sendo 1 igual a 'nenhum' e 5 igual a 'muitos')",
        "Voce indicaria para um amigo? (Marque de 1 a 5, sendo 1 igual a 'nunca', 3 igual 'as vezes' e 5 igual a 'sempre')"
    };


    public int Pontuacao;
    public int quantidadeEsquivas; // Contador de vezes que o jogador esquivou
    public int quantidadeAtaques; // Contador de vezes que o jogador atacou
    public int quantidadeDanoRecebido; // Contador de vezes que o jogador foi atingido


    void Start()
    {
        // Busca automaticamente as referencias dos componentes
        BuscarReferenciasAutomaticamente();

        canvasQuestionario.gameObject.SetActive(false);
        textoamostra.gameObject.SetActive(false);
        txtInsert.gameObject.SetActive(false);
        botaoProsseguir.gameObject.SetActive(false);

        // Busca o objeto do jogador e coleta seus dados
        jogadorObj = GameObject.Find("Jogador");

        if (jogadorObj == null)
        {
            jogadorObj = GameObject.FindGameObjectWithTag("Player"); // Encontra o jogador na cena pela tag
        }

        // Inicialmente desativa todos os botoes de resposta
        foreach (Button btn in botoesResposta)
        {
            btn.gameObject.SetActive(false);
        }

        // Configura o listener para o botao de prosseguir
        botaoProsseguir.onClick.AddListener(ProsseguirPergunta);
    }

    // Metodo publico para ser chamado por outro script quando o jogo termina
    public void IniciarQuestionario()
    {
        perguntaAtual = 0;
        respostas.Clear();

        Pontuacao = jogadorObj.GetComponent<Jogador>().Pontuacao;
        quantidadeEsquivas = jogadorObj.GetComponent<Jogador>().quantidadeEsquivas;
        quantidadeAtaques = jogadorObj.GetComponent<Jogador>().quantidadeAtaques;
        quantidadeDanoRecebido = jogadorObj.GetComponent<Jogador>().quantidadeDanoRecebido;

        canvasQuestionario.gameObject.SetActive(true);
        textoamostra.gameObject.SetActive(true);
        txtInsert.gameObject.SetActive(true);
        botaoProsseguir.gameObject.SetActive(true);

        if (jogadorObj != null)
        {
            Jogador jogadorScript = jogadorObj.GetComponent<Jogador>();
            if (jogadorScript != null)
            {
                dadosJogador = $"{jogadorScript.Pontuacao},{jogadorScript.quantidadeEsquivas},{jogadorScript.quantidadeAtaques},{jogadorScript.quantidadeDanoRecebido}";
            }
        }

        // Define o texto inicial baseado na vida do jogador
        bool jogadorVivo = jogadorObj != null && jogadorObj.GetComponent<Jogador>().vidaAtual > 0;
        textoamostra.text = jogadorVivo ?
            "Parabens pela sua determinacao! Sua opiniao e valiosa: " :
            "Voce falhou, mas toda jornada tem seus obstaculos. Tente novamente! Queremos ouvir sua voz: ";

        // Mostra a primeira pergunta
        textoamostra.text += "\n\n" + perguntas[perguntaAtual];

        // Ativa apenas o campo de texto e botao de prosseguir para as primeiras perguntas
        txtInsert.gameObject.SetActive(true);
        botaoProsseguir.gameObject.SetActive(true);

        foreach (Button btn in botoesResposta)
        {
            btn.gameObject.SetActive(false);
        }
    }

    // Metodo chamado quando o botao de prosseguir e pressionado
    private void ProsseguirPergunta()
    {
        // Para as duas primeiras perguntas (nome e idade), usa o texto do InputField
        if (perguntaAtual < 2)
        {
            if (!string.IsNullOrEmpty(txtInsert.text))
            {
                respostas.Add(txtInsert.text);
                txtInsert.text = "";
                AvancarPergunta();
            }
        }
        // Para as perguntas de multipla escolha, verifica se uma resposta foi selecionada
        else
        {
            // Verifica se alguma resposta foi selecionada (esta logica pode ser ajustada conforme necessidade)
            bool respostaSelecionada = false;
            foreach (string resposta in respostas)
            {
                if (resposta != null && resposta.Length > 0)
                {
                    respostaSelecionada = true;
                    break;
                }
            }

            if (respostaSelecionada)
            {
                AvancarPergunta();
            }
        }
    }

    // Avanca para a proxima pergunta ou finaliza o questionario
    private void AvancarPergunta()
    {
        perguntaAtual++;

        if (perguntaAtual >= perguntas.Length)
        {
            SalvarDados();
            canvasQuestionario.gameObject.SetActive(false);

            SceneManager.LoadScene("Menu");
            return;
        }

        textoamostra.text = perguntas[perguntaAtual];

        // Ativa botoes para perguntas de multipla escolha (pergunta 3 em diante)
        if (perguntaAtual >= 2)
        {
            txtInsert.gameObject.SetActive(false);
            botaoProsseguir.gameObject.SetActive(true);
            foreach (Button btn in botoesResposta)
            {
                btn.gameObject.SetActive(true);
            }
        }
        else
        {
            // Para a segunda pergunta (idade), mantem o campo de texto
            txtInsert.gameObject.SetActive(true);
            botaoProsseguir.gameObject.SetActive(true);
            foreach (Button btn in botoesResposta)
            {
                btn.gameObject.SetActive(false);
            }
        }
    }

    // Metodo chamado quando um botao de resposta e pressionado
    public void RespostaBotao(int numeroBotao)
    {
        respostas.Add(numeroBotao.ToString());
    }

    // Salva os dados no arquivo CSV
    private void SalvarDados()
    {
        try
        {
            string caminho = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Feedbacs_MagoAle.csv");
            bool arquivoExiste = File.Exists(caminho);

            using (StreamWriter writer = new StreamWriter(caminho, true))
            {
                if (!arquivoExiste)
                {
                    writer.WriteLine("Pontuacao,Esquivas,Ataques,DanoRecebido,Nome,Idade,Diversao,Dificuldade,Objetivos,Visual,Jogabilidade,Bugs,Indicacao");
                }
                writer.WriteLine($"{dadosJogador},{string.Join(",", respostas)}");
            }

            Debug.Log("Dados salvos com sucesso em: " + caminho);
        }
        catch (Exception e)
        {
            Debug.LogError("Erro ao salvar dados: " + e.Message);
        }
    }

    // Metodo para buscar automaticamente as referencias dos componentes
    private void BuscarReferenciasAutomaticamente()
    {
        // Busca o canvas dentro do objeto atual
        if (canvasQuestionario == null)
            canvasQuestionario = GetComponent<Canvas>();

        // Busca o componente Text com o nome especifico
        if (textoamostra == null)
            textoamostra = GameObject.Find("textoamostra")?.GetComponent<Text>();

        // Busca o InputField com o nome especifico
        if (txtInsert == null)
            txtInsert = GameObject.Find("txtInsert")?.GetComponent<InputField>();

        // Busca e armazena todos os botoes com os nomes B_1 a B_5
        if (botoesResposta == null || botoesResposta.Length == 0)
        {
            botoesResposta = new Button[5];
            for (int i = 0; i < 5; i++)
            {
                GameObject botaoObj = GameObject.Find($"B_{i + 1}");
                if (botaoObj != null)
                {
                    botoesResposta[i] = botaoObj.GetComponent<Button>();
                }
            }
        }

        // Busca o botao de prosseguir
        if (botaoProsseguir == null)
        {
            GameObject prosseguirObj = GameObject.Find("B_Prosseguir");
            if (prosseguirObj != null)
            {
                botaoProsseguir = prosseguirObj.GetComponent<Button>();
            }
        }

        // Log de verificacao para debug
        Debug.Log("Referencias buscadas automaticamente: " +
                  (canvasQuestionario != null) + " " +
                  (textoamostra != null) + " " +
                  (txtInsert != null) + " " +
                  (botoesResposta != null && botoesResposta.Length == 5) + " " +
                  (botaoProsseguir != null));
    }
}