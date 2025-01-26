using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEditor.Search;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }

    public event EventHandler OnSelectionStart;
    public event EventHandler OnSelectionEnd;


    private Vector2 selectStartPos;
    private Camera _cam;

    private void Awake()
    {
        Instance = this;
        _cam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectStartPos = Input.mousePosition;
            OnSelectionStart?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonUp(0))
        {
            var selectEndPos = Input.mousePosition;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var query = new EntityQueryBuilder(allocator: Allocator.Temp)
                .WithAll<Selected>()
                .Build(em);

            var eArray = query.ToEntityArray(Allocator.Temp);
            var eSelectors = query.ToComponentDataArray<Selected>(Allocator.Temp);
            for (int i = 0; i < eArray.Length; i++)
            {
                em.SetComponentEnabled<Selected>(eArray[i], false);
                var el = eSelectors[i];
                el.onDeselected = true;
                em.SetComponentData(eArray[i], el);
            }

            Rect selectArea = GetSelectionRect();
            float size = selectArea.width + selectArea.height;
            float minSize = 40f;
            bool isMultiselection = size > minSize;

            if (isMultiselection)
            {
                query = new EntityQueryBuilder(allocator: Allocator.Temp)
                    .WithAll<LocalTransform, Unit>()
                    .WithPresent<Selected>()
                    .Build(em);
                eArray = query.ToEntityArray(Allocator.Temp);


                NativeArray<LocalTransform> initArr =
                query.ToComponentDataArray<LocalTransform>(Allocator.Temp);
                for (int i = 0; i < initArr.Length; i++)
                {
                    var mover = initArr[i];
                    var scrPoint = _cam.WorldToScreenPoint(mover.Position);
                    if (selectArea.Contains(scrPoint))
                    {
                        em.SetComponentEnabled<Selected>(eArray[i], true);

                        var selected = em.GetComponentData<Selected>(eArray[i]);
                        selected.onSelected = true;
                        em.SetComponentData(eArray[i], selected);
                    }
                }
            }
            else
            {
                query = em.CreateEntityQuery(typeof(PhysicsWorldSingleton));
                var physWorld = query.GetSingleton<PhysicsWorldSingleton>();
                var colls = physWorld.CollisionWorld;

                UnityEngine.Ray cameraRay = _cam.ScreenPointToRay(Input.mousePosition);

                int unitsLayer = 6;

                var ray = new RaycastInput()
                {
                    Start = cameraRay.GetPoint(0f),
                    End = cameraRay.GetPoint(9999f),
                    Filter = new CollisionFilter()
                    {
                        GroupIndex = 0,
                        BelongsTo = ~0u,
                        CollidesWith = 1u << unitsLayer,
                    }
                };


                if (colls.CastRay(ray, out Unity.Physics.RaycastHit hit))
                {
                    if (em.HasComponent<Unit>(hit.Entity))
                    {
                        em.SetComponentEnabled<Selected>(hit.Entity, true);
                        var selected = em.GetComponentData<Selected>(hit.Entity);
                        selected.onSelected = true;
                        em.SetComponentData(hit.Entity, selected);
                    }
                }
            }

            OnSelectionEnd?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mPos = MouseWorldPosition.Instance.GetPosition();

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var query = new EntityQueryBuilder(allocator: Allocator.Temp)
                .WithAll<UnitMover, Selected>()
                .Build(em);

            NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
            NativeArray<UnitMover> initArr =
                query.ToComponentDataArray<UnitMover>(Allocator.Temp);

            var positions = GenPositions(mPos, initArr.Length);

            for (int i = 0; i < initArr.Length; i++)
            {
                var mover = initArr[i];
                mover.targetPos = positions[i];
                initArr[i] = mover;
            }
            query.CopyFromComponentDataArray(initArr);
        }
    }

    public Rect GetSelectionRect()
    {
        var selectEndPos = Input.mousePosition;

        Vector2 lowerLeft = new Vector2(
            Mathf.Min(selectStartPos.x, selectEndPos.x),
            Mathf.Min(selectStartPos.y, selectEndPos.y));

        Vector2 upperRight = new Vector2(
            Mathf.Max(selectStartPos.x, selectEndPos.x),
            Mathf.Max(selectStartPos.y, selectEndPos.y));

        return new Rect(lowerLeft.x, 
                        lowerLeft.y, 
                        upperRight.x - lowerLeft.x,
                        upperRight.y - lowerLeft.y);
    }

    private NativeArray<float3> GenPositions(float3 target, int count)
    {
        NativeArray<float3> arr = new NativeArray<float3>(count, Allocator.Temp);

        if(count == 0)        
            return arr;
        

        arr[0] = target;

        if(count == 1)       
            return arr;

        float ringSize = 2.2f;
        int ring = 0;
        int pos = 1;

        while (pos < count)
        {
            int ringSizeCount = 3 + ring * 2;

            for (int i = 0; i < ringSizeCount; i++)
            {
                float angle = i * (2 * Mathf.PI / ringSizeCount);
                float3 ringVector = math.rotate(quaternion.RotateY(angle), new float3((ring + 1) * ringSize, 0f, 0f));
                float3 newPos = target + ringVector;

                arr[pos] = newPos;
                pos++;

                if(pos >= count)
                    break;
            }

            ring++;
        }
        return arr;
    }
}
