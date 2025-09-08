using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System;

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

    void Start()
    {
        // Busca automaticamente as referencias dos componentes
        BuscarReferenciasAutomaticamente();

        // Inicialmente desativa todos os botoes de resposta
        foreach (Button btn in botoesResposta)
        {
            btn.gameObject.SetActive(false);
        }
        // Configura o campo de texto para capturar a tecla Enter
        txtInsert.onEndEdit.AddListener(HandleTextInput);
    }

    // Metodo para buscar automaticamente as referencias dos componentes
    private void BuscarReferenciasAutomaticamente()
    {
        // Busca o canvas dentro do objeto atual
        canvasQuestionario = GetComponent<Canvas>();

        // Busca o componente Text com o nome especifico
        textoamostra = GameObject.Find("textoamostra").GetComponent<Text>();

        // Busca o InputField com o nome especifico
        txtInsert = GameObject.Find("txtInsert").GetComponent<InputField>();

        // Busca e armazena todos os botoes com os nomes B_1 a B_5
        botoesResposta = new Button[5];
        for (int i = 0; i < 5; i++)
        {
            botoesResposta[i] = GameObject.Find($"B_{i + 1}").GetComponent<Button>();
        }

        // Log de verificacao para debug
        Debug.Log("Referencias buscadas automaticamente: " +
                  (canvasQuestionario != null) + " " +
                  (textoamostra != null) + " " +
                  (txtInsert != null) + " " +
                  (botoesResposta.Length == 5));
    }

    // Metodo publico para ser chamado por outro script quando o jogo termina
    public void IniciarQuestionario()
    {
        // Busca o objeto do jogador e coleta seus dados
        GameObject jogadorObj = GameObject.Find("Jogador");
        if (jogadorObj != null)
        {
            Jogador jogadorScript = jogadorObj.GetComponent<Jogador>();
            dadosJogador = $"{jogadorScript.Pontuacao},{jogadorScript.quantidadeEsquivas},{jogadorScript.quantidadeAtaques},{jogadorScript.quantidadeDanoRecebido}";
        }

        // Ativa o canvas do questionario
        canvasQuestionario.gameObject.SetActive(true);
        // Define o texto inicial baseado na vida do jogador
        textoamostra.text = GameObject.Find("Jogador").GetComponent<Jogador>().vidaAtual == 0 ?
            "Voce falhou, mas toda jornada tem seus obstaculos. Tente novamente! Queremos ouvir sua voz: " :
            "Parabens pela sua determinacao! Sua opiniao e valiosa: ";
        // Adiciona o convite para o questionario
        textoamostra.text += "Por favor, responda um questionario rapido iniciando pelo seu nome.";
    }

    // Manipula a entrada de texto do usuario
    private void HandleTextInput(string input)
    {
        if (!string.IsNullOrEmpty(input) && Input.GetKey(KeyCode.Return))
        {
            respostas.Add(input);
            AvancarPergunta();
            txtInsert.text = "";
            txtInsert.gameObject.SetActive(false);
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
            return;
        }

        textoamostra.text = perguntas[perguntaAtual];

        // Ativa botoes para perguntas de multipla escolha (pergunta 2 em diante)
        if (perguntaAtual >= 2)
        {
            foreach (Button btn in botoesResposta)
            {
                btn.gameObject.SetActive(true);
            }
        }
    }

    // Metodo chamado quando um botao de resposta e pressionado
    public void RespostaBotao(int numeroBotao)
    {
        respostas.Add(numeroBotao.ToString());
        AvancarPergunta();
    }

    // Salva os dados no arquivo CSV
    private void SalvarDados()
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
    }
}
