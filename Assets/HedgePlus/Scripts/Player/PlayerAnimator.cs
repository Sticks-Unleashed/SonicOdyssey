using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [System.Serializable] public class Sound
    {
        public string Name;
        public AudioClip clip;
    }

    public Animator animator;
    PlayerController player;
    PlayerActions Actions;
    float PrevRot;
    float CurRot;
    float DeltaRotation;
    public GameObject SpinBall;
    public ParticleSystem SpeedParticles;
    public ParticleSystem SpinDashParticles;
    public float ParticleStartSpeed = 50;
    public float RotationDeadZone;
    public float TurnSmoothing; //This is purely for animation
    float Turning;
    public PlayerVoice Voice;
    public AudioSource ObjectSoundSource;
    public AudioSource RingSoundSource;
    public AudioSource ActionSoundSource;
    public AudioSource VoiceSource;
    public Sound[] ObjectSounds;
    public Sound[] ActionSounds;
    public Sound[] EnemySounds;
    bool PitchShifted;
    public AudioClip GetSoundFromBank (string name, Sound[] bank)
    {
        AudioClip sound = null;
        for (int i = 0; i < bank.Length; i++)
        {
            if (bank[i].Name == name)
            {
                sound = bank[i].clip;
            }
        }
        return sound;
    }
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<PlayerController>();
        Actions = GetComponent<PlayerActions>();
        CurRot = Vector3.Dot(transform.right, transform.forward);
        PrevRot = CurRot;
    }

    // Update is called once per frame
    void Update()
    {
        //Here we'll be setting all the main animator values.
        HedgeMath.SplitPlanarVector(player.rigidBody.velocity, player.GroundNormal, out Vector3 Ground, out Vector3 Air);
        Vector3 LocalAirVelocity = transform.InverseTransformDirection(Air);
        animator.SetInteger("State", Actions.StateIndex);
        animator.SetBool("Grounded", player.Grounded);
        animator.SetFloat("AirSpeed", LocalAirVelocity.y);
        animator.SetBool("Crouching", player.Crouching);
        animator.SetBool("Braking", player.Skidding);
        animator.SetBool("Dead", PlayerHealth.IsDead);
        SpinBall.GetComponent<Animator>().SetFloat("Speed", Ground.magnitude);

        SpinBall.SetActive(PlayerActions.currentState is JumpState);

        if (PlayerActions.currentState is SpinDashState)
        {
            SpinDashState s = PlayerActions.currentState as SpinDashState;
            animator.SetFloat("GroundSpeed", s.SpinDashCharge);
            if (!SpinDashParticles.isPlaying)
            {
                SpinDashParticles.Play();
            }
        }
        else
        {
            animator.SetFloat("GroundSpeed", Ground.magnitude);
            if (SpinDashParticles.isPlaying)
            {
                SpinDashParticles.Stop();
            }
        }

        //Set turn amount
        ///To get the turn amount, we first get which direction Sonic is turning by using a dot product between Sonic's right vector and Sonic's velocity.
        ///If the dot is positive, he is turning right. If it is negative, he is turning left.
        ///Once we have the turning amount, we set the actual Turn float. If the rotation amount is above the dead zone, we lerp Turning to the turn amount.
        ///If it is under the dead zone, we lerp it to 0.
        DeltaRotation = Vector3.Dot(transform.right, player.rigidBody.velocity.normalized) * 10;
        DeltaRotation = HedgeMath.ClampFloat(DeltaRotation, -1, 1);
        if (DeltaRotation > RotationDeadZone || DeltaRotation < -RotationDeadZone)
        {
            Turning = Mathf.Lerp(Turning, DeltaRotation, Time.deltaTime * TurnSmoothing);
        } else
        {
            Turning = Mathf.Lerp(Turning, 0f, Time.deltaTime * TurnSmoothing);
        }
        animator.SetFloat("HorizontalInput", Turning);

        ///Here we're simply playing and stopping the speed line particles depending on whether or not Sonic is up to speed
        ///and whether or not the particles are already playing.
        if (player.rigidBody.velocity.magnitude >= ParticleStartSpeed && !SpeedParticles.isPlaying)
        {
            SpeedParticles.Play();
        } else if (player.rigidBody.velocity.magnitude < ParticleStartSpeed && SpeedParticles.isPlaying)
        {
            SpeedParticles.Stop();
        }
    }

    public void PlayRingSound(bool overlap)
    {
        if (overlap)
        {
            ObjectSoundSource.PlayOneShot(GetSoundFromBank("Ring", ObjectSounds));
        } else
        {
            if (RingSoundSource.isPlaying)
            {
                float pitch = (PitchShifted = !PitchShifted) ? 0.985f : 1f;
                RingSoundSource.pitch = pitch;
            }
            else
            {
                RingSoundSource.pitch = 1f;
            }
            RingSoundSource.Play();
        }
    }
    public void PlayCapsuleSound()
    {
        ObjectSoundSource.PlayOneShot(GetSoundFromBank("Capsule", ObjectSounds));
    }

    public void PlayOneUp()
    {
        ObjectSoundSource.PlayOneShot(GetSoundFromBank("OneUp", ObjectSounds));
    }

    public void PlayJumpSound()
    {
        ActionSoundSource.clip = GetSoundFromBank("Jump", ActionSounds);
        ActionSoundSource.Play();
    }

    public void PlayDoubleJumpSound()
    {
        ActionSoundSource.clip = GetSoundFromBank("DoubleJump", ActionSounds);
        ActionSoundSource.Play();
    }

    public void SpinDashLoop()
    {
        ActionSoundSource.clip = GetSoundFromBank("Adventure Spindash", ActionSounds);
        ActionSoundSource.loop = true;
        ActionSoundSource.Play();
    }

    public void SpinDashSingle()
    {
        ActionSoundSource.clip = GetSoundFromBank("Classic Spindash", ActionSounds);
        ActionSoundSource.Play();
    }

    public void SpinDashRelease()
    {
        ActionSoundSource.clip = GetSoundFromBank("Spindash Release", ActionSounds);
        ActionSoundSource.loop = false;
        ActionSoundSource.Play();
    }
    public void PlayHomingSound()
    {
        ActionSoundSource.clip = GetSoundFromBank("Homing", ActionSounds);
        ActionSoundSource.Play();
    }

    public void PlayTargetSound()
    {
        ActionSoundSource.PlayOneShot(GetSoundFromBank("Target", EnemySounds));
    }

    public void PlayEnemyHit()
    {
        AudioClip RandomSound = null;
        int RNG = Random.Range(0, 2);
        switch (RNG)
        {
            case 0:
                RandomSound = GetSoundFromBank("Hit1", EnemySounds);
                break;
            case 1:
                RandomSound = GetSoundFromBank("Hit2", EnemySounds);
                break;
            case 2:
                RandomSound = GetSoundFromBank("Hit3", EnemySounds);
                break;
            default:
                RandomSound = GetSoundFromBank("Hit1", EnemySounds);
                break;
        }
        ActionSoundSource.PlayOneShot(RandomSound);
    }

    public void PlayJumpVoice()
    {
        VoiceSource.clip = Voice.GetVoiceClip("Jump");
        if (VoiceSource.clip != null)
            VoiceSource.Play();
    }

    public void PlayHomingVoice()
    {
        VoiceSource.clip = Voice.GetVoiceClip("Homing");
        if (VoiceSource.clip != null)
            VoiceSource.Play();
    }

    public void PlayHurtVoice()
    {
        VoiceSource.clip = Voice.GetVoiceClip("Damage");
        if (VoiceSource.clip != null)
            VoiceSource.Play();
    }

    public void PlayDeathVoice()
    {
        VoiceSource.clip = Voice.GetVoiceClip("Death");
        if (VoiceSource.clip != null)
            VoiceSource.Play();
    }

}
