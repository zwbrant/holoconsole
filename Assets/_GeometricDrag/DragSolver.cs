using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Zane;
using Vec3 = UnityEngine.Vector3;

public class DragSolver : MonoBehaviour
{
    public const float AirMass = 1f;
    public static Vec3 Wind = Vec3.zero;

    [Range(0, 1f)]
    public float DragMulti = .01f;
    public bool EnableBurst = true;
    public int BatchSize = 32;
    public bool DebugAngleMags = true;
    public bool DebugForceVectors = false;
    public bool DebugVelocityVector = false;
    public bool UseSimpleDrag = false;
    [Header("References")]
    public MeshFilter MeshFilter;
    public Rigidbody Rbody;

    private Vec3[] _normals;
    private Mesh _mesh;
    private int[] _triIndices;
    private Color32[] _colors;

    // persistent
    private NativeArray<Triangle> _nLocalTris;
    private NativeArray<Vec3> _nVertices;
    // temp
    private NativeArray<Vec3> _nDragForces;
    private NativeArray<Vec3> _nMidpoints;
    private NativeArray<DragResult> _nDragResults;
    private NativeArray<DragResult> _nWindResults;
    private NativeArray<float> _nAngleMags;


    // Start is called before the first frame update
    void Start()
    {
        if (MeshFilter == null)
            _mesh = GetComponent<MeshFilter>().mesh;
        else
            _mesh = MeshFilter.mesh;
        if (Rbody == null)
            Rbody = GetComponent<Rigidbody>();
        if (Rbody == null)
            Rbody = gameObject.AddComponent<Rigidbody>();

        _triIndices = _mesh.triangles;
        _normals = new Vec3[_mesh.triangles.Length / 3];
        _colors = new Color32[_mesh.vertices.Length];

        _nVertices = new NativeArray<Vec3>(_mesh.vertices, Allocator.Persistent);
        _nLocalTris = new NativeArray<Triangle>(_mesh.triangles.Length / 3, Allocator.Persistent);

        for (int i = 0; i < _mesh.triangles.Length; i += 3)
        {
            Triangle tri = new Triangle(
                _mesh.vertices[_mesh.triangles[i]],
                _mesh.vertices[_mesh.triangles[i]],
                _mesh.vertices[_mesh.triangles[i]]);
            _nLocalTris[i / 3] = tri;
        }

    }

    private JobHandle _dragJob;
    private JobHandle _windJob;

    private void Update()
    {
        if (!EnableBurst)
            return;

        _nDragForces = new NativeArray<Vec3>(_mesh.triangles.Length / 3, Allocator.TempJob);
        _nMidpoints = new NativeArray<Vec3>(_mesh.triangles.Length / 3, Allocator.TempJob);
        _nAngleMags = new NativeArray<float>(_mesh.triangles.Length / 3, Allocator.TempJob);
        _nDragResults = new NativeArray<DragResult>(_mesh.triangles.Length / 3, Allocator.TempJob);


        var j1 = new DragUpdateJob() {
            vertices = _nVertices,
            localTris = _nLocalTris,
            dragForces = _nDragForces,
            midpoints = _nMidpoints,
            rotation = transform.rotation,
            position = transform.position,
            localScale = transform.localScale,
            airVelocity = -Rbody.velocity,
            angleMags = _nAngleMags,
            dragMulti = DragMulti,
            useSimpleDrag = UseSimpleDrag,
            dragResults =_nDragResults
        };

        if (Wind.magnitude > 0.01f)
        {
            _nWindResults = new NativeArray<DragResult>(_mesh.triangles.Length / 3, Allocator.TempJob);

            var j2 = new DragUpdateJob()
            {
                vertices = _nVertices,
                localTris = _nLocalTris,
                dragForces = _nDragForces,
                midpoints = _nMidpoints,
                rotation = transform.rotation,
                position = transform.position,
                localScale = transform.localScale,
                airVelocity = Wind,
                angleMags = _nAngleMags,
                dragMulti = DragMulti,
                useSimpleDrag = UseSimpleDrag,
                dragResults = _nWindResults
            };

            _windJob = j2.Schedule(_mesh.triangles.Length / 3, BatchSize);
        }
        else if (_nWindResults.IsCreated)
            _nWindResults.Dispose();


        _dragJob = j1.Schedule(_mesh.triangles.Length / 3, BatchSize);

    }

    private void LateUpdate()
    {
        if (!EnableBurst)
            return;

        _dragJob.Complete();
        if (_nWindResults.IsCreated)
            _windJob.Complete();

        for (int i = 0; i < _nLocalTris.Length; i++)
        {

            if (DebugAngleMags)
                UpdateDebugColors(i, _nAngleMags[i]);

            if (!float.IsNaN(_nDragResults[i].DragForce.x))
            {
                Rbody.AddForceAtPosition(_nDragResults[i].DragForce, _nDragResults[i].ForceOrigin);
                if (DebugForceVectors)
                    Debug.DrawLine(_nMidpoints[i], _nMidpoints[i] + _nDragForces[i], Color.yellow);
            }

            if (_nWindResults.IsCreated && _nWindResults.Length > 0 &&!float.IsNaN(_nWindResults[i].DragForce.x))
                Rbody.AddForceAtPosition(_nDragResults[i].DragForce, _nDragResults[i].ForceOrigin);

        }

        if (DebugAngleMags)
            _mesh.colors32 = _colors;

        _nDragResults.Dispose();
        if (_nWindResults.IsCreated)
            _nWindResults.Dispose();
        _nDragForces.Dispose();
        _nMidpoints.Dispose();
        _nAngleMags.Dispose();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (EnableBurst)
            return;

        Vec3 pos = transform.position;
        Vec3 dragVect = -Rbody.velocity;

        if (dragVect.magnitude <= 0)
            return;

        if (DebugVelocityVector)
            Debug.DrawLine(pos, pos + dragVect);

        for (int i = 0; i < _mesh.triangles.Length; i += 3)
        {
            Vec3 v1, v2, v3;
            v1 = transform.TransformPoint(_nVertices[_triIndices[i]]);
            v2 = transform.TransformPoint(_nVertices[_triIndices[i + 1]]);
            v3 = transform.TransformPoint(_nVertices[_triIndices[i + 2]]);

            var tri = new Triangle(v1, v2, v3);

            // save triangle normal
            _normals[i / 3] = tri.Normal;

            // calculate the angle of this triangles resistance
            var cosAngle = Vec3.Dot(tri.Normal, dragVect) / (dragVect.magnitude * tri.Normal.magnitude);
            var angle = Mathf.Acos(cosAngle);

            // magnitude of drag: 180 = 1, 135 = 0.5, < 90 = 0
            var surfAngleMag = Mathf.Clamp((angle - Mathf.PI / 2) / (Mathf.PI / 2), 0, 1);

            //Debug.DrawLine(midPoint, midPoint + (surfNorm * triArea), Color.red);

            var fluidDensity = 1f;
            var velSqu = Rbody.velocity.sqrMagnitude;

            var dragForce2 = -.5f * fluidDensity * velSqu * tri.Area * surfAngleMag * Vec3.Normalize(Rbody.velocity);

            //var dragForce = -tri.Normal * tri.Area * surfAngleMag * dragVect.magnitude * DragMulti;

            if (dragForce2.magnitude > 0f)
                Rbody.AddForceAtPosition(dragForce2, tri.Midpoint);

            //Debug.DrawLine(tri.Midpoint, tri.Midpoint + Vector3.Reflect(dragVect, tri.Normal) * .1f, Color.green);

            if (DebugForceVectors)
                Debug.DrawLine(tri.Midpoint, tri.Midpoint + dragForce2, Color.yellow);

            UpdateDebugColors(i / 3, surfAngleMag);
        }

        _mesh.colors32 = _colors;
    }

    Color32 _red = new Color32(255, 0, 0, 1);
    Color32 _green = new Color32(0, 255, 0, 1);
    private void UpdateDebugColors(int triIndex, float dragMag)
    {
        var index = triIndex * 3;
        var c = Color32.Lerp(_green, _red, dragMag);
        _colors[_triIndices[index]] = c;
        _colors[_triIndices[index + 1]] = c;
        _colors[_triIndices[index + 2]] = c;
    }

    public struct DragUpdateJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Triangle> localTris;
        [ReadOnly]
        public NativeArray<Vec3> vertices;
        public Quaternion rotation;
        public Vec3 position;
        public Vec3 localScale;
        public Vec3 airVelocity;
        public float dragMulti;
        public bool useSimpleDrag;

        public NativeArray<DragResult> dragResults;
        public NativeArray<Vec3> dragForces;
        public NativeArray<Vec3> midpoints;
        public NativeArray<float> angleMags;

        public void Execute(int index)
        {
             DragV2(index);
        }

        public void DragV2(int i)
        {
            DragResult result;
            int triIndex = i * 3;

            // convert local vertice positions to world
            var p1 = Math.TransformPoint(localTris[i].P1, position, rotation, localScale);
            var p2 = Math.TransformPoint(localTris[i].P2, position, rotation, localScale);
            var p3 = Math.TransformPoint(localTris[i].P3, position, rotation, localScale);
            var tri = new Triangle(p1, p2, p3);

            // calculate the angle of this triangles resistance
            var cosAngle = Vec3.Dot(tri.Normal, airVelocity) / (airVelocity.magnitude * tri.Normal.magnitude);
            var angle = Mathf.Acos(cosAngle);

            // magnitude of drag: 180 = 1, 135 = 0.5, < 90 = 0
            angleMags[i] = Mathf.Clamp((angle - Mathf.PI / 2) / (Mathf.PI / 2), 0, 1);

            var velSqu = Mathf.Pow(airVelocity.magnitude, 2);


            result.DragForce = -.5f * velSqu * tri.Area * (angleMags[i] * dragMulti) * Vec3.Normalize(tri.Normal);

            result.ForceOrigin = tri.Midpoint;

            dragResults[i] = result;
        }
    }

    public struct DragResult {
        public Vec3 DragForce;
        public Vec3 ForceOrigin;
    }

    private void OnDisable()
    {
        _nLocalTris.Dispose();
        _nVertices.Dispose();
    }
}
