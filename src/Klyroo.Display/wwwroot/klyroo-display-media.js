// Drives the <audio> or <video> element of KlyAudioPlayer and KlyVideoPlayer. The component owns the rules (clip,
// play count, lock); this module moves the element and reports what it does back to .NET.

const states = new WeakMap();

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

/** Starts listening to the element. `options`: { start, end, loop, muted, rate, volume, track }. */
export function attach(media, dotnet, options) {
  detach(media);
  const state = { dotnet, options, clipEnding: false };

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
      reachEnd();
    },
    pause: () => {
      // A natural end also fires "pause"; the "ended" event reports it.
      if (state.clipEnding || media.ended) {
        state.clipEnding = false;
        return;
      }
      report(state, 'PlayingChange', false);
    },
  };
  for (const [type, listener] of Object.entries(state.listeners)) media.addEventListener(type, listener);
  states.set(media, state);

  apply(media, state);
  if (media.readyState >= 1) state.listeners.loadedmetadata();
}

/** Replaces the options of an attached element. */
export function update(media, options) {
  const state = states.get(media);
  if (!state) return;
  state.options = options;
  apply(media, state);
}

/** Plays, from `from` seconds when given. A refused play (autoplay policy, unsupported source) is reported. */
export async function play(media, from) {
  const state = states.get(media);
  if (from !== null && from !== undefined) media.currentTime = from;
  try {
    await media.play();
  } catch {
    if (state) report(state, 'PlayFailed');
  }
}

export function pause(media) {
  media.pause();
}

export function seek(media, time) {
  media.currentTime = time;
}

export function detach(media) {
  const state = states.get(media);
  if (!state) return;
  clearTimeout(state.endTimer);
  for (const [type, listener] of Object.entries(state.listeners)) media.removeEventListener(type, listener);
  states.delete(media);
}
