using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class zoom : MonoBehaviour
{
    [SerializeField] GameObject piano;
    [SerializeField] GameObject PianoObject;
    private float scroll;
    [SerializeField] GameObject KeyA0;
    [SerializeField] GameObject KeyC8;
    Transform TransformKeyA0;
    Transform TransformKeyC8;
    Vector3 V3;
    Vector3 V3Add;
    Transform PianoObjectTransform;
    Transform PianoTransform;
    bool PushControll = false;
    float ZoomLimit = 0.45f;

    void Start()
    {

        PianoTransform = piano.transform;
        PianoObjectTransform = PianoObject.transform;
        TransformKeyA0 = KeyA0.transform;
        TransformKeyC8 = KeyC8.transform;
        V3Add = new Vector3(0.25f, 0, 0);



        PianoTransform.localScale = new Vector3(PlayerPrefs.GetFloat("PianoScale", 0.7f), PianoTransform.localScale.y, PianoTransform.localScale.z);

        if (PianoTransform.localScale.x < ZoomLimit)
        {
            PianoTransform.localScale = new Vector3(ZoomLimit, PianoTransform.localScale.y, PianoTransform.localScale.z);
        }


    }



    void Update()
    {

        if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightApple) || Input.GetKey(KeyCode.LeftApple)))
        {
            PushControll = true;
        }

        else if (PushControll)
        {
            PushControll = false;

        }

        scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0.0f && PushControll)
        {
            if ((PianoTransform.localScale.x >= ZoomLimit) || (scroll > 0.0f))
            {

                if (TransformKeyA0.position.x > PianoControll.dwnlimmit)
                {
                    V3 = PianoObjectTransform.position;
                    V3.x += (PianoControll.dwnlimmit - TransformKeyA0.position.x);
                    PianoObject.transform.position = V3;

                }

                if (TransformKeyC8.position.x < PianoControll.uplimmit)
                {
                    V3 = PianoObjectTransform.position;
                    V3.x += (PianoControll.uplimmit - TransformKeyC8.position.x);
                    PianoObject.transform.position = V3;
                }

                PianoTransform.localScale += scroll * V3Add;

                PlayerPrefs.SetFloat("PianoScale", piano.transform.localScale.x);
                PlayerPrefs.Save();
            }
        }



    }
}