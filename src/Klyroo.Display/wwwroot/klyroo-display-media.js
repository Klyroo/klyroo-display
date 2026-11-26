// Drives the <audio> or <video> element of KlyAudioPlayer and KlyVideoPlayer. The component owns the rules (clip,
// play count, lock); this module moves the element and reports what it does back to .NET.

const states = new WeakMap();

// The keys a focused player reacts to. Only keys pressed on the player root count: a key on one of its buttons or sliders
// belongs to that control (Space on a focused button already clicks it), so handling it here would act twice.
const playerKeys = new Set([' ', 'k', 'K', 'm', 'M']);

function apply(media, state) {
  const { muted, rate, volume, track } = state.options;
  media.muted = muted;
  if (rate > 0) media.playbackRate = rate;
  media.volume = Math.min(1, Math.max(0, volume));
  const tracks = media.textTracks;
  if (tracks && track !== undefined) {
    for (let i = 0; i < tracks.length; i++) tracks[i].mode = i === track ? 'showing' : 'disabled';
  }
}

function report(state, method, ...args) {
  state.dotnet.invokeMethodAsync(method, ...args).catch(() => {});
}

/**
 * Starts listening to the element, and to keys and fullscreen changes of the player root when given.
 * `options`: { start, end, loop, muted, rate, volume, track, keyboard }.
 */
export function attach(media, dotnet, options, root) {
  detach(media);
  const state = { dotnet, options, clipEnding: false, root: root ?? null };

  // True when the clip end was reached and handled. "timeupdate" fires only every 250 ms or so, so a timer aimed at
  // the end also calls it, to stop within a few milliseconds instead of a quarter second late.
  const reachEnd = () => {
    clearTimeout(state.endTimer);
    const { start, end, loop } = state.options;
    if (end === null || end === undefined || media.paused) return false;
    if (media.currentTime < end) {
      state.endTimer = setTimeout(reachEnd, ((end - media.currentTime) / (media.playbackRate || 1)) * 1000);
      return false;
    }
    if (loop) {
      media.currentTime = start;
      return true;
    }
    // The clip ends before the file does: stop here and report the end, not a user pause.
    state.clipEnding = true;
    media.pause();
    report(state, 'Ended');
    return true;
  };

  state.listeners = {
    timeupdate: () => {
      const { end } = state.options;
      // A paused element past the end was seeked there; the component already handled it.
      if (media.paused && end !== null && end !== undefined && media.currentTime >= end) return;
      if (!reachEnd()) report(state, 'TimeUpdate', media.currentTime);
    },
    ended: () => report(state, 'Ended'),
    loadedmetadata: () => {
      if (Number.isFinite(media.duration)) report(state, 'DurationChange', media.duration);
      apply(media, state);
    },
    play: () => {
      report(state, 'PlayingChange', true);
      // Started at or past End (a media key after the clip ended): the component starts a new play from Start, so
      // ending it here would spend that play without a sound.
      const { end } = state.options;
      if (end === null || end === undefined || media.currentTime < end) reachEnd();
    },
    pause: () => {
      // A natural end also fires "pause"; the "ended" event reports it.
      if (state.clipEnding || media.ended) {
        state.clipEnding = false;
        return;
      }
      report(state, 'PlayingChange', false);
    },
    enterpictureinpicture: () => report(state, 'PictureInPictureChange', true),
    leavepictureinpicture: () => report(state, 'PictureInPictureChange', false),
  };
  for (const [type, listener] of Object.entries(state.listeners)) media.addEventListener(type, listener);

  if (state.root) {
    state.rootListeners = {
      keydown: (event) => {
        if (event.target !== state.root || !state.options.keyboard) return;
        if (event.ctrlKey || event.metaKey || event.altKey || !playerKeys.has(event.key)) return;
        // Space would also scroll the page.
        event.preventDefault();
        report(state, 'Key', event.key);
      },
      fullscreenchange: () => report(state, 'FullscreenChange', document.fullscreenElement === state.root),
    };
    for (const [type, listener] of Object.entries(state.rootListeners)) state.root.addEventListener(type, listener);
  }
  states.set(media, state);

  apply(media, state);
  if (media.readyState >= 1) state.listeners.loadedmetadata();

  const pictureInPicture = media instanceof HTMLVideoElement
    && document.pictureInPictureEnabled === true
    && typeof media.requestPictureInPicture === 'function';
  const fullscreen = !!state.root
    && document.fullscreenEnabled === true
    && typeof state.root.requestFullscreen === 'function';
  report(state, 'Capabilities', pictureInPicture, fullscreen);
}

/** Replaces the options of an attached element. */
export function update(media, options) {
  const state = states.get(media);
  if (!state) return;
  state.options = options;
  apply(media, state);
}

/**
 * Plays, from `from` seconds when given. It returns at once: the play promise stays pending while the media buffers.
 * A refused play (autoplay policy, unsupported source) is reported; a play cut short by a pause is not.
 */
export function play(media, from) {
  const state = states.get(media);
  if (from !== null && from !== undefined) media.currentTime = from;
  media.play().catch((error) => {
    if (state && error?.name !== 'AbortError' && media.paused) report(state, 'PlayFailed');
  });
}

export function pause(media) {
  media.pause();
}

export function seek(media, time) {
  media.currentTime = time;
}

/** Opens the video in a picture-in-picture window, or closes the window when it already shows this video. */
export function togglePictureInPicture(media) {
  if (document.pictureInPictureElement === media) {
    document.exitPictureInPicture().catch(() => {});
  } else if (typeof media.requestPictureInPicture === 'function' && !media.disablePictureInPicture) {
    // Refused without a recent user gesture or when the browser forbids it; the button simply does nothing then.
    media.requestPictureInPicture().catch(() => {});
  }
}

/** Shows the player root fullscreen, or leaves fullscreen when the root is already fullscreen. */
export function toggleFullscreen(media) {
  const root = states.get(media)?.root;
  if (!root) return;
  if (document.fullscreenElement === root) {
    document.exitFullscreen().catch(() => {});
  } else if (typeof root.requestFullscreen === 'function') {
    root.requestFullscreen().catch(() => {});
  }
}

export function detach(media) {
  const state = states.get(media);
  if (!state) return;
  clearTimeout(state.endTimer);
  for (const [type, listener] of Object.entries(state.listeners)) media.removeEventListener(type, listener);
  if (state.root && state.rootListeners) {
    for (const [type, listener] of Object.entries(state.rootListeners)) state.root.removeEventListener(type, listener);
  }
  states.delete(media);
}
