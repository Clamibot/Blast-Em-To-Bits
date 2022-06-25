using UnityEngine;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate the SgtCloudsphere.DepthTex field.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtCloudsphere))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtCloudsphereDepth")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Cloudsphere Depth")]
	public class SgtCloudsphereDepthTex : MonoBehaviour
	{
		/// <summary>The width of the generated texture. A higher value can result in a smoother transition.</summary>
		public int Width { set { if (width != value) { width = value; UpdateTexture(); } } get { return width; } } [FSA("Width")] [SerializeField] private int width = 256;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format { set { if (format != value) { format = value; UpdateTexture(); } } get { return format; } } [FSA("Format")] [SerializeField] private TextureFormat format = TextureFormat.ARGB32;

		/// <summary>The rim transition style.</summary>
		public SgtEase.Type RimEase { set { if (rimEase != value) { rimEase = value; UpdateTexture(); } } get { return rimEase; } } [FSA("RimEase")] [SerializeField] private SgtEase.Type rimEase = SgtEase.Type.Exponential;

		/// <summary>The rim color.</summary>
		public Color RimColor { set { if (rimColor != value) { rimColor = value; UpdateTexture(); } } get { return rimColor; } } [FSA("RimColor")] [SerializeField] private Color rimColor = new Color(1.0f, 0.0f, 0.0f, 0.25f);

		/// <summary>The rim transition sharpness.</summary>
		public float RimPower { set { if (rimPower != value) { rimPower = value; UpdateTexture(); } } get { return rimPower; } } [FSA("RimPower")] [SerializeField] private float rimPower = 5.0f;

		/// <summary>The density of the atmosphere.</summary>
		public float AlphaDensity { set { if (alphaDensity != value) { alphaDensity = value; UpdateTexture(); } } get { return alphaDensity; } } [FSA("AlphaDensity")] [SerializeField] private float alphaDensity = 50.0f;

		/// <summary>The strength of the density fading in the upper atmosphere.</summary>
		public float AlphaFade { set { if (alphaFade != value) { alphaFade = value; UpdateTexture(); } } get { return alphaFade; } } [FSA("Radius")] [SerializeField] private float alphaFade = 2.0f;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		[System.NonSerialized]
		private SgtCloudsphere cachedCloudsphere;

		[System.NonSerialized]
		private bool cachedCloudsphereSet;

		public SgtCloudsphere CachedCloudsphere
		{
			get
			{
				if (cachedCloudsphereSet == false)
				{
					cachedCloudsphere    = GetComponent<SgtCloudsphere>();
					cachedCloudsphereSet = true;
				}

				return cachedCloudsphere;
			}
		}

		public Texture2D GeneratedTexture
		{
			get
			{
				return generatedTexture;
			}
		}

#if UNITY_EDITOR
		/// <summary>This method allows you to export the generated texture as an asset.
		/// Once done, you can remove this component, and set the <b>SgtCloudsphere</b> component's <b>DepthTex</b> setting using the exported asset.</summary>
		[ContextMenu("Export Texture")]
		public void ExportTexture()
		{
			var importer = SgtHelper.ExportTextureDialog(generatedTexture, "Cloudsphere Depth");

			if (importer != null)
			{
				importer.textureCompression  = UnityEditor.TextureImporterCompression.Uncompressed;
				importer.alphaSource         = UnityEditor.TextureImporterAlphaSource.FromInput;
				importer.wrapMode            = TextureWrapMode.Clamp;
				importer.filterMode          = FilterMode.Trilinear;
				importer.anisoLevel          = 16;
				importer.alphaIsTransparency = true;

				importer.SaveAndReimport();
			}
		}
#endif

		[ContextMenu("Apply Texture")]
		public void ApplyTexture()
		{
			CachedCloudsphere.DepthTex = generatedTexture;
		}

		[ContextMenu("Remove Texture")]
		public void RemoveTexture()
		{
			if (CachedCloudsphere.DepthTex == generatedTexture)
			{
				CachedCloudsphere.DepthTex = null;
			}
		}

		protected virtual void OnEnable()
		{
			UpdateTexture();
		}

		protected virtual void OnDisable()
		{
			RemoveTexture();
		}

		protected virtual void OnDestroy()
		{
			SgtHelper.Destroy(generatedTexture);
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			UpdateTexture();
		}

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			UpdateTexture();
		}
#endif

		private void UpdateTexture()
		{
			if (width > 0)
			{
				// Destroy if invalid
				if (generatedTexture != null)
				{
					if (generatedTexture.width != width || generatedTexture.height != 1 || generatedTexture.format != format)
					{
						generatedTexture = SgtHelper.Destroy(generatedTexture);
					}
				}

				// Create?
				if (generatedTexture == null)
				{
					generatedTexture = SgtHelper.CreateTempTexture2D("Depth (Generated)", width, 1, format);

					generatedTexture.wrapMode = TextureWrapMode.Clamp;

					ApplyTexture();
				}

				var stepU = 1.0f / (width - 1);

				for (var x = 0; x < width; x++)
				{
					WritePixel(stepU * x, x);
				}

				generatedTexture.Apply();
			}

			ApplyTexture();
		}

		private void WritePixel(float u, int x)
		{
			var rim   = SgtEase.Evaluate(rimEase, Mathf.Pow(1.0f - u, rimPower));
			var color = Color.Lerp(Color.white, rimColor, rim * rimColor.a);

			color.a = 1.0f - Mathf.Pow(1.0f - Mathf.Pow(u, alphaFade), alphaDensity);

			generatedTexture.SetPixel(x, 0, color);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtCloudsphereDepthTex))]
	public class SgtCloudsphereDepthTex_Editor : SgtEditor<SgtCloudsphereDepthTex>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.Width < 1));
				Draw("width", "The width of the generated texture. A higher value can result in a smoother transition.");
			EndError();
			Draw("format", "The format of the generated texture.");

			Separator();

			Draw("rimEase", "The rim transition style.");
			Draw("rimColor", "The rim color.");
			BeginError(Any(t => t.RimPower < 1.0f));
				Draw("rimPower", "The rim transition sharpness.");
			EndError();

			Separator();

			BeginError(Any(t => t.AlphaDensity < 1.0f));
				Draw("alphaDensity", "The density of the atmosphere.");
			EndError();
			BeginError(Any(t => t.AlphaFade < 1.0f));
				Draw("alphaFade", "The strength of the density fading in the upper atmosphere.");
			EndError();
		}
	}
}
#endif