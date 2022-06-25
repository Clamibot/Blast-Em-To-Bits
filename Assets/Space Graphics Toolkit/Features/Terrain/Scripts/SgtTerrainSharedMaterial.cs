﻿using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render the <b>SgtTerrain</b> component using the specified <b>SgtSharedMaterial</b>.
	/// Components like <b>SgtAtmosphere</b> give you a shared material.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtTerrain))]
	public class SgtTerrainSharedMaterial : MonoBehaviour
	{
		/// <summary>The shared material that will be rendered.</summary>
		public SgtSharedMaterial SharedMaterial { set { sharedMaterial = value; } get { return sharedMaterial; } } [SerializeField] private SgtSharedMaterial sharedMaterial;

		private SgtTerrain cachedTerrain;

		protected virtual void OnEnable()
		{
			cachedTerrain = GetComponent<SgtTerrain>();

			cachedTerrain.OnDrawQuad += HandleDrawQuad;
		}

		protected virtual void OnDisable()
		{
			cachedTerrain.OnDrawQuad -= HandleDrawQuad;
		}

		private void HandleDrawQuad(Camera camera, SgtTerrainQuad quad, Matrix4x4 matrix, int layer)
		{
			if (SgtHelper.Enabled(sharedMaterial) == true && sharedMaterial.Material != null)
			{
				Graphics.DrawMesh(quad.CurrentMesh, matrix, sharedMaterial.Material, gameObject.layer, camera);
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtTerrainSharedMaterial))]
	public class SgtTerrainSharedMaterial_Editor : SgtEditor<SgtTerrainSharedMaterial>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.SharedMaterial == null));
				Draw("sharedMaterial", "The shared material that will be rendered.");
			EndError();
		}
	}
}
#endif