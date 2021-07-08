using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RopeGenerator
{
    /*The Limited Int is an integer that has been "extended" to overflow at a certain Max and Min
      Does not check which is max and which is min. 
      If not withing range, default is Max.
      Be careful.
    */
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
        //No max nor min. Set to default values: system max, min int
        public LimitedInt(int value)
        {
            Max = int.MaxValue; Min = int.MinValue;
            _Integer = value;
        }
    }

    #region MeshData
    Mesh mesh;
    Vector3[] verticesArr; //Specifies points in a 3D space
    int[] trianglesArr;    //Specifies the ordering in which vertices are connected to create a triangle. Read in sets of 3.
    Renderer rend;
    Vector2[] uvsArr;
    private Material meshMaterial;
    #endregion

    #region MeshSettings
    private int NumberSides; //Used to set the quality of the rope.
    //public Transform ropeEnd1, ropeEnd2; 
    //public Transform p1, p2;    //Points used to create bezier curve.
    private List<Transform> ropePoints;//(x,y,z) of two positions in space that dictate where the rope starts (1) and where it ends (2)
    private float Radius;    //Specifies how wide the rope is.
    private float Angle;    //Calculated based off NumberSides
    private int Quality; //Quality of bezier curve
    #endregion

    public RopeGenerator(int numSides, float rad, int curvQuality, List<Transform> points, Material mat, GameObject obj)
    {
        NumberSides = numSides;
        Radius = rad;
        Quality = curvQuality;
        ropePoints = points;
        meshMaterial = mat;
        mesh = new Mesh();
        //Create a new instance of a Mesh and set the mesh component that this is attached to.
        obj.GetComponent<MeshFilter>().mesh = mesh;
        rend = obj.GetComponent<Renderer>();
        rend.material = mat;
    }

    public void CreateRope()
    {
        /*Calculate angle between each vertex.
          A shape (triangle, square, pentagon, hexagon, etc) can be thought of as points on a circle 
          equally spaced apart by an angle. This angle is determined by 2Pi / Number of Sides .
          As the number of sides increases to infinity,  the more the shape resembles a circle, 
          which is why the sides define the quality of the rope.
        */
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        //Offsets. Used to keep track of how many vertices there are / which ring we are currently on.
        int offset1 = 0, offset2 = 0;
        offset1 = offset2;
        offset2 = offset1 + NumberSides + 1;

        Angle = 2 * (float)Mathf.PI / NumberSides;

        //Sets amount of bezier curve points to produce.
        float unit = 1.0f / (float)Quality;
        float normalizedQuality = unit;

        //Add in first ring and base.
        //vertices.AddRange(CreateRingVertices(ropeEnd1.position, BezierCurve.GetBezierPoint(normalizedQuality, ropeEnd1, p1, p2, ropeEnd2), 1));
        vertices.AddRange(CreateRingVertices(ropePoints[0].transform.position, BezierCurve.GetBezierPointCubic(normalizedQuality, ropePoints), -1));
        triangles.AddRange(CreateRingBase(offset: 0, -1));

        for (int i = 0; i < Quality; ++i)
        {
            //First bezier point is used to create ring. Second bezier point is used to angle the ring's vertices towards that point.
            vertices.AddRange(CreateRingVertices(BezierCurve.GetBezierPointCubic(normalizedQuality, ropePoints), BezierCurve.GetBezierPointCubic(normalizedQuality + unit, ropePoints), -1));
            triangles.AddRange(CreateSides(offset1, offset2));

            //Update current vertex count.
            offset1 = offset2;
            offset2 = offset1 + NumberSides + 1;

            normalizedQuality = normalizedQuality + unit;
        }
        //Add final face. Does not connect with anything in front so we do not any vertices.
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

    /*Calculates a ring's vertices
        To calculate the vertices of a ring we use the parametric equation of a circle.
        x = (Center.x + Radius * cos(Angle * CullingDir * Integer + offset))
        y = (Center.y + Radius * sin(Angle * CullingDir * Integer + offset))

        We then rotate the vertices to match the direction of the next point.
    */
    private List<Vector3> CreateRingVertices(Vector3 center, Vector3 nextPoint, int cullingDirection = -1)
    {
        List<Vector3> ringVertices = new List<Vector3>();
        ringVertices.Add(new Vector3(center.x, center.y, center.z));

        float x, y, z;
        z = center.z;

        for (int V = 1; V < NumberSides + 1; ++V)
        {
            y = (center.y + Radius * Mathf.Cos((Angle * V) * cullingDirection));
            x = (center.x + Radius * Mathf.Sin((Angle * V) * cullingDirection));

            ringVertices.Add(new Vector3(x, y, z));
        }
        //Rotate
        Vector3 relativePos = nextPoint - center;
        Quaternion lookAtAngle = Quaternion.LookRotation(relativePos);  //Get rotation towards next point relative to current point.

        for (int i = 0; i < ringVertices.Count; ++i)
        {
            ringVertices[i] = lookAtAngle * (ringVertices[i] - center) + center;
            //ringVertices - center normalizes. Multiplication handles the angle translation.  + center gets rid of normalization.
        }
        return ringVertices;
    }

    /*Creates a base by specifying the triangles. See documentation for deeper explanation of algorithm.
      In essence, we create a triangle by creating sets of three vertices. The numbering matches the order in which the vertices were added.
      First, we add the center triangle. Then, the following vertex on the array/list (i.e. offset + 1). Finally, the second vertex after the center one (i.e. offset + 2)
      We then create the following triangle. -> offset, offset +1 (+1), offset +2 (+1)
      The values rollback so we make use of the Limited Integer class to set a max and a minimum.
      The cullingDirection var allows it to move backwards.
    */
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
            }
            else if (T % 3 == 1) // On B
            {
                B.Integer = b_Offset + (cullingDirection * b);
                triangles.Add(B.Integer);
                ++b;
            }
            else // On C
            {
                C.Integer = c_Offset.Integer + (cullingDirection * c);
                triangles.Add(C.Integer);
                ++c;
            }
        }
        return triangles;
    }

    /*Creates the sides connecting rings.
      The sides are created by making rectangles from the corresponding vertex to the next one. Naturally, the rectangle`s are each created with two triangles.
      Because of this, we require two offsets. One for the 'current' ring, and another for the next one. The offsets represent how many vertices before each ring.
      Culling Direction switches the ordering in which these connections are performed. That is, B and C, and F and Ct switch with reach other respectively.
      See documentation for deeper explanation.
      In all frankness, I believe this method could be improved. Not what it does,  but how it does it and being smarter with C# capabilities.
    */
    private List<int> CreateSides(int offset1, int offset2, bool CullingDir = true)    //True -> CW, False -> CCW
    {
        int a, b, c, f;
        a = 1; b = 1; c = 2; f = 2;

        int a_offset, b_offset, c_offset, f_offset;
        a_offset = f_offset = offset1;
        b_offset = c_offset = offset2;

        LimitedInt A = new LimitedInt(a_offset);
        LimitedInt B = new LimitedInt(b_offset);
        LimitedInt C = new LimitedInt(offset2 + NumberSides, offset2 + 1, c_offset);
        LimitedInt F = new LimitedInt(offset1 + NumberSides, offset1 + 1, f_offset);

        List<int> sides = new List<int>();

        //Determines CW/CCW in which the triangle is created, i.e. culling direction.
        int on_A = 0, on_B = 2, on_C = 1, on_At = 3, on_F = 4, on_Ct = 5;
        //Default is set
        if (!CullingDir)
        {
            on_B = 1; on_C = 2; //Switch
            on_F = 5; on_Ct = 4;
        }

        for (int T = 0; T < NumberSides * 2 * 3; ++T)
        {
            if (T % 6 == on_A)
            {
                A.Integer = a_offset + a;
                sides.Add(A.Integer);
            }
            else if (T % 6 == on_B)
            {
                B.Integer = b_offset + b;
                sides.Add(B.Integer);
                ++b;
            }
            else if (T % 6 == on_C)
            {
                C.Integer = c_offset + c;
                sides.Add(C.Integer);
            }
            else if (T % 6 == on_At)
            {
                sides.Add(A.Integer);
                ++a;
            }
            else if (T % 6 == on_F) //On F
            {
                F.Integer = f_offset + f;
                sides.Add(F.Integer);
                ++f;
            }
            else if (T % 6 == on_Ct)//on C'
            {
                sides.Add(C.Integer);
                ++c;
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

    //Adds the finalized vertices, triangles, and UVs to the mesh object.
    void UpdateRope()
    {
        mesh.Clear();

        mesh.vertices = verticesArr;
        mesh.triangles = trianglesArr;

        mesh.RecalculateNormals();
        mesh.uv = uvsArr;
        return;
    }
}
