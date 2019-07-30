using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace AuxLib.Sound
{
    /// <summary>
    /// Component that manages audio playback for all sound effects.
    /// </summary>
    public class AudioManager : GameComponent
    {
        #region Singleton


        /// <summary>
        /// The singleton for this type
        /// </summary>
        private static AudioManager audioManager = null;


        #endregion


        #region Audio Data

        /// <summary>
        /// File list of all wav audio files
        /// </summary>
        private FileInfo[] audioFileList;
        private FileInfo[] musicFileList;

        /// <summary>
        /// Content folder containing audio files
        /// </summary>
        private DirectoryInfo audioFolder;
        private DirectoryInfo musicFolder;

        /// <summary>
        /// Collection of all loaded sound effects
        /// </summary>
        private static Dictionary<string, SoundEffect> soundList;
        public static float EffectVolume { get; set; }

        /// <summary>
        /// Looping song used as the in-game soundtrack
        /// </summary>
        private static Dictionary<string, Song> musicList;
        public static float MusicVolume { set { MediaPlayer.Volume = value; } }

        #endregion


        #region Initialization Methods

        /// <summary>
        /// Constructs the manager for audio playback of all sound effects.
        /// </summary>
        /// <param name="game">The game that this component will be attached to.</param>
        /// <param name="audioFolder">The directory containing audio files.</param>
        private AudioManager(Game game, DirectoryInfo audioDirectory, DirectoryInfo songDirectory)
            : base(game)
        {
            try
            {
                audioFolder = audioDirectory;
                audioFileList = audioFolder.GetFiles("*.xnb");
                soundList = new Dictionary<string, SoundEffect>();

                for (var i = 0; i < audioFileList.Length; i++)
                {
                    var fn = Path.GetFileNameWithoutExtension(audioFileList[i].Name);
                    soundList[fn] = game.Content.Load<SoundEffect>("SFX\\" + fn);
                    soundList[fn].Name = fn;
                }

                musicFolder = songDirectory;
                musicFileList = musicFolder.GetFiles("*.xnb");
                musicList = new Dictionary<string, Song>();

                for (var i = 0; i < musicFileList.Length; i++)
                {
                    var fn = Path.GetFileNameWithoutExtension(musicFileList[i].Name);
                    musicList[fn] = game.Content.Load<Song>("Music\\" + fn);                    
                }


            }
            catch (NoAudioHardwareException)
            {
                // silently fall back to silence
            }
        }

        public static void Initialize(Game game, DirectoryInfo audioDirectory, DirectoryInfo songDirectory)
        {
            if (game == null)
                return;

            audioManager = new AudioManager(game, audioDirectory, songDirectory);
            game.Components.Add(audioManager);
        }

        public static void PlaySoundTrack(string name, bool repeat = true, bool overwrite = false)
        {
            
            if (!musicList.ContainsKey(name))
                return;
            if (MediaPlayer.State == MediaState.Stopped)
            {
                MediaPlayer.Play(musicList[name]);
                
            }
            else if (MediaPlayer.State == MediaState.Playing && overwrite)
            {
                MediaPlayer.Play(musicList[name]);
            }
            MediaPlayer.IsRepeating = repeat;
            return;

        }

        public static void StopSoundTrack()
        {
            MediaPlayer.Stop();
        }

        #endregion


        #region Sound Play Methods

        /// <summary>
        /// Plays a fire-and-forget sound effect by name.
        /// </summary>
        /// <param name="soundName">The name of the sound to play.</param>
        public static void PlaySoundEffect(string soundName)
        {
            if (audioManager == null || soundList == null)
                return;

            if (soundList.ContainsKey(soundName))
            {
                soundList[soundName].Play();
            }
        }

        /// <summary>
        /// Plays a sound effect by name and returns an instance of that sound.
        /// </summary>
        /// <param name="soundName">The name of the sound to play.</param>
        /// <param name="looped">True if sound effect should loop.</param>
        /// <param name="instance">The SoundEffectInstance created for this sound effect.</param>
        public static void PlaySoundEffect(string soundName, bool looped, out SoundEffectInstance instance)
        {
            instance = null;
            if (audioManager == null || soundList == null)
                return;

            if (soundList.ContainsKey(soundName))
            {
                try
                {
                    instance = soundList[soundName].CreateInstance();
                    if (instance != null)
                    {
                        instance.IsLooped = looped;
                        instance.Play();
                    }
                }
                catch (InstancePlayLimitException)
                {
                    // silently fail (returns null instance) if instance limit reached
                }
            }
        }

        #endregion

    }
}