using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoControll : MonoBehaviour
{
    float middlepos;
    float C4posx;
    float x_;
    private float scroll;
    private float speed = 0.1f;


    public static float x_underlimitt = -1.503f;
    public static float x_underlimitt_init = -1.503f;
    public static float x_upperlimitt = -0.162f;
    public static float x_upperlimitt_init = -0.162f;
    public static float piao_length = 1.071f;
    public static float keywidth = 0.0105f;

    GameObject A0;
    Vector3 V3;


    [SerializeField] GameObject PianoObject;
    [SerializeField] GameObject KeyA0;
    [SerializeField] GameObject KeyC8;
    Transform PianoObjectTransform;
    Transform TransformKeyA0;
    Transform TransformKeyC8;
    bool PushControll = false;

    public static float uplimmit = 0.30f;
    public static float dwnlimmit = -0.30f;

    void Start()
    {
        C4posx = GameObject.Find("39").transform.position.x;
        x_ = 0.0f - C4posx;   //C4の座標が真中に来るようにpianoを移動
        V3 = new Vector3(PianoObject.transform.position.x + x_, PianoObject.transform.position.y, PianoObject.transform.position.z);

        PianoObjectTransform = PianoObject.transform;
        PianoObjectTransform.position = V3;
        TransformKeyA0 = KeyA0.transform;
        TransformKeyC8 = KeyC8.transform;


    }
    void Update()
    {


        scroll = Input.GetAxis("Mouse ScrollWheel");

        if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightApple) || Input.GetKey(KeyCode.LeftApple)))
        {
            PushControll = true;
        }

        else if (PushControll)
        {
            PushControll = false;

        }


        if (!PushControll && scroll != 0)
        {

            V3.x = PianoObjectTransform.position.x - scroll * speed;

            if ((TransformKeyA0.position.x <= dwnlimmit) && (TransformKeyC8.position.x >= uplimmit))
            {
                PianoObjectTransform.position = V3;
            }


            else if (TransformKeyC8.position.x >= uplimmit)
            {
                if (scroll > 0.0f)
                {
                    PianoObjectTransform.position = V3;
                }
            }
            else if (TransformKeyA0.position.x <= dwnlimmit)
            {
                if (scroll < 0.0f)
                {
                    PianoObjectTransform.position = V3;
                }
            }

        }
    }
}




