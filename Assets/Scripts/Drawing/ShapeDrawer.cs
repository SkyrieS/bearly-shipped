using LibTessDotNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ShapeDrawer : MonoBehaviour
{
    [SerializeField] private float updateDistanceThreshold = 0.025f;
    [SerializeField] private float closeThreshold = 0.1f;
    [SerializeField] private float meshHalfThickness = 0.01f;
    [SerializeField] private List<Material> meshMaterials;

    [Header("Ink Settings")]
    [SerializeField] private float maxInk = 5f;
    [SerializeField] private RectTransform inkBar;
    [SerializeField] private RectTransform projectedInkBar;

    [Header("Drawing Sound")]
    [SerializeField] private AudioSource drawingAudioSource;
    [SerializeField] private AudioClip drawingLoopClip;

    private LineRenderer _lineRenderer;

    private float _currentInk;
    private Material _currentDrawMaterial;
    private List<Vector3> _points = new List<Vector3>();
    private Plane _drawingPlane = new Plane(Vector3.up, Vector3.zero);

    private Coroutine _clearCoroutine;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 0;
        _currentInk = maxInk;
        UpdateInkBars(0f);
    }

    private void OnDisable()
    {
        if (_points.Count > 0)
        {
            StartColorAndClear(Color.red, 0f);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _points.Clear();
            _lineRenderer.positionCount = 0;
            if (_clearCoroutine != null)
            {
                StopCoroutine(_clearCoroutine);
                _clearCoroutine = null;
            }
            _lineRenderer.startColor = Color.white;
            _lineRenderer.endColor = Color.white;
            UpdateInkBars(0f);

            _currentDrawMaterial = meshMaterials[Random.Range(0, meshMaterials.Count)];
            _lineRenderer.material = _currentDrawMaterial;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                _drawingPlane = new Plane(Vector3.up, hit.point);
            }
            else
            {
                _drawingPlane = new Plane(Vector3.up, Vector3.zero);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            _points.Clear();
            _lineRenderer.positionCount = 0;
            _lineRenderer.startColor = Color.white;
            _lineRenderer.endColor = Color.white;
            UpdateInkBars(0f);
            return;
        }

        float projectedInk = 0f;

        if (Input.GetMouseButton(0))
        {
            if (_currentInk <= 0f)
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float enter;
            Vector3 worldPoint = Vector3.zero;
            if (_drawingPlane.Raycast(ray, out enter))
            {
                worldPoint = ray.GetPoint(enter);

                float dist = 0f;
                if (_points.Count > 0)
                    dist = Vector3.Distance(_points[_points.Count - 1], worldPoint);

                projectedInk = GetShapeLengthWithExtra(_points, worldPoint, false);

                if (projectedInk > _currentInk)
                {
                    UpdateInkBars(_currentInk);
                    return;
                }

                if (_points.Count == 0 || dist > updateDistanceThreshold)
                {
                    _points.Add(worldPoint);
                    _lineRenderer.positionCount = _points.Count;
                    _lineRenderer.SetPositions(_points.ToArray());
                }
                UpdateInkBars(projectedInk);
            }
        }

        if (Input.GetMouseButtonUp(0) && _points.Count > 2)
        {
            float shapeLength = GetShapeLength(_points, true);
            if (Vector3.Distance(_points[0], _points[_points.Count - 1]) < closeThreshold)
            {
                if (_currentInk >= shapeLength)
                {
                    GenerateGeometryWithLibTess(_points, 1f);
                    _currentInk -= shapeLength;
                    UpdateInkBars(0f);
                    StartColorAndClear(Color.green, 0f);
                    StartDrawingSound();
                    PopupManager.Instance.HidePopup("Draw shape");
                    PopupManager.Instance.ShowPopup("Drag shape", "You can also move shapes in Drag mode", 5f);

                }
                else
                {
                    UpdateInkBars(0f);
                    StartColorAndClear(Color.red, 1f);
                }
            }
            else
            {
                UpdateInkBars(0f);
                StartColorAndClear(Color.red, 1f);
            }
        }
    }

    private void StartDrawingSound()
    {
        if (drawingAudioSource != null && drawingLoopClip != null && !drawingAudioSource.isPlaying)
        {
            drawingAudioSource.clip = drawingLoopClip;
            drawingAudioSource.Play();
        }
    }

    private float GetShapeLength(List<Vector3> pts, bool closed)
    {
        float len = 0f;
        for (int i = 1; i < pts.Count; i++)
            len += Vector3.Distance(pts[i - 1], pts[i]);
        if (closed && pts.Count > 2)
            len += Vector3.Distance(pts[pts.Count - 1], pts[0]);
        return len;
    }

    private float GetShapeLengthWithExtra(List<Vector3> pts, Vector3 extra, bool closed)
    {
        float len = GetShapeLength(pts, false);
        if (pts.Count > 0)
            len += Vector3.Distance(pts[pts.Count - 1], extra);
        if (closed && pts.Count > 1)
            len += Vector3.Distance(extra, pts[0]);
        return len;
    }

    private void UpdateInkBars(float projectedInk)
    {
        float ratio = Mathf.Clamp01(_currentInk / maxInk);
        inkBar.localScale = new Vector3(ratio, 1f, 1f);

        if (ratio == 0)
        {
            projectedInkBar.localScale = new Vector3(0, 1f, 1f);
            return;
        }

        float projectedRatio = Mathf.Clamp01(projectedInk / maxInk);
        projectedInkBar.localScale = new Vector3(1 / ratio * projectedRatio, 1f, 1f);
    }

    private void StartColorAndClear(Color color, float delay)
    {
        if (_clearCoroutine != null)
            StopCoroutine(_clearCoroutine);
        _clearCoroutine = StartCoroutine(ColorAndClearRoutine(color, delay));
    }

    private IEnumerator ColorAndClearRoutine(Color color, float delay)
    {
        _lineRenderer.startColor = color;
        _lineRenderer.endColor = color;
        yield return new WaitForSeconds(delay);
        _lineRenderer.positionCount = 0;
        _points.Clear();
        _lineRenderer.startColor = Color.white;
        _lineRenderer.endColor = Color.white;
        _clearCoroutine = null;
        UpdateInkBars(0f);
    }

    private static float SignedArea(List<Vector3> pts)
    {
        float area = 0f;
        for (int i = 0, j = pts.Count - 1; i < pts.Count; j = i++)
        {
            area += (pts[j].x * pts[i].y) - (pts[i].x * pts[j].y);
        }
        return area * 0.5f;
    }

    public void GenerateGeometryWithLibTess(List<Vector3> pointLists, float textureScale)
    {
        Vector3 centroid = Vector3.zero;
        foreach (var pt in pointLists)
            centroid += pt;
        centroid /= pointLists.Count;

        List<Vector3> centeredPoints = new List<Vector3>(pointLists.Count);
        foreach (var pt in pointLists)
            centeredPoints.Add(pt - centroid);

        if (centeredPoints.Count > 0 && SignedArea(centeredPoints) >= 0f)
        {
            centeredPoints.Reverse();
        }

        var triangulatedShape = new TriangulatedShape2D(centeredPoints);

        int triCount = triangulatedShape.TriangleCount;
        int[] tris = triangulatedShape.Triangles;
        ContourVertex[] vertices = triangulatedShape.Vertices;

        Vector3 thicknessDir = Camera.main != null ? Camera.main.transform.forward.normalized : Vector3.forward;

        int vertCount = vertices.Length;
        Vector3[] meshVertices = new Vector3[vertCount * 2];
        Vector2[] meshUVs = new Vector2[vertCount * 2];
        Vector3[] meshNormals = new Vector3[vertCount * 2];

        for (int i = 0; i < vertCount; i++)
        {
            var v = vertices[i].Position;
            meshVertices[i] = new Vector3(v.X, v.Y, v.Z) + thicknessDir * meshHalfThickness;
            Vector3 uv3D = this.transform.TransformPoint(new Vector3(v.X, v.Y, 0));
            meshUVs[i] = new Vector2(uv3D.x / textureScale, uv3D.y / textureScale);
            meshNormals[i] = thicknessDir;
        }
        for (int i = 0; i < vertCount; i++)
        {
            var v = vertices[i].Position;
            meshVertices[i + vertCount] = new Vector3(v.X, v.Y, v.Z) - thicknessDir * meshHalfThickness;
            Vector3 uv3D = this.transform.TransformPoint(new Vector3(v.X, v.Y, 0));
            meshUVs[i + vertCount] = new Vector2(uv3D.x / textureScale, uv3D.y / textureScale);
            meshNormals[i + vertCount] = -thicknessDir;
        }

        var contour = centeredPoints;
        int contourCount = contour.Count;
        int[] contourToTessIdx = new int[contourCount];
        for (int i = 0; i < contourCount; i++)
        {
            Vector3 c = contour[i];
            float minDist = float.MaxValue;
            int minIdx = -1;
            for (int j = 0; j < vertCount; j++)
            {
                var v = vertices[j].Position;
                float dist = Vector2.SqrMagnitude(new Vector2(c.x, c.y) - new Vector2(v.X, v.Y));
                if (dist < minDist)
                {
                    minDist = dist;
                    minIdx = j;
                }
            }
            contourToTessIdx[i] = minIdx;
        }

        int[] meshTriangles = new int[triCount * 3 * 2];
        for (int i = 0; i < triCount; i++)
        {
            meshTriangles[i * 3] = tris[i * 3];
            meshTriangles[i * 3 + 1] = tris[i * 3 + 2];
            meshTriangles[i * 3 + 2] = tris[i * 3 + 1];

            meshTriangles[triCount * 3 + i * 3] = tris[i * 3] + vertCount;
            meshTriangles[triCount * 3 + i * 3 + 1] = tris[i * 3 + 1] + vertCount;
            meshTriangles[triCount * 3 + i * 3 + 2] = tris[i * 3 + 2] + vertCount;
        }

        List<int> sideTris = new List<int>();
        List<Vector3> sideNormals = new List<Vector3>();
        List<Vector3> sideVerts = new List<Vector3>();
        List<Vector2> sideUVs = new List<Vector2>();

        for (int i = 0; i < contourCount; i++)
        {
            int next = (i + 1) % contourCount;

            int i0 = contourToTessIdx[i];
            int i1 = contourToTessIdx[next];
            int j0 = i0 + vertCount;
            int j1 = i1 + vertCount;

            Vector3 v0 = meshVertices[i0];
            Vector3 v1 = meshVertices[i1];
            Vector3 v2 = meshVertices[j1];
            Vector3 v3 = meshVertices[j0];

            Vector3 edge = (v1 - v0).normalized;
            Vector3 normal = Vector3.Cross(thicknessDir, edge).normalized;

            int baseIdx = meshVertices.Length + sideVerts.Count;

            sideVerts.Add(v0); sideNormals.Add(normal); sideUVs.Add(meshUVs[i0]);
            sideVerts.Add(v1); sideNormals.Add(normal); sideUVs.Add(meshUVs[i1]);
            sideVerts.Add(v2); sideNormals.Add(normal); sideUVs.Add(meshUVs[i1]);
            sideVerts.Add(v3); sideNormals.Add(normal); sideUVs.Add(meshUVs[i0]);

            sideTris.Add(baseIdx + 0);
            sideTris.Add(baseIdx + 1);
            sideTris.Add(baseIdx + 2);

            sideTris.Add(baseIdx + 0);
            sideTris.Add(baseIdx + 2);
            sideTris.Add(baseIdx + 3);
        }

        Vector3[] allVerts = new Vector3[meshVertices.Length + sideVerts.Count];
        meshVertices.CopyTo(allVerts, 0);
        sideVerts.CopyTo(allVerts, meshVertices.Length);

        Vector2[] allUVs = new Vector2[meshUVs.Length + sideUVs.Count];
        meshUVs.CopyTo(allUVs, 0);
        sideUVs.CopyTo(allUVs, meshUVs.Length);

        Vector3[] allNormals = new Vector3[meshNormals.Length + sideNormals.Count];
        meshNormals.CopyTo(allNormals, 0);
        sideNormals.CopyTo(allNormals, meshNormals.Length);

        int[] allTris = new int[meshTriangles.Length + sideTris.Count];
        meshTriangles.CopyTo(allTris, 0);
        for (int i = 0; i < sideTris.Count; i++)
            allTris[meshTriangles.Length + i] = sideTris[i];

        UnityEngine.Mesh mesh = new UnityEngine.Mesh();
        mesh.vertices = allVerts;
        mesh.uv = allUVs;
        mesh.normals = allNormals;
        mesh.triangles = allTris;

        GameObject meshObj = new GameObject("DrawnMesh", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider), typeof(Rigidbody), typeof(DraggableShape));
        meshObj.layer = LayerMask.NameToLayer("Terrain");
        meshObj.GetComponent<MeshFilter>().mesh = mesh;
        meshObj.GetComponent<MeshRenderer>().material = _currentDrawMaterial;

        var meshCollider = meshObj.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = true;

        var meshRigidbody = meshObj.GetComponent<Rigidbody>();
        meshRigidbody.position = centroid;
    }
}

public class TriangulatedShape2D
{
    protected Tess _tess;

    public int[] Triangles
    {
        get { return _tess.Elements; }
        set { }
    }

    public int TriangleCount
    {
        get { return _tess.ElementCount; }
        set { }
    }

    public ContourVertex[] Vertices
    {
        get { return _tess.Vertices; }
        set { }
    }

    public int VertexCount
    {
        get { return _tess.VertexCount; }
        set { }
    }

    public TriangulatedShape2D(List<Vector2> pointList)
    {
        tesselate(new List<List<Vector2>>() { pointList });
    }

    public TriangulatedShape2D(List<List<Vector2>> pointLists)
    {
        tesselate(pointLists);
    }

    public TriangulatedShape2D(List<Vector3> pointList)
    {
        tesselate(new List<List<Vector3>>() { pointList });
    }

    public TriangulatedShape2D(List<List<Vector3>> pointLists)
    {
        tesselate(pointLists);
    }

    protected void tesselate(List<List<Vector3>> pointLists)
    {
        _tess = new Tess();
        for (int p = 0; p < pointLists.Count; p++)
        {
            var contour = new ContourVertex[pointLists[p].Count];
            for (int i = 0; i < pointLists[p].Count; i++)
            {
                contour[i].Position = new Vec3(pointLists[p][i].x, pointLists[p][i].y, pointLists[p][i].z);
            }
            _tess.AddContour(contour, ContourOrientation.Original);
        }
        _tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);
    }

    protected void tesselate(List<List<Vector2>> pointLists)
    {
        _tess = new Tess();
        for (int p = 0; p < pointLists.Count; p++)
        {
            var contour = new ContourVertex[pointLists[p].Count];
            for (int i = 0; i < pointLists[p].Count; i++)
            {
                contour[i].Position = new Vec3(pointLists[p][i].x, pointLists[p][i].y, 0);
            }
            _tess.AddContour(contour, ContourOrientation.Original);
        }
        _tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);
    }
}