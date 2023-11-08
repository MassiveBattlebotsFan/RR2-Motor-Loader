using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Dont_Destroy_My_Stuff : MonoBehaviour
{
	public int base_id = 0;
	private int id = 0;
	private Vector3 position;
	private Quaternion rotation;
	private void Awake()
	{
		this.id = gameObject.GetInstanceID();
		position = gameObject.transform.localPosition;
		rotation = gameObject.transform.localRotation;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}
	private void Update()
    {
		if (id == base_id)
		{
			base.gameObject.transform.localPosition = position;
			base.gameObject.transform.localRotation = rotation;
		}
    }
}
