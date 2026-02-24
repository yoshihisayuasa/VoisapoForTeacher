using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Scalecotroll : MonoBehaviour
{
    public static int[,] arr_scale = new int[30, 2];


    public static int[,] arr_sngl = new int[,] { { 0, 1 } };
    public static int[,,] arr_orig = new int[30, 30, 2]; //0 scalename 1,key,2beat

    public static int[,] arr_orig_cash = new int[30, 2];

    public int s; //自作スケールの番号
    private int i;

    int[,] arr_fvtn = new int[,] { { 0, 3 }, { 4, 3 }, { 7, 3 }, { 0, 1 }, { 2, 1 }, { 4, 1 }, { 5, 1 }, { 7, 1 }, { 5, 1 }, { 4, 1 }, { 2, 1 }, { 0, 2 } };//初めの3つは和音
    int[,] arr_thrtn = new int[,] { { 0, 3 }, { 4, 3 }, { 7, 3 }, { 0, 1 }, { 4, 1 }, { 7, 1 }, { 4, 1 }, { 0, 2 } };
    int[,] arr_Oct = new int[,] { { 0, 3 }, { 4, 3 }, { 7, 3 }, { 0, 1 }, { 4, 1 }, { 7, 1 }, { 12, 1 }, { 7, 1 }, { 4, 1 }, { 0, 1 } };

    int[,] arr_OctHf = new int[,] { { 0, 3 }, { 4, 3 }, { 7, 3 }, { 0, 1 }, { 4, 1 }, { 7, 1 }, { 12, 1 }, { 16, 1 }, { 19, 1 }, { 17, 1 }, { 14, 1 }, { 11, 1 }, { 7, 1 }, { 5, 1 }, { 2, 1 }, { 0, 2 } };
    int[,] arr_OctRpt = new int[,] { { 0, 3 }, { 4, 3 }, { 7, 3 }, { 0, 1 }, { 4, 1 }, { 7, 1 }, { 12, 1 }, { 12, 1 }, { 12, 1 }, { 12, 1 }, { 7, 1 }, { 4, 1 }, { 0, 1 } };
    int[,] arr_OctDwn = new int[,] { { -12, 3 }, { -8, 3 }, { -5, 3 }, { 0, 1 }, { -5, 1 }, { -8, 1 }, { -12, 1 } };

    int[,] arr_OctRptDwn = new int[,] { { -12, 3 }, { -8, 3 }, { -5, 3 }, { 0, 1 }, { 0, 1 }, { 0, 1 }, { 0, 1 }, { -5, 1 }, { -8, 1 }, { -12, 1 } };
    int[,] arr_OctRptVib = new int[,] { { 0, 3 }, { 4, 3 }, { 7, 3 }, { 0, 1 }, { 4, 1 }, { 7, 1 }, { 12, 1 }, { 12, 1 }, { 12, 1 }, { 12, 3 }, { 7, 1 }, { 4, 1 }, { 0, 1 } };
    int[,] arr_BknApg = new int[,] { { 0, 3 }, { 4, 3 }, { 7, 3 }, { 0, 1 }, { 7, 1 }, { 4, 1 }, { 12, 1 }, { 7, 1 }, { 4, 1 }, { 0, 1 } };
    int[,] arr_Major = new int[,] { { 0, 3 }, { 4, 3 }, { 7, 3 } };
    public static int[] scale_max_min = new int[] { 0, 87 };
    int array_scale_max = 0;
    int array_scale_min = 0;


    private void scale_switch(int[,] clkd_scle)
    {

        arr_scale = clkd_scle;

    }


    public void single()
    {
        scale_switch(arr_sngl);
        max_min(arr_sngl);

    }
    public void thrtn()
    {
        scale_switch(arr_thrtn);
        max_min(arr_thrtn);

    }

    public void fvtn()
    {
        scale_switch(arr_fvtn);
        max_min(arr_fvtn);
    }
    public void Oct()
    {
        scale_switch(arr_Oct);
        max_min(arr_Oct);
    }
    public void OctHf()
    {
        scale_switch(arr_OctHf);
        max_min(arr_OctHf);
    }

    public void OctRpt()
    {
        scale_switch(arr_OctRpt);
        max_min(arr_OctRpt);
    }
    public void OctDwn()
    {
        scale_switch(arr_OctDwn);
        max_min(arr_OctDwn);
    }
    public void OctRptDwn()
    {
        scale_switch(arr_OctRptDwn);
        max_min(arr_OctRptDwn);
    }
    public void OctRptVib()
    {
        scale_switch(arr_OctRptVib);
        max_min(arr_OctRptVib);
    }
    public void BknApg()
    {
        scale_switch(arr_BknApg);
        max_min(arr_BknApg);
    }

    public void Major()
    {
        scale_switch(arr_Major);
        max_min(arr_Major);
    }
    public void orig()
    {
        arr_scale = arr_sngl;
        //  public int s;
        while (i < 30)
        {
            if (arr_orig[s, i, 1] == 0)
            {
                for (int r = 0; r < i + 1; r++)
                {
                    for (int m = 0; m < 2; m++)
                    {
                        arr_orig_cash[r, m] = arr_orig[s, r, m];
                        max_min(arr_orig_cash);
                    }
                }
                if (arr_scale != arr_orig_cash)
                {
                    arr_scale = arr_orig_cash;
                }
                break;
            }
            i = i + 1;
        }
    }

    private void max_min(int[,] arr_scale)
    {
        int arr_scale_Length = arr_scale.Length / 2;



        for (int i = 3; i < arr_scale_Length; i++)
        {

            if (array_scale_max < arr_scale[i, 0])
            {
                array_scale_max = arr_scale[i, 0];
            }
            if (array_scale_min > arr_scale[i, 0])
            {
                array_scale_min = arr_scale[i, 0];
            }
        }

        scale_max_min[0] = array_scale_max;
        scale_max_min[1] = array_scale_min;


    }
}
