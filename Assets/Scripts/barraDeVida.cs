using UnityEngine;
using UnityEngine.UI;

public class barraDeVida : MonoBehaviour
{
    public Slider slider;
    
    public void TakeDamage(float damage)
    {
        slider.value -= damage;
        slider.value = Mathf.Min(slider.value, 0, slider.value);
    }

    public void setMax(float max)
    {
        slider.maxValue = max;
    }

    public void setAtual(float vida)
    {
        slider.value = vida;
    }
}