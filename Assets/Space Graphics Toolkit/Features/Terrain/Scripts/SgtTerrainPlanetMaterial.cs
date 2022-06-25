using UnityEngine;
using Unity.Mathematics;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render the <b>SgtTerrain</b> component using the <b>SGT Planet</b> shader.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtTerrain))]
	public class SgtTerrainPlanetMaterial : MonoBehaviour
	{
		/// <summary>The planet material that will be rendered.</summary>
		public Material Material { set { material = value; } get { return material; } } [SerializeField] private Material material;

		/// <summary>Normals bend incorrectly on high detail planets, so it's a good idea to fade them out. This allows you to set the camera distance at which the normals begin to fade out in local space.</summary>
		public double NormalFadeRange { set { normalFadeRange = value; } get { return normalFadeRange; } } [SerializeField] private double normalFadeRange;

		private SgtTerrain cachedTerrain;

		private float bumpScale;

		protected virtual void OnEnable()
		{
			cachedTerrain = GetComponent<SgtTerrain>();

			cachedTerrain.OnDrawQuad += HandleDrawQuad;
		}

		protected virtual void OnDisable()
		{
			cachedTerrain.OnDrawQuad -= HandleDrawQuad;
		}

		protected virtual void Update()
		{
			if (normalFadeRange > 0.0)
			{
				var localPosition = cachedTerrain.GetObserverLocalPosition();
				var localAltitude = math.length(localPosition);
				var localHeight   = cachedTerrain.GetLocalHeight(localPosition);

				bumpScale = (float)math.saturate((localAltitude - localHeight) / normalFadeRange);
			}
			else
			{
				bumpScale = 1.0f;
			}
		}

		private void HandleDrawQuad(Camera camera, SgtTerrainQuad quad, Matrix4x4 matrix, int layer)
		{
			if (material != null)
			{
				var properties = quad.Properties;

				properties.SetFloat(SgtShader._BumpScale, bumpScale);

				Graphics.DrawMesh(quad.CurrentMesh, matrix, material, gameObject.layer, camera, 0, properties);
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtTerrainPlanetMaterial))]
	public class SgtTerrainPlanetMaterial_Editor : SgtEditor<SgtTerrainPlanetMaterial>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.Material == null));
				Draw("material", "The planet material that will be rendered.");
			EndError();
			Draw("normalFadeRange", "Normals bend incorrectly on high detail planets, so it's a good idea to fade them out. This allows you to set the camera distance at which the normals begin to fade out in local space.");
		}
	}
}
#endif