using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using MonoMod;
using Mono.Cecil;


[Serializable]
public class DataDumpClass
{
    [SerializeField]
    public List<Mesh_Wrapper> meshes = new List<Mesh_Wrapper>();
    [SerializeField]
    public Component_Info compinfo;
    [SerializeField]
    public string stringid;
}

[Serializable]
public class patch_Component_Info : Component_Info
{

}

public class patch_Robot_Resources : Robot_Resources
{
    [MonoModIgnore]
    private static bool filled;

    private static bool dumped = false;

    public static extern void orig_fillResources();

    public static void dumpParts()
    {
        if (!dumped)
        {
            Directory.CreateDirectory(GlobalDirectories.RobotDirectory.FullName);
            foreach (var i in patch_Robot_Resources.std_comp_list)
            {
                DataDumpClass data = new DataDumpClass();
                data.stringid = i.Key;
                data.compinfo = new Component_Info();
                data.compinfo = i.Value.GetComponent<Component_Info>();
                Debug.Log("adding meshes");
                foreach (var j in i.Value.transform.GetComponentsInChildren<MeshFilter>())
                {
                    data.meshes.Add(new Mesh_Wrapper(j.mesh));
                }
                if(data.compinfo.comp_type == CompType.SpinMotor)
                {
                    JSON_Motor_Data temp = new JSON_Motor_Data(i.Value.GetComponent<Comp_Info_Motor>());
                    Debug.Log("CompType.SpinMotor, dumping motorinfo wrapper class");
                    File.WriteAllText(GlobalDirectories.RobotDirectory.FullName + new string(i.Key.Where(m => !System.IO.Path.GetInvalidFileNameChars().Contains(m)).ToArray<char>()) + "-motorinfo.json", JsonUtility.ToJson(temp, true));
                    File.WriteAllText(GlobalDirectories.RobotDirectory.FullName + new string(i.Key.Where(m => !System.IO.Path.GetInvalidFileNameChars().Contains(m)).ToArray<char>()) + "-compinfo2.json", JsonUtility.ToJson(temp.GetCompInfo(), true));
                }
                //data[i.Key]["comp_info"] = JsonUtility.ToJson(i.Value.GetComponent<Component_Info>(), true);
                //File.WriteAllText(GlobalDirectories.RobotDirectory.FullName + new string(i.Key.Where(m => !System.IO.Path.GetInvalidFileNameChars().Contains(m)).ToArray<char>()) + "-compinfo.json", JsonUtility.ToJson(i.Value.GetComponent<Component_Info>(), true));
                //File.WriteAllText(GlobalDirectories.RobotDirectory.FullName + new string(i.Key.Where(m => !System.IO.Path.GetInvalidFileNameChars().Contains(m)).ToArray<char>()) + ".json", JsonUtility.ToJson(data, true));
            }
            Debug.Log(GlobalDirectories.RobotDirectory.FullName);
            //Debug.Log(temp.Length);
            //File.WriteAllText(GlobalDirectories.RobotDirectory.FullName + "componentList.json", temp);
            Debug.Log("Dumped components");
            dumped = true;
        }
    }

    public static void fillResources()
    {
        orig_fillResources();
        dumpParts();

    }
}
