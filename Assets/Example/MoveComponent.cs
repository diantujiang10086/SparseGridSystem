using UnityEngine;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class MoveComponent
{
    public float speed = 5f;

    private float2 _startPos;
    private float2 _endPos;
    private float _t = 0f;
    private float _totalDistance = 0f;
    private Unit unit;

    public void Start(Unit unit)
    {
        this.unit = unit;
        speed = Random.Range(10, 30);
        CreatePosition();
    }

    public void Update()
    {
        float delta = (speed * Time.deltaTime) / _totalDistance;
        _t += delta;

        unit.Position = math.lerp(_startPos, _endPos, _t);

        if (_t >= 1f)
        {
            CreatePosition();
        }
    }
    void CreatePosition()
    {
        _t = 0;
        _startPos = unit.Position;
        _endPos = new float2(Random.Range(0, 100f), Random.Range(0, 100f));
        _totalDistance = math.distance(_startPos, _endPos);
    }
}
