#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
#define UNITY
#endif

using System;
using System.Collections.Generic;
#if UNITY
using UnityEngine;
#endif

namespace LibBSP {
#if UNITY
	using Vector3d = Vector3;
#endif

	/// <summary>
	/// Static class containing helper methods for <see cref="Plane"/> objects.
	/// </summary>
	public static class PlaneExtensions {

		/// <summary>
		/// Intersects three <see cref="Plane"/>s at a <see cref="Vector3d"/>. Returns NaN for all components if two or more <see cref="Plane"/>s are parallel.
		/// </summary>
		/// <param name="p1"><see cref="Plane"/> to intersect.</param>
		/// <param name="p2"><see cref="Plane"/> to intersect.</param>
		/// <param name="p3"><see cref="Plane"/> to intersect.</param>
		/// <returns>Point of intersection if all three <see cref="Plane"/>s meet at a point, (NaN, NaN, NaN) otherwise.</returns>
		public static Vector3d Intersection(Plane p1, Plane p2, Plane p3) {
			Vector3d aN = p1.normal;
			Vector3d bN = p2.normal;
			Vector3d cN = p3.normal;

			var partSolx1 = (bN.y * cN.z) - (bN.z * cN.y);
			var partSoly1 = (bN.z * cN.x) - (bN.x * cN.z);
			var partSolz1 = (bN.x * cN.y) - (bN.y * cN.x);
			var det = (aN.x * partSolx1) + (aN.y * partSoly1) + (aN.z * partSolz1);
			if (det == 0) {
				return new Vector3d(float.NaN, float.NaN, float.NaN);
			}

			return new Vector3d((p1.distance * partSolx1 + p2.distance * (cN.y * aN.z - cN.z * aN.y) + p3.distance * (aN.y * bN.z - aN.z * bN.y)) / det,
			                    (p1.distance * partSoly1 + p2.distance * (aN.x * cN.z - aN.z * cN.x) + p3.distance * (bN.x * aN.z - bN.z * aN.x)) / det,
			                    (p1.distance * partSolz1 + p2.distance * (cN.x * aN.y - cN.y * aN.x) + p3.distance * (aN.x * bN.y - aN.y * bN.x)) / det);
		}

		/// <summary>
		/// Intersects this <see cref="Plane"/> with two other <see cref="Plane"/>s at a <see cref="Vector3d"/>. Returns NaN for all components if two or more <see cref="Plane"/>s are parallel.
		/// </summary>
		/// <param name="p1">This <see cref="Plane"/>.</param>
		/// <param name="p2"><see cref="Plane"/> to intersect.</param>
		/// <param name="p3"><see cref="Plane"/> to intersect.</param>
		/// <returns>Point of intersection if all three <see cref="Plane"/>s meet at a point, (NaN, NaN, NaN) otherwise.</returns>
		public static Vector3d Intersect(this Plane p1, Plane p2, Plane p3) {
			return Intersection(p1, p2, p3);
		}

		/// <summary>
		/// Intersects a <see cref="Plane"/> "<paramref name="p"/>" with a <see cref="Ray"/> "<paramref name="r"/>" at a <see cref="Vector3d"/>. Returns NaN for all components if they do not intersect.
		/// </summary>
		/// <param name="p"><see cref="Plane"/> to intersect with.</param>
		/// <param name="r"><see cref="Ray"/> to intersect.</param>
		/// <returns>Point of intersection if "<paramref name="r"/>" intersects "<paramref name="p"/>", (NaN, NaN, NaN) otherwise.</returns>
		public static Vector3d Intersection(Plane p, Ray r) {
#if UNITY
			float enter;
#else
			double enter;
#endif
			bool intersected = p.Raycast(r, out enter);
			if (intersected || enter != 0) {
				return r.GetPoint(enter);
			} else {
				return new Vector3d(float.NaN, float.NaN, float.NaN);
			}
		}

		/// <summary>
		/// Intersects this <see cref="Plane"/> with a <see cref="Ray"/> "<paramref name="r"/>" at a <see cref="Vector3d"/>. Returns NaN for all components if they do not intersect.
		/// </summary>
		/// <param name="p">This <see cref="Plane"/>.</param>
		/// <param name="r"><see cref="Ray"/> to intersect.</param>
		/// <returns>Point of intersection if "<paramref name="r"/>" intersects this <see cref="Plane"/>, (NaN, NaN, NaN) otherwise.</returns>
		public static Vector3d Intersect(this Plane p, Ray r) {
			return Intersection(p, r);
		}

		/// <summary>
		/// Intersects a <see cref="Plane"/> "<paramref name="p"/>" with this <see cref="Ray"/> at a <see cref="Vector3d"/>. Returns NaN for all components if they do not intersect.
		/// </summary>
		/// <param name="r">This <see cref="Ray"/>.</param>
		/// <param name="p"><see cref="Plane"/> to intersect with.</param>
		/// <returns>Point of intersection if this <see cref="Ray"/> intersects "<paramref name="p"/>", (NaN, NaN, NaN) otherwise.</returns>
		public static Vector3d Intersect(this Ray r, Plane p) {
			return Intersection(p, r);
		}

		/// <summary>
		/// Intersects two <see cref="Plane"/>s at a <see cref="Ray"/>. Returns NaN for all components of both <see cref="Vector3d"/>s of the <see cref="Ray"/> if the <see cref="Plane"/>s are parallel.
		/// </summary>
		/// <param name="p1"><see cref="Plane"/> to intersect.</param>
		/// <param name="p2"><see cref="Plane"/> to intersect.</param>
		/// <returns>Line of intersection where "<paramref name="p1"/>" intersects "<paramref name="p2"/>", ((NaN, NaN, NaN) + p(NaN, NaN, NaN)) otherwise.</returns>
		public static Ray Intersection(Plane p1, Plane p2) {
			Vector3d direction = Vector3d.Cross(p1.normal, p2.normal);
			if (direction == Vector3d.zero) {
				return new Ray(new Vector3d(float.NaN, float.NaN, float.NaN), new Vector3d(float.NaN, float.NaN, float.NaN));
			}
			// If x == 0, solve for y in terms of z, or z in terms of y	

			Vector3d origin;

			Vector3d sqrDirection = Vector3d.Scale(direction, direction);
			if (sqrDirection.x >= sqrDirection.y && sqrDirection.x >= sqrDirection.z) {
				var denom = (p1.normal.y * p2.normal.z) - (p2.normal.y * p1.normal.z);
				origin = new Vector3d(0,
				                      ((p1.normal.z * p2.distance) - (p2.normal.z * p1.distance)) / denom,
				                      ((p2.normal.y * p1.distance) - (p1.normal.y * p2.distance)) / denom);
			} else if (sqrDirection.y >= sqrDirection.x && sqrDirection.y >= sqrDirection.z) {
				var denom = (p1.normal.x * p2.normal.z) - (p2.normal.x * p1.normal.z);
				origin = new Vector3d(((p1.normal.z * p2.distance) - (p2.normal.z * p1.distance)) / denom,
				                      0,
				                      ((p2.normal.x * p1.distance) - (p1.normal.x * p2.distance)) / denom);
			} else {
				var denom = (p1.normal.x * p2.normal.y) - (p2.normal.x * p1.normal.y);
				origin = new Vector3d(((p1.normal.y * p2.distance) - (p2.normal.y * p1.distance)) / denom,
				                      ((p2.normal.x * p1.distance) - (p1.normal.x * p2.distance)) / denom,
				                      0);
			}

			return new Ray(origin, direction);
		}

		/// <summary>
		/// Intersects this <see cref="Plane"/> with another <see cref="Plane"/> at a <see cref="Ray"/>. Returns NaN for all components of both <see cref="Vector3d"/>s of the <see cref="Ray"/> if the <see cref="Plane"/>s are parallel.
		/// </summary>
		/// <param name="p1">This <see cref="Plane"/>.</param>
		/// <param name="p2"><see cref="Plane"/> to intersect.</param>
		/// <returns>Line of intersection where this <see cref="Plane"/> intersects "<paramref name="p2"/>", ((NaN, NaN, NaN) + p(NaN, NaN, NaN)) otherwise.</returns>
		public static Ray Intersect(this Plane p1, Plane p2) {
			return Intersection(p1, p2);
		}

		/// <summary>
		/// Generates three points which can be used to define this <see cref="Plane"/>.
		/// </summary>
		/// <param name="p">This <see cref="Plane"/>.</param>
		/// <param name="scalar">Scale of distance between the generated points. The points will define the same <see cref="Plane"/> but will be farther apart the larger this value is. Must not be zero.</param>
		/// <returns>Three points which define this <see cref="Plane"/>.</returns>
		public static Vector3d[] GenerateThreePoints(this Plane p, float scalar = 16) {
			Vector3d[] points = new Vector3d[3];
			// Figure out if the plane is parallel to two of the axes.
			if (p.normal.y == 0 && p.normal.z == 0) {
				// parallel to plane YZ
				points[0] = new Vector3d(p.distance / p.normal.x, -scalar, scalar);
				points[1] = new Vector3d(p.distance / p.normal.x, 0, 0);
				points[2] = new Vector3d(p.distance / p.normal.x, scalar, scalar);
				if (p.normal.x > 0) {
					Array.Reverse(points);
				}
			} else if (p.normal.x == 0 && p.normal.z == 0) {
				// parallel to plane XZ
				points[0] = new Vector3d(scalar, p.distance / p.normal.y, -scalar);
				points[1] = new Vector3d(0, p.distance / p.normal.y, 0);
				points[2] = new Vector3d(scalar, p.distance / p.normal.y, scalar);
				if (p.normal.y > 0) {
					Array.Reverse(points);
				}
			} else if (p.normal.x == 0 && p.normal.y == 0) {
				// parallel to plane XY
				points[0] = new Vector3d(-scalar, scalar, p.distance / p.normal.z);
				points[1] = new Vector3d(0, 0, p.distance / p.normal.z);
				points[2] = new Vector3d(scalar, scalar, p.distance / p.normal.z);
				if (p.normal.z > 0) {
					Array.Reverse(points);
				}
			} else if (p.normal.x == 0) {
				// If you reach this point the plane is not parallel to any two-axis plane.
				// parallel to X axis
				points[0] = new Vector3d(-scalar, scalar * scalar, (-(scalar * scalar * p.normal.y - p.distance)) / p.normal.z);
				points[1] = new Vector3d(0, 0, p.distance / p.normal.z);
				points[2] = new Vector3d(scalar, scalar * scalar, (-(scalar * scalar * p.normal.y - p.distance)) / p.normal.z);
				if (p.normal.z > 0) {
					Array.Reverse(points);
				}
			} else if (p.normal.y == 0) {
				// parallel to Y axis
				points[0] = new Vector3d((-(scalar * scalar * p.normal.z - p.distance)) / p.normal.x, -scalar, scalar * scalar);
				points[1] = new Vector3d(p.distance / p.normal.x, 0, 0);
				points[2] = new Vector3d((-(scalar * scalar * p.normal.z - p.distance)) / p.normal.x, scalar, scalar * scalar);
				if (p.normal.x > 0) {
					Array.Reverse(points);
				}
			} else if (p.normal.z == 0) {
				// parallel to Z axis
				points[0] = new Vector3d(scalar * scalar, (-(scalar * scalar * p.normal.x - p.distance)) / p.normal.y, -scalar);
				points[1] = new Vector3d(0, p.distance / p.normal.y, 0);
				points[2] = new Vector3d(scalar * scalar, (-(scalar * scalar * p.normal.x - p.distance)) / p.normal.y, scalar);
				if (p.normal.y > 0) {
					Array.Reverse(points);
				}
			} else {
				// If you reach this point the plane is not parallel to any axis. Therefore, any two coordinates will give a third.
				points[0] = new Vector3d(-scalar, scalar * scalar, -(-scalar * p.normal.x + scalar * scalar * p.normal.y - p.distance) / p.normal.z);
				points[1] = new Vector3d(0, 0, p.distance / p.normal.z);
				points[2] = new Vector3d(scalar, scalar * scalar, -(scalar * p.normal.x + scalar * scalar * p.normal.y - p.distance) / p.normal.z);
				if (p.normal.z > 0) {
					Array.Reverse(points);
				}
			}
			return points;
		}

		/// <summary>
		/// Gets the signed distance from this <see cref="Plane"/> to a given point.
		/// </summary>
		/// <param name="p">This <see cref="Plane"/>.</param>
		/// <param name="to">Point to get the distance to.</param>
		/// <returns>Signed distance from this <see cref="Plane"/> to the given point.</returns>
		/// <remarks>Unity uses the plane equation "Ax + By + Cz + D = 0" while Quake-based engines
		/// use "Ax + By + Cz = D". The distance equation needs to be evaluated differently from
		/// Unity's default implementation to properly apply to planes read from BSPs.</remarks>
#if UNITY
		public static float GetBSPDistanceToPoint(this Plane p, Vector3d to) {
			return (p.normal.x * to.x + p.normal.y * to.y + p.normal.z * to.z - p.distance) / p.normal.magnitude;
#else
		public static double GetBSPDistanceToPoint(this Plane p, Vector3d to) {
			return p.GetDistanceToPoint(to);
#endif
		}

		/// <summary>
		/// Is <paramref name="v"/> on the positive side of this <see cref="Plane"/>?
		/// </summary>
		/// <param name="p">This <see cref="Plane"/>.</param>
		/// <param name="v">Point to get the side for.</param>
		/// <returns><c>true</c> if <paramref name="v"/> is on the positive side of this <see cref="Plane"/>.</returns>
		/// <remarks>Unity uses the plane equation "Ax + By + Cz + D = 0" while Quake-based engines
		/// use "Ax + By + Cz = D". The distance equation needs to be evaluated differently from
		/// Unity's default implementation to properly apply to planes read from BSPs.</remarks>
		public static bool GetBSPSide(this Plane p, Vector3d v) {
			return p.GetBSPDistanceToPoint(v) > 0;
		}

		/// <summary>
		/// Determines whether the given <see cref="Vector3d"/> is contained in this <see cref="Plane"/>.
		/// </summary>
		/// <param name="v">Point.</param>
		/// <returns><c>true</c> if the <see cref="Vector3d"/> is contained in this <see cref="Plane"/>.</returns>
		/// <remarks>Unity uses the plane equation "Ax + By + Cz + D = 0" while Quake-based engines
		/// use "Ax + By + Cz = D". The distance equation needs to be evaluated differently from
		/// Unity's default implementation to properly apply to planes read from BSPs.</remarks>
		public static bool BSPContains(this Plane p, Vector3d v) {
			var distanceTo = p.GetBSPDistanceToPoint(v);
			return distanceTo < 0.001 && distanceTo > -0.001;
		}

		/// <summary>
		/// Factory method to parse a <c>byte</c> array into a <c>List</c> of <see cref="Plane"/> objects.
		/// </summary>
		/// <param name="data">The data to parse.</param>
		/// <param name="type">The map type.</param>
		/// <param name="version">The version of this lump.</param>
		/// <returns>A <c>List</c> of <see cref="Plane"/> objects.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="data" /> was null.</exception>
		/// <exception cref="ArgumentException">This structure is not implemented for the given maptype.</exception>
		/// <remarks>This function goes here since it can't go into Unity's Plane class, and so can't depend
		/// on having a constructor taking a byte array.</remarks>
		public static List<Plane> LumpFactory(byte[] data, MapType type, int version = 0) {
			if (data == null) {
				throw new ArgumentNullException();
			}
			int structLength = 0;
			switch (type) {
				case MapType.Quake:
				case MapType.Nightfire:
				case MapType.SiN:
				case MapType.SoF:
				case MapType.Source17:
				case MapType.Source18:
				case MapType.Source19:
				case MapType.Source20:
				case MapType.Source21:
				case MapType.Source22:
				case MapType.Source23:
				case MapType.Source27:
				case MapType.L4D2:
				case MapType.DMoMaM:
				case MapType.Vindictus:
				case MapType.Quake2:
				case MapType.Daikatana:
				case MapType.TacticalInterventionEncrypted: {
					structLength = 20;
					break;
				}
				case MapType.STEF2:
				case MapType.MOHAA:
				case MapType.STEF2Demo:
				case MapType.Raven:
				case MapType.Quake3:
				case MapType.FAKK:
				case MapType.CoD:
				case MapType.CoD2:
				case MapType.CoD4: {
					structLength = 16;
					break;
				}
				default: {
					throw new ArgumentException("Map type " + type + " doesn't use a Plane lump or the lump is unknown.");
				}
			}
			int numObjects = data.Length / structLength;
			List<Plane> lump = new List<Plane>(numObjects);
			for (int i = 0; i < numObjects; ++i) {
				Vector3d normal = new Vector3d(BitConverter.ToSingle(data, structLength * i), BitConverter.ToSingle(data, (structLength * i) + 4), BitConverter.ToSingle(data, (structLength * i) + 8));
				float distance = BitConverter.ToSingle(data, (structLength * i) + 12);
				lump.Add(new Plane(normal, distance));
			}
			return lump;
		}

		/// <summary>
		/// Gets the index for this lump in the BSP file for a specific map format.
		/// </summary>
		/// <param name="type">The map type.</param>
		/// <returns>Index for this lump, or -1 if the format doesn't have this lump or it's not implemented.</returns>
		public static int GetIndexForLump(MapType type) {
			switch (type) {
				case MapType.FAKK:
				case MapType.MOHAA:
				case MapType.STEF2:
				case MapType.STEF2Demo:
				case MapType.Quake:
				case MapType.Quake2:
				case MapType.SiN:
				case MapType.Daikatana:
				case MapType.SoF:
				case MapType.Nightfire:
				case MapType.Vindictus:
				case MapType.TacticalInterventionEncrypted:
				case MapType.Source17:
				case MapType.Source18:
				case MapType.Source19:
				case MapType.Source20:
				case MapType.Source21:
				case MapType.Source22:
				case MapType.Source23:
				case MapType.Source27:
				case MapType.L4D2:
				case MapType.DMoMaM: {
					return 1;
				}
				case MapType.CoD:
				case MapType.Raven:
				case MapType.Quake3: {
					return 2;
				}
				case MapType.CoD4:
				case MapType.CoD2: {
					return 4;
				}
				default: {
					return -1;
				}
			}
		}
	}
}
