using DG.Tweening;
using UnityEngine;

public class ColorParticle : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] listParticleColor;
    public void setColorParticle(Color color)
    {
        foreach (ParticleSystem particleColor in listParticleColor)
        {
            if (particleColor != null)
            {
                var main = particleColor.main;
                main.startColor = color;
            }
        }
        DOVirtual.DelayedCall(1.5f, () =>
        {
            gameObject.SetActive(false);
        });
    }

}
