using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyMovementOverTime : MonoBehaviour
{
    [SerializeField] private float time;

    private bool sfxPlayed;

    void Start()
    {
        Destroy(this.GetComponent<SimpleMoveWithSpeed>(), time);
    }

    void Update()
    {
        if (Input.anyKey)
        {
            Destroy(this.GetComponent<SimpleMoveWithSpeed>());
            this.transform.position = new Vector3(0, 0.2284598f, 0);
        }

        if (!this.TryGetComponent<SimpleMoveWithSpeed>(out SimpleMoveWithSpeed simpleMoveWithSpeed))
        {
            if (!sfxPlayed)
            {
                AudioManager.instance.PlaySfx(GlobalSfx.Click);
                sfxPlayed = true;

                StartCoroutine(WaitForSfxAndLoadScene());
            }
        }
    }

    private IEnumerator WaitForSfxAndLoadScene()
    {
        AudioSource sfxSource = AudioManager.instance.GetSfxSource(GlobalSfx.Click);

        if (sfxSource != null)
        {
            while (sfxSource.isPlaying)
            {
                yield return null;
            }
        }

        SceneManager.LoadScene(1);
    }
}
