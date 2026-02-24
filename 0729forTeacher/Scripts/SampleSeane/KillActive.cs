using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KillActive : MonoBehaviour
{

    // Start is called before the first frame update
    public void ActiveThis()
    {

        this.gameObject.SetActive(true);

    }

    public void KillThis()
    {
        this.gameObject.SetActive(false);
    }


}
