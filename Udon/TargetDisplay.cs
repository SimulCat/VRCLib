
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Wrapper.Modules;

[RequireComponent(typeof(ParticleSystem))]
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]

public class TargetDisplay : UdonSharpBehaviour
{
    [SerializeField] private ParticleSystem displayParticles;
    [SerializeField] private int bufferMax = 40;
    [SerializeField] private Vector3 decalRotation3D = Vector3.zero;
    [SerializeField] private Vector3 groundRotation3D = Vector3.zero;
    private Color[] colourBuf;
    private Vector3[] locationBuf;
    private Vector3[] rotationBuf;
    private int bufferCount = 0;
    private bool started = false;
    [SerializeField]
    float particleSize = 0.001f;

    [SerializeField]
    private float markerLifetime = 10f;
    public float MarkerLifetime { 
        get => markerLifetime;
        set
        {
            if (value == markerLifetime) 
                return;
            markerLifetime = value;
            needsReview = true;
        }
    } 
    Vector3 particleSize3D = Vector3.zero;
    bool needsReview = false;
    bool dissolveRequired = false;
    public void SetPlay(bool isPlaying)
    {
        if (isPlaying)
            displayParticles.Play();
        else
            displayParticles.Pause();
    }
    public float ParticleSize
    {
        get => particleSize; 
        set
        {
            if (particleSize != value)
            {
                particleSize = value;
                needsReview = true;
            }
            particleSize3D = Vector3.one * particleSize;
        }
    }
//    [SerializeField] bool useLocalX;
  //  Vector3 localPos = Vector3.zero;
    public void PlotParticle(Vector3 location, Color color, bool onGround, float lifetime = -1f)
    {
        if (!started)
            return;
        if (bufferCount >= bufferMax) 
            return;
        if (lifetime > 0) 
            markerLifetime = lifetime;
        locationBuf[bufferCount] = location; 
        colourBuf[bufferCount] = color;
        rotationBuf[bufferCount] = onGround ? groundRotation3D : decalRotation3D;
        bufferCount++;
    }

    public void Dissolve()
    {
        dissolveRequired = true;
    }

    public void Clear()
    {
        if (displayParticles != null)
            displayParticles.Clear();
    }

    private float polltime = 2;
    [SerializeField]
    private int particleCount;

    private ParticleSystem.Particle[] particles;
    private void LateUpdate()
    {
        polltime -= Time.deltaTime;
        bool isTime = (polltime < 0);
        if (isTime)
            polltime += 0.3f;
        if (bufferCount <= 0)
            return;
        if (!isTime && (needsReview || !dissolveRequired) && (bufferCount < bufferMax))
            return;
        //localPos = transform.position;
        if (displayParticles == null)
        {
            bufferCount = 0;
            return;
        }
        //float myScale = displayParticles.transform.localScale.x;
        //float particleScale = myScale * displayParticles.main.startSize.constant;
        particleCount = displayParticles.particleCount;
        if ((particles == null) || (particles.Length < bufferMax + particleCount ))
            particles = new ParticleSystem.Particle[bufferMax + particleCount];
        particleCount = displayParticles.GetParticles(particles);
        int updateCount = 0;
        for (int i = 0; i < bufferCount; i++)
        {
            var particle = new ParticleSystem.Particle();
            particle.position = locationBuf[i];
            particle.startColor = colourBuf[i];
            //particle.startSize = locationBuf[i].w;
            particle.startSize3D = particleSize3D;
            particle.startLifetime = markerLifetime;
            particle.remainingLifetime = markerLifetime;
            particle.rotation3D = rotationBuf[i];
            particles[particleCount++] = particle;
            updateCount++;
        }
        bufferCount = 0;
        if (needsReview || dissolveRequired)
        {
            updateCount++;
            for (int i = 0; i < particleCount; i++)
            {
                if (needsReview)
                {
                    particles[i].startSize3D = particleSize3D;
                    if (particles[i].remainingLifetime > markerLifetime)
                        particles[i].remainingLifetime = markerLifetime;
                }
                if (dissolveRequired)
                    particles[i].remainingLifetime /= 5;
            }
            needsReview = false;
            dissolveRequired = false;
        }
        if (updateCount > 0)
            displayParticles.SetParticles(particles,particleCount);
    }
    void Start()
    {
        polltime = Random.Range(1f, 3f);
        if (displayParticles == null)
            displayParticles = GetComponent<ParticleSystem>();
        colourBuf = new Color[bufferMax+1];
        locationBuf = new Vector3[bufferMax+1];
        rotationBuf = new Vector3[bufferMax+1];
        ParticleSize = particleSize;
        started = true;
    }
}
