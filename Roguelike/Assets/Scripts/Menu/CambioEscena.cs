using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class TestingDeTransition : MonoBehaviour
{
    //tiempo en elque se tarda en hacer el efecto de fade
    public float fadeTime = 1;

    //La imagen que va hacer el efecto de fade 
    public Image fadeImage;

    //Escena a cambiar 
    public int sceneToChange;
    // Start is called before the first frame update
    void Start()
    {
        //Iniciar con un color sin transparencia
        fadeImage.color = Color.black;

        //DoFade hace una interpolación a la transparencia de la imagen. 
        //0 = transparencia, 1 = color sólido
        fadeImage.DOFade(0, fadeTime);
    }

    /// <summary>
    /// Manda la imagen a color sólido
    /// </summary>

    public void FadeIn()
    {
        //DoFade hace una interpolación a la transparencia de la imagen. 
        //0 = transparencia, 1 = color sólido
        fadeImage.DOFade(1, fadeTime).OnComplete(SceneChange);
    }

    /// <summary>
    /// Manda la imagen a transparencia
    /// </summary>

    public void FadeOut()
    {
        //DoFade hace una interpolación a la transparencia de la imagen. 
        //0 = transparencia, 1 = color sólido
        fadeImage.DOFade(0, fadeTime);
    }

    /// <summary>
    /// Cmbio de escena
    /// </summary>

    private void SceneChange()
    {
        SceneManager.LoadScene(sceneToChange);
    }
}