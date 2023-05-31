using B83.Win32;
using System.Collections.Generic;
using UnityEngine;


public class FileDragAndDrop : MonoBehaviour
{
    public GameObject fileObject;
    public GameObject container;

    void OnEnable()
    {
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;
    }
    void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    void OnFiles(List<string> aFiles, POINT aPos)
    {
        foreach (string file in aFiles)
        {
            var fi = new System.IO.FileInfo(file);
            var ext = fi.Extension.ToLower();
            if (ext != ".obj")
            {
                continue;
            }
            GameObject go = Instantiate(fileObject, container.transform);
            go.GetComponent<FileObject>().fileText.text = file;
        }
    }

}
