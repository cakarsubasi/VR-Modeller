using B83.Win32;
using MeshesIO;
using System.Collections.Generic;
using System.IO;
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

    public void OnClickExport()
    {
        GameObject go = ObjectController.Instance.SelectedGameobject;
        if (go == null) return;

        SceneDescription scene = new SceneDescription();
        scene.objects.Add(go.GetComponent<MeshController>().EditableMesh);
        scene.worldTransforms.Add(go.transform.localToWorldMatrix);

        string str = WavefrontIO.Unparse(scene);

        string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string path = Path.Combine(folderPath, go.name + ".obj");

        int i = 1;
        while (File.Exists(path))
        {
            path = Path.Combine(folderPath, go.name + i + ".obj");
            i++;
        }

        File.WriteAllText(path, str);
    }


}
