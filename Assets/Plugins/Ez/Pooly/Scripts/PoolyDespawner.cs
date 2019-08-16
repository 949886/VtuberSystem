// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Ez.Pooly
{
    /// <summary>
    /// Configure a timed despawner
    /// </summary>
    public class PoolyDespawner : MonoBehaviour
    {
        /// <summary>
        /// Despawner type
        /// </summary>
        public enum DespawnAfter
        {
            /// <summary>
            /// Despawn After Time - this despawner will automatically despawn the prefab after a given duration.
            /// </summary>
            Time = 0,
            /// <summary>
            /// Despawn After Sound Played - this despawner will automatically despawn the prefab after the attached AudioSource played it's AudioClip.
            /// </summary>
            SoundPlayed = 1,
            /// <summary>
            /// Despawn After Effect Played - this despawner will automatically despawn the prefab after the attached PartycleSystem played.
            /// You can also configure the duration settings.
            /// </summary>
            EffectPlayed = 2,
            /// <summary>
            /// Despawn After Collision - this despawner will automatically despawn the prefab after any of the selected OnCollision events happen. You can also configure the duration settings.
            /// <para>The OnCollision event types are OnCollisionEnter, OnCollisionStay and OnCollisionExit</para>
            /// </summary>
            Collision = 3,
            /// <summary>
            /// Despawn After Trigger - this despawner will automatically despawn the prefab after any of the selected OnTrigger events happen. You can also configure the duration settings.
            /// <para>The OnTrigger events types are OnTriggerEnter, OnTriggerStay and OnTriggerExit</para>
            /// </summary>
            Trigger = 4,
            /// <summary>
            /// Despawn After Collision2D - this despawner will automatically despawn the prefab after any of the selected OnCollision2D events happen. You can also configure the duration settings.
            /// <para>The OnCollision2D event types are OnCollisionEnter2D, OnCollisionStay2D and OnCollisionExit2D</para>
            /// </summary>
            Collision2D = 5,
            /// <summary>
            /// Despawn After Trigger2D - this despawner will automatically despawn the prefab after any of the selected OnTrigger2D events happen. You can also configure the duration settings.
            /// <para>The OnTrigger2D events types are OnTriggerEnter2D, OnTriggerStay2D and OnTriggerExit2D</para>
            /// </summary>
            Trigger2D = 6
        }

        /// <summary>
        /// Set the despawner type. This changes adjusts the despawner functionality as needed
        /// </summary>
        public DespawnAfter despawnAfter = DespawnAfter.Time;

        /// <summary>
        /// When should the timer automatically start
        /// </summary>
        public enum AutoStart
        {
            /// <summary>
            /// The timer will automatically start in the OnSpawned method
            /// </summary>
            OnSpawned = 0,
            /// <summary>
            /// The timer not start automatically. You are responsible to call the StartTimer method yourself when needed.
            /// </summary>
            Never = 1
        }
        /// <summary>
        /// Set the timer behaviour. Should it automatically start on Awake, OnSpawned or Never.
        /// </summary>
        public AutoStart autoStart = AutoStart.OnSpawned;

        /// <summary>
        /// How long after the timer has started, should the transform (this script is attached to) call Pooly.Despawn
        /// </summary>
        public float duration = 5f;
        ///// <summary>
        ///// Set true if you want to despawn after a set random duration interval, min (inclusive) - max (inclusive), instead after a fixed duration.
        ///// </summary>
        //public bool randomDuration = false;
        ///// <summary>
        ///// Minimum duration interval used to determine the random duration despawn time.
        ///// </summary>
        //public float randomDuraionMinimum = 2f;
        ///// <summary>
        ///// Maximum duration interval used to determine the random duration despawn time.
        ///// </summary>
        //public float randomDurationMaximum = 5f;

        /// <summary>
        /// Activate a timer for the Despawn After Collision / Trigger / Collision2D / Trigger2D options, that initiates a despawn even if the collision/trigger/collision2D/trigger2D didn't happen.
        /// This uses the duration as the despawn time.
        /// </summary>
        public bool orDespawnAfterTime = true;
        /// <summary>
        /// Used by all the Despawn After Collision / Trigger / Collision2D / Trigger2D options, to allow only a certain tag to initiate a despawn.
        /// </summary>
        public bool onlyWithTag = false;
        /// <summary>
        /// The tag that, if this gameObject Collides/Triggers/Collides2D/Triggers2D with, will get despawned.
        /// <para>Note: onlyWithTag needs to be true and you should be in on of the following modes: Despawn After Collision / Trigger / Collision2D / Trigger2D.</para>
        /// </summary>
        public string targetTag = "Untagged";

        public bool despawnOnCollisionEnter = false, despawnOnCollisionStay = false, despawnOnCollisionExit = false;
        public bool despawnOnTriggerEnter = false, despawnOnTriggerStay = false, despawnOnTriggerExit = false;
        public bool despawnOnCollisionEnter2D = false, despawnOnCollisionStay2D = false, despawnOnCollisionExit2D = false;
        public bool despawnOnTriggerEnter2D = false, despawnOnTriggerStay2D = false, despawnOnTriggerExit2D = false;

        private AudioSource _aSource;
        /// <summary>
        /// Returns the AudioSource attached to this gameObject or one of it's children
        /// </summary>
        public AudioSource aSource { get { if(_aSource == null) { _aSource = GetComponentInChildren<AudioSource>(false); } return _aSource; } }

        private ParticleSystem _pSystem;
        /// <summary>
        /// Retruns the ParticleSystem attached to this gameObject or one of it's children
        /// </summary>
        public ParticleSystem pSystem { get { if(_pSystem == null) { _pSystem = GetComponentInChildren<ParticleSystem>(false); } return _pSystem; } }

        /// <summary>
        /// Should the Total Duration take into account the ParticleSystem Duration
        /// </summary>
        public bool useParticleSystemDuration = true;
        /// <summary>
        /// Should the Total Duration take into account the ParticleSystem Start Delay
        /// </summary>
        public bool useParticleSystemStartDelay = true;
        /// <summary>
        /// Should the Total Duration take into account the ParticleSystem Start Lifetime
        /// </summary>
        public bool useParticleSystemStartLifetime = true;
        /// <summary>
        /// You can adjust the Total Duration by using this setting (used for Despawn After Effect Played)
        /// </summary>
        public float extraTime = 0;
        /// <summary>
        /// Should the AudioSource or the ParticleSystem play when the prefab has been spawned (default: true)
        /// </summary>
        public bool playOnSpawn = true;

        /// <summary>
        /// UnityEvent that gets triggered right before this gameObject is despawned.
        /// </summary>
        public UnityEvent OnDespawn = new UnityEvent();
        /// <summary>
        /// Reference to the despawner's Coroutine.
        /// </summary>
        private Coroutine cStartTimer;
        /// <summary>
        /// WaitForSeconds used before despawning to avoid GC.
        /// </summary>
        private WaitForSeconds despawnWaitForSeconds = null;
        /// <summary>
        /// Current duration for the despawnWaitForSeconds. Used to generate a new despawnWaitForSeconds if the despawn delay changes.
        /// </summary>
        private float despawnWFSCurrentDuration = 0f;
        /// <summary>
        /// WaitForSecondsRealtime used before despawning to avoid GC.
        /// </summary>
        private WaitForSecondsRealtime despawnWaitForSecondsRealTime = null;
        /// <summary>
        /// Current duration for the despawnWaitForSecondsRealTime. Used to generate a new despawnWaitForSecondsRealTime if the despawn RT delay changes.
        /// </summary>
        private float despawnWFSRealTimeCurrentDuration = 0f;

        /// <summary>
        /// Set if the timer should use RealTime (that cannot be paused) instead of GameTime (that can be paused).
        /// </summary>
        public bool useRealTime = false;

        void Awake()
        {
            switch(despawnAfter)
            {
                case DespawnAfter.Time: break;
                case DespawnAfter.SoundPlayed: StopSoundOnAwake(); break;
                case DespawnAfter.EffectPlayed: StopEffectOnAwake(); break;
                case DespawnAfter.Collision:
                    if(despawnOnCollisionEnter) gameObject.AddComponent<OnCollisionEnterListener>().despawner = this;
                    if(despawnOnCollisionStay) gameObject.AddComponent<OnCollisionStayListener>().despawner = this;
                    if(despawnOnCollisionExit) gameObject.AddComponent<OnCollisionExitListener>().despawner = this;
                    break;
                case DespawnAfter.Trigger:
                    if(despawnOnTriggerEnter) gameObject.AddComponent<OnTriggerEnterListener>().despawner = this;
                    if(despawnOnTriggerStay) gameObject.AddComponent<OnTriggerStayListener>().despawner = this;
                    if(despawnOnTriggerExit) gameObject.AddComponent<OnTriggerExitListener>().despawner = this;
                    break;
                case DespawnAfter.Collision2D:
                    if(despawnOnCollisionEnter2D) gameObject.AddComponent<OnCollisionEnter2DListener>().despawner = this;
                    if(despawnOnCollisionStay2D) gameObject.AddComponent<OnCollisionStay2DListener>().despawner = this;
                    if(despawnOnCollisionExit2D) gameObject.AddComponent<OnCollisionExit2DListener>().despawner = this;
                    break;
                case DespawnAfter.Trigger2D:
                    if(despawnOnTriggerEnter2D) gameObject.AddComponent<OnTriggerEnter2DListener>().despawner = this;
                    if(despawnOnTriggerStay2D) gameObject.AddComponent<OnTriggerStay2DListener>().despawner = this;
                    if(despawnOnTriggerExit2D) gameObject.AddComponent<OnTriggerExit2DListener>().despawner = this;
                    break;
            }
        }

        void OnSpawned()
        {
            switch(despawnAfter)
            {
                case DespawnAfter.Time: if(autoStart == AutoStart.OnSpawned) { StartTimer(duration); } break;
                case DespawnAfter.SoundPlayed: if(playOnSpawn) { PlaySound(); } break;
                case DespawnAfter.EffectPlayed: if(playOnSpawn) { PlayEffect(); } break;
                case DespawnAfter.Collision: if(orDespawnAfterTime) { StartTimer(duration); } break;
                case DespawnAfter.Trigger: if(orDespawnAfterTime) { StartTimer(duration); } break;
                case DespawnAfter.Collision2D: if(orDespawnAfterTime) { StartTimer(duration); } break;
                case DespawnAfter.Trigger2D: if(orDespawnAfterTime) { StartTimer(duration); } break;
            }
        }

        public void ExecuteOnCollisionEnter(Collision collision)
        {
            if(despawnAfter != DespawnAfter.Collision || !despawnOnCollisionEnter) { return; }
            if(onlyWithTag) { if(!collision.gameObject.tag.Equals(targetTag)) { return; } }
            StartTimer();
        }

        public void ExecuteCollisionStay(Collision collision)
        {
            if(despawnAfter != DespawnAfter.Collision || !despawnOnCollisionStay) { return; }
            if(onlyWithTag) { if(!collision.gameObject.tag.Equals(targetTag)) { return; } }
            StartTimer();
        }

        public void ExecuteCollisionExit(Collision collision)
        {
            if(despawnAfter != DespawnAfter.Collision || !despawnOnCollisionExit) { return; }
            if(onlyWithTag) { if(!collision.gameObject.tag.Equals(targetTag)) { return; } }
            StartTimer();
        }

        public void ExecuteTriggerEnter(Collider other)
        {
            if(despawnAfter != DespawnAfter.Trigger || !despawnOnTriggerEnter) { return; }
            if(onlyWithTag) { if(!other.tag.Equals(targetTag)) { return; } }
            StartTimer();
        }

        public void ExecuteTriggerStay(Collider other)
        {
            if(despawnAfter != DespawnAfter.Trigger || !despawnOnTriggerStay) { return; }
            if(onlyWithTag) { if(!other.tag.Equals(targetTag)) { return; } }
            StartTimer();
        }

        public void ExecuteTriggerExit(Collider other)
        {
            if(despawnAfter != DespawnAfter.Trigger || !despawnOnTriggerExit) { return; }
            if(onlyWithTag) { if(!other.tag.Equals(targetTag)) { return; } }
            StartTimer();
        }

        public void ExecuteCollisionEnter2D(Collision2D collision)
        {
            if(despawnAfter != DespawnAfter.Collision2D || !despawnOnCollisionEnter2D) { return; }
            if(onlyWithTag) { if(!collision.gameObject.tag.Equals(targetTag)) { return; } }
            StartTimer();
        }

        public void ExecuteCollisionStay2D(Collision2D collision)
        {
            if(despawnAfter != DespawnAfter.Collision2D || !despawnOnCollisionStay2D) { return; }
            if(onlyWithTag) { if(!collision.gameObject.tag.Equals(targetTag)) { return; } }
            StartTimer();
        }

        public void ExecuteCollisionExit2D(Collision2D collision)
        {
            if(despawnAfter != DespawnAfter.Collision2D || !despawnOnCollisionExit2D) { return; }
            if(onlyWithTag) { if(!collision.gameObject.tag.Equals(targetTag)) { return; } }
            StartTimer();
        }

        public void ExecuteTriggerEnter2D(Collider2D collision)
        {
            if(despawnAfter != DespawnAfter.Trigger2D || !despawnOnTriggerEnter2D) { return; }
            if(onlyWithTag) { if(!collision.gameObject.tag.Equals(targetTag)) { return; } }
            StartTimer();
        }

        public void ExecuteTriggerStay2D(Collider2D collision)
        {
            if(despawnAfter != DespawnAfter.Trigger2D || !despawnOnTriggerStay2D) { return; }
            if(onlyWithTag) { if(!collision.gameObject.tag.Equals(targetTag)) { return; } }
            StartTimer();
        }

        public void ExecuteTriggerExit2D(Collider2D collision)
        {
            if(despawnAfter != DespawnAfter.Trigger2D || !despawnOnTriggerExit2D) { return; }
            if(onlyWithTag) { if(!collision.gameObject.tag.Equals(targetTag)) { return; } }
            StartTimer();
        }

        /// <summary>
        /// Plays the AudioClip referenced inside the AudioSource and executes the StartTimer (aka despawn despawn after time) taking into account the AudioClip length (duration)
        /// </summary>
        private void PlaySound()
        {
            if(aSource == null) { Debug.LogWarning("[Pooly] No AudioSource was found on the '" + gameObject.name + "' gameObject or its children. Despawning..."); StartTimer(); return; }
            if(aSource.clip == null) { Debug.LogWarning("[Pooly] The AudioSource found on the '" + gameObject.name + "' gameObject does not have an AudioClip referenced. Despawning..."); StartTimer(); return; }
            aSource.Play();
            StartTimer(aSource.clip.length);
        }
        /// <summary>
        /// Makes sure the sound is played only when it's spawned and not when the gameObject is Instantiated.
        /// </summary>
        private void StopSoundOnAwake()
        {
            if(aSource == null) { Debug.LogWarning("[Pooly] No AudioSource was found on the '" + gameObject.name + "' gameObject or its children."); return; }
            if(aSource.clip == null) { Debug.LogWarning("[Pooly] The AudioSource found on the '" + gameObject.name + "' gameObject does not have an AudioClip referenced."); return; }
            aSource.playOnAwake = false;
            if(aSource.isPlaying) { aSource.Stop(); }
        }

        /// <summary>
        /// Plays the ParticleSystem and executes the StartTimer (aka despawn after time) taking into account the Total Duration calcualted from the values set in the inspector
        /// </summary>
        private void PlayEffect()
        {
            if(pSystem == null) { Debug.LogWarning("[Pooly] No ParticleSystem was found on the '" + gameObject.name + "' gameObject or its children. Despawning..."); StartTimer(); return; }
            pSystem.Clear(true);
            pSystem.Play();
            StartTimer(pSystemTotalDuration);
        }
        /// <summary>
        /// Makes sure the partyle system is played only when it's spawned and not when the gameObject is Instantiated.
        /// </summary>
        private void StopEffectOnAwake()
        {
            if(pSystem == null) { Debug.LogWarning("[Pooly] No ParticleSystem was found on the '" + gameObject.name + "' gameObject or its children."); return; }

            var main = pSystem.main;
            main.playOnAwake = true;

            if(pSystem.isPlaying) { pSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); }
        }

        /// <summary>
        /// Returns the total duration by taking into account the settings from the inspector
        /// </summary>
        public float pSystemTotalDuration
        {
            get
            {
                if(pSystem == null) { return 0; }
                var main = pSystem.main;
                return (useParticleSystemDuration ? main.duration : 0)
                     + (useParticleSystemStartDelay ? main.startDelay.constant : 0)
                     + (useParticleSystemStartLifetime ? main.startLifetime.constant : 0)
                     + extraTime;
            }
        }

        /// <summary>
        /// Starts a despawn timer with a set duration.
        /// If the timerType was set to Start.Never you can manually call this method in order to start the timer.
        /// <para>If the duration is less than or equal to zero, it will trigger an instant despawn.</para>
        /// </summary>
        public void StartTimer(float duration = 0)
        {
            if(cStartTimer != null) { StopCoroutine(cStartTimer); cStartTimer = null; }
            if(duration <= 0) { OnDespawn.Invoke(); Pooly.Despawn(transform); return; }
            cStartTimer = StartCoroutine(iStartTimer(duration));
        }
        /// <summary>
        /// Starts a despawn timer with a random duration.
        /// If the timerType was set to Start.Never you can manually call this method in order to start the timer.
        /// <para>If durationMinimum and durationMaximum are less than or equal to zero, or if durationMinimum is greater than durationMaximum, it will trigger an instant despawn.</para>
        /// </summary>
        /// <param name="durationMinimum">Minimum duration time.</param>
        /// <param name="durationMaximum">Maximumt duration time.</param>
        public void StartTimer(float durationMinimum, float durationMaximum)
        {
            if(cStartTimer != null) { StopCoroutine(cStartTimer); cStartTimer = null; }
            if(durationMinimum < 0) { durationMinimum = 0; }
            if(durationMaximum < 0) { durationMaximum = 0; }
            if(durationMinimum == durationMaximum && durationMinimum == 0) { OnDespawn.Invoke(); Pooly.Despawn(transform); return; }
            if(durationMinimum > durationMaximum) { OnDespawn.Invoke(); Pooly.Despawn(transform); return; }
            cStartTimer = StartCoroutine(iStartTimer(Random.Range(durationMinimum, durationMaximum)));
        }

        /// <summary>
        /// Despawns this gameObject by returning it to Pooly's prefab pool.
        /// </summary>
        public void Despawn()
        {
            if(cStartTimer != null) { StopCoroutine(cStartTimer); cStartTimer = null; }
            OnDespawn.Invoke();
            Pooly.Despawn(transform);
        }

        private IEnumerator iStartTimer(float duration)
        {
            if(useRealTime)
            {
                if(despawnWaitForSecondsRealTime == null || despawnWFSRealTimeCurrentDuration != duration)
                { despawnWaitForSecondsRealTime = new WaitForSecondsRealtime(duration); despawnWFSRealTimeCurrentDuration = duration; }
                yield return despawnWaitForSecondsRealTime;
            }
            else
            {
                if(despawnWaitForSeconds == null || despawnWFSCurrentDuration != duration)
                { despawnWaitForSeconds = new WaitForSeconds(duration); despawnWFSCurrentDuration = duration; }
                yield return despawnWaitForSeconds;
            }
            OnDespawn.Invoke();
            cStartTimer = null;
            Pooly.Despawn(transform);
        }
    }
    #region Physics Listener Classes
    #region OnCollision
    [HideInInspector]
    public class OnCollisionEnterListener : MonoBehaviour
    {
        public PoolyDespawner despawner;
        private void OnCollisionEnter(Collision collision) { if(despawner != null) despawner.ExecuteOnCollisionEnter(collision); }
    }
    [HideInInspector]
    public class OnCollisionStayListener : MonoBehaviour
    {
        public PoolyDespawner despawner;
        private void OnCollisionStay(Collision collision) { { if(despawner != null) despawner.ExecuteCollisionStay(collision); } }
    }
    [HideInInspector]
    public class OnCollisionExitListener : MonoBehaviour
    {
        public PoolyDespawner despawner;
        private void OnCollisionExit(Collision collision) { if(despawner != null) despawner.ExecuteCollisionExit(collision); }
    }
    #endregion
    #region OnTrigger
    [HideInInspector]
    public class OnTriggerEnterListener : MonoBehaviour
    {
        public PoolyDespawner despawner;
        private void OnTriggerEnter(Collider other) { if(despawner != null) despawner.ExecuteTriggerEnter(other); }
    }
    [HideInInspector]
    public class OnTriggerStayListener : MonoBehaviour
    {
        public PoolyDespawner despawner;
        private void OnTriggerStay(Collider other) { if(despawner != null) despawner.ExecuteTriggerStay(other); }
    }
    [HideInInspector]
    public class OnTriggerExitListener : MonoBehaviour
    {
        public PoolyDespawner despawner;
        private void OnTriggerExit(Collider other) { if(despawner != null) despawner.ExecuteTriggerExit(other); }
    }
    #endregion
    #region OnCollision2D
    [HideInInspector]
    public class OnCollisionEnter2DListener : MonoBehaviour
    {
        public PoolyDespawner despawner;
        private void OnCollisionEnter2D(Collision2D collision) { if(despawner != null) despawner.ExecuteCollisionEnter2D(collision); }
    }
    [HideInInspector]
    public class OnCollisionStay2DListener : MonoBehaviour
    {
        public PoolyDespawner despawner;
        private void OnCollisionStay2D(Collision2D collision) { if(despawner != null) despawner.ExecuteCollisionStay2D(collision); }
    }
    [HideInInspector]
    public class OnCollisionExit2DListener : MonoBehaviour
    {
        public PoolyDespawner despawner;
        private void OnCollisionExit2D(Collision2D collision) { if(despawner != null) despawner.ExecuteCollisionExit2D(collision); }
    }
    #endregion
    #region OnTrigger2D
    [HideInInspector]
    public class OnTriggerEnter2DListener : MonoBehaviour
    {
        public PoolyDespawner despawner;
        private void OnTriggerEnter2D(Collider2D collision) { if(despawner != null) despawner.ExecuteTriggerEnter2D(collision); }
    }
    [HideInInspector]
    public class OnTriggerStay2DListener : MonoBehaviour
    {
        public PoolyDespawner despawner;
        private void OnTriggerStay2D(Collider2D collision) { if(despawner != null) despawner.ExecuteTriggerStay2D(collision); }
    }
    [HideInInspector]
    public class OnTriggerExit2DListener : MonoBehaviour
    {
        public PoolyDespawner despawner;
        private void OnTriggerExit2D(Collider2D collision) { if(despawner != null) despawner.ExecuteTriggerExit2D(collision); }
    }
    #endregion
    #endregion
}
