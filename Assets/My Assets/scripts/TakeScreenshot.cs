

// By default, screenshot files are placed next to the executable bundle -- we don't want this in a
// shipping game, as it will fail if the user doesn't have write access to the Applications folder.
// Instead we should place the screenshots on the user's desktop. However, the ~/ notation doesn't
// work, and Unity doesn't have a mechanism to return special paths. Therefore, the correct way to
// solve this is probably with a plug-in to return OS specific special paths.

// Mono/.NET has functions to get special paths... see discussion page. --Aarku

using UnityEngine;
using System.Collections;

public class TakeScreenshot : MonoBehaviour
{    
	private int screenshotCount = 0;

	private float nextActionTime = 0.0f;
	public float period = 4.0f;


	void Start(){
		StartCoroutine ("SS");
	}

	// Check for screenshot key each frame
	IEnumerator SS(){
		while( true ){
			GameObject[] g = GameObject.FindGameObjectsWithTag("Tree");
			for (int i = 0; i < g.Length; i++) {
				g[i].renderer.enabled = false;
			}
			g = GameObject.FindGameObjectsWithTag("Predator");
			/*Material m;
			for (int i = 0; i < g.Length; i++) {
				GameObject model = g[i].transform.Find("joint2");
				SkinnedMeshRenderer skin =  model.GetComponent<SkinnedMeshRenderer>();
				if ( m == null )
					m = skin.material[0];
				//Cambiar matiria "skin.material[0]"
			}*/


			string screenshotFilename;
			do
			{
				screenshotCount++;
				screenshotFilename = "/hola" + screenshotCount + ".png";
				
			} while (System.IO.File.Exists(screenshotFilename));
			
			Application.CaptureScreenshot(screenshotFilename);
			//Application.CaptureScreenshot("C:/Users/EndUser/Documents/New Unity Project CVVVV/Hola/hola.png");


			//Regresa los objetos a la normalidad
			/*
			for (int i = 0; i < g.Length; i++) {
				GameObject model = g[i].transform.Find("joint2");
				SkinnedMeshRenderer skin =  model.GetComponent<SkinnedMeshRenderer>();
				skin.material[0] = m;
			}*/
			g = GameObject.FindGameObjectsWithTag("Tree");
			for (int i = 0; i < g.Length; i++) {
				g[i].renderer.enabled = true;
			}
			Debug.Log("Tomo Foto");
			yield return new WaitForSeconds(period);
		}
	}
}