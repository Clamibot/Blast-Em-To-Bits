using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace SpaceGraphicsToolkit
{
	public class SgtTerrainQuad
	{
		public SgtTerrainCube Cube { get { return cube; } } private SgtTerrainCube cube;

		public double3 CubeC { get { return cubeC; } } private double3 cubeC;

		public double3 CubeH { get { return cubeH; } } private double3 cubeH;

		public double3 CubeV { get { return cubeV; } } private double3 cubeV;

		public double3 CubeO { get { return cubeO; } } private double3 cubeO;

		public double Twist { get { return twist; } } private double twist;

		public int Face { get { return face; } } private int face;

		public MaterialPropertyBlock Properties { get { if (properties == null) properties = new MaterialPropertyBlock(); return properties; } } private MaterialPropertyBlock properties;

		public Mesh CurrentMesh;
		public Mesh PendingMesh;

		public Texture2D CurrentSplat;
		public Texture2D PendingSplat;

		public NativeArray<double3> Points;
		public NativeArray<int>     Biomes;

		public SgtLongBounds CurrentInner;
		public SgtLongBounds CurrentOuter;
		public SgtLongBounds PendingOuter;
		public SgtLongBounds PendingInner;
		public SgtLongBounds VirtualInner;
		public SgtLongBounds VirtualOuter;

		public SgtTerrainQuad(SgtTerrainCube newCube, int newFace, double newTwist, double3 newCubeC, double3 newCubeH, double3 newCubeV)
		{
			cube  = newCube;
			face  = newFace;
			twist = newTwist;
			cubeC = SgtTerrainTopology.Tilt(newCubeC);
			cubeH = SgtTerrainTopology.Tilt(newCubeH);
			cubeV = SgtTerrainTopology.Tilt(newCubeV);
			cubeO = math.cross(math.normalize(cubeH), cubeV);

			Allocate();
		}

		public void Allocate()
		{
			if (Points.IsCreated == false)
			{
				Points = new NativeArray<double3>(0, Allocator.Persistent);
			}

			if (Biomes.IsCreated == false)
			{
				Biomes = new NativeArray<int>(0, Allocator.Persistent);
			}

			CurrentInner.Clear();
			CurrentOuter.Clear();
			PendingOuter.Clear();
			PendingInner.Clear();
			VirtualInner.Clear();
			VirtualOuter.Clear();
		}

		public void Dispose()
		{
			if (Points.IsCreated == true) Points.Dispose();

			if (Biomes.IsCreated == true) Biomes.Dispose();

			Object.DestroyImmediate(CurrentMesh);
			Object.DestroyImmediate(PendingMesh);
			Object.DestroyImmediate(CurrentSplat);
			Object.DestroyImmediate(PendingSplat);
		}

		public void Swap()
		{
			if (PendingMesh != null)
			{
				if (CurrentMesh != null)
				{
					CurrentMesh.Clear();

					SgtTerrain.DespawnMesh(CurrentMesh);
				}

				CurrentMesh = PendingMesh;
				PendingMesh = null;
			}

			if (PendingSplat != null)
			{
				if (CurrentSplat != null)
				{
					Object.DestroyImmediate(CurrentSplat);
				}

				CurrentSplat = PendingSplat;
				PendingSplat = null;
			}

			CurrentOuter = PendingOuter;
			CurrentInner = PendingInner;
		}
	}
}