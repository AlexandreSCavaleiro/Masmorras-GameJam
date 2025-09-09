using UnityEngine;
using UnityEngine.UI;

public class barraDeVida : MonoBehaviour
{
    public Slider slider;
    private Transform alvoJogador; // Referencia para o transform do jogador

    void Start()
    {
        alvoJogador = GameObject.FindGameObjectWithTag("Player").transform; // Encontra o jogador pela tag
        setMax();
        setAtual();
    }

    public void Update()
    {
        setMax();
        setAtual();
    }

    public void setMax()
    {
        slider.maxValue = alvoJogador.GetComponent<Jogador>().vidaMaxima;
    }

    public void setAtual()
    {
        slider.value = alvoJogador.GetComponent<Jogador>().vidaAtual;
    }
}