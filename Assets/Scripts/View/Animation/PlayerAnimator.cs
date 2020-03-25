using UnityEngine;

// wanna be state machine for player animations
public class PlayerAnimator : SpriteAnimator {
    public float runningHorizontalSpeedThreshold;

    public float fallSpeedThreshold;
    public float jumpMidPointSpeedThreshold;
    public float jumpDownSlowSpeedThreshold;

    [Header("Dust")]
    public GameObject dustLandPrefab;
    public GameObject dustRunPrefab;
    public GameObject dustStopPrefab;
    public float dustStepPeriod;
    public float dustStepPeriodRand;

    [Header("Sfx settings")]
    public float stepPeriod;
    public float stepPeriodRand;

    public ParticleSystem bounceParticles;

    private State state;
    
    private JumpPhase jumpPhase; 

    private string idleAnimName = "Idle";

    private float lastStepSfxTime;
    private float lastStepDustTime;

    public void Refresh(Vector2 velocity, Vector2 input, Vector2 groundNormal, Vector2 groundPos, bool grounded, bool wallSliding, bool jumped, bool wallJumped, bool bounced, bool dead) {

        // change state if needed:

        bool wasIdle = this.state == State.Idle;
        bool wasRunning = this.state == State.Run;
        bool wasJumping = this.state == State.Jump || this.state == State.WallJump;
        bool wasWallJumping = false;

        if(this.state == State.Dead && !dead) {
            this.state = State.Idle;
        }
        
        string runAnimName = "Run";

        switch(this.state) {
            case State.Idle:
                {
                    if(jumped || (!grounded && Mathf.Abs(velocity.y) > this.fallSpeedThreshold)) {
                        this.state = State.Jump;
                    } else {
                        if(Mathf.Abs(input.x) > 0.4f) {
                            this.state = State.Run;
                        }
                    }
                }
                break;

            case State.Jump:
                {
                    if(grounded) {
                        if(Mathf.Abs(input.x) > 0.4f) {
                            this.state = State.Run;
                        } else {
                            this.state = State.Idle;
                        }
                    } else if(wallSliding && velocity.y < 0) {
                        this.state = State.WallSlide;
                    }
                }
                break;

            case State.Run:
                {
                    if(jumped || (!grounded && Mathf.Abs(velocity.y) > this.fallSpeedThreshold)) {
                        this.state = State.Jump;
                    } else if (wallSliding && velocity.y < 0) {
                        this.state = State.WallSlide;
                    } else if(Mathf.Abs(input.x) <= 0.4f) {
                        this.state = State.Idle;
                    }
                }
                break;

            case State.WallSlide:
                {
                    if(wallJumped) {
                        this.state = State.WallJump;
                    } else {
                        if(!wallSliding) {
                            this.state = State.Jump; // falling down
                        }
                    }
                }
                break;

            case State.WallJump:
                {
                    wasWallJumping = true;
                    this.state = State.Jump;
                }
                break;
        }
        

        if(dead && this.state != State.Dead) {
            this.state = State.Dead;

            this.bounceParticles.Stop();
        }


        // state logic:

        switch(this.state) {
            case State.Idle:
                {
                    if(!wasIdle) {
                        if(wasRunning) {
                            this.idleAnimName = "IdleSlowdown";
                            PlayDust(groundPos, groundNormal, this.dustStopPrefab);
                        } else if(wasJumping) {
                            this.idleAnimName = "JumpLand";
                            Landed(groundPos, groundNormal);
                        } else {
                            this.idleAnimName = "Idle";
                        }
                    } else {
                        if(this.idleAnimName == "IdleCapeWave") {
                            if(this.frame == 0) {
                                this.idleAnimName = "Idle";
                            }
                        } else if(this.idleAnimName != "Idle") {
                            if(this.finishedSingleShot) {
                                this.idleAnimName = "Idle";
                            }
                        } else {
                            if(this.frame == 0 && Random.value < 0.02f) {
                                this.idleAnimName = "IdleCapeWave";
                            }
                        }
                    }
                }
                break;

            case State.Jump:
                {
                    if((!wasJumping && jumped) || wallJumped) {
                        SoundManager.instance.PlayPlayerJumpSfx();
                    }

                    if(Mathf.Abs(velocity.y) < this.jumpMidPointSpeedThreshold) {
                        this.jumpPhase = JumpPhase.MidPoint;
                    } else if(velocity.y > 0) {
                        this.jumpPhase = JumpPhase.Up;
                    } else if(velocity.y > -this.jumpDownSlowSpeedThreshold) {
                        this.jumpPhase = JumpPhase.DownSlow;
                    } else {
                        this.jumpPhase = JumpPhase.DownFast;
                    }
                }
                break;

            case State.Run:
                {
                    if(!wasRunning) {
                        if(wasJumping) {
                            Landed(groundPos, groundNormal);
                        }
                    }

                    if(Time.time - this.lastStepSfxTime > this.stepPeriod) {
                        SoundManager.instance.PlayPlayerStepSfx();
                        this.lastStepSfxTime = Time.time + Random.value * this.stepPeriodRand;
                    }

                    if(Time.time - this.lastStepDustTime > this.dustStepPeriod) {
                        PlayDust(groundPos, groundNormal, this.dustRunPrefab);
                        this.lastStepDustTime = Time.time + Random.value * this.dustStepPeriodRand;
                    }
                }
                break;

            case State.WallSlide:
                {
                    
                }
                break;

            case State.WallJump:
                {
                    if(wallJumped) {
                        SoundManager.instance.PlayPlayerJumpSfx();
                    }
                }
                break;
        }


        // set active animation according to state:
        
        switch(this.state) {
            case State.Idle:
                SetActive(this.idleAnimName);
                break;
            case State.Run:
                SetActive(runAnimName);
                break;
            case State.WallSlide:
                SetActive("Wallslide1");
                break;
            case State.WallJump:
                SetActive("Walljump");
                break;
            case State.Jump:
                switch(this.jumpPhase) {
                    case JumpPhase.Up:
                        SetActive("JumpUp", true, !wasWallJumping); // don't reset timer in case we're coming from wall jump
                        break;
                    case JumpPhase.MidPoint:
                        SetActive("JumpMidPoint");
                        break;
                    case JumpPhase.DownSlow:
                        SetActive("JumpDownSlow");
                        break;
                    case JumpPhase.DownFast:
                        SetActive("JumpDownFast");
                        break;
                }
                break;
            case State.Dead:
                SetActive("Death");
                break;
        }

        // misc:
        if(bounced) {
            this.bounceParticles.Play();
        }
    }

    private void Landed(Vector2 groundPos, Vector2 groundNormal) {
        GameObject dust = Pool.instance.Grab(this.dustLandPrefab);
        dust.transform.position = groundPos;
        dust.transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(groundNormal.y, groundNormal.x) * Mathf.Rad2Deg - 90);

        SoundManager.instance.PlayPlayerLandSfx();

        this.lastStepDustTime = Time.time;
        this.lastStepSfxTime = Time.time + Random.value * this.stepPeriodRand;
    }

    private void PlayDust(Vector2 groundPos, Vector2 groundNormal, GameObject prefab) {
        GameObject dust = Pool.instance.Grab(prefab);
        dust.transform.position = groundPos;

        bool facingRight = this.transform.eulerAngles.y < 90f;
        float angle = Mathf.Atan2(groundNormal.y, groundNormal.x) * Mathf.Rad2Deg;
        dust.transform.localEulerAngles = new Vector3(
            0,
            facingRight ? 0f : 180f,
            facingRight ? (angle - 90f) : (90f - angle)
        );
    }

    private enum State {
        Idle,
        Run,
        WallSlide,
        WallJump,
        Jump,
        Dead,
    }

    private enum JumpPhase {
        Up,
        MidPoint,
        DownSlow,
        DownFast,
    }
}