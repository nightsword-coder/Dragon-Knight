using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // 单例模式
    public static AudioManager Instance;
    
    // 场景BGM配置
    [System.Serializable]
    public class SceneBGMConfig
    {
        public string sceneName;      // 场景名称（可使用*通配符）
        public string bgmName;        // BGM唯一标识（建议使用名称，而非直接引用clip）
        public AudioClip bgmClip;     // 对应的BGM
        public bool forceKeepBGM;     // 强制保持BGM
    }
    
    public List<SceneBGMConfig> sceneConfigs = new List<SceneBGMConfig>();
    
    // 音频源组件
    private AudioSource bgmSource;
    private string currentSceneName = "";
    private string currentBGMName = "";
    private float currentBGMTime = 0f;  // 记录BGM播放进度
    private bool isBGMPlaying = false;  // BGM播放状态
    private bool isBGMKept = false;     // 是否保持BGM

    void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 创建音频源
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        
        // 监听场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        Debug.Log($"AudioManager初始化完成，已配置{sceneConfigs.Count}个场景BGM");
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string newSceneName = scene.name;
        HandleSceneBGM(newSceneName);
    }
    
    /// <summary>
    /// 处理场景BGM播放（保持播放进度）
    /// </summary>
    private void HandleSceneBGM(string sceneName)
    {
        // 查找匹配的BGM配置
        SceneBGMConfig matchedConfig = FindMatchingConfig(sceneName);
        
        // 场景有BGM配置
        if (matchedConfig != null)
        {
            // 强制保持BGM或BGM名称相同
            if (matchedConfig.forceKeepBGM || matchedConfig.bgmName == currentBGMName)
            {
                Debug.Log($"场景 {sceneName} 保持BGM: {matchedConfig.bgmName}");
                isBGMKept = true;
                ResumeBGM(); // 恢复播放
                return;
            }
            
            // 新BGM，记录当前进度并切换
            if (isBGMPlaying && !isBGMKept)
            {
                currentBGMTime = bgmSource.time;
            }
            
            PlayBGM(matchedConfig.bgmClip, matchedConfig.bgmName, 0.5f);
            currentSceneName = sceneName;
            currentBGMName = matchedConfig.bgmName;
            isBGMKept = false;
            Debug.Log($"场景 {sceneName} 播放新BGM: {matchedConfig.bgmName}");
            return;
        }
        
        // 场景无BGM配置：保持上一场景BGM
        if (!string.IsNullOrEmpty(currentBGMName))
        {
            Debug.Log($"场景 {sceneName} 无BGM配置，保持上一场景BGM: {currentBGMName}");
            isBGMKept = true;
            ResumeBGM();
        }
        else
        {
            Debug.LogWarning($"场景 {sceneName} 无BGM配置，且上一场景也无BGM");
            StopBGM();
        }
    }
    
    /// <summary>
    /// 查找匹配的场景配置（支持*通配符）
    /// </summary>
    private SceneBGMConfig FindMatchingConfig(string sceneName)
    {
        // 精确匹配
        foreach (var config in sceneConfigs)
        {
            if (config.sceneName == sceneName)
                return config;
        }
        
        // 通配符匹配
        foreach (var config in sceneConfigs)
        {
            if (config.sceneName.EndsWith("*"))
            {
                string prefix = config.sceneName.Substring(0, config.sceneName.Length - 1);
                if (sceneName.StartsWith(prefix))
                    return config;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 播放BGM（带淡入效果）
    /// </summary>
    public void PlayBGM(AudioClip clip, string bgmName, float fadeTime = 0.5f)
    {
        if (clip == null)
        {
            Debug.LogError("BGM clip为空，无法播放");
            StopBGM();
            return;
        }
        
        StartCoroutine(FadeBGM(clip, fadeTime));
        currentBGMName = bgmName;
        isBGMPlaying = true;
    }
    
    private IEnumerator FadeBGM(AudioClip newClip, float fadeTime)
    {
        // 淡出现有BGM
        if (fadeTime > 0f && bgmSource.isPlaying && !isBGMKept)
        {
            float startVolume = bgmSource.volume;
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                bgmSource.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
                yield return null;
            }
            bgmSource.Stop();
        }
        
        // 播放新BGM
        bgmSource.clip = newClip;
        bgmSource.volume = 1f;
        bgmSource.Play();
    }
    
    /// <summary>
    /// 暂停BGM（记录播放进度）
    /// </summary>
    public void PauseBGM()
    {
        if (bgmSource.isPlaying)
        {
            currentBGMTime = bgmSource.time;
            bgmSource.Pause();
            isBGMPlaying = false;
        }
    }
    
    /// <summary>
    /// 恢复BGM播放
    /// </summary>
    public void ResumeBGM()
    {
        if (!bgmSource.isPlaying && bgmSource.clip != null)
        {
            bgmSource.time = currentBGMTime;
            bgmSource.Play();
            isBGMPlaying = true;
        }
    }
    
    /// <summary>
    /// 停止BGM
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
        currentBGMName = "";
        currentBGMTime = 0f;
        isBGMPlaying = false;
        isBGMKept = false;
    }
    
    /// <summary>
    /// 设置BGM音量
    /// </summary>
    public void SetVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume);
    }
    
    /// <summary>
    /// 手动设置场景BGM（用于动态配置）
    /// </summary>
    public void SetSceneBGM(string sceneName, string bgmName, AudioClip clip, bool forceKeepBGM = false)
    {
        // 查找现有配置
        for (int i = 0; i < sceneConfigs.Count; i++)
        {
            if (sceneConfigs[i].sceneName == sceneName)
            {
                sceneConfigs[i].bgmName = bgmName;
                sceneConfigs[i].bgmClip = clip;
                sceneConfigs[i].forceKeepBGM = forceKeepBGM;
                return;
            }
        }
        
        // 添加新配置
        SceneBGMConfig newConfig = new SceneBGMConfig
        {
            sceneName = sceneName,
            bgmName = bgmName,
            bgmClip = clip,
            forceKeepBGM = forceKeepBGM
        };
        sceneConfigs.Add(newConfig);
    }
}