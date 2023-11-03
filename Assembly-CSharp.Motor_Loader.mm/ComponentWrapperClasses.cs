using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using MonoMod;
using Mono.Cecil;


public static class Custom_Part_Info
{
    public static string comp_info_file = ".RR2Comp.json";
    public static string mesh_file = ".RR2Mesh.json";
    public static string texture_file = ".RR2Tex.json";
    public static string base_folder = GlobalDirectories.RobotDirectory.FullName;
    public static string GetInfoPath(string filename)
    {
        return base_folder + GetSafeName(filename) + comp_info_file;
    }
    public static string GetMeshPath(string filename)
    {
        return base_folder + GetSafeName(filename) + mesh_file;
    }
    public static string GetTexPath(string filename)
    {
        return base_folder + GetSafeName(filename) + texture_file;
    }
    public static string GetSafeName(string filename)
    {
        return new string(filename.Where(m => !System.IO.Path.GetInvalidFileNameChars().Contains(m)).ToArray<char>());
    }
}

[Serializable]
public class JSON_Part_Data
{
    [ComponentName]
    public string name;
    [TextArea(5, 5)]
    public string description;
    public string string_ID;
    public float weight;
    public Vector3 reference_position;
    public Vector3 center_of_mass;
    public CompType comp_type = 0;  
}

[Serializable]
public class JSON_Motor_Data : JSON_Part_Data
{
    public float max_voltage;
    public float kV_rating;
    public float internal_resistance;
    public float max_cont_current;
    public float max_rotor_rpm;
    public float max_rpm;
    public float max_safe_torque;
    public float stall_torque;
    public bool is_brushless;
    
    public JSON_Motor_Data()
    {
        this.comp_type = CompType.SpinMotor;
    }
    public JSON_Motor_Data(Comp_Info_Motor construct_from)
    {
        this.comp_type = CompType.SpinMotor;
        if (construct_from == null) return;
        this.max_voltage = construct_from.max_volts_before_fried;
        this.max_rpm = construct_from.max_rpm;
        this.max_rotor_rpm = construct_from.max_rotor_RPM;
        this.description = construct_from.description;
        this.name = construct_from.comp_name;
        this.stall_torque = construct_from.stall_torque;
        this.max_safe_torque = construct_from.max_safe_torque;
        this.internal_resistance = construct_from.internal_resistance;
        this.is_brushless = construct_from.is_brushless;
        this.weight = construct_from.weight;
        this.max_cont_current = construct_from.max_continuous_current;
        this.center_of_mass = construct_from.localCOM;
        this.reference_position = construct_from.referencePosition;
        this.string_ID = construct_from.comp_string_id;
        this.kV_rating = construct_from.kV_rating;
    }
    public Comp_Info_Motor GetCompInfo()
    {
        Comp_Info_Motor temp = new Comp_Info_Motor();
        temp.max_volts_before_fried = this.max_voltage;
        temp.max_rpm = this.max_rpm;
        temp.max_rotor_RPM = this.max_rotor_rpm;
        temp.description = this.description;
        temp.comp_name = this.name;
        temp.stall_torque = this.stall_torque;
        temp.max_safe_torque = this.max_safe_torque;
        temp.internal_resistance = this.internal_resistance;
        temp.is_brushless = this.is_brushless;
        temp.weight = this.weight;
        temp.max_continuous_current = this.max_cont_current;
        temp.localCOM = this.center_of_mass;
        temp.referencePosition = this.reference_position;
        temp.comp_type = this.comp_type;
        temp.comp_string_id = this.string_ID;
        temp.kV_rating = this.kV_rating;
        temp.comp_set = 2;
        return temp;
    }
}

public class Mesh_Construct_Wrapper
{
    public Mesh mesh;
    public Texture2D texture;
    public string name;

    public Mesh_Construct_Wrapper(string newname)
    {
        this.name = newname;
    }

    public GameObject GetRenderableObject()
    {
        Debug.Log("GetRenderableObject for " + this.name);
        GameObject temp = new GameObject();
        temp.name = this.name;
        temp.AddComponent<MeshFilter>().mesh = this.mesh;
        temp.AddComponent<MeshRenderer>().material.mainTexture = this.texture;
        temp.AddComponent<MeshCollider>().sharedMesh = this.mesh;
        temp.GetComponent<MeshCollider>().convex = true;
        return temp;
    }
}

public class Motor_Reconstructor
{
    public JSON_Motor_Data json_data;
    public GameObject motor = new GameObject();
    public GameObject body = new GameObject();
    //public List<GameObject> body_go = new List<GameObject>();
    public GameObject axle = new GameObject();
    //public List<GameObject> axle_go = new List<GameObject>();
    public GameObject electrical_sparks = new GameObject();
    public GameObject motor_smoke = new GameObject();
    public List<Mesh_Construct_Wrapper> body_meshes = new List<Mesh_Construct_Wrapper>();
    public List<Mesh_Construct_Wrapper> axle_meshes = new List<Mesh_Construct_Wrapper>();

    public Motor_Reconstructor(JSON_Motor_Data data_from)
    {
        json_data = data_from;
    }

    public void ReconstructMeshes()
    {
        foreach(var i in body_meshes)
        {
            GameObject temp = i.GetRenderableObject();
            temp.transform.parent = this.body.transform;
        }
        foreach (var i in axle_meshes)
        {
            GameObject temp = i.GetRenderableObject();
            temp.transform.parent = this.axle.transform;
        }
    }
}