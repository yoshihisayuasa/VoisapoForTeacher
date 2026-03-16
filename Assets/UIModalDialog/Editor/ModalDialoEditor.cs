using UnityEngine;
using System.Collections;
using UnityEditor;

/// <summary>
/// Editor class for ModalDialogs
/// </summary>
public class ModalDialogEditor 
{

  [MenuItem("GameObject/UI Jayanam/Modal Dialog Manager")]
  static void NewModalDialogManager()
  {
    Object unityObj = Resources.Load("Prefabs/ModalDialogManager", typeof(GameObject));

    PrefabUtility.InstantiatePrefab(unityObj);
  }

 
  [MenuItem ("GameObject/UI/Modal dialog")]
  static void NewModalDialog()
  {
    GameObject canvasObj = Selection.activeGameObject;
    Canvas canvas = canvasObj.GetComponent<Canvas>();

    if (canvas != null)
    {
      UnityEngine.Object modalDlgObj = Resources.Load("Prefabs/ModalDialog", typeof(GameObject));

      GameObject modalDlg = PrefabUtility.InstantiatePrefab(modalDlgObj) as GameObject;

      modalDlg.transform.SetParent(canvasObj.transform, true);

      RectTransform rectTransformDlg = modalDlg.transform as RectTransform;

      rectTransformDlg.offsetMin = new Vector2(0, 0);
      rectTransformDlg.offsetMax = new Vector2(0, 0);

    }
    else
    {
      Debug.LogError("This operation has to be executed on a UI Canvas object");
    }

  }


}
