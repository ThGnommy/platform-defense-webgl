using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class SoundManager : MonoBehaviour
{
    // FMOD
    [FMODUnity.EventRef]
    public string footsteps;
    [FMODUnity.EventRef]
    public string jump;
    [FMODUnity.EventRef]
    public string land;

    // Footsteps
    FMOD.Studio.EventInstance footstepsEv;
    FMOD.Studio.PARAMETER_DESCRIPTION MaterialDescription;
    FMOD.Studio.PARAMETER_ID Material_ID;
    int m_Material;

    // Land 
    FMOD.Studio.EventInstance landEv;
    FMOD.Studio.EventDescription LandDescription;
    FMOD.Studio.PARAMETER_DESCRIPTION ImpactDescription;
    FMOD.Studio.PARAMETER_ID Impact_ID;
    int m_Impact;

    public AudioSource sfxSource;
    public AudioSource musicSource;
    public AudioMixerGroup environmentOutputChannel;

    public List<Sound> sounds;

    public float spatialSoundsMinDistance, spatialSoundsMaxDistance;
    public AnimationCurve spatialSoundFalloffCurve;

    public static SoundManager instance {
        get;
        private set;
    }

    private MaterialZone.Zone materialZone;

    private Player player;

    public void Awake() {
        SoundManager.instance = this;
        // begin playing music
        this.musicSource.Play();
        this.sfxSource.outputAudioMixerGroup = this.environmentOutputChannel;
    }

    private void Start()
    {
        footstepsEv = FMODUnity.RuntimeManager.CreateInstance(footsteps);
        FMODUnity.RuntimeManager.StudioSystem.getParameterDescriptionByName("Material", out MaterialDescription);
        Material_ID = MaterialDescription.id;

        landEv = FMODUnity.RuntimeManager.CreateInstance(land);
        landEv.getDescription(out LandDescription);
        LandDescription.getParameterDescriptionByName("Impact", out ImpactDescription);
        Impact_ID = ImpactDescription.id;
    }

    private void Update()
    {
        if(player.velocity[1] <= 0)
        {
            SetLandParameter();
        }
    }

    public void SetZone(MaterialZone.Zone zone) {
        this.materialZone = zone;
    }

    public void SetPlayer(Player player) {
        this.player = player;
    }

    public void PlayPlayerJumpSfx() {
        SoundId id = SoundId.None;
        switch (this.materialZone) {
            case MaterialZone.Zone.Grass:
                id = SoundId.PlayerJumpGrass;
                break;
            case MaterialZone.Zone.Rock:
                id = SoundId.PlayerJumpRock;
                break;
            case MaterialZone.Zone.Wood:
                id = SoundId.PlayerJumpWood;
                break;

            default:
                return;
        }

        PlaySfx(id);
        FMODUnity.RuntimeManager.PlayOneShot(jump, transform.position);
    }

    private bool LandSoftSfx()
    {
        float p_Vel = player.velocity[1];

        if(p_Vel > -50 && p_Vel < -30)
        {
            return true;
        }

        return false;
    }

    private bool LandHardSfx()
    {
        float p_Vel = player.velocity[1];

        if (p_Vel < -50)
        {
            return true;
        }

        return false;
    }

    private void SetLandParameter()
    {
        if(!LandSoftSfx() && !LandHardSfx()) 
        { 
            m_Impact = 0;
            landEv.setParameterByID(Impact_ID, m_Impact);
        }
        if(LandSoftSfx() && !LandHardSfx()) 
        { 
            m_Impact = 1;
            landEv.setParameterByID(Impact_ID, m_Impact);
        }
        if (!LandSoftSfx() && LandHardSfx()) 
        { 
            m_Impact = 2;
            landEv.setParameterByID(Impact_ID, m_Impact);
        }
    }

    public void PlayPlayerLandSfx() {
        SoundId id = SoundId.None;
        switch (this.materialZone) {
            case MaterialZone.Zone.Grass:
                id = SoundId.PlayerLandGrass;
                m_Material = 0;
                FMODUnity.RuntimeManager.StudioSystem.setParameterByID(Material_ID, m_Material);
                break;
            case MaterialZone.Zone.Rock:
                id = SoundId.PlayerLandRock;
                m_Material = 1;
                FMODUnity.RuntimeManager.StudioSystem.setParameterByID(Material_ID, m_Material);
                landEv.start();
                break;
            case MaterialZone.Zone.Wood:
                m_Material = 2;
                FMODUnity.RuntimeManager.StudioSystem.setParameterByID(Material_ID, m_Material);
                landEv.start();
                id = SoundId.PlayerLandWood;
                break;
            default:
                return;
        }

        PlaySfx(id);
    }

    public void PlayPlayerStepSfx() {
        SoundId id = SoundId.None;

        switch (this.materialZone) {
            case MaterialZone.Zone.Grass:
                id = SoundId.PlayerStepGrass;
                m_Material = 0;
                FMODUnity.RuntimeManager.StudioSystem.setParameterByID(Material_ID, m_Material);
                footstepsEv.start();
                break;
            case MaterialZone.Zone.Rock:
                id = SoundId.PlayerStepRock;
                m_Material = 1;
                FMODUnity.RuntimeManager.StudioSystem.setParameterByID(Material_ID, m_Material);
                footstepsEv.start();
                break;
            case MaterialZone.Zone.Wood:
                id = SoundId.PlayerStepWood;
                m_Material = 2;
                FMODUnity.RuntimeManager.StudioSystem.setParameterByID(Material_ID, m_Material);
                footstepsEv.start();
                break;

            default:
                return;
        }

        PlaySfx(id);
    }

    public Sound GetSound(SoundId id) {
        return this.sounds.Find(o => o.id == id);
    }

    public void PlaySfx(SoundId id) {
        Sound sound = this.sounds.Find(o => o.id == id);
        if (sound == null) {
            Debug.LogWarning("Couldn't find sound with id " + id.ToString());
            return;
        }

        sound.ApplyPitch(this.sfxSource);

        this.sfxSource.PlayOneShot(sound.clip, sound.volume);
    }

    public void PlayAtPosition(SoundId id, Vector2 pos) {
        float dist = Vector2.Distance(this.player.transform.position, pos);
        if(dist > this.spatialSoundsMaxDistance) {
            return;
        }

        Sound sound = this.sounds.Find(o => o.id == id);
        if (sound == null) {
            Debug.LogWarning("Couldn't find sound with id " + id.ToString());
            return;
        }

        sound.ApplyPitch(this.sfxSource);
    
        float t = (dist - this.spatialSoundsMinDistance) / (this.spatialSoundsMaxDistance - this.spatialSoundsMinDistance);
        t = Mathf.Clamp01(t);

        this.sfxSource.PlayOneShot(sound.clip, sound.volume * this.spatialSoundFalloffCurve.Evaluate(t));
    }
}

[System.Serializable]
public class Sound {
    public SoundId id;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;

    public AudioMixerGroup outputChannel;

    public bool randomPitch;
    [Range(0, 2f)]
    public float pitchMin = 1f, pitchMax = 1;

    public void ApplyPitch(AudioSource source) {
        if(this.randomPitch) {
            source.pitch = Random.Range(this.pitchMin, this.pitchMax);
        } else {
            source.pitch = 1f;
        }
    }
}

[System.Serializable]
public enum SoundId {
    None,

    PlayerJumpGrass,
    PlayerJumpRock,
    PlayerJumpWood,
    PlayerStepGrass,
    PlayerStepRock,
    PlayerStepWood,

    BulletTurretShot,
    BombTurretShot,
    IceTurretShot,

    TokenPickup,
    TokenAmbient,
    UIMove,
    UIBack,
    UISelect,

    BombTurretHit,
    IceTurretHit,

    Placeholder1,
    PlayerSpikesDeath,

    JumpyPlatform,

    BouncyEnemyJump,
    BouncyEnemyDeath,
    
    RunningEnemyDeath,

    PlayerLandGrass,
    PlayerLandRock,
    PlayerLandWood,

    BouncyEnemyLand,
    RunningEnemyLand,

    Placeholder2,
    Placeholder3,
    Placeholder4,
    Placeholder5,
    Placeholder6,
    Placeholder7,
    Placeholder8,
    Placeholder9,

}