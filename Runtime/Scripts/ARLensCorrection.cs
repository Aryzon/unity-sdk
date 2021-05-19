using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ARLensCorrection : MonoBehaviour {

	public Shader lensShader;
	private Material material;

	private Vector2 shift = Vector2.zero;
	private float barrelDistortion = 0.0f;
	private Vector3 colorCorrection = Vector3.one;

	public void setShift (Vector2 value) {
		shift = value;
	}

	public void setDistortion (float value) {
		barrelDistortion = value;
	}

	public void setColorCorrection (Vector3 value) {
		colorCorrection = value;
	}

	void OnRenderImage (RenderTexture sourceTexture, RenderTexture destTexture) 
	{
		if(lensShader != null)
		{
			if (material == null) {
				material = new Material (lensShader);
			}
				
			material.SetFloat ("_X", shift.x);
			material.SetFloat ("_Y", shift.y);
			material.SetFloat ("_BarrelDistortion", barrelDistortion);
			material.SetFloat ("_R", colorCorrection.x);
			material.SetFloat ("_G", colorCorrection.y);
			material.SetFloat ("_B", colorCorrection.z);

			Graphics.Blit(sourceTexture, destTexture, material); 

		}
		else
		{
			Graphics.Blit(sourceTexture, destTexture);
			material = null;
		}
	}

	void OnDisable ()
	{
		if(material)
		{
			DestroyImmediate(material);
		}
	}
}
