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
    Vector3[] vertices; //Specifies points in a 3D space
    int[] triangles;    //Specifies the ordering in which vertices are connected to create a triangle. Read in sets of 3.
    #endregion

    #region MeshSettings
    public int NumberSides; //Used to set the quality of the rope.
    public Transform ropeEnd1, ropeEnd2; //(x,y,z) of two positions in space that dictate where the rope starts (1) and where it ends (2)
    public float Radius;    //Specifies how wide the rope is.
    private float Angle;    //Calculated based off NumberSides
    #endregion
    void Start()
    {
        mesh = new Mesh();
        //Create a new instance of a Mesh and set the mesh component that this is attached to.
        GetComponent<MeshFilter>().mesh = mesh;

        /*Calculate angle between each vertex.
          A shape (triangle, square, pentagon, hexagon, etc) can be thought of as points on a circle 
          equally spaced apart by an angle. This angle is determined by 2Pi / Number of Sides .
          As the number of sides increases to infinity,  the more the shape resembles a circle, 
          which is why the sides define the quality of the rope.
        */

        Angle = 2 * (float)Mathf.PI / NumberSides;
        Vector3[] lowerBaseVerts = CreateRingVertices(ropeEnd1.position, 1);    //Unexpected. Thought these would have the same culling direction :shrug: Find out later.
        int[] lowerBaseTriangles = CreateRingBase(offset: 0, -1);
        //this.vertices = lowerBaseVerts;
        //this.triangles = lowerBaseTriangles;
        Vector3[] secondVerts = CreateRingVertices(ropeEnd2.position, 1);
        int[] secondTriangles = CreateRingBase(offset: 7, 1);

        //Creating actual vector3[] and int[] arrays
        this.vertices = new Vector3[(NumberSides + 1) * 2]; //Only set up for two rings
        this.triangles = new int[(NumberSides * 3 * 2)];
        //Add to mesh
        lowerBaseVerts.CopyTo(this.vertices, 0);
        secondVerts.CopyTo(this.vertices, 7);

        lowerBaseTriangles.CopyTo(this.triangles, 0);
        secondTriangles.CopyTo(this.triangles, (3 * NumberSides));

        UpdateRope();
    }

    /*
        To calculate the vertices of a ring we use the parametric equation of a circle.
        x = (Center.x + Radius * cos(Angle * CullingDir * Integer + offset))
        y = (Center.y + Radius * sin(Angle * CullingDir * Integer + offset))
    */
    private Vector3[] CreateRingVertices(Vector3 center, int cullingDirection = -1, float angleOffset = 0.0f)
    {
        Vector3[] ringVertices = new Vector3[NumberSides + 1];
        ringVertices[0] = new Vector3(center.x, center.y, center.z);

        float x, y, z;
        z = center.z;

        for (int V = 1; V < NumberSides + 1; ++V)
        {
            x = (center.x + Radius * Mathf.Cos((Angle * V) * cullingDirection + angleOffset));
            y = (center.y + Radius * Mathf.Sin((Angle * V) * cullingDirection + angleOffset));
            ringVertices[V] = new Vector3(x, y, z);
        }
        return ringVertices;
    }

    //Creates a base by specifying the triangles.
    private int[] CreateRingBase(int offset, int cullingDirection)
    {
        int MAX = NumberSides + offset;
        int MIN = offset + 1;

        int b_Offset = (offset + 1);
        LimitedInt c_Offset = new LimitedInt(MAX, MIN, (b_Offset + cullingDirection));

        int b = 0, c = 0;
        LimitedInt B = new LimitedInt(MAX, MIN, b_Offset);
        LimitedInt C = new LimitedInt(MAX, MIN, c_Offset.Integer);

        int[] triangles = new int[NumberSides * 3];

        for (int T = 0; T < (NumberSides * 3); ++T)
        {
            if (T % 3 == 0) // On A
            {
                triangles[T] = offset;
                Debug.Log("<color=red>" + triangles[T] + "</color>");
            }
            else if (T % 3 == 1) // On B
            {
                B.Integer = b_Offset + (cullingDirection * b);
                triangles[T] = B.Integer;
                ++b;
                Debug.Log("<color=blue>" + triangles[T] + "</color>");
            }
            else // On C
            {
                C.Integer = c_Offset.Integer + (cullingDirection * c);
                triangles[T] = C.Integer;
                ++c;
                Debug.Log("<color=green>" + triangles[T] + "</color>");
            }
        }
        return triangles;
    }

    void UpdateRope()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        return;
    }
}
