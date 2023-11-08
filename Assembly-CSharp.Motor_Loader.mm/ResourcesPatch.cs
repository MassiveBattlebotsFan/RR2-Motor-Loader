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
    public List<Mesh_Wrapper> body_meshes = new List<Mesh_Wrapper>();
    [SerializeField]
    public List<Mesh_Wrapper> axle_meshes = new List<Mesh_Wrapper>();
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
    private static bool testPartLoaded = false;

    public static extern void orig_fillResources();


    public static void loadPartsFromNewlineList()
    {
        if (!File.Exists(Custom_Part_Info.base_folder + "list.txt"))
        {
            Debug.Log("No custom part list file found, aborting custom part reconstruction");
            return;
        }
        string[] part_name_list = File.ReadAllLines(Custom_Part_Info.base_folder + "list.txt");
        foreach(var i in part_name_list)
        {
            string trimmed_i = i.Trim();
            Debug.Log("Checking for RR2Mesh and RR2Comp files for " + trimmed_i);
            if (!File.Exists(Custom_Part_Info.GetInfoPath(trimmed_i)))
            {
                Debug.Log(Custom_Part_Info.GetInfoPath(trimmed_i) + " does not exist, skipping " + trimmed_i);
                continue;
            }
            if (!File.Exists(Custom_Part_Info.GetMeshPath(trimmed_i)))
            {
                Debug.Log(Custom_Part_Info.GetMeshPath(trimmed_i) + " does not exist, skipping " + trimmed_i);
                continue;
            }
            Debug.Log("RR2Mesh and RR2Comp files exist for " + trimmed_i + ", reconstructing...");
            loadTestPart(trimmed_i);
        }
    }

    public static void loadTestPart(string filename)
    {
        string filedata = File.ReadAllText(Custom_Part_Info.GetInfoPath(filename));
        JSON_Motor_Data motor_data = JsonUtility.FromJson<JSON_Motor_Data>(filedata);
        Motor_Reconstructor reconstructor = new Motor_Reconstructor(motor_data);
        DataDumpClass mesh_info_dump = JsonUtility.FromJson<DataDumpClass>(File.ReadAllText(Custom_Part_Info.GetMeshPath(filename)));
        foreach(var i in mesh_info_dump.body_meshes)
        {
            reconstructor.body_meshes.Add(new Mesh_Construct_Wrapper(i, filename));
        }
        foreach (var i in mesh_info_dump.axle_meshes)
        {
            reconstructor.axle_meshes.Add(new Mesh_Construct_Wrapper(i, filename));
        }
        reconstructor.ReconstructMeshes();
        GameObject motor = reconstructor.ReconstructMotor();
        if (motor != null)
        {
            Debug.Log("Reconstruction produced a GameObject");
            std_comp_list.Add(motor_data.string_ID, motor);
        }
        else
        {
            Debug.LogError("Reconstruction failed!");
        }
    }

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
                if (data.compinfo.comp_type == CompType.SpinMotor)
                {
                    Debug.Log("adding meshes");
                    //List<int> already_added_this = new List<int>();
                    Debug.Log(i.Value.transform.childCount);
                    if(i.Value.transform.childCount < 4 || i.Value.transform.Find("Axle") == null || i.Value.transform.Find("Body") == null)
                    {
                        Debug.LogWarning(i.Key + " is invalid, skipping");
                        continue;
                    }
                    Debug.Log("Body: " + i.Value.transform.Find("Body").childCount);
                    for (var j = 0; j < i.Value.transform.Find("Body").childCount; j++)
                    {
                        if (i.Value.transform.Find("Body").GetChild(j) != null && i.Value.transform.Find("Body").GetChild(j).GetComponent<MeshFilter>() != null)
                        {
                            data.body_meshes.Add(new Mesh_Wrapper(i.Value.transform.Find("Body").GetChild(j).GetComponent<MeshFilter>().mesh));
                        }
                        else
                        {
                            Debug.LogWarning("Mesh invalid");
                        }
                    }
                    Debug.Log("Axle: " + i.Value.transform.Find("Axle").childCount);
                    for (var j = 0; j < i.Value.transform.Find("Axle").childCount; j++)
                    {
                        if (i.Value.transform.Find("Axle").GetChild(j) != null && i.Value.transform.Find("Axle").GetChild(j).GetComponent<MeshFilter>() != null)
                        {
                            if (i.Value.transform.Find("Axle").GetChild(j).gameObject.name != "Attachment")
                            {
                                data.axle_meshes.Add(new Mesh_Wrapper(i.Value.transform.Find("Axle").GetChild(j).GetComponent<MeshFilter>().mesh));
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Mesh invalid");
                        }
                        
                    }
                    JSON_Motor_Data temp = new JSON_Motor_Data(i.Value.GetComponent<Comp_Info_Motor>());
                    temp.SetAttachmentInfo(i.Value.transform.Find("Axle").Find("Attachment").gameObject);
                    temp.SetMeshLocationInfo(i.Value.transform.gameObject);
                    Debug.Log("CompType.SpinMotor, dumping motorinfo wrapper class");
                    File.WriteAllText(Custom_Part_Info.GetInfoPath(i.Key), JsonUtility.ToJson(temp, true));
                    File.WriteAllText(Custom_Part_Info.GetMeshPath(i.Key), JsonUtility.ToJson(data, true));
                    //File.WriteAllText(GlobalDirectories.RobotDirectory.FullName + Custom_Part_Info.GetSafeName(i.Key) + "-compinfo2.json", JsonUtility.ToJson(temp.GetCompInfo(), true));
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
        //dumpParts();
        if (!testPartLoaded)
        {
            //loadTestPart("test");
            loadPartsFromNewlineList();
            foreach(var i in std_comp_list.Keys)
            {
                Debug.Log(i);
            }
            testPartLoaded = true;
        }
    }
}
