using GeometricDrag;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class PropellerController : MonoBehaviour
{
    public LinearMapping LinearInput;
    public bool OverrideLinearInput = false;

[Range(0f, 130f)]
    public float Speed = 1f;
    public GlobalWindSource WindSource;
    public ParticleSystem WindParticles;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!OverrideLinearInput)
            Speed = LinearInput.value * 130f;

        transform.Rotate((Vector3.right), 10f * Time.deltaTime * Speed);

        WindSource.Strength = Speed * .8f;

        UpdateWindParticles();
    }

    void UpdateWindParticles()
    {
        var main = WindParticles.main;
        
        float tunnelHeight = 4.116f;

        float maxLife = tunnelHeight / Speed;

        main.startSpeed = Speed * 1.2f;
        main.startLifetime = maxLife * 12 * (Speed / (130 + 10))  * 1.4f;

        var emission = WindParticles.emission;
        emission.rateOverTime = Speed * .4f;
        
        
    }

}
