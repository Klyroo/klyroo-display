using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Klyroo.Display;

/// <summary>What a media player component receives from its <c>audio</c> or <c>video</c> element.</summary>
internal interface IMediaElementHost
{
    Task OnMediaTimeUpdateAsync(double time);
    Task OnMediaEndedAsync();
    Task OnMediaDurationAsync(double duration);
    Task OnMediaPlayingChangedAsync(bool playing);
    Task OnMediaPlayFailedAsync();
}

/// <summary>The element state a player pushes to the browser. <see cref="End"/> is set only for an explicit clip end.</summary>
internal readonly record struct MediaElementOptions(double Start, double? End, bool Loop, bool Muted, double Rate, double Volume, int Track);

/// <summary>
/// Drives a media element through <c>klyroo-display-media.js</c>. Every call is a no-op until <see cref="AttachAsync"/>
/// has run, which happens after the first interactive render, so prerendering and static rendering keep working.
/// </summary>
internal sealed class MediaElementInterop(IMediaElementHost host) : IAsyncDisposable
{
    private const string ModulePath = "./_content/Klyroo.Display/klyroo-display-media.js";

    private IJSObjectReference? _module;
    private DotNetObjectReference<MediaElementInterop>? _reference;
    private ElementReference _element;
    private MediaElementOptions _options;
    private bool _disposed;
    private bool _playPending;
    private double? _pendingFrom;

    public async Task AttachAsync(IJSRuntime js, ElementReference element, MediaElementOptions options)
    {
        IJSObjectReference module;
        try
        {
            module = await js.InvokeAsync<IJSObjectReference>("import", ModulePath);
        }
        catch (JSDisconnectedException)
        {
            return;
        }
        if (_disposed)
        {
            // The player was removed while the module loaded: its element is gone.
            await DisposeModuleAsync(module);
            return;
        }

        _reference = DotNetObjectReference.Create(this);
        _element = element;
        _options = options;
        await module.InvokeVoidAsync("attach", element, _reference, ToScript(options));
        _module = module;
        if (_disposed)
        {
            // Removed during the attach call: DisposeAsync ran before _module was set.
            _module = null;
            await DisposeModuleAsync(module);
            _reference.Dispose();
            _reference = null;
            return;
        }

        // A Play pressed before the module was ready.
        if (_playPending)
        {
            _playPending = false;
            await module.InvokeVoidAsync("play", element, _pendingFrom);
        }
    }

    public async Task UpdateAsync(MediaElementOptions options)
    {
        if (_module is null || options == _options) return;
        _options = options;
        await _module.InvokeVoidAsync("update", _element, ToScript(options));
    }

    public Task PlayAsync(double? from)
    {
        if (_module is null && !_disposed)
        {
            // Keep the earliest start point: a resume does not move a play that has not started yet.
            _pendingFrom = _playPending ? _pendingFrom ?? from : from;
            _playPending = true;
        }
        return InvokeAsync("play", from);
    }

    public Task PauseAsync()
    {
        _playPending = false;
        return InvokeAsync("pause");
    }

    public Task SeekAsync(double time) => InvokeAsync("seek", time);

    [JSInvokable("TimeUpdate")]
    public Task TimeUpdate(double time) => host.OnMediaTimeUpdateAsync(time);

    [JSInvokable("Ended")]
    public Task Ended() => host.OnMediaEndedAsync();

    [JSInvokable("DurationChange")]
    public Task DurationChange(double duration) => host.OnMediaDurationAsync(duration);

    [JSInvokable("PlayingChange")]
    public Task PlayingChange(bool playing) => host.OnMediaPlayingChangedAsync(playing);

    [JSInvokable("PlayFailed")]
    public Task PlayFailed() => host.OnMediaPlayFailedAsync();

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        if (_module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("detach", _element);
            }
            catch (JSDisconnectedException)
            {
                // The circuit is gone, and the element with it.
            }
            await DisposeModuleAsync(_module);
            _module = null;
        }
        _reference?.Dispose();
        _reference = null;
    }

    private static async Task DisposeModuleAsync(IJSObjectReference module)
    {
        try
        {
            await module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
        }
    }

    private async Task InvokeAsync(string identifier, params object?[] args)
    {
        if (_module is null) return;
        await _module.InvokeVoidAsync(identifier, [_element, .. args]);
    }

    // Plain values only, so the call needs no reflection-based serialization of a custom type.
    private static Dictionary<string, object?> ToScript(MediaElementOptions o) => new()
    {
        ["start"] = o.Start,
        ["end"] = o.End,
        ["loop"] = o.Loop,
        ["muted"] = o.Muted,
        ["rate"] = o.Rate,
        ["volume"] = o.Volume,
        ["track"] = o.Track,
    };
}
