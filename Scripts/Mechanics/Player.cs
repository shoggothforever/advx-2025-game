using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SaveYourself.Core.Simulation;
using SaveYourself.Model;
using SaveYourself.Gameplay;
using SaveYourself.Core;
using SaveYourself.Utils;
namespace SaveYourself.Mechanics
{
    public class Player : basePlayer,TimeReverse.ITimeTrackable
    {
        //public AudioClip jumpAudio;
        //public AudioClip respawnAudio;
        //public AudioClip ouchAudio;
        public bool isReverse = false;
        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 3;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>  
        public float jumpTakeOffSpeed = 3;

        public JumpState jumpState = JumpState.Grounded;

        public Health health;
        private bool stopJump;
        /*internal new*/
        public Collider2D collider2d;
        /*internal new*/
        public AudioSource audioSource;
        // 控制角色的输入检测
        public bool controlEnabled = true;
        public Vector2 lastVelocity;
        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = GetModel<PlatformerModel>();
        static int nextId = common.reversableRoleInitialID;
        Animator anim;

        public int Id { get; private set; }
        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            Id = nextId++;
            anim = GetComponent<Animator>();
        }

        public TimeReverse.TimedAction RecordSnapshot()
        {
            return new TimeReverse.TimedAction
            {
                time = TimeManager.Instance.currentTime,
                objId = Id,
                type = TimeReverse.ActionType.Position,
                pos=transform.position,
                //payload = JsonUtility.ToJson(transform.position)
            };
        }

        public void ApplySnapshot(TimeReverse.TimedAction a)
        {
            //body.MovePosition(a.pos);
            transform.position = a.pos;
            body.velocity = Vector2.zero;
        }
        public bool DetectMove()
        {
            bool wasMoving = lastVelocity.sqrMagnitude > 0.02f;
            bool isMoving = body.velocity.sqrMagnitude > 0.02f;
            return wasMoving != isMoving;
        }
        public Bounds Bounds => collider2d.bounds;

        protected override void Update()
        {
            if (controlEnabled)
            {
                move.x = Input.GetAxis("Horizontal");
                if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
                    jumpState = JumpState.PrepareToJump;
                else if (Input.GetButtonUp("Jump"))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
            }
            else
            {
                move.x = 0;
            }
            if (TimeManager.Instance.phase == TimeManager.Phase.Reverse)
            {
                lastVelocity = body.velocity;
            }
            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
            {
                if (!isReverse) spriteRenderer.flipX = false;
                else spriteRenderer.flipX = true;
            }
            else if (move.x < -0.01f)
            {
                if (!isReverse) spriteRenderer.flipX = true;
                else spriteRenderer.flipX = false;
            }
            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }
        void OnCollisionStay2D(Collision2D c)
        {
            if (!c.collider.CompareTag("Box")) return;
            Rigidbody2D rbBox = c.collider.attachedRigidbody;
            Vector2 dir = c.contacts[0].normal * -1;   // 反向推
            rbBox.AddForce(dir * common.pushForce, ForceMode2D.Force);
        }
        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}