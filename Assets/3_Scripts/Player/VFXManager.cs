using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VFXManager : MonoBehaviour
{
    public enum VFXType
    {
        Heal,
        Burn,
        Block,
        Attack,
        Death,
        Stun,
        LevelUp,
        Poison,
        Shield,
        Movement
    }

    [System.Serializable]
    public class VFXData
    {
        public VFXType effectType;
        public GameObject vfxPrefab;
        public AudioClip soundEffect;
        public float duration = 2f;
        public Vector3 positionOffset = Vector3.zero;
        public bool followTarget = false;
        public float delayBetweenMultiple = 0.1f;
    }

    [Header("VFX Settings")]
    [SerializeField] private List<VFXData> vfxDatabase = new List<VFXData>();
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float maxSimultaneousSounds = 3f;
    
    public static VFXManager Instance { get; private set; }
    
    private Dictionary<VFXType, VFXData> vfxLookup = new Dictionary<VFXType, VFXData>();
    private int currentSoundCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeVFXLookup();
        }
        else
        {
            Destroy(gameObject);
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    private void InitializeVFXLookup()
    {
        vfxLookup.Clear();
        foreach (var vfxData in vfxDatabase)
        {
            if (!vfxLookup.ContainsKey(vfxData.effectType))
            {
                vfxLookup.Add(vfxData.effectType, vfxData);
            }
            else
            {
                Debug.LogWarning($"Duplicate VFX type found: {vfxData.effectType}");
            }
        }
    }

    #region Single Target VFX
    public void PlayVFX(VFXType effectType, Vector3 position)
    {
        PlayVFXAtPosition(effectType, position, null);
    }

    public void PlayVFX(VFXType effectType, Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning($"Cannot play VFX {effectType}: target is null");
            return;
        }
        
        PlayVFXAtPosition(effectType, target.position, target);
    }

    public void PlayVFX(VFXType effectType, GameObject target)
    {
        if (target == null)
        {
            Debug.LogWarning($"Cannot play VFX {effectType}: target GameObject is null");
            return;
        }
        
        PlayVFXAtPosition(effectType, target.transform.position, target.transform);
    }
    #endregion

    #region Multiple Target VFX
    public void PlayVFXAtMultiplePositions(VFXType effectType, Vector3[] positions)
    {
        if (positions == null || positions.Length == 0) return;
        
        StartCoroutine(PlayMultipleVFXCoroutine(effectType, positions, null));
    }

    public void PlayVFXAtMultipleTargets(VFXType effectType, Transform[] targets)
    {
        if (targets == null || targets.Length == 0) return;
        
        Vector3[] positions = new Vector3[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            positions[i] = targets[i] != null ? targets[i].position : Vector3.zero;
        }
        
        StartCoroutine(PlayMultipleVFXCoroutine(effectType, positions, targets));
    }

    public void PlayVFXAtMultipleTargets(VFXType effectType, GameObject[] targets)
    {
        if (targets == null || targets.Length == 0) return;
        
        Transform[] transforms = new Transform[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            transforms[i] = targets[i] != null ? targets[i].transform : null;
        }
        
        PlayVFXAtMultipleTargets(effectType, transforms);
    }

    public void PlayVFXAtMultipleTargets(VFXType effectType, List<EnemyUnit> enemies)
    {
        if (enemies == null || enemies.Count == 0) return;
        
        Vector3[] positions = new Vector3[enemies.Count];
        Transform[] transforms = new Transform[enemies.Count];
        
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] != null)
            {
                positions[i] = enemies[i].transform.position;
                transforms[i] = enemies[i].transform;
            }
        }
        
        StartCoroutine(PlayMultipleVFXCoroutine(effectType, positions, transforms));
    }
    #endregion

    #region Core VFX Logic
    private void PlayVFXAtPosition(VFXType effectType, Vector3 position, Transform target)
    {
        if (!vfxLookup.TryGetValue(effectType, out VFXData vfxData))
        {
            Debug.LogWarning($"VFX type {effectType} not found in database");
            return;
        }

        Vector3 spawnPosition = position + vfxData.positionOffset;
        
        if (vfxData.vfxPrefab != null)
        {
            GameObject vfxInstance = Instantiate(vfxData.vfxPrefab, spawnPosition, Quaternion.identity);
            
            if (vfxData.followTarget && target != null)
            {
                StartCoroutine(FollowTargetCoroutine(vfxInstance, target, vfxData.positionOffset, vfxData.duration));
            }
            else
            {
                Destroy(vfxInstance, vfxData.duration);
            }
        }

        PlaySoundEffect(vfxData.soundEffect);
    }

    private IEnumerator PlayMultipleVFXCoroutine(VFXType effectType, Vector3[] positions, Transform[] targets)
    {
        if (!vfxLookup.TryGetValue(effectType, out VFXData vfxData))
        {
            Debug.LogWarning($"VFX type {effectType} not found in database");
            yield break;
        }

        PlaySoundEffect(vfxData.soundEffect);

        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 spawnPosition = positions[i] + vfxData.positionOffset;
            Transform target = targets != null && i < targets.Length ? targets[i] : null;
            
            if (vfxData.vfxPrefab != null)
            {
                GameObject vfxInstance = Instantiate(vfxData.vfxPrefab, spawnPosition, Quaternion.identity);
                
                if (vfxData.followTarget && target != null)
                {
                    StartCoroutine(FollowTargetCoroutine(vfxInstance, target, vfxData.positionOffset, vfxData.duration));
                }
                else
                {
                    Destroy(vfxInstance, vfxData.duration);
                }
            }

            if (i < positions.Length - 1 && vfxData.delayBetweenMultiple > 0)
            {
                yield return new WaitForSeconds(vfxData.delayBetweenMultiple);
            }
        }
    }

    private IEnumerator FollowTargetCoroutine(GameObject vfxInstance, Transform target, Vector3 offset, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration && vfxInstance != null && target != null)
        {
            vfxInstance.transform.position = target.position + offset;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
        }
    }

    private void PlaySoundEffect(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        
        if (currentSoundCount < maxSimultaneousSounds)
        {
            audioSource.PlayOneShot(clip);
            StartCoroutine(DecrementSoundCountAfterClip(clip.length));
        }
    }

    private IEnumerator DecrementSoundCountAfterClip(float clipLength)
    {
        currentSoundCount++;
        yield return new WaitForSeconds(clipLength);
        currentSoundCount--;
    }
    #endregion

    #region Convenience Methods for Specific Effects
    public void PlayHealVFX(Vector3 position) => PlayVFX(VFXType.Heal, position);
    public void PlayHealVFX(Transform target) => PlayVFX(VFXType.Heal, target);
    
    public void PlayBurnVFX(Vector3 position) => PlayVFX(VFXType.Burn, position);
    public void PlayBurnVFX(List<EnemyUnit> enemies) => PlayVFXAtMultipleTargets(VFXType.Burn, enemies);
    
    public void PlayBlockVFX(Transform target) => PlayVFX(VFXType.Block, target);
    public void PlayAttackVFX(Vector3 position) => PlayVFX(VFXType.Attack, position);
    public void PlayDeathVFX(Transform target) => PlayVFX(VFXType.Death, target);
    
    public void PlayStunVFX(Vector3 position) => PlayVFX(VFXType.Stun, position);
    public void PlayStunVFX(Transform target) => PlayVFX(VFXType.Stun, target);
    public void PlayStunVFX(GameObject target) => PlayVFX(VFXType.Stun, target);
    #endregion

    #region Editor Helpers
    [ContextMenu("Validate VFX Database")]
    private void ValidateVFXDatabase()
    {
        InitializeVFXLookup();
        Debug.Log($"VFX Database initialized with {vfxLookup.Count} entries");
        
        foreach (var kvp in vfxLookup)
        {
            if (kvp.Value.vfxPrefab == null)
            {
                Debug.LogWarning($"VFX type {kvp.Key} is missing a prefab!");
            }
        }
    }
    #endregion
}