using UnityEngine;
using System.Collections;

public class zoomCamera : MonoBehaviour {
	public float minFov = 1;
	public float maxFov = 90;
	public float sensitivity = 10.0f;
	private float lastScrollWell;
	private float scrollWell;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		scrollWell = Input.GetAxis ("Mouse ScrollWheel");
		if (lastScrollWell != scrollWell) {

			//Obtiene el zoom de la camara
			float fov = Camera.main.fieldOfView;

			//Le aumenta el giro del raton
			fov += scrollWell * sensitivity;

			//Acota los posibles valores
			fov = Mathf.Clamp( fov, minFov, maxFov);


			//Fija el nuevo zoom de la camara
			Camera.main.fieldOfView = fov;

			//Fija la sencivilidad aumentada del mouse
			GetComponent<MouseLook> ().sensitivityY = 10 * (fov / (maxFov - minFov));
			transform.parent.gameObject.GetComponent<MouseLook> ().sensitivityX = 15 * (fov / (maxFov - minFov));

			lastScrollWell = scrollWell;
		}
	}
}
