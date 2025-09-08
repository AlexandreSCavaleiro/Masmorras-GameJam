using UnityEngine;
using UnityEngine.UI;

public class barraDeVida : MonoBehaviour
{
    public Slider slider;

    public void setMax(float max)
    {
        slider.maxValue = max;
        slider.value = max;
    }

    public void setAtual(float vida)
    {
        slider.value = vida;
    }


}