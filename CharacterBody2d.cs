using Godot;
using System;

public partial class CharacterBody2D : Godot.CharacterBody2D
{
	// Public Constants
	public const float ACCELERATION = 3000f;
	public const float MAX_SPEED = 18000f;
	public const float LIMIT_SPEED_Y = 1200f;
	public const float JUMP_FORCE = 36000f;
	public const float MIN_JUMP_FORCE = 12000f;
	public const int MAX_COYOTE_TIME = 6;
	public const int JUMP_BUFFER_TIME = 10;
	public const float WALL_JUMP_FORCE = 18000f;
	public const float GRAVITY = 2100f;
	public const float DASH_SPEED = 36000f;
	public const float WALL_SLIDE_FACTOR = 0.8f;

	// Variables
	private Vector2 velocity = Vector2.Zero;
	private Vector2 axis = Vector2.Zero;

	private int coyoteTimer = 0;
	private int jumpBufferTimer = 0;
	private int wallJumpTimer = 0;
	private int dashTimer = 0;

	private bool canJump = false;
	private bool wallSliding = false;
	private bool isGrabbing = false;
	private bool isDashing = false;
	private bool hasDashed = false;

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		ApplyGravity(dt);
		GetInputAxis();
		HandleDash(dt);
		HandleWallSlide(dt);
		HandleHorizontalMovement(dt);
		HandleJumpBuffer(dt);

		if (Input.IsActionJustPressed("jump"))
		{
			if (canJump)
			{
				PerformJump();
			}
			else if (GetNode<RayCast2D>("RayCast2D").IsColliding())
			{
				PerformWallJump();
			}
			else
			{
				jumpBufferTimer = JUMP_BUFFER_TIME;
			}
		}

		AdjustJumpHeight();

		Velocity = velocity;
		MoveAndSlide();

		if (IsOnFloor())
			hasDashed = false;
	}

	private void ApplyGravity(float delta)
	{
		if (velocity.Y <= LIMIT_SPEED_Y && !isDashing)
			velocity.Y += GRAVITY * delta;
	}

	private void GetInputAxis()
	{
		axis.X = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
		axis.Y = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");
	}

	private void HandleHorizontalMovement(float delta)
	{
		if (wallJumpTimer > 0)
		{
			wallJumpTimer--;
			return;
		}

		if (axis.X != 0)
			velocity.X = Mathf.Clamp(velocity.X + axis.X * ACCELERATION * delta, -MAX_SPEED, MAX_SPEED);
		else
			velocity.X = Mathf.Lerp(velocity.X, 0, 0.4f);
	}

	private void PerformJump()
	{
		velocity.Y = -JUMP_FORCE;
	}

	private void PerformWallJump()
	{
		wallJumpTimer = JUMP_BUFFER_TIME;
		var rayCast = GetNode<RayCast2D>("RayCast2D");
		if (rayCast.IsColliding())
			velocity.X = -WALL_JUMP_FORCE * rayCast.GetCollisionNormal().X;

		velocity.Y = -JUMP_FORCE;
	}

	private void HandleJumpBuffer(float delta)
	{
		if (jumpBufferTimer > 0)
		{
			if (IsOnFloor())
				PerformJump();

			jumpBufferTimer--;
		}

		if (IsOnFloor())
			canJump = true;
		else
		{
			coyoteTimer++;
			if (coyoteTimer > MAX_COYOTE_TIME)
				canJump = false;
		}
	}

	private void HandleWallSlide(float delta)
	{
		if (!canJump && GetNode<RayCast2D>("RayCast2D").IsColliding())
		{
			wallSliding = true;
			velocity.Y *= WALL_SLIDE_FACTOR;
		}
		else
		{
			wallSliding = false;
		}
	}

	private void HandleDash(float delta)
	{
		if (!hasDashed && Input.IsActionJustPressed("dash"))
		{
			velocity = axis * DASH_SPEED;
			isDashing = true;
			hasDashed = true;
		}

		if (isDashing)
		{
			dashTimer++;
			if (dashTimer >= (int)(0.25 / delta))
			{
				isDashing = false;
				dashTimer = 0;
			}
		}
	}

	private void AdjustJumpHeight()
	{
		if (Input.IsActionJustReleased("ui_up") && velocity.Y < -MIN_JUMP_FORCE)
			velocity.Y = -MIN_JUMP_FORCE;
} }
	
