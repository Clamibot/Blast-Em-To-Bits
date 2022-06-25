using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to create dynamic mesh LOD planets suitable for use with the <b>SGT Planet</b> shader.</summary>
	[ExecuteInEditMode]
	public class SgtTerrainPlanet : SgtTerrain
	{
		private NativeArray<double3> currentPoints;
		private NativeArray<double3> pendingPoints;
		private NativeArray<double>  heights;

		private NativeArray<float3> positions;
		private NativeArray<float3> normals;
		private NativeArray<float4> tangents;
		private NativeArray<float4> coords;
		private NativeArray<int>    indices;

		private bool           running;
		private SgtTerrainQuad quad;
		private JobHandle      handle;
		private int            age;

		protected override bool IsRunning
		{
			get
			{
				return running;
			}
		}

		public static SgtTerrainPlanet Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtTerrainPlanet Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Terrain Planet", layer, parent, localPosition, localRotation, localScale);
			var instance   = gameObject.AddComponent<SgtTerrainPlanet>();

			gameObject.AddComponent<SgtTerrainPlanetMaterial>();

			return instance;
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem(SgtHelper.GameObjectMenuPrefix + "Terrain Planet", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = SgtHelper.GetSelectedParent();
			var instance = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(instance);
		}
#endif

		protected override void OnEnable()
		{
			base.OnEnable();

			//currentPoints = new NativeArray<double3>(0, Allocator.Persistent);
			//pendingPoints = new NativeArray<double3>(0, Allocator.Persistent);

			heights = new NativeArray<double>(0, Allocator.Persistent);

			positions = new NativeArray<float3>(0, Allocator.Persistent);
			normals   = new NativeArray<float3>(0, Allocator.Persistent);
			tangents  = new NativeArray<float4>(0, Allocator.Persistent);
			coords    = new NativeArray<float4>(0, Allocator.Persistent);
			indices   = new NativeArray<int>(0, Allocator.Persistent);
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			if (currentPoints.IsCreated == true) currentPoints.Dispose();
			if (pendingPoints.IsCreated == true) pendingPoints.Dispose();
			if (heights.IsCreated == true) heights.Dispose();

			positions.Dispose();
			normals.Dispose();
			tangents.Dispose();
			coords.Dispose();
			indices.Dispose();
		}

		protected override void UpdateJob()
		{
			if (running == true)
			{
				age++;

				if (age > 2)
				{
					running = false;

					handle.Complete();

					quad.PendingMesh = SpawnMesh();
					quad.Points      = pendingPoints;

					quad.PendingMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

					quad.PendingMesh.SetVertices(SgtHelper.ConvertNativeArray(positions));
					quad.PendingMesh.SetNormals(SgtHelper.ConvertNativeArray(normals));
					quad.PendingMesh.SetTangents(SgtHelper.ConvertNativeArray(tangents));
					quad.PendingMesh.SetUVs(0, SgtHelper.ConvertNativeArray(coords));
#if UNITY_2019_3_OR_NEWER
					quad.PendingMesh.SetIndices(SgtHelper.ConvertNativeArray(indices), MeshTopology.Triangles, 0);
#else
					quad.PendingMesh.SetTriangles(SgtHelper.ConvertNativeArray(indices), 0);
#endif
					quad.PendingMesh.RecalculateBounds();
					quad.PendingMesh.UploadMeshData(false);

					// Swap arrays
					pendingPoints = currentPoints;
					currentPoints = default(NativeArray<double3>);
				}
			}
		}

		protected override void ScheduleJob(SgtTerrainQuad newQuad)
		{
			running       = true;
			age           = 0;
			quad          = newQuad;
			currentPoints = quad.Points;

			// Build arrays
			var quadsX   = (int)quad.PendingOuter.SizeX;
			var quadsY   = (int)quad.PendingOuter.SizeY;
			var pointsX  = quadsX + 1;
			var pointsY  = quadsY + 1;
			var samplesX = pointsX + 4;
			var samplesY = pointsY + 4;

			SgtHelper.UpdateNativeArray(ref pendingPoints, samplesX * samplesY);
			SgtHelper.UpdateNativeArray(ref heights, samplesX * samplesY);
			SgtHelper.UpdateNativeArray(ref positions, pointsX * pointsY);
			SgtHelper.UpdateNativeArray(ref normals, pointsX * pointsY);
			SgtHelper.UpdateNativeArray(ref tangents, pointsX * pointsY);
			SgtHelper.UpdateNativeArray(ref coords, pointsX * pointsY);
			SgtHelper.UpdateNativeArray(ref indices, quadsX * quadsY * 6);

			// Build jobs
			var quadJob = new QuadJob();

			quadJob.Radius = radius;
			quadJob.Middle = quad.Cube.Middle;
			quadJob.Step   = quad.Cube.Step;
			quadJob.CubeC  = quad.CubeC;
			quadJob.CubeH  = quad.CubeH;
			quadJob.CubeV  = quad.CubeV;
			quadJob.CubeO  = quad.CubeO;

			quadJob.CurrentPoints = currentPoints;
			quadJob.PendingPoints = pendingPoints;
			quadJob.CurrentOuter  = quad.CurrentOuter.RectXY;
			quadJob.PendingOuter  = quad.PendingOuter.RectXY;
			quadJob.Heights       = heights;

			var verticesJob = new VerticesJob();

			verticesJob.Twist        = quad.Twist;
			verticesJob.PendingOuter = quad.PendingOuter.RectXY;
			verticesJob.VirtualOuter = quad.VirtualOuter.RectXY;

			verticesJob.Points    = pendingPoints;
			verticesJob.Positions = positions;
			verticesJob.Normals   = normals;
			verticesJob.Tangents  = tangents;
			verticesJob.Coords    = coords;
			verticesJob.Heights   = heights;

			var indicesJob = new IndicesJob();

			indicesJob.Inner   = quad.PendingInner.RectXY;
			indicesJob.Outer   = quad.PendingOuter.RectXY;
			indicesJob.Indices = indices;

			// Schedule everything
			handle = quadJob.Schedule();

			InvokeScheduleHeights(pendingPoints, heights, ref handle);

			handle = verticesJob.Schedule(handle);
			handle = indicesJob.Schedule(handle);
		}

		[BurstCompile]
		public struct QuadJob : IJob
		{
			public long    Middle;
			public double  Radius;
			public double  Step;
			public double3 CubeC;
			public double3 CubeH;
			public double3 CubeV;
			public double3 CubeO;

			public SgtLongRect CurrentOuter;
			public SgtLongRect PendingOuter;

			public NativeArray<double3> CurrentPoints;
			public NativeArray<double3> PendingPoints;
			public NativeArray<double>  Heights;

			public void Execute()
			{
				var currentExpanded = CurrentOuter.SizeX > 0 && CurrentOuter.SizeY > 0 ? CurrentOuter.GetExpanded(2) : default(SgtLongRect);
				var pendingExpanded = PendingOuter.GetExpanded(2);
				var expandedStep    = (int)currentExpanded.SizeX + 1;
				var index           = 0;

				for (var y = pendingExpanded.minY; y <= pendingExpanded.maxY; y++)
				{
					for (var x = pendingExpanded.minX; x <= pendingExpanded.maxX; x++)
					{
						if (currentExpanded.Contains(x, y) == true)
						{
							var expandedX = (int)(x - currentExpanded.minX);
							var expandedY = (int)(y - currentExpanded.minY);

							PendingPoints[index] = CurrentPoints[expandedX + expandedY * expandedStep];

							Heights[index] = double.NegativeInfinity;
						}
						else
						{
							var faceU = (x + Middle) * Step;
							var faceV = (y + Middle) * Step;
							var faceW = 0.0;

							if (faceU < 0.0) {faceW = -faceU; faceU = 0.0; }
							if (faceV < 0.0) {faceW = -faceV; faceV = 0.0; }
							if (faceU > 1.0) {faceW = faceU - 1.0; faceU = 1.0; }
							if (faceV > 1.0) {faceW = faceV - 1.0; faceV = 1.0; }

							var cubePos = CubeC + CubeH * faceU + CubeV * faceV + CubeO * faceW;

							PendingPoints[index] = SgtTerrainTopology.UnitCubeToSphere(cubePos);

							Heights[index] = Radius;
						}

						index++;
					}
				}
			}
		}

		[BurstCompile]
		public struct VerticesJob : IJob
		{
			public double      Twist;
			public SgtLongRect PendingOuter;
			public SgtLongRect VirtualOuter;

			public NativeArray<double3> Points;
			public NativeArray<double>  Heights;
			public NativeArray<float3>  Positions;
			public NativeArray<float3>  Normals;
			public NativeArray<float4>  Tangents;
			public NativeArray<float4>  Coords;

			public void Execute()
			{
				for (var i = 0; i < Heights.Length; i++)
				{
					var height = Heights[i];

					if (double.IsNegativeInfinity(height) == false)
					{
						Points[i] *= height;
						Heights[i] = double.NegativeInfinity;
					}
				}

				var pointsX  = (int)(PendingOuter.maxX - PendingOuter.minX) + 1;
				var samplesX = pointsX + 4;
				var index    = 0;

				for (var y = PendingOuter.minY; y <= PendingOuter.maxY; y++)
				{
					for (var x = PendingOuter.minX; x <= PendingOuter.maxX; x++)
					{
						var idx   = (int)((x - PendingOuter.minX + 2) + (y - PendingOuter.minY + 2) * samplesX);
						var stx = 1;
						var sty = samplesX;

						if (x == VirtualOuter.minX || x == VirtualOuter.maxX || y == VirtualOuter.minY || y == VirtualOuter.maxY)
						{
							idx += (int)(x % 2) + (int)(y % 2) * samplesX;
							stx *= 2;
							sty *= 2;
						}

						var position = Points[idx];
						var normal   = GetNormal(idx, stx, sty);
						var tangent  = GetTangent(normal);
						var coord    = GetCoords(position);

						Positions[index] = (float3)position;
						Normals[index] = (float3)normal;
						Tangents[index] = (float4)tangent;
						Coords[index] = (float4)coord;

						index++;
					}
				}
			}

			private double3 GetNormal(int index, int stepX, int stepY)
			{
				var differenceX = Points[index + stepX] - Points[index - stepX];
				var differenceY = Points[index - stepY] - Points[index + stepY];

				return math.normalize(math.cross(differenceX, differenceY));
			}

			private double4 GetTangent(double3 normal)
			{
				var tangent = math.normalize(math.cross(normal, new double3(0.0, 1.0, 0.0)));

				return new double4(tangent, -1.0f);
			}

			private double4 GetCoords(double3 point)
			{
				var d  = math.normalize(point);
				var sU = (1.75 - math.atan2(d.x, d.z) / (math.PI_DBL * 2.0) - Twist) % 1.0 + Twist;
				var sV = math.asin(d.y) / math.PI_DBL + 0.5;
				var pU = d.x * 0.5;
				var pV = d.z * 0.5;

				return new double4(sU, sV, pU, pV);
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtTerrainPlanet))]
	public class SgtTerrainPlanet_Editor : SgtTerrain_Editor<SgtTerrainPlanet>
	{
		protected override void OnInspector()
		{
			base.OnInspector();
		}
	}
}
#endif