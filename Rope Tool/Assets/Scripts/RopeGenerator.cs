using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RopeGenerator : MonoBehaviour
{

    #region HelperClasses

    //The Limited Int is an integer that has been "extended" to overflow at a certain Max and Min
    //Does not check which is max and which is min. 
    //If not withing range, default is Max.
    //Be careful.
    class LimitedInt
    {
        private int Max { get; set; }
        private int Min { get; set; }
        private int _Integer;

        public int Integer
        {
            get { return _Integer; }
            set
            {
                if (value > _Integer)   //Adding
                {
                    if (value > Max)
                    {
                        //Calculate difference between the new value and Max
                        int diff = value - Max - 1; //-1 to include Min
                        _Integer = Min + diff;
                    }
                    else
                        _Integer = value;
                }
                else if (value < _Integer)  //Subtracting
                {
                    if (value < Min)
                    {
                        //Calculate difference between new value and Min
                        int diff = Min - value - 1; //-1 to include Max
                        _Integer = Max - diff;
                    }
                    else
                        _Integer = value;
                }
                //else    
                //Do nothing

            }
        }
        public LimitedInt(int max, int min, int value)
        {
            Max = max; Min = min;
            if (value <= max && value >= min)
                _Integer = value;
            else
                _Integer = max;
        }
    }
    #endregion


    #region MeshData
    Mesh mesh;
    Vector3[] verticesArr; //Specifies points in a 3D space
    int[] trianglesArr;    //Specifies the ordering in which vertices are connected to create a triangle. Read in sets of 3.
    Renderer rend;
    Vector2[] uvsArr;
    #endregion

    #region MeshSettings
    public int NumberSides; //Used to set the quality of the rope.
    public Transform ropeEnd1, ropeEnd2; //(x,y,z) of two positions in space that dictate where the rope starts (1) and where it ends (2)
    public Transform p1, p2;
    public float Radius;    //Specifies how wide the rope is.
    private float Angle;    //Calculated based off NumberSides
    public int Quality; //Quality of bezier curve
    #endregion
    void Start()
    {
        mesh = new Mesh();
        //Create a new instance of a Mesh and set the mesh component that this is attached to.
        GetComponent<MeshFilter>().mesh = mesh;

        rend = GetComponent<Renderer>();

        /*Calculate angle between each vertex.
          A shape (triangle, square, pentagon, hexagon, etc) can be thought of as points on a circle 
          equally spaced apart by an angle. This angle is determined by 2Pi / Number of Sides .
          As the number of sides increases to infinity,  the more the shape resembles a circle, 
          which is why the sides define the quality of the rope.
        */
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        //Offsets
        int offset1 = 0, offset2 = 0;
        offset1 = offset2;
        offset2 = offset1 + NumberSides + 1;

        Angle = 2 * (float)Mathf.PI / NumberSides;


        /*Vector3[] lowerBaseVerts = CreateRingVertices(ropeEnd1.position, 1);    //Unexpected. Thought these would have the same culling direction :shrug: Find out later.
        int[] lowerBaseTriangles = CreateRingBase(offset: 0, -1);
        //this.vertices = lowerBaseVerts;
        //this.triangles = lowerBaseTriangles;
        Vector3[] secondVerts = CreateRingVertices(ropeEnd2.position, 1);
        //int[] secondTriangles = CreateRingBase(offset: 7, 1);

        //Creating sides
        int[] sidesTriangles = CreateSides(offset1, offset2);
        //Creating actual vector3[] and int[] arrays
        this.vertices = new Vector3[(NumberSides + 1) * 2]; //Only set up for two rings
        this.triangles = new int[999];
        //Add to mesh
        lowerBaseVerts.CopyTo(this.vertices, 0);
        secondVerts.CopyTo(this.vertices, 7);

        lowerBaseTriangles.CopyTo(this.triangles, 0);
        sidesTriangles.CopyTo(this.triangles, 3 * NumberSides);
        //secondTriangles.CopyTo(this.triangles, (3 * NumberSides));
        */
        vertices.AddRange(CreateRingVertices(ropeEnd1.position, 1));
        triangles.AddRange(CreateRingBase(offset: 0, -1));


        float unit = 1.0f / (float)Quality;
        float normalizedQuality = unit;
        for (int i = 0; i < Quality; ++i)
        {
            vertices.AddRange(CreateRingVertices(BezierCurve.GetBezierPoint(normalizedQuality, ropeEnd1, p1, p2, ropeEnd2), 1));
            triangles.AddRange(CreateSides(offset1, offset2));

            offset1 = offset2;
            offset2 = offset1 + NumberSides + 1;

            normalizedQuality = normalizedQuality + unit;
        }
        //Add final face
        triangles.AddRange(CreateRingBase(offset1, 1));

        verticesArr = vertices.ToArray();
        trianglesArr = triangles.ToArray();

        List<Vector2> uvs = new List<Vector2>();
        //Create UVs
        foreach (Vector3 vec3 in vertices)
        {
            uvs.Add(UV_Mapper(vec3));
        }
        uvsArr = uvs.ToArray();
        UpdateRope();
    }

    /*
        To calculate the vertices of a ring we use the parametric equation of a circle.
        x = (Center.x + Radius * cos(Angle * CullingDir * Integer + offset))
        y = (Center.y + Radius * sin(Angle * CullingDir * Integer + offset))
    */
    private List<Vector3> CreateRingVertices(Vector3 center, int cullingDirection = -1, float angleOffset = 0.0f)
    {
        List<Vector3> ringVertices = new List<Vector3>();
        ringVertices.Add(new Vector3(center.x, center.y, center.z));

        float x, y, z;
        z = center.z;

        for (int V = 1; V < NumberSides + 1; ++V)
        {
            x = (center.x + Radius * Mathf.Cos((Angle * V) * cullingDirection + angleOffset));
            y = (center.y + Radius * Mathf.Sin((Angle * V) * cullingDirection + angleOffset));
            ringVertices.Add(new Vector3(x, y, z));
        }
        return ringVertices;
    }

    //Creates a base by specifying the triangles.
    private List<int> CreateRingBase(int offset, int cullingDirection)
    {
        int MAX = NumberSides + offset;
        int MIN = offset + 1;

        int b_Offset = (offset + 1);
        LimitedInt c_Offset = new LimitedInt(MAX, MIN, (b_Offset + cullingDirection));

        int b = 0, c = 0;
        LimitedInt B = new LimitedInt(MAX, MIN, b_Offset);
        LimitedInt C = new LimitedInt(MAX, MIN, c_Offset.Integer);

        List<int> triangles = new List<int>();

        for (int T = 0; T < (NumberSides * 3); ++T)
        {
            if (T % 3 == 0) // On A
            {
                triangles.Add(offset);
                //Debug.Log("<color=red>" + triangles[T] + "</color>");
            }
            else if (T % 3 == 1) // On B
            {
                B.Integer = b_Offset + (cullingDirection * b);
                triangles.Add(B.Integer);
                ++b;
                //Debug.Log("<color=blue>" + triangles[T] + "</color>");
            }
            else // On C
            {
                C.Integer = c_Offset.Integer + (cullingDirection * c);
                triangles.Add(C.Integer);
                ++c;
                //Debug.Log("<color=green>" + triangles[T] + "</color>");
            }
        }
        return triangles;
    }

    //Creates the sides connecting rings.
    private List<int> CreateSides(int offset1, int offset2)
    {
        int a, b, c, f;
        a = 1; b = 1; c = 2; f = 2;
        int a_offset, b_offset, c_offset, f_offset;
        a_offset = f_offset = offset1;
        b_offset = c_offset = offset2;
        int A = a_offset;
        int B = b_offset;
        LimitedInt C = new LimitedInt(offset2 + NumberSides, offset2 + 1, c_offset);
        LimitedInt F = new LimitedInt(offset1 + NumberSides, offset1 + 1, f_offset);

        List<int> sides = new List<int>();

        for (int T = 0; T < NumberSides * 2 * 3; ++T)
        {
            if (T % 6 == 0)  //On A
            {
                A = a_offset + a;
                sides.Add(A);
                Debug.Log("<color=red>" + sides[T] + "</color>");
            }
            else if (T % 6 == 2) //On B
            {
                B = b_offset + b;
                sides.Add(B);
                ++b;
                Debug.Log("<color=red>" + sides[T] + "</color>");
            }
            else if (T % 6 == 1) //On C
            {
                C.Integer = c_offset + c;
                sides.Add(C.Integer);
                Debug.Log("<color=red>" + sides[T] + "</color>");
            }
            else if (T % 6 == 3) // On A'
            {
                sides.Add(A);
                ++a;
                Debug.Log("<color=blue>" + sides[T] + "</color>");
            }
            else if (T % 6 == 4) //On C'
            {
                F.Integer = f_offset + f;
                sides.Add(F.Integer);
                ++f;
                Debug.Log("<color=blue>" + sides[T] + "</color>");

            }
            else //on F
            {

                sides.Add(C.Integer);
                ++c;
                Debug.Log("<color=blue>" + sides[T] + "</color>");
            }
        }
        return sides;
    }

    //Creates UV mapping
    //Using pseudocode from here: https://stackoverflow.com/questions/42628741/texture-mapping-on-a-cylinder-in-c-building-a-raytracer
    Vector2 UV_Mapper(Vector3 point)
    {
        float theta = (float)Math.Atan2(point.y, point.x);

        float rawU = (float)(theta / (2 * Math.PI));

        float u = 1.0f - (rawU + 0.5f);

        float v = point.y % 1;

        return new Vector2(u, v);
    }
    void UpdateRope()
    {
        mesh.Clear();

        mesh.vertices = verticesArr;
        mesh.triangles = trianglesArr;

        mesh.RecalculateNormals();
        mesh.uv = uvsArr;
        //rend.material.mainTextureScale = new Vector2(1, 1);
        return;
    }
}
