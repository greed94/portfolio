#define AVPRO_PACKAGE_UNITYUI
#if (UNITY_2019_2_OR_NEWER && AVPRO_PACKAGE_UNITYUI) || (!UNITY_2019_2_OR_NEWER)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RenderHeads.Media.AVProVideo;
using RenderHeads.Media.AVProVideo.Demos.UI;
using Metalive;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MediaPlayerUI : MonoBehaviour
{
    public MediaPlayer _mediaPlayer = null;


    [Header("[Options]")]
    [SerializeField] bool _useAudioFading = true;

    [Header("[Optional Components]")]
    [SerializeField] OverlayManager _overlayManager = null;
    [SerializeField] MediaPlayer _thumbnailMediaPlayer = null;
    [SerializeField] RectTransform _timelineTip = null;

    [Header("[UI Components]")]
    [SerializeField] RectTransform _canvasTransform = null;
    [SerializeField] GameObject controls;
    [SerializeField] GameObject overlays;
    [SerializeField] Slider _sliderTime = null;
    [SerializeField] Text _textTimeDuration = null;
    [SerializeField] Slider _sliderVolume = null;
    [SerializeField] Button _buttonPlayPause = null;
    [SerializeField] Button _buttonVolume = null;
    [SerializeField] Button _buttonOption = null;
    [SerializeField] HorizontalSegmentsPrimitive _segmentsSeek = null;
    [SerializeField] HorizontalSegmentsPrimitive _segmentsBuffered = null;
    [SerializeField] HorizontalSegmentsPrimitive _segmentsProgress = null;
    [SerializeField] Image[] fadeImages;


    private bool isPlaybar;
    private bool _wasPlayingBeforeTimelineDrag;
    private Material _playPauseMaterial;
    private Material _volumeMaterial;
    private float _audioVolume = 1f;

    private float _audioFade = 0f;
    private bool _isAudioFadingUpToPlay = true;
    [HideInInspector] public bool isOptionBar;
    private bool _isHoveringOverTimeline;
    private const float AudioFadeDuration = 0.25f;
    private float _audioFadeTime = 0f;

    private CancellationTokenSource optionbarCts;

    private readonly LazyShaderProperty _propMorph = new LazyShaderProperty("_Morph");
    private readonly LazyShaderProperty _propMute = new LazyShaderProperty("_Mute");
    private readonly LazyShaderProperty _propVolume = new LazyShaderProperty("_Volume");

    public virtual void Start()
    {
        isPlaybar = Setting.Video.option;

        if (fadeImages.Length != 0)
            FadeIn();

        if (!isPlaybar)
        {
            isOptionBar = false;
            controls.SetActive(false);
            overlays.SetActive(false);
            return;
        }

        if (_mediaPlayer)
        {
            _audioVolume = _mediaPlayer.AudioVolume;
        }
        isOptionBar = true;

        // 버튼,슬라이더 및 이벤트에 AddListener
        SetupOptionButton();
        SetupPlayPauseButton();
        SetupVolumeButton();
        CreateTimelineDragEvents();
        CreateVolumeSliderEvents();
        UpdateVolumeSlider();
    }

    // 360도 아이콘 페이드 인
    private async void FadeIn()
    {
        fadeImages[0].gameObject.SetActive(true);
        fadeImages[0].DOKill();
        fadeImages[1].gameObject.SetActive(true);
        fadeImages[1].DOKill();

        await UniTask.Delay(500);
        fadeImages[0].DOFade(0, 2).onComplete += () => fadeImages[0].gameObject.SetActive(false);
        fadeImages[1].DOFade(0, 2).onComplete += () => fadeImages[1].gameObject.SetActive(false);
    }

    // 5초 뒤 자동으로 재생 바가 사라지는 기능
    public async void AutoOffOptionBar()
    {
        isOptionBar = true;
        optionbarCts = new CancellationTokenSource();

        try
        {
            await UniTask.Delay(5000, false, PlayerLoopTiming.Update, optionbarCts.Token);
            _canvasTransform.gameObject.SetActive(false);
            isOptionBar = false;
        }
        catch when (optionbarCts.IsCancellationRequested)
        {

        }

    }

    // 재생 바 Off
    public void OffOptionBar()
    {
        isOptionBar = false;
        optionbarCts.Cancel();
    }

    private struct UserInteraction
    {
        public static float InactiveTime;
        private static Vector3 _previousMousePos;
        private static int _lastInputFrame;

        public static bool IsUserInputThisFrame()
        {
            if (Time.frameCount == _lastInputFrame)
            {
                return true;
            }
#if (!ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER)
            bool touchInput = (Input.touchSupported && Input.touchCount > 0);
            bool mouseInput = (Input.mousePresent && (Input.mousePosition != _previousMousePos || Input.mouseScrollDelta != Vector2.zero || Input.GetMouseButton(0)));

            if (touchInput || mouseInput)
            {
                _previousMousePos = Input.mousePosition;
                _lastInputFrame = Time.frameCount;
                return true;
            }

            return false;
#else
				return true;
#endif
        }
    }

    private Material DuplicateMaterialOnImage(Graphic image)
    {
        // Assign a copy of the material so we aren't modifying the material asset file
        image.material = new Material(image.material);
        return image.material;
    }

    private void SetupOptionButton()
    {
        _buttonOption.onClick.AddListener(OnPlayOptionButton);
    }

    // 화면을 누르면 재생바가 OnOff되는 기능
    private void OnPlayOptionButton()
    {
        if (isOptionBar)
        {
            _canvasTransform.gameObject.SetActive(false);
            isOptionBar = false;
        }
        else
        {
            _canvasTransform.gameObject.SetActive(true);
            isOptionBar = true;
        }
    }

    private void SetupPlayPauseButton()
    {
        if (_buttonPlayPause)
        {
            _buttonPlayPause.onClick.AddListener(OnPlayPauseButtonPressed);
            _playPauseMaterial = DuplicateMaterialOnImage(_buttonPlayPause.GetComponent<Image>());
        }
    }

    private void SetupVolumeButton()
    {
        if (_buttonVolume)
        {
            _buttonVolume.onClick.AddListener(OnVolumeButtonPressed);
            _volumeMaterial = DuplicateMaterialOnImage(_buttonVolume.GetComponent<Image>());
        }
    }

    private void OnPlayPauseButtonPressed()
    {
        if (optionbarCts != null)
            optionbarCts.Cancel();
        TogglePlayPause();
    }

    private void OnVolumeButtonPressed()
    {
        if (optionbarCts != null)
            optionbarCts.Cancel();
        ToggleMute();
    }




    void UpdateAudioFading()
    {
        // Increment fade timer
        if (_audioFadeTime < AudioFadeDuration)
        {
            _audioFadeTime = Mathf.Clamp(_audioFadeTime + Time.deltaTime, 0f, AudioFadeDuration);
        }

        // Trigger pause when audio faded down
        if (_audioFadeTime >= AudioFadeDuration)
        {
            if (!_isAudioFadingUpToPlay)
            {
                Pause(skipFeedback: true);
            }
        }

        // Apply audio fade value
        if (_mediaPlayer.Control != null && _mediaPlayer.Control.IsPlaying())
        {
            _audioFade = Mathf.Clamp01(_audioFadeTime / AudioFadeDuration);
            if (!_isAudioFadingUpToPlay)
            {
                _audioFade = (1f - _audioFade);
            }
            ApplyAudioVolume();
        }
    }

    public void TogglePlayPause()
    {
        if (_mediaPlayer && _mediaPlayer.Control != null)
        {
            if (_useAudioFading && _mediaPlayer.Info.HasAudio())
            {
                if (_mediaPlayer.Control.IsPlaying())
                {
                    if (_overlayManager)
                    {
                        _overlayManager.TriggerFeedback(OverlayManager.Feedback.Pause);
                    }
                    _isAudioFadingUpToPlay = false;
                }
                else if (_mediaPlayer.Control.IsFinished())
                {
                    _mediaPlayer.Stop();
                    _isAudioFadingUpToPlay = true;
                    Play();
                }
                else
                {
                    _isAudioFadingUpToPlay = true;
                    Play();
                }
                _audioFadeTime = 0f;
            }
            else
            {
                if (_mediaPlayer.Control.IsPlaying())
                {
                    Pause();
                }
                else if (_mediaPlayer.Control.IsFinished())
                {
                    _mediaPlayer.Stop();
                    Play();
                }
                else
                {
                    Play();
                }
            }
        }
    }

    public void Play()
    {
        if (_mediaPlayer && _mediaPlayer.Control != null)
        {
            if (_overlayManager)
            {
                _overlayManager.TriggerFeedback(OverlayManager.Feedback.Play);
            }
            _mediaPlayer.Play();
        }
    }

    private void Pause(bool skipFeedback = false)
    {
        if (_mediaPlayer && _mediaPlayer.Control != null)
        {
            if (!skipFeedback)
            {
                if (_overlayManager)
                {
                    _overlayManager.TriggerFeedback(OverlayManager.Feedback.Pause);
                }
            }
            _mediaPlayer.Pause();
        }
    }

    public void SeekRelative(float deltaTime)
    {
        if (_mediaPlayer && _mediaPlayer.Control != null)
        {
            TimeRange timelineRange = GetTimelineRange();
            double time = _mediaPlayer.Control.GetCurrentTime() + deltaTime;
            time = System.Math.Max(time, timelineRange.startTime);
            time = System.Math.Min(time, timelineRange.startTime + timelineRange.duration);
            _mediaPlayer.Control.Seek(time);

            if (_overlayManager)
            {
                _overlayManager.TriggerFeedback(deltaTime > 0f ? OverlayManager.Feedback.SeekForward : OverlayManager.Feedback.SeekBack);
            }
        }
    }

    public void ChangeAudioVolume(float delta)
    {
        if (_mediaPlayer && _mediaPlayer.Control != null)
        {
            // Change volume
            _audioVolume = Mathf.Clamp01(_audioVolume + delta);

            // Update the UI
            UpdateVolumeSlider();

            // Trigger the overlays
            if (_overlayManager)
            {
                _overlayManager.TriggerFeedback(delta > 0f ? OverlayManager.Feedback.VolumeUp : OverlayManager.Feedback.VolumeDown);
            }
        }
    }

    public void ToggleMute()
    {
        if (_mediaPlayer && _mediaPlayer.Control != null)
        {
            if (_mediaPlayer.AudioMuted)
            {
                MuteAudio(false);
            }
            else
            {
                MuteAudio(true);
            }
        }
    }

    private void MuteAudio(bool mute)
    {
        if (_mediaPlayer && _mediaPlayer.Control != null)
        {
            // Change mute
            _mediaPlayer.AudioMuted = mute;

            // Update the UI
            // The UI element is constantly updated by the Update() method

            // Trigger the overlays
            if (_overlayManager)
            {
                _overlayManager.TriggerFeedback(mute ? OverlayManager.Feedback.VolumeMute : OverlayManager.Feedback.VolumeUp);
            }
        }
    }

    private void CreateTimelineDragEvents()
    {
        EventTrigger trigger = _sliderTime.gameObject.GetComponent<EventTrigger>();
        if (trigger != null)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((data) => { OnTimeSliderBeginDrag(); });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Drag;
            entry.callback.AddListener((data) => { OnTimeSliderDrag(); });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback.AddListener((data) => { OnTimeSliderEndDrag(); });
            trigger.triggers.Add(entry);
        }
    }

    private void CreateVolumeSliderEvents()
    {
        if (_sliderVolume != null)
        {
            EventTrigger trigger = _sliderVolume.gameObject.GetComponent<EventTrigger>();
            if (trigger != null)
            {
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerDown;
                entry.callback.AddListener((data) => { OnVolumeSliderDrag(); });
                trigger.triggers.Add(entry);

                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.Drag;
                entry.callback.AddListener((data) => { OnVolumeSliderDrag(); });
                trigger.triggers.Add(entry);

                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerUp;
                entry.callback.AddListener((data) => { AutoOffOptionBar(); });
                trigger.triggers.Add(entry);

            }
        }
    }

    private void OnVolumeSliderDrag()
    {
        if (_mediaPlayer && _mediaPlayer.Control != null)
        {
            _audioVolume = _sliderVolume.value;
            ApplyAudioVolume();

            // OptionBar에서 Interaction이 있는 경우, OptionBar를 Off하지 않기 위해 CancellationToken Cancel
            if (optionbarCts != null && optionbarCts.IsCancellationRequested == false)
            {
                optionbarCts.Cancel();
            }
        }
    }

    private void ApplyAudioVolume()
    {
        if (_mediaPlayer)
        {
            _mediaPlayer.AudioVolume = (_audioVolume * _audioFade);
        }
    }

    private void UpdateVolumeSlider()
    {
        if (_sliderVolume)
        {
            if (_mediaPlayer)
            {
                _sliderVolume.value = _audioVolume;
            }
        }
    }


    private void OnTimeSliderBeginDrag()
    {
        if (_mediaPlayer && _mediaPlayer.Control != null)
        {
            // OptionBar에서 Interaction이 있는 경우, OptionBar를 Off하지 않기 위해 CancellationToken Cancel
            if (optionbarCts != null && optionbarCts.IsCancellationRequested == false)
                optionbarCts.Cancel();

            _isHoveringOverTimeline = true;
            _sliderTime.transform.localScale = new Vector3(1f, 2.5f, 1f);

            _wasPlayingBeforeTimelineDrag = _mediaPlayer.Control.IsPlaying();
            if (_wasPlayingBeforeTimelineDrag)
            {
                _mediaPlayer.Pause();
            }
            OnTimeSliderDrag();
        }
    }

    private void OnTimeSliderDrag()
    {
        if (_mediaPlayer && _mediaPlayer.Control != null)
        {
            TimeRange timelineRange = GetTimelineRange();
            double time = timelineRange.startTime + (_sliderTime.value * timelineRange.duration);
            _mediaPlayer.Control.Seek(time);
            _isHoveringOverTimeline = true;
        }
    }

    private void OnTimeSliderEndDrag()
    {
        if (_mediaPlayer && _mediaPlayer.Control != null)
        {
            AutoOffOptionBar();

            _isHoveringOverTimeline = false;
            _sliderTime.transform.localScale = new Vector3(1f, 1f, 1f);

            if (_wasPlayingBeforeTimelineDrag)
            {
                _mediaPlayer.Play();
                _wasPlayingBeforeTimelineDrag = false;
            }
        }
    }

    private TimeRange GetTimelineRange()
    {
        if (_mediaPlayer.Info != null)
        {
            return Helper.GetTimelineRange(_mediaPlayer.Info.GetDuration(), _mediaPlayer.Control.GetSeekableTimes());
        }
        return new TimeRange();
    }


    void Update()
    {
        if (!_mediaPlayer || !isPlaybar)
            return;

        UpdateAudioFading();
        UpdatePlayPauseButton();

        if (_mediaPlayer.Info != null)
        {
            TimeRange timelineRange = GetTimelineRange();

            // Update timeline hover popup
#if (!ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER)
            if (_timelineTip != null)
            {
                if (_isHoveringOverTimeline)
                {
                    Vector2 canvasPos;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasTransform, Input.mousePosition, null, out canvasPos);

                    _segmentsSeek.gameObject.SetActive(true);
                    _timelineTip.gameObject.SetActive(true);
                    Vector3 mousePos = _canvasTransform.TransformPoint(canvasPos);

                    _timelineTip.position = new Vector2(mousePos.x, _timelineTip.position.y);

                    if (UserInteraction.IsUserInputThisFrame())
                    {
                        // Work out position on the timeline
                        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(this._sliderTime.GetComponent<RectTransform>());
                        float x = Mathf.Clamp01((canvasPos.x - bounds.min.x) / bounds.size.x);

                        double time = (double)x * timelineRange.Duration;

                        // Seek to the new position
                        if (_thumbnailMediaPlayer != null && _thumbnailMediaPlayer.Control != null)
                        {
                            _thumbnailMediaPlayer.Control.SeekFast(time);
                        }

                        // Update time text
                        Text hoverText = _timelineTip.GetComponentInChildren<Text>();
                        if (hoverText != null)
                        {
                            time -= timelineRange.startTime;
                            time = System.Math.Max(time, 0.0);
                            time = System.Math.Min(time, timelineRange.Duration);
                            hoverText.text = Helper.GetTimeString(time, false);
                        }

                        {
                            // Update seek segment when hovering over timeline
                            if (_segmentsSeek != null)
                            {
                                float[] ranges = new float[2];
                                if (timelineRange.Duration > 0.0)
                                {
                                    double t = ((_mediaPlayer.Control.GetCurrentTime() - timelineRange.startTime) / timelineRange.duration);
                                    ranges[1] = x;
                                    ranges[0] = (float)t;
                                }
                                _segmentsSeek.Segments = ranges;
                            }
                        }
                    }
                }
                else
                {
                    _timelineTip.gameObject.SetActive(false);
                    _segmentsSeek.gameObject.SetActive(false);
                }
            }
#endif

            // Updated stalled display
            if (_overlayManager)
            {
                _overlayManager.Reset();
                if (_mediaPlayer.Info.IsPlaybackStalled())
                {
                    _overlayManager.TriggerStalled();
                }
            }

            if (_playPauseMaterial != null)
            {
                float t = _playPauseMaterial.GetFloat(_propMorph.Id);
                float d = 1f;
                if (_mediaPlayer.Control.IsPlaying())
                {
                    d = -1f;
                }
                t += d * Time.deltaTime * 6f;
                t = Mathf.Clamp01(t);
                _playPauseMaterial.SetFloat(_propMorph.Id, t);
            }

            // Animation volume/mute button
            if (_volumeMaterial != null)
            {
                float t = _volumeMaterial.GetFloat(_propMute.Id);
                float d = 1f;
                if (!_mediaPlayer.AudioMuted)
                {
                    d = -1f;
                }
                t += d * Time.deltaTime * 6f;
                t = Mathf.Clamp01(t);
                _volumeMaterial.SetFloat(_propMute.Id, t);
                _volumeMaterial.SetFloat(_propVolume.Id, _audioVolume);
            }

            // Update time/duration text display
            if (_textTimeDuration)
            {
                string t1 = Helper.GetTimeString((_mediaPlayer.Control.GetCurrentTime() - timelineRange.startTime), false);
                string d1 = Helper.GetTimeString(timelineRange.duration, false);
                _textTimeDuration.text = string.Format("{0} / {1}", t1, d1);
            }

            // Update volume slider
            if (!_useAudioFading)
            {
                UpdateVolumeSlider();
            }

            // Update time slider position
            if (_sliderTime && !_isHoveringOverTimeline)
            {
                double t = 0.0;
                if (timelineRange.duration > 0.0)
                {
                    t = ((_mediaPlayer.Control.GetCurrentTime() - timelineRange.startTime) / timelineRange.duration);
                }
                _sliderTime.value = Mathf.Clamp01((float)t);
            }


            // Update buffered segments
            if (_segmentsBuffered)
            {
                TimeRanges times = _mediaPlayer.Control.GetBufferedTimes();
                float[] ranges = null;
                if (times.Count > 0 && timelineRange.duration > 0.0)
                {
                    ranges = new float[times.Count * 2];
                    for (int i = 0; i < times.Count; i++)
                    {
                        ranges[i * 2 + 0] = Mathf.Max(0f, (float)((times[i].StartTime - timelineRange.startTime) / timelineRange.duration));
                        ranges[i * 2 + 1] = Mathf.Min(1f, (float)((times[i].EndTime - timelineRange.startTime) / timelineRange.duration));
                    }
                }
                _segmentsBuffered.Segments = ranges;
            }

            // Update progress segment
            if (_segmentsProgress)
            {
                TimeRanges times = _mediaPlayer.Control.GetBufferedTimes();
                float[] ranges = null;
                if (times.Count > 0 && timelineRange.Duration > 0.0)
                {
                    ranges = new float[2];
                    double x1 = (times.MinTime - timelineRange.startTime) / timelineRange.duration;
                    double x2 = ((_mediaPlayer.Control.GetCurrentTime() - timelineRange.startTime) / timelineRange.duration);
                    ranges[0] = Mathf.Max(0f, (float)x1);
                    ranges[1] = Mathf.Min(1f, (float)x2);
                }
                _segmentsProgress.Segments = ranges;
            }
        }
    }

    private void UpdatePlayPauseButton()
    {
        if (_mediaPlayer.Control.IsFinished())
        {
            Pause();
        }
    }
}

#endif

