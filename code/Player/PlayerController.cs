using Sandbox;
using System;

public partial class PlayerController : BasePlayerController
{
	[Net] public float SprintSpeed { get; set; } = 320.0f;
    [Net] public float WalkSpeed { get; set; } = 150.0f;
    [Net] public float DefaultSpeed { get; set; } = 190.0f;
    [Net] public float Acceleration { get; set; } = 10.0f;
    [Net] public float AirAcceleration { get; set; } = 50.0f;
    [Net] public float FallSoundZ { get; set; } = -30.0f;
    [Net] public float GroundFriction { get; set; } = 4.0f;
    [Net] public float StopSpeed { get; set; } = 100.0f;
    [Net] public float Size { get; set; } = 20.0f;
    [Net] public float DistEpsilon { get; set; } = 0.03125f;
    [Net] public float GroundAngle { get; set; } = 46.0f;
    [Net] public float Bounce { get; set; } = 0.0f;
    [Net] public float MoveFriction { get; set; } = 1.0f;
    [Net] public float StepSize { get; set; } = 18.0f;
    [Net] public float MaxNonJumpVelocity { get; set; } = 140.0f;
    [Net] public float BodyGirth { get; set; } = 32.0f;
    [Net] public float BodyHeight { get; set; } = 72.0f;
    [Net] public float EyeHeight { get; set; } = 64.0f;
    [Net] public float Gravity { get; set; } = 800.0f;
    [Net] public float AirControl { get; set; } = 30.0f;
    public bool Swimming { get; set; } = false;
    [Net] public bool AutoJump { get; set; } = false;

    public Duck Duck;
    public Unstuck Unstuck;

    public static int MaxAirJumps { get; set; } = 2;
	private int maxAirJumpsValue = MaxAirJumps;

    public PlayerController()
    {
        Duck = new Duck( this );
        Unstuck = new Unstuck( this );
    }

    /// <summary>
    /// This is temporary, get the hull size for the player's collision
    /// </summary>
    public override BBox GetHull()
    {
        var girth = BodyGirth * 0.5f;
        var mins = new Vector3( -girth, -girth, 0 );
        var maxs = new Vector3( +girth, +girth, BodyHeight );

        return new BBox( mins, maxs );
    }


    // Duck body height 32
    // Eye Height 64
    // Duck Eye Height 28

    protected Vector3 mins;
    protected Vector3 maxs;

    public virtual void SetBBox( Vector3 mins, Vector3 maxs )
    {
        if ( this.mins == mins && this.maxs == maxs )
            return;

        this.mins = mins;
        this.maxs = maxs;
    }

    /// <summary>
    /// Update the size of the bbox. We should really trigger some shit if this changes.
    /// </summary>
    public virtual void UpdateBBox()
    {
        var girth = BodyGirth * 0.5f;

        var mins = new Vector3( -girth, -girth, 0 ) * Pawn.Scale;
        var maxs = new Vector3( +girth, +girth, BodyHeight ) * Pawn.Scale;

        Duck.UpdateBBox( ref mins, ref maxs, Pawn.Scale );

        SetBBox( mins, maxs );
    }

    protected float SurfaceFriction;


    public override void FrameSimulate()
    {
        base.FrameSimulate();

        EyeRotation = Input.Rotation;
    }

    public override void Simulate()
    {
        EyeLocalPosition = Vector3.Up * (EyeHeight * Pawn.Scale);
        UpdateBBox();

        EyeLocalPosition += TraceOffset;
        EyeRotation = Input.Rotation;

        RestoreGroundPos();

        if ( Unstuck.TestAndFix() )
            return;

        CheckLadder();
        Swimming = Pawn.WaterLevel > 0.6f;
        
        // Start Gravity
        if ( !Swimming && !IsTouchingLadder )
        {
            Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
            Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;

            BaseVelocity = BaseVelocity.WithZ( 0 );
        }

        if ( AutoJump ? Input.Down( InputButton.Jump ) : Input.Pressed( InputButton.Jump ) )
        {
            CheckJumpButton();
        }

        // Fricion is handled before we add in any base velocity. That way, if we are on a conveyor,
        //  we don't slow when standing still, relative to the conveyor.
        var bStartOnGround = GroundEntity != null;
        //bool bDropSound = false;
        if ( bStartOnGround )
        {
	        Velocity = Velocity.WithZ( 0 );

            if ( GroundEntity != null )
            {
                ApplyFriction( GroundFriction * SurfaceFriction );
            }
        }

        //
        // Work out wish velocity.. just take input, rotate it to view, clamp to -1, 1
        //
        WishVelocity = new Vector3( Input.Forward, Input.Left, 0 );
        var inSpeed = WishVelocity.Length.Clamp( 0, 1 );
        WishVelocity *= Input.Rotation.Angles().WithPitch( 0 ).ToRotation();

        if ( !Swimming && !IsTouchingLadder )
        {
            WishVelocity = WishVelocity.WithZ( 0 );
        }

        WishVelocity = WishVelocity.Normal * inSpeed;
        WishVelocity *= GetWishSpeed();

        Duck.PreTick();

        var bStayOnGround = false;
        if ( Swimming )
        {
            ApplyFriction( 1 );
            WaterMove();
        }
        else if ( IsTouchingLadder )
        {
            LadderMove();
        }
        else if ( GroundEntity != null )
        {
            bStayOnGround = true;
            WalkMove();
        }
        else
        {
            AirMove();
        }

        CategorizePosition( bStayOnGround );

        // FinishGravity
        if ( !Swimming && !IsTouchingLadder )
        {
            Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
        }


        if ( GroundEntity != null )
        {
            Velocity = Velocity.WithZ( 0 );
        }

        // CheckFalling(); // fall damage etc

        // Land Sound
        // Swim Sounds

        SaveGroundPos();

        if ( !Debug )
	        return;

	    DebugOverlay.Box( Position + TraceOffset, mins, maxs, Color.Red );
        DebugOverlay.Box( Position, mins, maxs, Color.Blue );

        var lineOffset = 0;
        if ( Host.IsServer ) lineOffset = 10;

        DebugOverlay.ScreenText( lineOffset + 0, $"        Position: {Position}" );
        DebugOverlay.ScreenText( lineOffset + 1, $"        Velocity: {Velocity}" );
        DebugOverlay.ScreenText( lineOffset + 2, $"    BaseVelocity: {BaseVelocity}" );
        DebugOverlay.ScreenText( lineOffset + 3, $"    GroundEntity: {GroundEntity} [{GroundEntity?.Velocity}]" );
        DebugOverlay.ScreenText( lineOffset + 4, $" SurfaceFriction: {SurfaceFriction}" );
        DebugOverlay.ScreenText( lineOffset + 5, $"    WishVelocity: {WishVelocity}" );
    }

    public virtual float GetWishSpeed()
    {
        var ws = Duck.GetWishSpeed();
        if ( ws >= 0 ) return ws;

        if ( Input.Down( InputButton.Run ) ) return SprintSpeed;
        return Input.Down( InputButton.Walk ) ? WalkSpeed : DefaultSpeed;
    }

    public virtual void WalkMove()
    {
        var wishdir = WishVelocity.Normal;
        var wishspeed = WishVelocity.Length;

        WishVelocity = WishVelocity.WithZ( 0 );
        WishVelocity = WishVelocity.Normal * wishspeed;

        Velocity = Velocity.WithZ( 0 );
        Accelerate( wishdir, wishspeed, 0, Acceleration );
        Velocity = Velocity.WithZ( 0 );

        // Add in any base velocity to the current velocity.
        Velocity += BaseVelocity;

        try
        {
            if ( Velocity.Length < 1.0f )
            {
                Velocity = Vector3.Zero;
                return;
            }

            // first try just moving to the destination
            var dest = (Position + Velocity * Time.Delta).WithZ( Position.z );

            var pm = TraceBBox( Position, dest );

            if ( pm.Fraction == 1 )
            {
                Position = pm.EndPosition;
                StayOnGround();
                return;
            }

            StepMove();
        }
        finally
        {

            // Now pull the base velocity back out.   Base velocity is set if you are on a moving object, like a conveyor (or maybe another monster?)
            Velocity -= BaseVelocity;
        }

        StayOnGround();
    }

    public virtual void StepMove()
    {
        MoveHelper mover = new MoveHelper( Position, Velocity );
        mover.Trace = mover.Trace.Size( mins, maxs ).Ignore( Pawn );
        mover.MaxStandableAngle = GroundAngle;

        mover.TryMoveWithStep( Time.Delta, StepSize );

        Position = mover.Position;
        Velocity = mover.Velocity;
    }

    public virtual void Move()
    {
        var mover = new MoveHelper( Position, Velocity );
        mover.Trace = mover.Trace.Size( mins, maxs ).Ignore( Pawn );
        mover.MaxStandableAngle = GroundAngle;

        mover.TryMove( Time.Delta );

        Position = mover.Position;
        Velocity = mover.Velocity;
    }

    /// <summary>
    /// Add our wish direction and speed onto our velocity
    /// </summary>
    public virtual void Accelerate( Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
    {
	    if ( speedLimit > 0 && wishspeed > speedLimit )
            wishspeed = speedLimit;

        // See if we are changing direction a bit
        var currentspeed = Velocity.Dot( wishdir );

        // Reduce wishspeed by the amount of veer.
        var addspeed = wishspeed - currentspeed;

        // If not going to add any speed, done.
        if ( addspeed <= 0 )
            return;

        // Determine amount of acceleration.
        var accelspeed = acceleration * Time.Delta * wishspeed * SurfaceFriction;

        // Cap at addspeed
        if ( accelspeed > addspeed )
            accelspeed = addspeed;

        Velocity += wishdir * accelspeed;
    }

    /// <summary>
    /// Remove ground friction from velocity
    /// </summary>
    public virtual void ApplyFriction( float frictionAmount = 1.0f )
    {
        // If we are in water jump cycle, don't apply friction
        //if ( player->m_flWaterJumpTime )
        //   return;

        // Not on ground - no friction

        // Calculate speed
        var speed = Velocity.Length;
        if ( speed < 0.1f ) return;

        // Bleed off some speed, but if we have less than the bleed
        //  threshold, bleed the threshold amount.
        var control = (speed < StopSpeed) ? StopSpeed : speed;

        // Add the amount to the drop amount.
        var drop = control * Time.Delta * frictionAmount;

        // scale the velocity
        var newspeed = speed - drop;
        if ( newspeed < 0 ) newspeed = 0;

        if ( newspeed == speed )
	        return;

	    newspeed /= speed;
        Velocity *= newspeed;
    }

    public virtual void CheckJumpButton()
    {
        // If we are in the water most of the way...
        if ( Swimming )
        {
            // swimming, not jumping
            ClearGroundEntity();

            Velocity = Velocity.WithZ( 100 );

            // play swimming sound
            //  if ( player->m_flSwimSoundTime <= 0 )
            {
                // Don't play sound again for 1 second
                //   player->m_flSwimSoundTime = 1000;
                //   PlaySwimSound();
            }

            return;
        }

        if ( GroundEntity == null && maxAirJumpsValue < 0 )
		{
			return;
		}

        if(GroundEntity != null && maxAirJumpsValue < 0)
        {
            maxAirJumpsValue = MaxAirJumps;
        }

        if(GroundEntity == null && maxAirJumpsValue >= 0)
        {
            Sound.FromWorld("quick-fart", Position);
            Particles.Create( "particles/fart.vpcf", Position + (Vector3.Up * 50));
        }

		maxAirJumpsValue--;

        ClearGroundEntity();

        var flGroundFactor = 1.0f;

        var flMul = 268.3281572999747f * 1.2f;

        if ( Duck.IsActive )
            flMul *= 0.8f;

        Velocity = Velocity.WithZ( flMul * flGroundFactor );

        Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

        AddEvent( "jump" );
    }

    public virtual void AirMove()
    {
        var wishdir = WishVelocity.Normal;
        var wishspeed = WishVelocity.Length;

        Accelerate( wishdir, wishspeed, AirControl, AirAcceleration );

        Velocity += BaseVelocity;

        Move();

        Velocity -= BaseVelocity;
    }

    public virtual void WaterMove()
    {
        var wishdir = WishVelocity.Normal;
        var wishspeed = WishVelocity.Length;

        wishspeed *= 0.8f;

        Accelerate( wishdir, wishspeed, 100, Acceleration );

        Velocity += BaseVelocity;

        Move();

        Velocity -= BaseVelocity;
    }

    bool IsTouchingLadder = false;
    Vector3 LadderNormal;

    public virtual void CheckLadder()
    {
        var wishvel = new Vector3( Input.Forward, Input.Left, 0 );
        wishvel *= Input.Rotation.Angles().WithPitch( 0 ).ToRotation();
        wishvel = wishvel.Normal;

        if ( IsTouchingLadder )
        {
            if ( Input.Pressed( InputButton.Jump ) )
            {
                Velocity = LadderNormal * 100.0f;
                IsTouchingLadder = false;

                return;

            }
            else if ( GroundEntity != null && LadderNormal.Dot( wishvel ) > 0 )
            {
                IsTouchingLadder = false;

                return;
            }
        }

        const float ladderDistance = 1.0f;
        var start = Position;
        var end = start + (IsTouchingLadder ? (LadderNormal * -1.0f) : wishvel) * ladderDistance;

        var pm = Trace.Ray( start, end )
                    .Size( mins, maxs )
                    .HitLayer( CollisionLayer.All, false )
                    .HitLayer( CollisionLayer.LADDER, true )
                    .Ignore( Pawn )
                    .Run();

        IsTouchingLadder = false;

        if ( !pm.Hit )
	        return;

	    IsTouchingLadder = true;
        LadderNormal = pm.Normal;
    }

    public virtual void LadderMove()
    {
        var velocity = WishVelocity;
        var normalDot = velocity.Dot( LadderNormal );
        var cross = LadderNormal * normalDot;
        Velocity = (velocity - cross) + (-normalDot * LadderNormal.Cross( Vector3.Up.Cross( LadderNormal ).Normal ));

        Move();
    }


    public virtual void CategorizePosition( bool bStayOnGround )
    {
        SurfaceFriction = 1.0f;

        // Doing this before we move may introduce a potential latency in water detection, but
        // doing it after can get us stuck on the bottom in water if the amount we move up
        // is less than the 1 pixel 'threshold' we're about to snap to.	Also, we'll call
        // this several times per frame, so we really need to avoid sticking to the bottom of
        // water on each call, and the converse case will correct itself if called twice.
        //CheckWater();

        var point = Position - Vector3.Up * 2;
        var vBumpOrigin = Position;
        
        //  Shooting up really fast.  Definitely not on ground trimed until ladder shit
        var bMovingUpRapidly = Velocity.z > MaxNonJumpVelocity;
        var bMovingUp = Velocity.z > 0;

        var bMoveToEndPos = false;

        if ( GroundEntity != null ) // and not underwater
        {
            bMoveToEndPos = true;
            point.z -= StepSize;
        }
        else if ( bStayOnGround )
        {
            bMoveToEndPos = true;
            point.z -= StepSize;
        }

        if ( bMovingUpRapidly || Swimming ) // or ladder and moving up
        {
            ClearGroundEntity();
            return;
        }

        var pm = TraceBBox( vBumpOrigin, point, 4.0f );

        if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
        {
            ClearGroundEntity();
            bMoveToEndPos = false;

            if ( Velocity.z > 0 )
                SurfaceFriction = 0.25f;
        }
        else
        {
            UpdateGroundEntity( pm );
        }

        if ( bMoveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
        {
            Position = pm.EndPosition;
        }

    }

    /// <summary>
    /// We have a new ground entity
    /// </summary>
    public virtual void UpdateGroundEntity( TraceResult tr )
    {
        GroundNormal = tr.Normal;

        // VALVE HACKHACK: Scale this to fudge the relationship between vphysics friction values and player friction values.
        // A value of 0.8f feels pretty normal for vphysics, whereas 1.0f is normal for players.
        // This scaling trivially makes them equivalent.  REVISIT if this affects low friction surfaces too much.
        SurfaceFriction = tr.Surface.Friction * 1.25f;
        if ( SurfaceFriction > 1 ) SurfaceFriction = 1;

        //if ( tr.Entity == GroundEntity ) return;

        Vector3 oldGroundVelocity = default;
        if ( GroundEntity != null ) oldGroundVelocity = GroundEntity.Velocity;

        var wasOffGround = GroundEntity == null;

        GroundEntity = tr.Entity;

        if ( GroundEntity != null )
        {
            BaseVelocity = GroundEntity.Velocity;
        }

        /*
            m_vecGroundUp = pm.m_vHitNormal;
            player->m_surfaceProps = pm.m_pSurfaceProperties->GetNameHash();
            player->m_pSurfaceData = pm.m_pSurfaceProperties;
            const CPhysSurfaceProperties *pProp = pm.m_pSurfaceProperties;

            const CGameSurfaceProperties *pGameProps = g_pPhysicsQuery->GetGameSurfaceproperties( pProp );
            player->m_chTextureType = (int8)pGameProps->m_nLegacyGameMaterial;
        */
    }

    /// <summary>
    /// We're no longer on the ground, remove it
    /// </summary>
    public virtual void ClearGroundEntity()
    {
        if ( GroundEntity == null ) return;

        GroundEntity = null;
        GroundNormal = Vector3.Up;
        SurfaceFriction = 1.0f;
    }

    /// <summary>
    /// Traces the current bbox and returns the result.
    /// liftFeet will move the start position up by this amount, while keeping the top of the bbox at the same
    /// position. This is good when tracing down because you won't be tracing through the ceiling above.
    /// </summary>
    public override TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
    {
        return TraceBBox( start, end, mins, maxs, liftFeet );
    }

    /// <summary>
    /// Try to keep a walking player on the ground when running down slopes etc
    /// </summary>
    public virtual void StayOnGround()
    {
        var start = Position + Vector3.Up * 2;
        var end = Position + Vector3.Down * StepSize;

        // See how far up we can go without getting stuck
        var trace = TraceBBox( Position, start );
        start = trace.EndPosition;

        // Now trace down from a known safe position
        trace = TraceBBox( start, end );

        if ( trace.Fraction is <= 0 or >= 0 ) return;
        if ( trace.StartedSolid ) return;
        if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ) return;

        // This is incredibly hacky. The real problem is that trace returning that strange value we can't network over.
        // float flDelta = fabs( mv->GetAbsOrigin().z - trace.m_vEndPos.z );
        // if ( flDelta > 0.5f * DIST_EPSILON )

        Position = trace.EndPosition;
    }

    void RestoreGroundPos()
    {
        if ( GroundEntity == null || GroundEntity.IsWorld )
            return;

        //var Position = GroundEntity.Transform.ToWorld( GroundTransform );
        //Pos = Position.Position;
    }

    void SaveGroundPos()
    {
        if ( GroundEntity == null || GroundEntity.IsWorld )
            return;

        //GroundTransform = GroundEntity.Transform.ToLocal( new Transform( Pos, Rot ) );
    }

}
