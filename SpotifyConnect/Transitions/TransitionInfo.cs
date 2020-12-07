using JetBrains.Annotations;
using SpotifyLib.Enums;

namespace SpotifyLib.SpotifyConnect.Transitions
{
    public class TransitionInfo

    {
        /// <summary>
        /// How the next track started
        /// </summary>
        public readonly Reason StartedReason;

        /// <summary>
        /// How the previous track ended
        /// </summary>
        public readonly Reason EndedReason;

        /// <summary>
        /// When the previous track ended
        /// </summary>
        public int EndedWhen = -1;

        public TransitionInfo(
            [NotNull] Reason endedReason,
            [NotNull] Reason startedReason)
        {
            StartedReason = startedReason;
            EndedReason = endedReason;
        }
        /// <summary>
        /// Context changed.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="withSkip"></param>
        /// <returns></returns>
        public static TransitionInfo ContextChange(
            [NotNull] SpotifyState state, bool withSkip)
        {
            var trans = new TransitionInfo(Reason.endplay,
                withSkip
                    ? Reason.clickrow
                    : Reason.playbtn);
            if (state.GetCurrentPlayable() != null) trans.EndedWhen = state.GetPosition();
            return trans;
        }
        /// <summary>
        /// Skipping to another track in the same context.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static TransitionInfo SkipTo([NotNull] SpotifyState state)
        {
            var trans = new TransitionInfo(
                Reason.endplay,
                Reason.clickrow);
            if (state.GetCurrentPlayable() != null) trans.EndedWhen = state.GetPosition();
            return trans;
        }
        /// <summary>
        /// Skipping to previous track.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static TransitionInfo SkippedPrev([NotNull] SpotifyState state)
        {
            var trans = new TransitionInfo(Reason.backbtn, Reason.backbtn);
            if (state.GetCurrentPlayable() != null) trans.EndedWhen = state.GetPosition();
            return trans;
        }
        /// <summary>
        /// Skipping to next track.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static TransitionInfo SkippedNext([NotNull] SpotifyState state)
        {
            var trans = new TransitionInfo(Reason.fwdbtn, Reason.fwdbtn);
            if (state.GetCurrentPlayable() != null) trans.EndedWhen = state.GetPosition();
            return trans;
        }
    }
}
